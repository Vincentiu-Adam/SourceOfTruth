using System.Collections;

public class UnitRepository
{
    private UnitData[] m_Units;
    public UnitData this[int index] => m_Units[index];

    public int Count => m_Units.Length;

    public float MinUnitHealth { get; private set; }
    public float MaxUnitHealth { get; private set; }

    public UnitRepository(int unitCount)
    {
        m_Units = new UnitData[unitCount];
    }

    public void AddUnit(int index, UnitData unit)
    {
        m_Units[index] = unit;

        float unitHealth = unit.UnitStats.BaseHealth;
        if (index == 0)
        {
            MinUnitHealth = unitHealth;
            MaxUnitHealth = unitHealth;

            return;
        }

        if (unitHealth < MinUnitHealth)
        {
            MinUnitHealth = unitHealth;
            return;
        }

        if (unitHealth > MaxUnitHealth)
        {
            MaxUnitHealth = unitHealth;
        }
    }

    public IEnumerator GetEnumerator()
    {
        return m_Units.GetEnumerator();
    }
}
