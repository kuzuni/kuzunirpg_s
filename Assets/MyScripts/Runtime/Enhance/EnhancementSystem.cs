using System.Collections.Generic;
using System;
using UnityEngine;
using Sirenix.OdinInspector;
using RPG.Player;
using RPG.Common;
using RPG.Core.Events;

namespace RPG.Enhancement
{
    // 강화 시스템 인터페이스
    public interface IEnhanceable
    {
        void ApplyEnhancement(EnhancementData enhancement);
        void ResetEnhancements();
    }

    // 강화 시스템
    public class EnhancementSystem : MonoBehaviour, IEnhanceable
    {
        [Title("Player Status Reference")]
        [SerializeField, Required] private PlayerStatus playerStatus;

        [Title("Enhancement Levels")]
        [SerializeField, TableList(ShowIndexLabels = true, DrawScrollView = true, MaxScrollViewHeight = 300)]
        private List<StatEnhancementLevel> enhancementLevels = new List<StatEnhancementLevel>();

        // 원본 스탯 저장
        [ShowInInspector, ReadOnly, DictionaryDrawerSettings(KeyLabel = "Stat Type", ValueLabel = "Original Value")]
        private Dictionary<StatType, float> originalStats = new Dictionary<StatType, float>();

        // 초기화 여부 확인
        [ShowInInspector, ReadOnly]
        private bool isInitialized = false;

        [Title("Test Controls")]
        [PropertySpace(10)]
        [EnumToggleButtons]
        public StatType testStatType = StatType.AttackPower;

        [ButtonGroup("Enhancement")]
        [Button("Enhance Selected Stat", ButtonSizes.Large), GUIColor(0.4f, 0.8f, 0.4f)]
        private void TestEnhanceStat()
        {
            if (!isInitialized)
            {
                Debug.LogError("먼저 Initialize를 실행해주세요!");
                return;
            }

            bool success = EnhanceStat(testStatType);
            if (success)
            {
                Debug.Log($"<color=green>{testStatType} 강화 성공!</color>");
            }
        }

        [ButtonGroup("Enhancement")]
        [Button("Max Selected Stat", ButtonSizes.Large), GUIColor(0.8f, 0.8f, 0.4f)]
        private void TestMaxStat()
        {
            if (!isInitialized)
            {
                Debug.LogError("먼저 Initialize를 실행해주세요!");
                return;
            }

            var enhancement = GetEnhancementLevel(testStatType);
            if (enhancement != null)
            {
                while (enhancement.currentLevel < enhancement.maxLevel)
                {
                    EnhanceStat(testStatType);
                }
            }
        }

        [ButtonGroup("Enhancement")]
        [Button("Reset All", ButtonSizes.Large), GUIColor(0.8f, 0.4f, 0.4f)]
        private void TestResetAll()
        {
            ResetEnhancements();
            Debug.Log("<color=yellow>모든 강화가 초기화되었습니다!</color>");
        }

        [Title("Quick Enhancement")]
        [HorizontalGroup("QuickEnhance", 0.5f)]
        [VerticalGroup("QuickEnhance/Left")]
        [Button("MaxHp +1", ButtonSizes.Medium), GUIColor(0.5f, 0.7f, 1f)]
        private void EnhanceMaxHp() => EnhanceStat(StatType.MaxHp);

        [VerticalGroup("QuickEnhance/Left")]
        [Button("Attack +1", ButtonSizes.Medium), GUIColor(1f, 0.5f, 0.5f)]
        private void EnhanceAttack() => EnhanceStat(StatType.AttackPower);

        [VerticalGroup("QuickEnhance/Left")]
        [Button("CritChance +1", ButtonSizes.Medium), GUIColor(1f, 0.8f, 0.5f)]
        private void EnhanceCritChance() => EnhanceStat(StatType.CritChance);

        [VerticalGroup("QuickEnhance/Right")]
        [Button("CritDamage +1", ButtonSizes.Medium), GUIColor(1f, 0.5f, 0.8f)]
        private void EnhanceCritDamage() => EnhanceStat(StatType.CritDamage);

        [VerticalGroup("QuickEnhance/Right")]
        [Button("AttackSpeed +1", ButtonSizes.Medium), GUIColor(0.5f, 1f, 0.5f)]
        private void EnhanceAttackSpeed() => EnhanceStat(StatType.AttackSpeed);

        [VerticalGroup("QuickEnhance/Right")]
        [Button("HpRegen +1", ButtonSizes.Medium), GUIColor(0.5f, 1f, 0.8f)]
        private void EnhanceHpRegen() => EnhanceStat(StatType.HpRegen);

        [Title("Initialization")]
        [Button("Initialize System", ButtonSizes.Large), EnableIf("@!isInitialized"), GUIColor(0.4f, 0.4f, 0.8f)]
        public void InitializeInEditor()
        {
            if (playerStatus != null)
            {
                Initialize(playerStatus);
                Debug.Log("<color=cyan>Enhancement System이 초기화되었습니다!</color>");
            }
            else
            {
                Debug.LogError("PlayerStatus가 설정되지 않았습니다!");
            }
        }

        // PlayerStatus 설정 및 초기화
        public void Initialize(PlayerStatus status)
        {
            playerStatus = status;
            InitializeEnhancementLevels();
            SaveOriginalStats();
            isInitialized = true;
        }

        private void InitializeEnhancementLevels()
        {
            // 모든 스탯 타입에 대해 초기화
            enhancementLevels = new List<StatEnhancementLevel>
            {
                new StatEnhancementLevel { statType = StatType.MaxHp, baseEnhancementValue = 20f, isPercentage = false, maxLevel = 100000 },
                new StatEnhancementLevel { statType = StatType.AttackPower, baseEnhancementValue = 3f, isPercentage = false, maxLevel = 100000 },
                new StatEnhancementLevel { statType = StatType.CritChance, baseEnhancementValue = 0.02f, isPercentage = false, maxLevel = 5000 },
                new StatEnhancementLevel { statType = StatType.CritDamage, baseEnhancementValue = 0.1f, isPercentage = false, maxLevel = 1000000 },
                new StatEnhancementLevel { statType = StatType.AttackSpeed, baseEnhancementValue = 5f, isPercentage = true, maxLevel = 10 },
                new StatEnhancementLevel { statType = StatType.HpRegen, baseEnhancementValue = 0.5f, isPercentage = false, maxLevel = 100000 }
            };
        }

        private void SaveOriginalStats()
        {
            if (playerStatus == null)
            {
                Debug.LogError("PlayerStatus is null in SaveOriginalStats!");
                return;
            }

            originalStats.Clear();
            originalStats[StatType.MaxHp] = playerStatus.MaxHp;
            originalStats[StatType.AttackPower] = playerStatus.AttackPower;
            originalStats[StatType.CritChance] = playerStatus.CritChance;
            originalStats[StatType.CritDamage] = playerStatus.CritDamage;
            originalStats[StatType.AttackSpeed] = playerStatus.AttackSpeed;
            originalStats[StatType.HpRegen] = playerStatus.HpRegen;
        }

        // 특정 스탯 강화
        public bool EnhanceStat(StatType statType)
        {
            if (!isInitialized)
            {
                Debug.LogError("EnhancementSystem이 초기화되지 않았습니다!");
                return false;
            }

            var enhancementLevel = enhancementLevels.Find(e => e.statType == statType);
            if (enhancementLevel == null) return false;

            if (enhancementLevel.currentLevel >= enhancementLevel.maxLevel)
            {
                Debug.LogWarning($"{statType}은(는) 이미 최대 레벨입니다!");
                return false;
            }

            enhancementLevel.currentLevel++;
            RecalculateStats();

            // 이벤트 발생 (기존 OnStatEnhanced 대체)
            GameEventManager.TriggerStatEnhanced(statType, enhancementLevel.currentLevel);

            if (enhancementLevel.currentLevel >= enhancementLevel.maxLevel)
            {
                // 이벤트 발생 (기존 OnEnhancementMaxed 대체)
                GameEventManager.TriggerStatEnhancementMaxed(statType);
            }

            Debug.Log($"{statType} 강화 완료! (Lv.{enhancementLevel.currentLevel})");
            return true;
        }

        // 모든 스탯 재계산
        private void RecalculateStats()
        {
            // 원본 값으로 초기화
            playerStatus.MaxHp = (int)originalStats[StatType.MaxHp];
            playerStatus.AttackPower = (int)originalStats[StatType.AttackPower];
            playerStatus.CritChance = originalStats[StatType.CritChance];
            playerStatus.CritDamage = originalStats[StatType.CritDamage];
            playerStatus.AttackSpeed = originalStats[StatType.AttackSpeed];
            playerStatus.HpRegen = originalStats[StatType.HpRegen];

            // 강화 적용
            foreach (var enhancement in enhancementLevels)
            {
                if (enhancement.currentLevel > 0)
                {
                    var enhancementData = new EnhancementData(
                        enhancement.statType,
                        enhancement.GetEnhancementValue(),
                        enhancement.isPercentage
                    );
                    ApplyEnhancement(enhancementData);
                }
            }

            // 스탯 변경 이벤트 발생
            NotifyStatChanges();
        }

        private void NotifyStatChanges()
        {
            GameEventManager.TriggerPlayerStatChanged(StatType.MaxHp, playerStatus.MaxHp);
            GameEventManager.TriggerPlayerStatChanged(StatType.AttackPower, playerStatus.AttackPower);
            GameEventManager.TriggerPlayerStatChanged(StatType.CritChance, playerStatus.CritChance);
            GameEventManager.TriggerPlayerStatChanged(StatType.CritDamage, playerStatus.CritDamage);
            GameEventManager.TriggerPlayerStatChanged(StatType.AttackSpeed, playerStatus.AttackSpeed);
            GameEventManager.TriggerPlayerStatChanged(StatType.HpRegen, playerStatus.HpRegen);
        }

        // 강화 적용
        public void ApplyEnhancement(EnhancementData enhancement)
        {
            switch (enhancement.statType)
            {
                case StatType.MaxHp:
                    if (enhancement.isPercentage)
                        playerStatus.MaxHp = (int)(playerStatus.MaxHp * (1 + enhancement.enhancementValue / 100));
                    else
                        playerStatus.MaxHp += (int)enhancement.enhancementValue;
                    break;

                case StatType.AttackPower:
                    if (enhancement.isPercentage)
                        playerStatus.AttackPower = (int)(playerStatus.AttackPower * (1 + enhancement.enhancementValue / 100));
                    else
                        playerStatus.AttackPower += (int)enhancement.enhancementValue;
                    break;

                case StatType.CritChance:
                    if (enhancement.isPercentage)
                        playerStatus.CritChance *= (1 + enhancement.enhancementValue / 100);
                    else
                        playerStatus.CritChance += enhancement.enhancementValue;
                    break;

                case StatType.CritDamage:
                    if (enhancement.isPercentage)
                        playerStatus.CritDamage *= (1 + enhancement.enhancementValue / 100);
                    else
                        playerStatus.CritDamage += enhancement.enhancementValue;
                    break;

                case StatType.AttackSpeed:
                    if (enhancement.isPercentage)
                        playerStatus.AttackSpeed *= (1 + enhancement.enhancementValue / 100);
                    else
                        playerStatus.AttackSpeed += enhancement.enhancementValue;
                    break;

                case StatType.HpRegen:
                    if (enhancement.isPercentage)
                        playerStatus.HpRegen *= (1 + enhancement.enhancementValue / 100);
                    else
                        playerStatus.HpRegen += enhancement.enhancementValue;
                    break;
            }
        }

        // 강화 초기화
        public void ResetEnhancements()
        {
            if (!isInitialized)
            {
                Debug.LogError("EnhancementSystem이 초기화되지 않았습니다!");
                return;
            }

            foreach (var enhancement in enhancementLevels)
            {
                enhancement.currentLevel = 0;
            }
            RecalculateStats();
        }

        // 강화 정보 가져오기
        public StatEnhancementLevel GetEnhancementLevel(StatType statType)
        {
            return enhancementLevels.Find(e => e.statType == statType);
        }

        // 디버그용 강화 정보 출력
        [Button("Show Enhancement Info", ButtonSizes.Large), PropertySpace(20)]
        public void DebugEnhancementInfo()
        {
            if (!isInitialized)
            {
                Debug.LogWarning("EnhancementSystem이 아직 초기화되지 않았습니다!");
                return;
            }

            Debug.Log("========== Enhancement Info ==========");
            foreach (var enhancement in enhancementLevels)
            {
                string color = enhancement.currentLevel >= enhancement.maxLevel ? "yellow" : "white";
                Debug.Log($"<color={color}>{enhancement.statType}: Lv.{enhancement.currentLevel}/{enhancement.maxLevel} " +
                         $"(+{enhancement.GetEnhancementValue()}{(enhancement.isPercentage ? "%" : "")})</color>");
            }
            Debug.Log("=====================================");
        }

        [Title("Current Enhancement Status")]
        [ShowInInspector, ReadOnly, ProgressBar(0, 1, 0.5f, 0.5f, 1f)]
        private float TotalEnhancementProgress
        {
            get
            {
                if (enhancementLevels == null || enhancementLevels.Count == 0) return 0;

                int totalCurrent = 0;
                int totalMax = 0;

                foreach (var enhancement in enhancementLevels)
                {
                    totalCurrent += enhancement.currentLevel;
                    totalMax += enhancement.maxLevel;
                }

                return totalMax > 0 ? (float)totalCurrent / totalMax : 0;
            }
        }
    }
}