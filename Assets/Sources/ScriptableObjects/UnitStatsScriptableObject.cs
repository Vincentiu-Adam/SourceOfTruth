using UnityEngine;

[CreateAssetMenu(fileName = "unit_stat_data", menuName = "SpreadsheetData/Units/UnitStatsScriptableObject")]
public class UnitStatsScriptableObject : SpreadsheetDataScriptableObject
{
    public int ID;

    public string Name;

    public float BaseHealth;
    public float AttackSpeed;
    public float AttackDamage;
    public float Armor;
}
