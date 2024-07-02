using UnityEngine;

public enum UnitTribeEnum
{
    HUMAN,
    BEAST,
    MECH
}

public enum UnitRarityEnum
{
    COMMON,
    UNCOMMON,
    EPIC,
    LEGENDARY
}

[CreateAssetMenu(fileName = "unit_misc_data", menuName = "SpreadsheetData/Units/UnitMiscDataScriptableObject")]
public class UnitMiscDataScriptableObject : ScriptableObject
{
    public UnitTribeEnum Tribe;
    public bool Fly;
    public UnitRarityEnum Rarity;
}
