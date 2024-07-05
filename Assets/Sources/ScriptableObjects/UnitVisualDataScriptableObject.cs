using UnityEngine;

public enum UnitVisualEnum
{
    MECH,
    MECH_FIGHTER_A,
    MECH_FIGHTER_B,
    MECH_FIGHTER_C,
    WIZARD_A,
    WIZARD_B,
    BEAST_A,
    MONSTER_A,
    MONSTER_B,
    MONSTER_C,
    MONSTER_D,
    MONSTER_E,
    MONSTER_F,
    KNIGHT,
    BANDITOS_A,
    BANDITOS_B
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
