using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "New Relic", menuName = "RPG/Relic")]
public class RelicData : ScriptableObject, IGachaItem
{
    [Title("기본 정보")]
    [HorizontalGroup("BasicInfo", 0.7f)]
    [VerticalGroup("BasicInfo/Left")]
    [LabelText("유물 이름")]
    [GUIColor("GetRarityColor")]
    public string relicName;
    
    [VerticalGroup("BasicInfo/Left")]
    [LabelText("유물 등급")]
    [EnumToggleButtons]
    public RelicRarity rarity = RelicRarity.Common;
    
    [VerticalGroup("BasicInfo/Left")]
    [LabelText("유물 타입")]
    [EnumToggleButtons]
    public RelicType relicType = RelicType.Balanced;
    
    [VerticalGroup("BasicInfo/Right")]
    [LabelText("유물 아이콘")]
    [PreviewField(80)]
    public Sprite icon;
    
    [Title("스탯 보너스")]
    [TableList(ShowIndexLabels = true, DrawScrollView = true)]
    public List<RelicStatBonus> statBonuses = new List<RelicStatBonus>();
    
    [Title("추가 정보")]
    [TextArea(3, 5)]
    [LabelText("설명")]
    public string description;
    
    // IGachaItem 인터페이스 구현
    public string ItemName => relicName;
    public Sprite Icon => icon;
    
    public int GetRarityLevel()
    {
        return (int)rarity;
    }
    
    public string GetRarityName()
    {
        return RelicRarityColors.GetRarityName(rarity);
    }
    
    public Color GetRarityColor()
    {
        return RelicRarityColors.GetRarityColor(rarity);
    }
    
    // 등급별 기본 배율 (레벨 1 기준)
    public float GetRarityMultiplier()
    {
        switch (rarity)
        {
            case RelicRarity.Common: return 1.0f;
            case RelicRarity.Rare: return 1.5f;
            case RelicRarity.Epic: return 2.0f;
            case RelicRarity.Legendary: return 3.0f;
            default: return 1.0f;
        }
    }
    
    [Title("미리보기")]
    [ShowInInspector, ReadOnly]
    [InfoBox("$GetRelicPreview", InfoMessageType.None)]
    private string GetRelicPreview()
    {
        string stats = "레벨 1 효과:\n";
        foreach (var bonus in statBonuses)
        {
            string value = bonus.isPercentage ? $"{bonus.bonusValue * GetRarityMultiplier():F1}%" : $"+{bonus.bonusValue * GetRarityMultiplier():F0}";
            stats += $"• {GetStatTypeName(bonus.statType)} {value}\n";
        }
        
        return $"[{GetRarityName()}] {relicName}\n{stats}";
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
    
    [Title("테스트 기능")]
    [Button("추천 스탯 설정", ButtonSizes.Medium)]
    [GUIColor(0.5f, 0.8f, 1f)]
    private void SetRecommendedStats()
    {
        statBonuses.Clear();
        
        switch (relicType)
        {
            case RelicType.Offensive:
                statBonuses.Add(new RelicStatBonus { statType = StatType.AttackPower, bonusValue = 10f, isPercentage = true });
                statBonuses.Add(new RelicStatBonus { statType = StatType.CritChance, bonusValue = 5f, isPercentage = true });
                break;
                
            case RelicType.Defensive:
                statBonuses.Add(new RelicStatBonus { statType = StatType.MaxHp, bonusValue = 15f, isPercentage = true });
                statBonuses.Add(new RelicStatBonus { statType = StatType.HpRegen, bonusValue = 2f, isPercentage = false });
                break;
                
            case RelicType.Balanced:
                statBonuses.Add(new RelicStatBonus { statType = StatType.AttackPower, bonusValue = 5f, isPercentage = true });
                statBonuses.Add(new RelicStatBonus { statType = StatType.MaxHp, bonusValue = 8f, isPercentage = true });
                statBonuses.Add(new RelicStatBonus { statType = StatType.AttackSpeed, bonusValue = 3f, isPercentage = true });
                break;
        }
        
        Debug.Log($"{relicType} 타입 추천 스탯이 설정되었습니다!");
    }
}

// 유물 스탯 보너스
[System.Serializable]
public class RelicStatBonus
{
    [TableColumnWidth(100)]
    public StatType statType;
    
    [TableColumnWidth(80)]
    [LabelText("보너스 값")]
    public float bonusValue;
    
    [TableColumnWidth(60)]
    [LabelText("퍼센트")]
    public bool isPercentage;
    
    // 실제 적용될 값 계산 (레벨과 등급 배율 적용)
    public float GetActualBonus(int level, float rarityMultiplier)
    {
        // 레벨당 20% 증가
        float levelMultiplier = 1f + (level - 1) * 0.2f;
        return bonusValue * rarityMultiplier * levelMultiplier;
    }
}