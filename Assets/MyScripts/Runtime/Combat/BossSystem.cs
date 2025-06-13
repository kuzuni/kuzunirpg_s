using UnityEngine;
using System;
using Sirenix.OdinInspector;
using RPG.Core.Events;
using RPG.Common;

namespace RPG.Combat
{
    public class BossSystem : MonoBehaviour
    {
        [Title("보스 설정")]
        [SerializeField] private bool isBossStage = false;
        [SerializeField] private float bossTimerDuration = 30f;

        [ShowInInspector, ReadOnly]
        [ProgressBar(0, "@bossTimerDuration", 0.8f, 0.3f, 0.3f)]
        private float remainingTime;

        private Coroutine bossTimerCoroutine;

        public void StartBossTimer()
        {
            isBossStage = true;
            remainingTime = bossTimerDuration;

            if (bossTimerCoroutine != null)
            {
                StopCoroutine(bossTimerCoroutine);
            }

            bossTimerCoroutine = StartCoroutine(BossTimerCoroutine());
        }

        private System.Collections.IEnumerator BossTimerCoroutine()
        {
            while (remainingTime > 0 && isBossStage)
            {
                remainingTime -= Time.deltaTime;
                yield return null;
            }

            if (isBossStage)
            {
                // 시간 초과 - 보스 전투 실패
                isBossStage = false;
                GameEventManager.TriggerBossFailed(0); // 현재 스테이지 번호는 StageManager에서 처리
            }
        }

        public void DefeatBoss()
        {
            if (!isBossStage) return;

            isBossStage = false;

            // 보스 보상 지급 (이벤트로 처리)
            GiveBossRewards();

            // 보스 처치 성공 이벤트는 StageManager에서 처리
        }

        private void GiveBossRewards()
        {
            Debug.Log("보스 처치 보상 지급!");

            // 보스 보상은 일반 보상의 5배
            GameEventManager.TriggerCurrencyChanged(CurrencyType.Gold, 5000);
            GameEventManager.TriggerCurrencyChanged(CurrencyType.Diamond, 50);
            GameEventManager.TriggerPlayerExpGained(2500);
        }

        public void StopBossTimer()
        {
            if (bossTimerCoroutine != null)
            {
                StopCoroutine(bossTimerCoroutine);
                bossTimerCoroutine = null;
            }
            isBossStage = false;
        }

        private void OnDestroy()
        {
            StopBossTimer();
        }
    }
}