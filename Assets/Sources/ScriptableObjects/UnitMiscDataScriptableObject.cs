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

//using an abstract class just to inherit from ScriptableObject
public abstract class SpreadsheetDataScriptableObject : ScriptableObject {};

[CreateAssetMenu(fileName = "unit_misc_data", menuName = "SpreadsheetData/Units/UnitMiscDataScriptableObject")]
public class UnitMiscDataScriptableObject : SpreadsheetDataScriptableObject
{
    public UnitTribeEnum Tribe;
    public bool Fly;
    public UnitRarityEnum Rarity;
}
