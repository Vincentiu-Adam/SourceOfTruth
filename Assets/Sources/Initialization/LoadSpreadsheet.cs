using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class LoadSpreadsheet : MonoBehaviour
{
    private const string SheetPath = "spreadsheet_data";

    private IEnumerator Start()
    {
        var loadOperation = Addressables.LoadAssetAsync<SpreadsheetScriptableObject>(SheetPath);
        yield return loadOperation;

        if (loadOperation.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
        {
            SpreadsheetScriptableObject spreadSheet = loadOperation.Result;
            Debug.LogFormat("Spreadsheet name : {0} and URL : {1}", spreadSheet.name, spreadSheet.URL);
        }
        else
        {
            Debug.LogErrorFormat("Loading Failed {0}", loadOperation.Status);
        }
    }
}
