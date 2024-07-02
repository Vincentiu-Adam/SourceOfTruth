
public class UnitData
{
    public float Health;

    private UnitStatsScriptableObject m_UnitStats;
    public UnitStatsScriptableObject UnitStats => m_UnitStats;

    private UnitMiscDataScriptableObject m_UnitMiscStats;
    public UnitMiscDataScriptableObject UnitMiscStats => m_UnitMiscStats;

    public UnitData(UnitStatsScriptableObject unitStats, UnitMiscDataScriptableObject unitMiscStats)
    {
        m_UnitStats = unitStats;
        m_UnitMiscStats = unitMiscStats;

        Health = unitStats.BaseHealth;
    }
}
