using System;
using UnityEngine;
using Sirenix.OdinInspector;

// 플레이어가 보유한 유물 인스턴스
[Serializable]
public class RelicInstance
{
    [HorizontalGroup("RelicInfo", 0.15f)]
    [VerticalGroup("RelicInfo/Icon")]
    [PreviewField(60), HideLabel]

    [ShowInInspector]
    public Sprite Icon => relicData?.icon;

    [VerticalGroup("RelicInfo/Details")]
    [LabelText("유물")]
    [GUIColor("GetRarityColor")]
    public RelicData relicData;

    [VerticalGroup("RelicInfo/Details")]
    [HorizontalGroup("RelicInfo/Details/Level")]
    [LabelText("레벨")]
    [ProgressBar(1, 100, ColorGetter = "GetLevelProgressColor")]
    public int level = 1;

    [HorizontalGroup("RelicInfo/Details/Level", Width = 100)]
    [Button("레벨업"), GUIColor(0.3f, 0.8f, 0.3f)]
    [ShowIf("@level < 100")]
    private void TestLevelUp()
    {
        if (level < 100)
        {
            level++;
            Debug.Log($"{relicData.relicName} Lv.{level}로 레벨업!");
        }
    }

    [ShowInInspector, ReadOnly]
    [LabelText("현재 효과")]
    [InfoBox("$GetCurrentEffectDescription", InfoMessageType.None)]
    private string CurrentEffect => "";

    // 기본 생성자 (Serialization을 위해 필요)
    public RelicInstance()
    {
        relicData = null;
        level = 1;
    }

    // 생성자
    public RelicInstance(RelicData data)
    {
        relicData = data;
        level = 1;
    }

    // 현재 레벨에서의 스탯 보너스 계산
    public float GetStatBonus(StatType statType)
    {
        if (relicData == null) return 0f;

        foreach (var bonus in relicData.statBonuses)
        {
            if (bonus.statType == statType)
            {
                return bonus.GetActualBonus(level, relicData.GetRarityMultiplier());
            }
        }

        return 0f;
    }

    // 모든 스탯 보너스 가져오기
    public System.Collections.Generic.Dictionary<StatType, float> GetAllStatBonuses()
    {
        var bonuses = new System.Collections.Generic.Dictionary<StatType, float>();

        if (relicData == null) return bonuses;

        foreach (var bonus in relicData.statBonuses)
        {
            bonuses[bonus.statType] = bonus.GetActualBonus(level, relicData.GetRarityMultiplier());
        }

        return bonuses;
    }

    // 합성 성공 확률 계산
    public float GetFusionSuccessRate()
    {
        // 1~30레벨: 100%
        if (level <= 30) return 1.0f;

        // 31~99레벨: 100%에서 30%로 감소
        float progress = (level - 30f) / 69f;
        float successRate = Mathf.Lerp(100f, 30f, progress);
        return successRate / 100f;
    }

    // UI 표시용 헬퍼
    private Color GetRarityColor()
    {
        return relicData != null ? relicData.GetRarityColor() : Color.white;
    }

    private Color GetLevelProgressColor(float value)
    {
        if (value >= 90) return new Color(1f, 0.8f, 0.2f); // 금색
        if (value >= 70) return new Color(1f, 0.5f, 0f);   // 주황색
        if (value >= 50) return new Color(0.8f, 0.3f, 0.8f); // 보라색
        if (value >= 30) return new Color(0.3f, 0.5f, 1f);   // 파란색
        return new Color(0.5f, 0.8f, 0.5f); // 녹색
    }

    private string GetCurrentEffectDescription()
    {
        if (relicData == null) return "유물 데이터 없음";

        string description = $"[{relicData.GetRarityName()}] {relicData.relicName} Lv.{level}\n";
        description += $"합성 성공률: {GetFusionSuccessRate():P0}\n\n";
        description += "현재 효과:\n";

        foreach (var bonus in relicData.statBonuses)
        {
            float actualBonus = bonus.GetActualBonus(level, relicData.GetRarityMultiplier());
            string value = bonus.isPercentage ? $"{actualBonus:F1}%" : $"+{actualBonus:F0}";
            description += $"• {GetStatTypeName(bonus.statType)} {value}\n";
        }

        return description;
    }

    private string GetStatTypeName(StatType statType)
    {
        switch (statType)
        {
            case StatType.MaxHp: return "최대 체력";
            case StatType.AttackPower: return "공격력";
            case StatType.CritChance: return "치명타 확률";
            case StatType.CritDamage: return "치명타 데미지";
            case StatType.AttackSpeed: return "공격 속도";
            case StatType.HpRegen: return "체력 재생";
            default: return statType.ToString();
        }
    }
}