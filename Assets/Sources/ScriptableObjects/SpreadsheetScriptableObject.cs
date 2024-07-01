using UnityEngine;

[CreateAssetMenu(fileName = "spreadsheet_data", menuName = "SourceOfTruth/SpreadsheetScriptableObject")]
public class SpreadsheetScriptableObject : ScriptableObject
{
    [SerializeField]
    private string id;
    public string ID => id;
}
