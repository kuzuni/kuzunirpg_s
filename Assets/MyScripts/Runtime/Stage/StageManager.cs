using UnityEngine;
using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using RPG.Core.Events;
using RPG.Common;
using RPG.Player;
using RPG.Managers;

namespace RPG.Stage
{
    public class StageManager : MonoBehaviour, IStageManager
    {
        [Title("시스템 참조")]
        [SerializeField, Required] private PlayerController playerController;
        [SerializeField, Required] private CurrencyManager currencyManager;  // 추가

        [Title("스테이지 설정")]
        [SerializeField] private List<StageData> stages = new List<StageData>();

        [ShowInInspector, ReadOnly]
        private int currentStageIndex = 0;

        [ShowInInspector, ReadOnly]
        private int currentMonsterIndex = 0;

        [ShowInInspector, ReadOnly]
        private int monstersKilledInStage = 0;

        public StageData CurrentStage => stages[currentStageIndex];
        public MonsterInstance CurrentMonster { get; private set; }

        private void Start()
        {
            // PlayerController 찾기
            if (playerController == null)
            {
                playerController = FindObjectOfType<PlayerController>();
                if (playerController == null)
                {
                    Debug.LogError("PlayerController를 찾을 수 없습니다!");
                }
            }

            // CurrencyManager 찾기
            if (currencyManager == null)
            {
                currencyManager = FindObjectOfType<CurrencyManager>();
                if (currencyManager == null)
                {
                    Debug.LogError("CurrencyManager를 찾을 수 없습니다!");
                }
            }

            // 테스트용 스테이지 생성
            if (stages.Count == 0)
            {
                CreateTestStages();
            }

            StartStage(1);
        }

        public void StartStage(int stageNumber)
        {
            currentStageIndex = Mathf.Clamp(stageNumber - 1, 0, stages.Count - 1);
            currentMonsterIndex = 0;
            monstersKilledInStage = 0;

            SpawnNextMonster();

            // 이벤트 발생
            GameEventManager.TriggerStageStarted(CurrentStage);
        }

        private void SpawnNextMonster()
        {
            if (currentMonsterIndex < CurrentStage.monsters.Count)
            {
                var monsterData = CurrentStage.monsters[currentMonsterIndex];
                CurrentMonster = new MonsterInstance(monsterData);

                Debug.Log($"몬스터 생성: {CurrentMonster.MonsterName}");

                // 몬스터 스폰 이벤트 발생
                GameEventManager.TriggerMonsterSpawned(
                    monsterData,
                    CurrentMonster.CurrentHp,
                    CurrentMonster.MaxHp
                );
            }
        }

        public void DamageMonster(int damage)
        {
            if (CurrentMonster == null) return;

            CurrentMonster.CurrentHp -= damage;

            // 몬스터 체력 변경 이벤트 발생
            GameEventManager.TriggerMonsterHealthChanged(
                CurrentMonster.CurrentHp,
                CurrentMonster.MaxHp
            );

            if (CurrentMonster.CurrentHp <= 0)
            {
                KillCurrentMonster();
            }
        }

        private void KillCurrentMonster()
        {
            if (CurrentMonster == null) return;

            // 보상 지급 - 실제로 플레이어에게 경험치 추가
            if (playerController != null)
            {
                // 경험치 추가 (이 메서드가 내부적으로 이벤트도 발생시킴)
                playerController.Status.AddExperience(CurrentMonster.data.expReward);
                Debug.Log($"경험치 {CurrentMonster.data.expReward} 획득!");
            }

            // ✅ 골드 보상 - CurrencyManager의 AddCurrency 사용
            if (currencyManager != null)
            {
                currencyManager.AddCurrency(CurrencyType.Gold, CurrentMonster.data.goldReward);
                Debug.Log($"골드 {CurrentMonster.data.goldReward} 획득!");
            }

            monstersKilledInStage++;
            currentMonsterIndex++;

            // 몬스터 처치 이벤트 발생
            GameEventManager.TriggerMonsterKilled(CurrentMonster.data);

            // 스테이지 진행도 이벤트 발생
            GameEventManager.TriggerStageProgress(GetStageProgress());

            // 다음 몬스터 또는 스테이지 완료
            if (currentMonsterIndex >= CurrentStage.monsters.Count)
            {
                CompleteStage();
            }
            else
            {
                SpawnNextMonster();
            }
        }

        public void CompleteStage()
        {
            // 스테이지 클리어 보상
            if (playerController != null)
            {
                // 스테이지 클리어 경험치 추가
                playerController.Status.AddExperience(CurrentStage.clearExp);
                Debug.Log($"스테이지 클리어! 보너스 경험치 {CurrentStage.clearExp} 획득!");
            }

            // ✅ 스테이지 클리어 골드 - CurrencyManager의 AddCurrency 사용
            if (currencyManager != null)
            {
                currencyManager.AddCurrency(CurrencyType.Gold, CurrentStage.clearGold);
                Debug.Log($"스테이지 클리어! 보너스 골드 {CurrentStage.clearGold} 획득!");
            }

            // 스테이지 클리어 이벤트 발생
            GameEventManager.TriggerStageCleared(CurrentStage.stageNumber);

            // 다음 스테이지로
            if (currentStageIndex < stages.Count - 1)
            {
                StartStage(currentStageIndex + 2);
            }
            else
            {
                Debug.Log("모든 스테이지 완료!");
            }
        }

        public float GetStageProgress()
        {
            if (CurrentStage.monsters.Count == 0) return 0;
            return (float)monstersKilledInStage / CurrentStage.monsters.Count;
        }

        private void CreateTestStages()
        {
            for (int i = 1; i <= 10; i++)
            {
                var stage = new StageData
                {
                    stageNumber = i,
                    stageName = $"숲의 입구 {i}",
                    monsters = new List<MonsterData>(),
                    clearGold = 100 * i,
                    clearExp = 50 * i
                };

                // 몬스터 추가
                for (int j = 0; j < 5 + i; j++)
                {
                    stage.monsters.Add(new MonsterData
                    {
                        monsterName = j < 5 ? "슬라임" : "고블린",
                        maxHp = 100 * i,
                        attackPower = 10 * i,
                        goldReward = 10 * i,
                        expReward = 5 * i
                    });
                }

                stages.Add(stage);
            }
        }

        [Title("디버그")]
        [Button("현재 몬스터 즉사", ButtonSizes.Large)]
        [GUIColor(0.8f, 0.3f, 0.3f)]
        private void DebugKillMonster()
        {
            if (CurrentMonster != null)
            {
                DamageMonster(CurrentMonster.CurrentHp);
            }
        }

        [Button("스테이지 스킵", ButtonSizes.Large)]
        [GUIColor(0.3f, 0.3f, 0.8f)]
        private void DebugSkipStage()
        {
            CompleteStage();
        }

        [Button("골드 100 추가 테스트", ButtonSizes.Medium)]
        [GUIColor(0.8f, 0.8f, 0.3f)]
        private void DebugAddGold()
        {
            if (currencyManager != null)
            {
                currencyManager.AddCurrency(CurrencyType.Gold, 100);
                Debug.Log("골드 100 추가!");
            }
            else
            {
                Debug.LogError("CurrencyManager가 없습니다!");
            }
        }


        [Button("현재 골드 확인", ButtonSizes.Medium)]
        private void DebugCheckGold()
        {
            if (currencyManager != null)
            {
                Debug.Log($"현재 골드: {currencyManager.Gold}");
            }
        }
    }
}