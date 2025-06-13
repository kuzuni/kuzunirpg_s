using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using RPG.UI.Base;

namespace RPG.UI.Panels
{// 퀘스트 패널
    public class QuestPanel : BaseUIPanel
    {
        [Title("퀘스트")]
        [TabGroup("Quests", "일일 퀘스트")]
        [ShowInInspector, ReadOnly]
        private List<string> dailyQuests = new List<string>
    {
        "몬스터 100마리 처치",
        "골드 10,000 획득",
        "스테이지 10 클리어"
    };

        [TabGroup("Quests", "주간 퀘스트")]
        [ShowInInspector, ReadOnly]
        private List<string> weeklyQuests = new List<string>
    {
        "보스 몬스터 50마리 처치",
        "장비 강화 20회",
        "유물 합성 10회"
    };

        public override void UpdatePanel()
        {
            RefreshQuestProgress();
        }

        private void RefreshQuestProgress()
        {
            Debug.Log("퀘스트 진행도 새로고침");
        }
    }
}
 