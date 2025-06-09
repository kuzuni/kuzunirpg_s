// ===== EquipmentRarity.cs (수정) =====
using UnityEngine;

public enum EquipmentRarity
{
    [InspectorName("일반")]
    Common,

    [InspectorName("고급")]
    Uncommon,

    [InspectorName("희귀")]
    Rare,

    [InspectorName("영웅")]
    Epic,

    [InspectorName("전설")]
    Legendary,

    [InspectorName("신화")]
    Mythic,

    [InspectorName("천상")]
    Celestial
}

// 등급별 색상 정의
public static class RarityColors
{
    public static Color GetRarityColor(EquipmentRarity rarity)
    {
        switch (rarity)
        {
            case EquipmentRarity.Common:
                return new Color(0.7f, 0.7f, 0.7f); // 회색
            case EquipmentRarity.Uncommon:
                return new Color(0.3f, 0.8f, 0.3f); // 녹색
            case EquipmentRarity.Rare:
                return new Color(0.3f, 0.5f, 1f); // 파란색
            case EquipmentRarity.Epic:
                return new Color(0.7f, 0.3f, 0.9f); // 보라색
            case EquipmentRarity.Legendary:
                return new Color(1f, 0.5f, 0f); // 주황색
            case EquipmentRarity.Mythic:
                return new Color(1f, 0.2f, 0.2f); // 빨간색
            case EquipmentRarity.Celestial:
                return new Color(1f, 0.9f, 0.3f); // 금색
            default:
                return Color.white;
        }
    }

    public static string GetRarityName(EquipmentRarity rarity)
    {
        switch (rarity)
        {
            case EquipmentRarity.Common: return "일반";
            case EquipmentRarity.Uncommon: return "고급";
            case EquipmentRarity.Rare: return "희귀";
            case EquipmentRarity.Epic: return "영웅";
            case EquipmentRarity.Legendary: return "전설";
            case EquipmentRarity.Mythic: return "신화";
            case EquipmentRarity.Celestial: return "천상";
            default: return "";
        }
    }
}
