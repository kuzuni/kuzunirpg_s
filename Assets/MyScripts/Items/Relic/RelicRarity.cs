using UnityEngine;

namespace RPG.Items.Relic
{

    public enum RelicRarity
    {
        [InspectorName("일반")]
        Common,

        [InspectorName("희귀")]
        Rare,

        [InspectorName("영웅")]
        Epic,

        [InspectorName("전설")]
        Legendary
    }

    // 유물 타입
    public enum RelicType
    {
        [InspectorName("공격형")]
        Offensive,      // 공격력, 치명타 관련

        [InspectorName("방어형")]
        Defensive,      // 체력, 회복 관련

        [InspectorName("균형형")]
        Balanced        // 여러 스탯 조합
    }

    // 유물 등급별 색상
    public static class RelicRarityColors
    {
        public static Color GetRarityColor(RelicRarity rarity)
        {
            switch (rarity)
            {
                case RelicRarity.Common:
                    return new Color(0.7f, 0.7f, 0.7f); // 회색
                case RelicRarity.Rare:
                    return new Color(0.3f, 0.5f, 1f);   // 파란색
                case RelicRarity.Epic:
                    return new Color(0.7f, 0.3f, 0.9f); // 보라색
                case RelicRarity.Legendary:
                    return new Color(1f, 0.5f, 0f);     // 주황색
                default:
                    return Color.white;
            }
        }

        public static string GetRarityName(RelicRarity rarity)
        {
            switch (rarity)
            {
                case RelicRarity.Common: return "일반";
                case RelicRarity.Rare: return "희귀";
                case RelicRarity.Epic: return "영웅";
                case RelicRarity.Legendary: return "전설";
                default: return "";
            }
        }
    }
}