
public class UnitData
{
    public float Health;

    public float AttackCounter;

    private UnitStatsScriptableObject m_UnitStats;
    public UnitStatsScriptableObject UnitStats => m_UnitStats;

    private UnitMiscDataScriptableObject m_UnitMiscData;
    public UnitMiscDataScriptableObject UnitMiscStats => m_UnitMiscData;

    private UnitVisualDataScriptableObject m_UnitVisualData;
    public UnitVisualDataScriptableObject UnitVisualData => m_UnitVisualData;

    public UnitData(UnitStatsScriptableObject unitStats, UnitMiscDataScriptableObject unitMiscData, UnitVisualDataScriptableObject unitVisualData)
    {
        m_UnitStats = unitStats;
        m_UnitMiscData = unitMiscData;
        m_UnitVisualData = unitVisualData;

        Health = unitStats.BaseHealth;
    }
}
