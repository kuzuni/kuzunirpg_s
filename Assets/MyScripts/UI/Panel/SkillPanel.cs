using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using RPG.UI.Base;
// 스킬 패널
namespace RPG.UI.Panels
{
    public class SkillPanel : BaseUIPanel
    {
        [Title("스킬 시스템")]
        [ShowInInspector, ReadOnly]
        private List<string> unlockedSkills = new List<string>();

        public override void UpdatePanel()
        {
            // 스킬 목록 업데이트
            RefreshSkillList();
        }

        private void RefreshSkillList()
        {
            Debug.Log("스킬 목록 새로고침");
        }
    }

}
