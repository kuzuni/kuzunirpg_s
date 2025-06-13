// 실제 뽑기 메서드들 (public으로 외부에서 호출 가능)
using RPG.Items.Equipment;
using Sirenix.OdinInspector;
namespace RPG.Gacha
{
    [System.Serializable]
    public class GachaRateTable
    {
        [TableColumnWidth(80)]
        [LabelText("등급")]
        public EquipmentRarity rarity;

        [TableColumnWidth(80)]
        [LabelText("확률 (%)")]
        [PropertyRange(0f, 100f)]
        [SuffixLabel("%", true)]
        public float probability;

        [TableColumnWidth(200)]
        [LabelText("세부 등급 확률 (1~5성)")]
        [HorizontalGroup("SubGrade", 0.2f)]
        public float[] subGradeProbabilities = new float[] { 40f, 30f, 20f, 7f, 3f };
    }

}
