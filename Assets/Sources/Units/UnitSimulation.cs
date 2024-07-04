
using UnityEngine;

public class UnitSimulation
{
    private const float AttackSpeedCoefficient = 0.5f;

    public void UpdateUnit(UnitData unit)
    {
        unit.AttackCounter += unit.UnitStats.AttackSpeed * Time.deltaTime * AttackSpeedCoefficient;
    }

    public void CheckUnit(UnitData unit)
    {
        if (unit.AttackCounter >= 1f)
        {
            unit.AttackCounter -= 1f;
        }

        if (unit.Health == 0)
        {
            unit.Health = unit.UnitStats.BaseHealth;
        }
    }

    public void ResetUnit(UnitData unit)
    {
        unit.AttackCounter = 0;
        unit.Health = unit.UnitStats.BaseHealth;
    }

    public void Attack(UnitData unit, UnitData targetUnit)
    {
        float damage = Mathf.Max(0, unit.UnitStats.AttackDamage - targetUnit.UnitStats.Armor);
        targetUnit.Health = Mathf.Max(0f, targetUnit.Health - damage);
    }
}
