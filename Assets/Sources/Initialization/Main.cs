using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;

using TMPro;

public class Main : MonoBehaviour
{
    private const string SheetPath = "spreadsheet_data";

    private const string UIPrefabName = "ui";

    private const string StagingParentName = "staging";
    private const string UnitParentName = "character_container";
    private const string UnitInfoUIParentName = "canvas/unit_info";

    private const string ReloadButtonName = "canvas/button_reload";
    private const string SimulateButtonName = "canvas/button_simulate";
    private const string LeftUnitSelectName = "canvas/left_unit_select";
    private const string RightUnitSelectName = "canvas/right_unit_select";

    private int m_LeftUnitIndex = -1;
    private int m_RightUnitIndex = -1;

    private GameObject m_UIPrefab = null;

    private Button m_ReloadButton = null;
    private Button m_SimulateButton = null;

    private TMP_Dropdown m_LeftUnitSelectDropdown = null;
    private TMP_Dropdown m_RightUnitSelectDropdown = null;

    private Transform m_UnitParent = null;

    private Transform m_LeftStagingSlot = null;
    private Transform m_RightStagingSlot = null;

    private UnitData m_LeftUnitData = null;
    private UnitData m_RightUnitData = null;

    private UnitPresentationData m_LeftUnitPresentationData = null;
    private UnitPresentationData m_RightUnitPresentationData = null;

    private UnitInfoUI m_LeftInfoUI = null;
    private UnitInfoUI m_RightInfoUI = null;

    private UnitRepository m_UnitRepository = null;
    private UnitPresentationRepository m_UnitPresentationRepository = null;

    private UnitSimulation m_UnitSimulation = null;
    private UnitPresentation m_UnitPresentation = null;

    private IEnumerator Start()
    {
        //disable update loop and only start when we click simulate
        enabled = false;

        SpreadsheetData resultData = new SpreadsheetData();
        yield return SpreadsheetUtility.LoadSpreadsheet(SheetPath, resultData);

        m_UnitSimulation = new UnitSimulation();
        m_UnitPresentation = new UnitPresentation();

        m_UnitRepository = SpreadsheetUtility.LoadUnitSpreadsheet(resultData.ResultJSON);
        Debug.LogFormat("Managed to load {0} units", m_UnitRepository.Count);

        Transform staging = GameObject.Find(StagingParentName).transform;
        m_LeftStagingSlot = staging.GetChild(0);
        m_RightStagingSlot = staging.GetChild(1);

        //instantiate a unit prefab parent
        m_UnitParent = new GameObject(UnitParentName).transform;
        m_UnitParent.position = new Vector3(5, 0, 0);

        yield return LoadUnitPrefabs(m_UnitRepository, m_UnitParent);
        yield return LoadUI();

        m_ReloadButton.onClick.AddListener(OnReloadSpreadsheet);
        m_SimulateButton.onClick.AddListener(OnSimulate);

        m_LeftUnitSelectDropdown.onValueChanged.AddListener(OnLeftUnitSelected);
        m_RightUnitSelectDropdown.onValueChanged.AddListener(OnRightUnitSelected);

        m_LeftUnitSelectDropdown.options.Clear();
        m_RightUnitSelectDropdown.options.Clear();

        List<string> options = new List<string>() { "Random" };
        foreach (UnitData unit in m_UnitRepository)
        {
            string id = unit.UnitStats.ID;
            options.Add(id[0].ToString().ToUpper() + id[1..]);
        }

        m_LeftUnitSelectDropdown.AddOptions(options);
        m_RightUnitSelectDropdown.AddOptions(options);

        OnLeftUnitSelected(0);
        OnRightUnitSelected(0);
    }

    private void Update()
    {
        //simulation phase
        m_UnitSimulation.UpdateUnit(m_LeftUnitData);
        m_UnitSimulation.UpdateUnit(m_RightUnitData);

        //simulation and presentation mix
        bool anyDead = false;
        if (m_LeftUnitData.AttackCounter >= 1f)
        {
            m_UnitSimulation.Attack(m_LeftUnitData, m_RightUnitData);
            m_UnitPresentation.Attack(m_LeftUnitPresentationData);

            //ui update
            float rightUnitHealthRatio = m_RightUnitData.Health / m_RightUnitData.UnitStats.BaseHealth;
            m_RightInfoUI.UpdateUnitHealth(m_RightUnitData.Health, rightUnitHealthRatio);

            if (m_RightUnitData.Health == 0)
            {
                m_UnitPresentation.Death(m_RightUnitPresentationData);

                anyDead = true;
            }
            else
            {
                m_UnitPresentation.Hit(m_RightUnitPresentationData);
            }
        }

        if (m_RightUnitData.AttackCounter >= 1f)
        {
            m_UnitSimulation.Attack(m_RightUnitData, m_LeftUnitData);
            m_UnitPresentation.Attack(m_RightUnitPresentationData);

            //ui update
            float leftUnitHealthRatio = m_LeftUnitData.Health / m_LeftUnitData.UnitStats.BaseHealth;
            m_LeftInfoUI.UpdateUnitHealth(m_LeftUnitData.Health, leftUnitHealthRatio);

            if (m_LeftUnitData.Health == 0)
            {
                m_UnitPresentation.Death(m_LeftUnitPresentationData);

                anyDead = true;
            }
            else
            {
                m_UnitPresentation.Hit(m_LeftUnitPresentationData);
            }
        }

        //simulation end phase
        m_UnitSimulation.CheckUnit(m_LeftUnitData);
        m_UnitSimulation.CheckUnit(m_RightUnitData);

        //first unit who wins, end simulation
        if (anyDead)
        {
            enabled = false;
        }
    }

    //on application quit gets called before all the destroy nonsense, so we use this to release while the objects are still available :(
    private void OnApplicationQuit()
    {
        foreach (UnitPresentationData unitPresentationData in m_UnitPresentationRepository)
        {
            Addressables.ReleaseInstance(unitPresentationData.GameObject);
        }

        Addressables.ReleaseInstance(m_UIPrefab);
    }

    private IEnumerator LoadUI()
    {
        var loadOperation = Addressables.InstantiateAsync(UIPrefabName);
        yield return loadOperation;

        m_UIPrefab = loadOperation.Result;

        Transform ui = m_UIPrefab.transform;
        ui.name = UIPrefabName;

        m_ReloadButton = ui.Find(ReloadButtonName).GetComponent<Button>();
        m_SimulateButton = ui.Find(SimulateButtonName).GetComponent<Button>();

        m_LeftUnitSelectDropdown = ui.Find(LeftUnitSelectName).GetComponent<TMP_Dropdown>();
        m_RightUnitSelectDropdown = ui.Find(RightUnitSelectName).GetComponent<TMP_Dropdown>();

        Transform unitInfoUIParent = ui.Find(UnitInfoUIParentName);

        m_LeftInfoUI = new UnitInfoUI(unitInfoUIParent.GetChild(0));
        m_RightInfoUI = new UnitInfoUI(unitInfoUIParent.GetChild(1));
    }

    private IEnumerator LoadUnitPrefabs(UnitRepository unitRepository, Transform unitParent)
    {
        m_UnitPresentationRepository = new UnitPresentationRepository(unitRepository.Count);

        int i = 0;
        foreach (UnitData unit in unitRepository)
        {
            string visualName = unit.UnitVisualData.Visual.ToString().ToLower();
            var loadOperation = Addressables.InstantiateAsync(visualName, unitParent);
            yield return loadOperation;

            GameObject instance = loadOperation.Result;
            instance.name = unit.UnitStats.ID + "_" + visualName;
            instance.SetActive(false);

            UnitPresentationData unitPresentationData = new UnitPresentationData(instance);
            m_UnitPresentationRepository.AddUnit(i++, unitPresentationData);
        }
    }

    private void OnReloadSpreadsheet()
    {

    }

    private void OnReload(int unitIndex, Transform stagingSlot, UnitInfoUI infoUI, ref UnitData unitToReload, ref UnitPresentationData unitPresentationToReload)
    {
        enabled = false;

        //reset previous units
        if (unitPresentationToReload != null)
        {
            unitPresentationToReload.GameObject.transform.SetParent(m_UnitParent, false);
            unitPresentationToReload.GameObject.SetActive(false);
        }

        unitToReload = m_UnitRepository[unitIndex];

        unitPresentationToReload = m_UnitPresentationRepository[unitIndex];
        unitPresentationToReload.GameObject.transform.SetParent(stagingSlot, false);

        //init unit info
        float healthDiff = m_UnitRepository.MaxUnitHealth - m_UnitRepository.MinUnitHealth;

        float unitHealthWeight = (unitToReload.UnitStats.BaseHealth - m_UnitRepository.MinUnitHealth) / healthDiff;
        infoUI.Init(unitToReload, unitHealthWeight);

        unitPresentationToReload.GameObject.SetActive(true);
    }


    private void OnLeftUnitSelected(int selection)
    {
        //unit index is selection - 1 -> since index 0 = random
        m_LeftUnitIndex = --selection;

        if (m_LeftUnitIndex == -1)
        {
            m_LeftUnitIndex = Random.Range(0, m_UnitRepository.Count);

            //set dropdown value to -1 so we can re-select first option (random)
            m_LeftUnitSelectDropdown.SetValueWithoutNotify(-1);
        }

        if (m_LeftUnitIndex != m_RightUnitIndex)
        {
            OnReload(m_LeftUnitIndex, m_LeftStagingSlot, m_LeftInfoUI, ref m_LeftUnitData, ref m_LeftUnitPresentationData);
        }
    }

    private void OnRightUnitSelected(int selection)
    {
        //unit index is selection - 1 -> since index 0 = random
        m_RightUnitIndex = --selection;

        if (m_RightUnitIndex == -1)
        {
            m_RightUnitIndex = Random.Range(0, m_UnitRepository.Count);

            //set dropdown value to -1 so we can re-select first option (random)
            m_RightUnitSelectDropdown.SetValueWithoutNotify(-1);
        }

        if (m_RightUnitIndex != m_LeftUnitIndex)
        {
            OnReload(m_RightUnitIndex, m_RightStagingSlot, m_RightInfoUI, ref m_RightUnitData, ref m_RightUnitPresentationData);
        }
    }

    private void OnSimulate()
    {
        //reset both units then try again
        m_UnitSimulation.ResetUnit(m_LeftUnitData);
        m_UnitSimulation.ResetUnit(m_RightUnitData);

        m_UnitPresentation.Idle(m_LeftUnitPresentationData);
        m_UnitPresentation.Idle(m_RightUnitPresentationData);

        m_LeftInfoUI.UpdateUnitHealth(m_LeftUnitData.Health, 1);
        m_RightInfoUI.UpdateUnitHealth(m_RightUnitData.Health, 1);

        enabled = true;
    }
}
