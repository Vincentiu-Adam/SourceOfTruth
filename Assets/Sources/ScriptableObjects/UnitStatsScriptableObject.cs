using UnityEngine;

[CreateAssetMenu(fileName = "unit_stat_data", menuName = "SpreadsheetData/Units/UnitStatsScriptableObject")]
public class UnitStatsScriptableObject : SpreadsheetDataScriptableObject
{
    [ReadOnly]
    public string ID;

    public string Name;

    [SpreadsheetName("Health")]
    public float BaseHealth;
    public float AttackSpeed;
    public float AttackDamage;
    public float Armor;
}
