using System.Collections;

public class UnitPresentationRepository
{
    private UnitPresentationData[] m_Units;
    public UnitPresentationData this[int index] => m_Units[index];

    public int Count => m_Units.Length;

    public UnitPresentationRepository(int unitCount)
    {
        m_Units = new UnitPresentationData[unitCount];
    }

    public void AddUnit(int index, UnitPresentationData unit)
    {
        m_Units[index] = unit;
    }

    public IEnumerator GetEnumerator()
    {
        return m_Units.GetEnumerator();
    }
}
