using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using RPG.Player;
using RPG.UI.Base;
// 캐릭터 패널

namespace RPG.UI.Panels
{

    public class CharacterPanel : BaseUIPanel
    {
        [Title("캐릭터 정보")]
        [BoxGroup("Stats")]
        [ShowInInspector, ReadOnly]
        private Dictionary<string, string> characterStats = new Dictionary<string, string>();

        [SerializeField] private PlayerController playerController;

        public override void UpdatePanel()
        {
            if (playerController == null) return;

            var status = playerController.Status;
            characterStats.Clear();
            characterStats["레벨"] = status.Level.ToString();
            characterStats["체력"] = $"{status.CurrentHp} / {status.MaxHp}";
            characterStats["공격력"] = status.AttackPower.ToString();
            characterStats["치명타 확률"] = $"{status.CritChance:P0}";
            characterStats["치명타 데미지"] = $"{status.CritDamage:F1}x";
            characterStats["공격 속도"] = $"{status.AttackSpeed:F2}/초";
            characterStats["체력 재생"] = $"{status.HpRegen:F1}/초";

            RefreshUI();
        }

        private void RefreshUI()
        {
            // UI 요소 업데이트 로직
            Debug.Log("캐릭터 패널 UI 새로고침");
        }

        [Title("강화 기능")]
        [Button("스탯 강화", ButtonSizes.Large)]
        [GUIColor(0.3f, 0.8f, 0.3f)]
        private void OpenEnhancementUI()
        {
            Debug.Log("강화 UI 열기");
        }
    }

}