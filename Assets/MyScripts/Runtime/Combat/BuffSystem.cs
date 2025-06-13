using UnityEngine;
using System;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using RPG.Player;
using RPG.Common;
// 버프 시스템
namespace RPG.Combat
{

    public class BuffSystem : MonoBehaviour
    {
        [Title("활성 버프")]
        [ShowInInspector, ReadOnly]
        [ListDrawerSettings(ShowFoldout = false)]
        private List<ActiveBuff> activeBuffs = new List<ActiveBuff>();

        private PlayerStatus playerStatus;

        [Serializable]
        public class ActiveBuff
        {
            public string buffName;
            public StatType targetStat;
            public float value;
            public float duration;
            public float remainingTime;
            public bool isPercentage;
        }

        private void Start()
        {
            playerStatus = GetComponent<PlayerStatus>();
        }

        private void Update()
        {
            // 버프 시간 업데이트
            for (int i = activeBuffs.Count - 1; i >= 0; i--)
            {
                activeBuffs[i].remainingTime -= Time.deltaTime;

                if (activeBuffs[i].remainingTime <= 0)
                {
                    RemoveBuff(activeBuffs[i]);
                    activeBuffs.RemoveAt(i);
                }
            }
        }

        public void AddBuff(string name, StatType stat, float value, float duration, bool isPercentage = false)
        {
            var buff = new ActiveBuff
            {
                buffName = name,
                targetStat = stat,
                value = value,
                duration = duration,
                remainingTime = duration,
                isPercentage = isPercentage
            };

            activeBuffs.Add(buff);
            ApplyBuff(buff);
        }

        private void ApplyBuff(ActiveBuff buff)
        {
            // TODO: 실제 스탯에 버프 적용
            Debug.Log($"버프 적용: {buff.buffName} - {buff.targetStat} +{buff.value}{(buff.isPercentage ? "%" : "")}");
        }

        private void RemoveBuff(ActiveBuff buff)
        {
            // TODO: 버프 제거 로직
            Debug.Log($"버프 종료: {buff.buffName}");
        }
    }
}