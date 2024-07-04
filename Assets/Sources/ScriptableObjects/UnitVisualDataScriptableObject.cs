using UnityEngine;

public enum UnitVisualEnum
{
    MECH,
    WIZARD_A,
    WIZARD_B
}

[CreateAssetMenu(fileName = "unit_visual_data", menuName = "SpreadsheetData/Units/UnitVisualDataScriptableObject")]
public class UnitVisualDataScriptableObject : SpreadsheetDataScriptableObject
{
    public UnitVisualEnum Visual;
}
