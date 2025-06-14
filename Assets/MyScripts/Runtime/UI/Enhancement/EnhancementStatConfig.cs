using UnityEngine;
using Sirenix.OdinInspector;
using RPG.Common;
using System.Collections.Generic;

namespace RPG.UI.Enhancement
{

    // 스탯별 설정을 위한 ScriptableObject
    [CreateAssetMenu(fileName = "EnhancementStatConfig", menuName = "RPG/UI/Enhancement Stat Config")]
    public class EnhancementStatConfig : ScriptableObject
    {
        [System.Serializable]
        public class StatIconPair
        {
            public StatType statType;
            public Sprite icon;
        }

        [TableList]
        public List<StatIconPair> statIcons = new List<StatIconPair>();

        public Sprite GetIcon(StatType statType)
        {
            var pair = statIcons.Find(x => x.statType == statType);
            return pair?.icon;
        }
    }
}