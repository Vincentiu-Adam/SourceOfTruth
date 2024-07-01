using System.Collections;

using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.AddressableAssets;

using Newtonsoft.Json.Linq;

public class LoadSpreadsheet : MonoBehaviour
{
    private const string GoogleSheetAPI = "https://sheets.googleapis.com/v4/spreadsheets/";
    private const string GoogleSheetGridData = "?includeGridData=true";

    private const string GoogleSheetAPIKey = "AIzaSyAU7GPI-PDABfkTu2cVF8dQNwlGjkcfUuI";

    private const string SheetPath = "spreadsheet_data";

    private IEnumerator Start()
    {
        var loadOperation = Addressables.LoadAssetAsync<SpreadsheetScriptableObject>(SheetPath);
        yield return loadOperation;

        if (loadOperation.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
        {
            SpreadsheetScriptableObject spreadSheet = loadOperation.Result;
            Debug.LogFormat("Spreadsheet name : {0} and URL : {1}", spreadSheet.name, spreadSheet.ID);

            string uri = GoogleSheetAPI + spreadSheet.ID + GoogleSheetGridData + "&key=" + GoogleSheetAPIKey;
            yield return LoadSpreadSheet(uri);
        }
        else
        {
            Debug.LogErrorFormat("Loading Failed {0}", loadOperation.Status);
        }
    }

    private IEnumerator LoadSpreadSheet(string uri)
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
                Debug.LogFormat("Result {0}", getSpreadsheet.downloadHandler.text);

                string json = getSpreadsheet.downloadHandler.text;
                JObject jsonSpreadsheet = JObject.Parse(json);

                PrintColumns(jsonSpreadsheet, 0, 0);
                PrintColumns(jsonSpreadsheet, 0, 1);
                break;
        }
    }

    private void PrintColumns(JObject spreadsheet, int sheetIndex, int rowIndex)
    {
        //fetch first data row column and get all string values from it
        JToken firstRow = spreadsheet["sheets"][sheetIndex]["data"][0]["rowData"][rowIndex]["values"];
        foreach (JToken rowItem in firstRow.Children())
        {
            JToken userValue = rowItem["userEnteredValue"];
            if (userValue != null)
            {
                string value = GetValue(userValue);
                if (value != null)
                {
                    Debug.LogFormat("Column name : {0}", value);
                }
            }
        }
    }

    private string GetValue(JToken userValue)
    {
        //fetch one of different values from the sheet ["numberValue", "stringValue", "boolValue"]
        JToken value = userValue["stringValue"];
        if (value != null)
        {
            return value.ToString();
        }

        value = userValue["numberValue"];
        if (value != null)
        {
            return value.ToString();
        }

        value = userValue["boolValue"];
        return value?.ToString();
    }
}
