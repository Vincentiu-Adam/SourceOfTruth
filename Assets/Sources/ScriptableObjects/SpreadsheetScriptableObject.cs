using UnityEngine;

[CreateAssetMenu(fileName = "spreadsheet_data", menuName = "SourceOfTruth/SpreadsheetScriptableObject")]
public class SpreadsheetScriptableObject : ScriptableObject
{
    [SerializeField]
    private string url;
    public string URL => url;
}
