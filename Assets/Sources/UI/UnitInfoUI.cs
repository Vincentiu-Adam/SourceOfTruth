using UnityEngine;
using UnityEngine.UI;

using TMPro;

public class UnitInfoUI
{
    private const string IDTextName = "horizontal_health_wisdom/slider_health/horizontal_stat_name_value/text_id";
    private const string HealthTextName = "horizontal_health_wisdom/slider_health/horizontal_stat_name_value/text_health_value";
    private const string ArmorTextName = "vertical_unit_info/horizontal_stats_ui/horizontal_stat_name_value/text_armor_value";
    private const string AttackSpeedTextName = "vertical_unit_info/horizontal_stats_ui/horizontal_stat_name_value/text_attack_speed_value";
    private const string AttackDamageTextName = "vertical_unit_info/horizontal_stats_ui/horizontal_stat_name_value/text_attack_damage_value";
    private const string SliderName = "horizontal_health_wisdom/slider_health";

    private const int MinSliderWidth = 400;
    private const int MaxSliderWidth = 560;

    private TextMeshProUGUI m_ID = null;
    private TextMeshProUGUI m_Health = null;
    private TextMeshProUGUI m_Armor = null;
    private TextMeshProUGUI m_AttackSpeed = null;
    private TextMeshProUGUI m_AttackDamage = null;

    private Slider m_Slider = null;
    private LayoutElement m_SliderLayoutElement = null;

    public UnitInfoUI(Transform unitInfo)
    {
        m_ID = unitInfo.Find(IDTextName).GetComponent<TextMeshProUGUI>();
        m_Health = unitInfo.Find(HealthTextName).GetComponent<TextMeshProUGUI>();
        m_Armor = unitInfo.Find(ArmorTextName).GetComponent<TextMeshProUGUI>();
        m_AttackSpeed = unitInfo.Find(AttackSpeedTextName).GetComponent<TextMeshProUGUI>();
        m_AttackDamage = unitInfo.Find(AttackDamageTextName).GetComponent<TextMeshProUGUI>();

        m_Slider = unitInfo.Find(SliderName).GetComponent<Slider>();
        m_SliderLayoutElement = m_Slider.GetComponent<LayoutElement>();
    }

    public void Init(UnitData unitData, float unitHealthWeight)
    {
        //capital first letter
        string id = unitData.UnitStats.ID;
        m_ID.text = id[0].ToString().ToUpper() + id[1..] + " :"; //range operator, huh !?
        m_Health.text = unitData.Health.ToString();

        m_Armor.text = unitData.UnitStats.Armor.ToString();

        m_AttackSpeed.text = unitData.UnitStats.AttackSpeed.ToString();
        m_AttackDamage.text = unitData.UnitStats.AttackDamage.ToString();

        m_Slider.value = 1f;

        float sliderWidth = Mathf.Lerp(MinSliderWidth, MaxSliderWidth, unitHealthWeight);
        m_SliderLayoutElement.minWidth = sliderWidth;
        m_SliderLayoutElement.preferredWidth = sliderWidth;
    }

    public void UpdateUnitHealth(float health, float healthRatio)
    {
        m_Health.text = health.ToString();

        m_Slider.value = healthRatio;
    }
}
