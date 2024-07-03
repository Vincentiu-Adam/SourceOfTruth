using System.Reflection;

using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;

using UnityEngine;
using UnityEngine.Networking;

using Newtonsoft.Json.Linq;

[CustomEditor(typeof(SpreadsheetScriptableObject))]
public class SpreadsheetEditor : Editor
{
    private const string GoogleSheetAPI = "https://sheets.googleapis.com/v4/spreadsheets/";
    private const string GoogleSheetGridData = "?includeGridData=true";

    private const string GoogleSheetAPIKey = "";

    private const string UnitCommonPath = "Assets/Units/";
    private const string UnitStatDataPath = UnitCommonPath + "UnitStatData/";
    private const string UnitMiscDataPath = UnitCommonPath + "UnitMiscData/";

    private const string UnitAddressableGroupName = "unit_data";

    private SpreadsheetScriptableObject m_SpreadSheet;

    public void OnEnable()
    {
        m_SpreadSheet = (SpreadsheetScriptableObject) target;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if (GUILayout.Button("Load"))
        {
            string uri = GoogleSheetAPI + m_SpreadSheet.ID + GoogleSheetGridData + "&key=" + GoogleSheetAPIKey;
            LoadSpreadSheet(uri);
        }
    }

    private void ParseSpreadSheet(string json)
    {
        JObject jsonSpreadsheet = JObject.Parse(json);

        //create a spreadsheet and fetch column data for a specific type
        JArray rows = (JArray) jsonSpreadsheet["sheets"][0]["data"][0]["rowData"];
        JArray spreadSheetColumns = (JArray) rows[0]["values"];

        var unitStats = SpreadsheetParser.CreateColumnDataForType<UnitStatsScriptableObject>(spreadSheetColumns);
        var unitMiscStats = SpreadsheetParser.CreateColumnDataForType<UnitMiscDataScriptableObject>(spreadSheetColumns);

        AddressableAssetGroup assetGroup = AddressableAssetSettingsDefaultObject.Settings.FindGroup(UnitAddressableGroupName);

        bool isSaveAsset = false;

        //row 0 contains column names so skip
        for (int i = 1; i < rows.Count; i++)
        {
            spreadSheetColumns = (JArray) rows[i]["values"];
            if (spreadSheetColumns == null)
            {
                continue;
            }

            //spreadsheet can have values object but not contain any relevant data skip if the case
            var rowUnitStats = CreateInstance<UnitStatsScriptableObject>();
            bool hasData = SpreadsheetParser.SetDataForTypeFromColumn(spreadSheetColumns, unitStats, ref rowUnitStats);

            var rowMiscStats = CreateInstance<UnitMiscDataScriptableObject>();
            hasData = SpreadsheetParser.SetDataForTypeFromColumn(spreadSheetColumns, unitMiscStats, ref rowMiscStats) || hasData;

            if (hasData)
            {
                isSaveAsset = SaveOrUpdateUnitData(rowUnitStats, rowMiscStats, assetGroup) || isSaveAsset;
            }
        }

        if (isSaveAsset)
        {
            AssetDatabase.SaveAssets();
        }
    }

    private void LoadSpreadSheet(string uri)
    {
        UnityWebRequest getSpreadsheet = UnityWebRequest.Get(uri);
        var op = getSpreadsheet.SendWebRequest();
        op.completed += (AsyncOperation operation) =>
        {
            switch (getSpreadsheet.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogErrorFormat("Connection Error {0}", getSpreadsheet.error);

                    break;

                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogErrorFormat("HTML Error {0}", getSpreadsheet.error);

                    break;

                case UnityWebRequest.Result.Success:
                    string json = getSpreadsheet.downloadHandler.text;
                    ParseSpreadSheet(json);

                    break;
            }

            getSpreadsheet.Dispose();
        };
    }

    private bool SaveOrUpdateUnitData(UnitStatsScriptableObject unitStats, UnitMiscDataScriptableObject unitMiscStats, AddressableAssetGroup addressableGroup)
    {
        bool isSaveAsset = false;

        string assetName = unitStats.ID.ToLower() + ".asset";
        string assetPath = UnitStatDataPath + assetName;

        var existingUnitStats = AssetDatabase.LoadAssetAtPath<UnitStatsScriptableObject>(assetPath);
        if (existingUnitStats != null)
        {
            CopyData(existingUnitStats, unitStats);
            EditorUtility.SetDirty(existingUnitStats);
        }
        else
        {
            AssetDatabase.CreateAsset(unitStats, assetPath);

            //also add asset to addressable group
            string guid = AssetDatabase.AssetPathToGUID(assetPath);

            var entry = AddressableAssetSettingsDefaultObject.Settings.CreateOrMoveEntry(guid, addressableGroup);
            entry.address = unitStats.ID.ToLower();

            isSaveAsset = true;
        }

        assetPath = UnitMiscDataPath + assetName;

        var existingMiscUnitStats = AssetDatabase.LoadAssetAtPath<UnitMiscDataScriptableObject>(assetPath);
        if (existingMiscUnitStats != null)
        {
            CopyData(existingMiscUnitStats, unitMiscStats);
            EditorUtility.SetDirty(existingMiscUnitStats);
        }
        else
        {
            AssetDatabase.CreateAsset(unitMiscStats, assetPath);

            //also add asset to addressable group
            string guid = AssetDatabase.AssetPathToGUID(assetPath);

            var entry = AddressableAssetSettingsDefaultObject.Settings.CreateOrMoveEntry(guid, addressableGroup);
            entry.address = unitStats.ID.ToLower();

            isSaveAsset = true;
        }

        return isSaveAsset;
    }

    private void CopyData<T>(T data, T otherData) where T : SpreadsheetDataScriptableObject
    {
        //only copy public fields since those are what we care for in the spreadsheet anyway
        foreach (FieldInfo field in typeof(T).GetFields())
        {
            object value = field.GetValue(otherData);
            field.SetValue(data, value);
        }
    }
}
