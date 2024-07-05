using UnityEngine;

public enum UnitVisualEnum
{
    MECH,
    WIZARD_A,
    WIZARD_B,
    BEAST_A
}

public enum UnitVFXEnum
{
    NONE,
    EXPLOSION,
    LASER,
    BLIZZARD
}

[CreateAssetMenu(fileName = "unit_visual_data", menuName = "SpreadsheetData/Units/UnitVisualDataScriptableObject")]
public class UnitVisualDataScriptableObject : SpreadsheetDataScriptableObject
{
    public UnitVisualEnum Visual;
    public UnitVFXEnum VFX;
}
