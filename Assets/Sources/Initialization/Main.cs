using System.Collections;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;

public class Main : MonoBehaviour
{
    private const string SheetPath = "spreadsheet_data";
    private const string UnitParentName = "character_container";
    private const string StagingParentName = "staging";
    private const string UIPrefabName = "ui";
    private const string UnitInfoUIParentName = "canvas/unit_info";

    private GameObject[] m_UnitPrefabs = null;
    private GameObject m_UIPrefab = null;

    private Button m_ReloadButton = null;

    private Transform m_UnitParent = null;

    private Transform m_LeftStagingSlot = null;
    private Transform m_RightStagingSlot = null;

    private GameObject m_LeftUnit = null;
    private GameObject m_RightUnit = null;

    private UnitInfoUI m_LeftInfoUI = null;
    private UnitInfoUI m_RightInfoUI = null;

    private UnitRepository m_UnitRepository = null;

    private IEnumerator Start()
    {
        SpreadsheetData resultData = new SpreadsheetData();
        yield return SpreadsheetUtility.LoadSpreadsheet(SheetPath, resultData);

        m_UnitRepository = SpreadsheetUtility.LoadUnitSpreadsheet(resultData.ResultJSON);
        Debug.LogFormat("Manager to load {0} units", m_UnitRepository.Count);

        Transform staging = GameObject.Find(StagingParentName).transform;
        m_LeftStagingSlot = staging.GetChild(0);
        m_RightStagingSlot = staging.GetChild(1);

        //instantiate a unit prefab parent
        m_UnitParent = new GameObject(UnitParentName).transform;
        m_UnitParent.position = new Vector3(5, 0, 0);

        yield return LoadUnitPrefabs(m_UnitRepository, m_UnitParent);
        yield return LoadUI();

        m_ReloadButton.onClick.AddListener(OnReload);

        OnReload();
    }

    private void OnDestroy()
    {
        foreach (GameObject unitPrefab in m_UnitPrefabs)
        {
            if (unitPrefab != null)
            {
                Addressables.ReleaseInstance(unitPrefab);
            }
        }

        if (m_UIPrefab != null)
        {
            Addressables.ReleaseInstance(m_UIPrefab);
        }
    }

    private IEnumerator LoadUI()
    {
        var loadOperation = Addressables.InstantiateAsync(UIPrefabName);
        yield return loadOperation;

        GameObject ui = loadOperation.Result;
        ui.name = UIPrefabName;

        m_ReloadButton = ui.GetComponentInChildren<Button>();

        Transform unitInfoUIParent = ui.transform.Find(UnitInfoUIParentName);

        m_LeftInfoUI = new UnitInfoUI(unitInfoUIParent.GetChild(0));
        m_RightInfoUI = new UnitInfoUI(unitInfoUIParent.GetChild(1));
    }

    private IEnumerator LoadUnitPrefabs(UnitRepository unitRepository, Transform unitParent)
    {
        m_UnitPrefabs = new GameObject[unitRepository.Count];

        int i = 0;
        foreach (UnitData unit in unitRepository)
        {
            string visualName = unit.UnitVisualData.Visual.ToString().ToLower();
            var loadOperation = Addressables.InstantiateAsync(visualName, unitParent);
            yield return loadOperation;

            GameObject instance = loadOperation.Result;
            instance.name = unit.UnitStats.ID + "_" + visualName;
            instance.SetActive(false);

            m_UnitPrefabs[i++] = loadOperation.Result;
        }
    }

    private void OnReload()
    {
        //reset previous units
        if (m_LeftUnit != null)
        {
            m_LeftUnit.transform.SetParent(m_UnitParent, false);
            m_RightUnit.transform.SetParent(m_UnitParent, false);

            m_LeftUnit.SetActive(false);
            m_RightUnit.SetActive(false);
        }

        int unitCount = m_UnitRepository.Count;

        //add to the left slot a random unit
        int randomUnit = Random.Range(0, unitCount);
        int randomSecondUnit = Random.Range(0, unitCount);

        while (randomSecondUnit == randomUnit) //redo until no duplicate
        {
            randomSecondUnit = Random.Range(0, unitCount);
        }

        m_LeftUnit = m_UnitPrefabs[randomUnit];
        m_LeftUnit.transform.SetParent(m_LeftStagingSlot, false);

        m_RightUnit = m_UnitPrefabs[randomSecondUnit];
        m_RightUnit.transform.SetParent(m_RightStagingSlot, false);

        //init unit info
        m_LeftInfoUI.Init(m_UnitRepository[randomUnit]);
        m_RightInfoUI.Init(m_UnitRepository[randomSecondUnit]);

        m_LeftUnit.SetActive(true);
        m_RightUnit.SetActive(true);
    }
}
