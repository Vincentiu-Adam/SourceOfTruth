using System.Collections;

using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.AddressableAssets;
using Newtonsoft.Json.Linq;

public class SpreadsheetData
{
    public string ResultJSON;
}

public class SpreadsheetUtility
{
    private const string GoogleSheetAPI = "https://sheets.googleapis.com/v4/spreadsheets/";
    private const string GoogleSheetGridData = "?includeGridData=true";

    private const string GoogleSheetAPIKey = "";

    public static IEnumerator LoadSpreadsheet(string sheetPath, SpreadsheetData resultData)
    {
        var loadOperation = Addressables.LoadAssetAsync<SpreadsheetScriptableObject>(sheetPath);
        yield return loadOperation;

        if (loadOperation.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
        {
            SpreadsheetScriptableObject spreadSheet = loadOperation.Result;
            Debug.LogFormat("Spreadsheet name : {0} and URL : {1}", spreadSheet.name, spreadSheet.ID);

            string uri = GoogleSheetAPI + spreadSheet.ID + GoogleSheetGridData + "&key=" + GoogleSheetAPIKey;
            yield return LoadSpreadSheet(uri, resultData);
            //LoadSpreadSheetVoid(uri);
        }
        else
        {
            Debug.LogErrorFormat("Loading Failed {0}", loadOperation.Status);
        }

        Addressables.Release(loadOperation);
    }

    public static UnitRepository LoadUnitSpreadsheet(string json)
    {
        JObject jsonSpreadsheet = JObject.Parse(json);

        //create a spreadsheet and fetch column data for a specific type
        JArray rows = (JArray) jsonSpreadsheet["sheets"][0]["data"][0]["rowData"];
        JArray spreadSheetColumns = (JArray) rows[0]["values"];

        var unitStats = SpreadsheetParser.CreateColumnDataForType<UnitStatsScriptableObject>(spreadSheetColumns);
        var unitMiscData = SpreadsheetParser.CreateColumnDataForType<UnitMiscDataScriptableObject>(spreadSheetColumns);
        var unitVisualData = SpreadsheetParser.CreateColumnDataForType<UnitVisualDataScriptableObject>(spreadSheetColumns);

        //row 0 contains column names so skip
        UnitData[] units = new UnitData[rows.Count]; //allocate as many units as the rows for now (lots of rows can still be empty)

        int unitCount = 0;
        for (int i = 1; i < rows.Count; i++)
        {
            spreadSheetColumns = (JArray) rows[i]["values"];
            if (spreadSheetColumns == null)
            {
                continue;
            }

            //spreadsheet can have values object but not contain any relevant data skip if the case
            var rowUnitStats = ScriptableObject.CreateInstance<UnitStatsScriptableObject>();
            bool hasData = SpreadsheetParser.SetDataForTypeFromColumn(spreadSheetColumns, unitStats, ref rowUnitStats);

            var rowMiscData = ScriptableObject.CreateInstance<UnitMiscDataScriptableObject>();
            hasData = SpreadsheetParser.SetDataForTypeFromColumn(spreadSheetColumns, unitMiscData, ref rowMiscData) || hasData;

            var rowVisualData = ScriptableObject.CreateInstance<UnitVisualDataScriptableObject>();
            hasData = SpreadsheetParser.SetDataForTypeFromColumn(spreadSheetColumns, unitVisualData, ref rowVisualData) || hasData;

            if (hasData)
            {
                units[unitCount++] = new UnitData(rowUnitStats, rowMiscData, rowVisualData);
            }
        }

        //copy all units to the repository
        UnitRepository unitRepository = new UnitRepository(unitCount);
        for (int i = 0; i < unitCount; i++)
        {
            unitRepository.AddUnit(i, units[i]);
        }

        return unitRepository;
    }

    private static IEnumerator LoadSpreadSheet(string uri, SpreadsheetData resultData)
    {
        UnityWebRequest getSpreadsheet = UnityWebRequest.Get(uri);
        yield return getSpreadsheet.SendWebRequest();

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
                resultData.ResultJSON = getSpreadsheet.downloadHandler.text;

                break;
        }

        getSpreadsheet.Dispose();
    }
}
