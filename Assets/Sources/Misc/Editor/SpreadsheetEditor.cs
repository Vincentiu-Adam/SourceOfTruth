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
    private const string UnitVisualDataPath = UnitCommonPath + "UnitVisualData/";

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
        var unitMiscData = SpreadsheetParser.CreateColumnDataForType<UnitMiscDataScriptableObject>(spreadSheetColumns);
        var unitVisualData = SpreadsheetParser.CreateColumnDataForType<UnitVisualDataScriptableObject>(spreadSheetColumns);

        AddressableAssetGroup assetGroup = AddressableAssetSettingsDefaultObject.Settings.FindGroup(UnitAddressableGroupName);

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

            var rowMiscData = CreateInstance<UnitMiscDataScriptableObject>();
            hasData = SpreadsheetParser.SetDataForTypeFromColumn(spreadSheetColumns, unitMiscData, ref rowMiscData) || hasData;

            var rowVisualData = CreateInstance<UnitVisualDataScriptableObject>();
            hasData = SpreadsheetParser.SetDataForTypeFromColumn(spreadSheetColumns, unitVisualData, ref rowVisualData) || hasData;

            if (hasData)
            {
                SaveOrUpdateUnitData(rowUnitStats, rowMiscData, rowVisualData, assetGroup);
            }
        }

        AssetDatabase.SaveAssets();
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

    private void SaveOrUpdateUnitData(UnitStatsScriptableObject unitStats, UnitMiscDataScriptableObject unitMiscData, UnitVisualDataScriptableObject unitVisualData, AddressableAssetGroup addressableGroup)
    {
        string address = unitStats.ID.ToLower();
        string assetName = address + ".asset";

        CreateOrCopyAsset(assetName, UnitStatDataPath, address, addressableGroup, unitStats);
        CreateOrCopyAsset(assetName, UnitMiscDataPath, address, addressableGroup, unitMiscData);
        CreateOrCopyAsset(assetName, UnitVisualDataPath, address, addressableGroup, unitVisualData);
    }

    private void CreateOrCopyAsset<T>(string assetName, string assetDataPath, string address, AddressableAssetGroup addressableGroup, T dataContainer) where T : SpreadsheetDataScriptableObject
    {
        string assetPath = assetDataPath + assetName;

        var existingDataContainer = AssetDatabase.LoadAssetAtPath<T>(assetPath);
        if (existingDataContainer != null)
        {
            CopyData(existingDataContainer, dataContainer);
            EditorUtility.SetDirty(existingDataContainer);

            return;
        }

        AssetDatabase.CreateAsset(dataContainer, assetPath);

        //also add asset to addressable group
        string guid = AssetDatabase.AssetPathToGUID(assetPath);

        var entry = AddressableAssetSettingsDefaultObject.Settings.CreateOrMoveEntry(guid, addressableGroup);
        entry.address = address.ToLower();
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
