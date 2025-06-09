

// ===== EquipmentData.cs (세부 등급 추가 버전) =====
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "New Equipment", menuName = "RPG/Equipment")]
public class EquipmentData : ScriptableObject
{
    [Title("기본 정보")]
    [HorizontalGroup("BasicInfo", 0.7f)]
    [VerticalGroup("BasicInfo/Left")]
    [LabelText("장비 이름")]
    [GUIColor("GetRarityColor")]
    public string equipmentName;

    [VerticalGroup("BasicInfo/Left")]
    [HorizontalGroup("BasicInfo/Left/Rarity")]
    [LabelText("장비 등급")]
    [EnumToggleButtons]
    public EquipmentRarity rarity = EquipmentRarity.Common;

    [HorizontalGroup("BasicInfo/Left/Rarity", Width = 100)]
    [LabelText("세부")]
    [PropertyRange(1, 5)]
    [SuffixLabel("성", true)]
    [DelayedProperty]
    public int subGrade = 1;

    [VerticalGroup("BasicInfo/Left")]
    [LabelText("장비 타입")]
    public EquipmentType equipmentType;

    [VerticalGroup("BasicInfo/Right")]
    [LabelText("장비 아이콘")]
    [PreviewField(80)]
    public Sprite icon;

    [Title("장비 스탯")]
    [ShowIf("equipmentType", EquipmentType.Weapon)]
    [LabelText("공격력 증가")]
    [PropertyRange(0, "@GetMaxStatByRarity()")]
    [SuffixLabel("DMG", true)]
    public int attackPowerBonus;

    [ShowIf("equipmentType", EquipmentType.Armor)]
    [LabelText("최대 체력 증가")]
    [PropertyRange(0, "@GetMaxStatByRarity() * 5")]
    [SuffixLabel("HP", true)]
    public int maxHpBonus;

    [ShowIf("equipmentType", EquipmentType.Ring)]
    [LabelText("체력 회복력 증가")]
    [PropertyRange(0f, "@GetMaxRegenByRarity()")]
    [SuffixLabel("HP/초", true)]
    public float hpRegenBonus;

    [Title("등급 정보")]
    [InfoBox("$GetGradeInfo", InfoMessageType.Info)]
    [ShowInInspector, ReadOnly]
    [ProgressBar(0, 100, ColorGetter = "GetRarityColor", Height = 30)]
    [LabelText("총 등급 보너스")]
    private float TotalRarityBonusPercent => (GetRarityBonus() + GetSubGradeBonus()) * 100f;

    [HorizontalGroup("GradeBonus", 0.5f)]
    [ShowInInspector, ReadOnly]
    [VerticalGroup("GradeBonus/Left")]
    [LabelText("기본 등급 보너스")]
    private string BaseRarityBonus => $"+{GetRarityBonus() * 100f:F0}%";

    [ShowInInspector, ReadOnly]
    [VerticalGroup("GradeBonus/Right")]
    [LabelText("세부 등급 보너스")]
    private string SubGradeBonus => $"+{GetSubGradeBonus() * 100f:F1}%";

    [Title("추가 정보")]
    [TextArea(3, 5)]
    [LabelText("설명")]
    public string description;

    [HorizontalGroup("Price")]
    [LabelText("구매 가격")]
    [PropertyRange(0, "@GetMaxPriceByRarity()")]
    public int buyPrice;

    [HorizontalGroup("Price")]
    [LabelText("판매 가격")]
    [PropertyRange(0, "@buyPrice / 2")]
    public int sellPrice;

    // 등급별 최대값 계산
    private int GetMaxStatByRarity()
    {
        switch (rarity)
        {
            case EquipmentRarity.Common: return 10;
            case EquipmentRarity.Uncommon: return 20;
            case EquipmentRarity.Rare: return 35;
            case EquipmentRarity.Epic: return 50;
            case EquipmentRarity.Legendary: return 75;
            case EquipmentRarity.Mythic: return 100;
            case EquipmentRarity.Celestial: return 150;
            default: return 10;
        }
    }

    private float GetMaxRegenByRarity()
    {
        switch (rarity)
        {
            case EquipmentRarity.Common: return 1f;
            case EquipmentRarity.Uncommon: return 2f;
            case EquipmentRarity.Rare: return 3.5f;
            case EquipmentRarity.Epic: return 5f;
            case EquipmentRarity.Legendary: return 7.5f;
            case EquipmentRarity.Mythic: return 10f;
            case EquipmentRarity.Celestial: return 15f;
            default: return 1f;
        }
    }

    private int GetMaxPriceByRarity()
    {
        switch (rarity)
        {
            case EquipmentRarity.Common: return 100;
            case EquipmentRarity.Uncommon: return 500;
            case EquipmentRarity.Rare: return 2000;
            case EquipmentRarity.Epic: return 5000;
            case EquipmentRarity.Legendary: return 10000;
            case EquipmentRarity.Mythic: return 25000;
            case EquipmentRarity.Celestial: return 50000;
            default: return 100;
        }
    }

    // 기본 등급 보너스
    public float GetRarityBonus()
    {
        switch (rarity)
        {
            case EquipmentRarity.Common: return 0f;
            case EquipmentRarity.Uncommon: return 0.1f;
            case EquipmentRarity.Rare: return 0.2f;
            case EquipmentRarity.Epic: return 0.35f;
            case EquipmentRarity.Legendary: return 0.5f;
            case EquipmentRarity.Mythic: return 0.75f;
            case EquipmentRarity.Celestial: return 1f;
            default: return 0f;
        }
    }

    // 세부 등급 보너스 (각 성마다 3% 추가)
    public float GetSubGradeBonus()
    {
        return (subGrade - 1) * 0.03f;
    }

    // 전체 등급 보너스
    public float GetTotalGradeBonus()
    {
        return GetRarityBonus() + GetSubGradeBonus();
    }

    // 최종 스탯 계산 (기본값 + 전체 등급 보너스)
    public int GetFinalAttackPower()
    {
        return Mathf.RoundToInt(attackPowerBonus * (1 + GetTotalGradeBonus()));
    }

    public int GetFinalMaxHp()
    {
        return Mathf.RoundToInt(maxHpBonus * (1 + GetTotalGradeBonus()));
    }

    public float GetFinalHpRegen()
    {
        return hpRegenBonus * (1 + GetTotalGradeBonus());
    }

    // 등급 표시용 문자열
    public string GetFullRarityName()
    {
        return $"{RarityColors.GetRarityName(rarity)} {subGrade}성";
    }

    // Odin Inspector 헬퍼
    private Color GetRarityColor()
    {
        return RarityColors.GetRarityColor(rarity);
    }

    private string GetGradeInfo()
    {
        return $"{GetFullRarityName()} - 총 보너스: +{GetTotalGradeBonus() * 100f:F1}% (기본 {GetRarityBonus() * 100f:F0}% + 세부 {GetSubGradeBonus() * 100f:F1}%)";
    }

    [Title("장비 미리보기")]
    [ShowInInspector, ReadOnly]
    [InfoBox("$GetEquipmentPreview", InfoMessageType.None)]
    private string GetEquipmentPreview()
    {
        string stats = "";

        switch (equipmentType)
        {
            case EquipmentType.Weapon:
                stats = $"공격력 +{GetFinalAttackPower()} (기본: {attackPowerBonus})";
                break;
            case EquipmentType.Armor:
                stats = $"최대 체력 +{GetFinalMaxHp()} (기본: {maxHpBonus})";
                break;
            case EquipmentType.Ring:
                stats = $"체력 회복 +{GetFinalHpRegen():F1}/초 (기본: {hpRegenBonus:F1})";
                break;
        }

        return $"[{GetFullRarityName()}] {equipmentName}\n{stats}";
    }

    [Title("테스트 기능")]
    [ButtonGroup("TestButtons")]
    [Button("등급별 추천 스탯", ButtonSizes.Medium)]
    [GUIColor(0.5f, 0.8f, 1f)]
    private void SetRecommendedStats()
    {
        float multiplier = 0.5f + (subGrade - 1) * 0.1f; // 1성: 50%, 5성: 90%

        switch (equipmentType)
        {
            case EquipmentType.Weapon:
                attackPowerBonus = Mathf.RoundToInt(GetMaxStatByRarity() * multiplier);
                break;
            case EquipmentType.Armor:
                maxHpBonus = Mathf.RoundToInt(GetMaxStatByRarity() * 5 * multiplier);
                break;
            case EquipmentType.Ring:
                hpRegenBonus = GetMaxRegenByRarity() * multiplier;
                break;
        }

        buyPrice = Mathf.RoundToInt(GetMaxPriceByRarity() * multiplier);
        sellPrice = buyPrice / 2;

        Debug.Log($"{GetFullRarityName()} 추천 스탯이 설정되었습니다!");
    }

    [ButtonGroup("TestButtons")]
    [Button("세부 등급 업그레이드", ButtonSizes.Medium)]
    [GUIColor(0.8f, 0.8f, 0.3f)]
    [EnableIf("@subGrade < 5")]
    private void UpgradeSubGrade()
    {
        subGrade++;
        Debug.Log($"{equipmentName}이(가) {GetFullRarityName()}로 업그레이드되었습니다!");
    }

    [ButtonGroup("TestButtons")]
    [Button("세부 등급 다운그레이드", ButtonSizes.Medium)]
    [GUIColor(0.8f, 0.3f, 0.3f)]
    [EnableIf("@subGrade > 1")]
    private void DowngradeSubGrade()
    {
        subGrade--;
        Debug.Log($"{equipmentName}이(가) {GetFullRarityName()}로 다운그레이드되었습니다!");
    }
}