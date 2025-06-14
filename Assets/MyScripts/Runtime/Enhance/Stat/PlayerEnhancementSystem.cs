using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using RPG.Player;
using RPG.Common;
using RPG.Core.Events;
using RPG.Enhancement.Base;

namespace RPG.Enhancement
{
    /// <summary>
    /// 플레이어 강화 시스템
    /// </summary>
    public class PlayerEnhancementSystem : BaseEnhancementSystem<PlayerStatus>
    {
        [Title("Player Status Reference")]
        [SerializeField, Required]
        private PlayerStatus playerStatus;

        // 원본 스탯 저장
        private Dictionary<StatType, float> originalStats = new Dictionary<StatType, float>();

        public override void Initialize(PlayerStatus status)
        {
            targetObject = status;
            playerStatus = status;
            InitializeEnhancementLevels();
            SaveOriginalStats();
            isInitialized = true;
        }

        protected override void InitializeEnhancementLevels()
        {
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

        protected override void SaveOriginalStats()
        {
            if (playerStatus == null) return;

            originalStats.Clear();
            originalStats[StatType.MaxHp] = playerStatus.MaxHp;
            originalStats[StatType.AttackPower] = playerStatus.AttackPower;
            originalStats[StatType.CritChance] = playerStatus.CritChance;
            originalStats[StatType.CritDamage] = playerStatus.CritDamage;
            originalStats[StatType.AttackSpeed] = playerStatus.AttackSpeed;
            originalStats[StatType.HpRegen] = playerStatus.HpRegen;
        }

        protected override void RecalculateStats()
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

        public override void ApplyEnhancement(EnhancementData enhancement)
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

        private void NotifyStatChanges()
        {
            GameEventManager.TriggerPlayerStatChanged(StatType.MaxHp, playerStatus.MaxHp);
            GameEventManager.TriggerPlayerStatChanged(StatType.AttackPower, playerStatus.AttackPower);
            GameEventManager.TriggerPlayerStatChanged(StatType.CritChance, playerStatus.CritChance);
            GameEventManager.TriggerPlayerStatChanged(StatType.CritDamage, playerStatus.CritDamage);
            GameEventManager.TriggerPlayerStatChanged(StatType.AttackSpeed, playerStatus.AttackSpeed);
            GameEventManager.TriggerPlayerStatChanged(StatType.HpRegen, playerStatus.HpRegen);
        }

        // 기존 EnhanceStat 오버라이드로 GameEventManager 호출 추가
        public override bool EnhanceStat(StatType statType, int levels = 1)
        {
            bool result = base.EnhanceStat(statType, levels);

            if (result)
            {
                var enhancementLevel = GetEnhancementLevel(statType);
                GameEventManager.TriggerStatEnhanced(statType, enhancementLevel.currentLevel);

                if (enhancementLevel.currentLevel >= enhancementLevel.maxLevel)
                {
                    GameEventManager.TriggerStatEnhancementMaxed(statType);
                }
            }

            return result;
        }
    }
}