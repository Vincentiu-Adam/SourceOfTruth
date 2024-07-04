using System.Collections;

public class UnitRepository
{
    private UnitData[] m_Units;
    public UnitData this[int index] => m_Units[index];

    public int Count => m_Units.Length;

    public UnitRepository(int unitCount)
    {
        m_Units = new UnitData[unitCount];
    }

    public void AddUnit(int index, UnitData unit)
    {
        m_Units[index] = unit;
    }

    public IEnumerator GetEnumerator()
    {
        return m_Units.GetEnumerator();
    }
}
