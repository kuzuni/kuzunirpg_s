using System;
using UnityEngine;
using RPG.Common;
using RPG.Stage;
using RPG.Items.Equipment;
using RPG.Items.Relic;

namespace RPG.Core.Events
{
    /// <summary>
    /// �߾� �̺�Ʈ ���� �ý���
    /// ��� ���� �� �̺�Ʈ�� �߾ӿ��� �����Ͽ� �ý��� �� ���յ��� ����
    /// </summary>
    public static class GameEventManager
    {
        #region Player Events

        // ���� & ����ġ
        public static event Action<int> OnPlayerLevelUp;
        public static event Action<int> OnPlayerExpGained;

        // ü��
        public static event Action<int, int> OnPlayerHealthChanged; // current, max
        public static event Action OnPlayerDeath;
        public static event Action<int> OnPlayerHealed;

        // ����
        public static event Action<StatType, float> OnPlayerStatChanged;

        #endregion

        #region Combat Events

        public static event Action<int, bool> OnDamageDealt; // damage, isCritical
        public static event Action<int> OnDamageTaken;

        #endregion

        #region Stage Events

        // �������� ����
        public static event Action<StageData> OnStageStarted;
        public static event Action<int> OnStageCleared; // stageNumber
        public static event Action<float> OnStageProgress; // 0~1

        // ����
        public static event Action<MonsterData, int, int> OnMonsterSpawned; // monster, currentHp, maxHp
        public static event Action<int, int> OnMonsterHealthChanged; // current, max
        public static event Action<MonsterData> OnMonsterKilled;

        // ����
        public static event Action<int> OnBossFailed; // stageNumber

        #endregion

        #region Currency Events

        public static event Action<CurrencyType, long> OnCurrencyChanged;
        public static event Action<CurrencyType, long> OnCurrencySpent;

        #endregion

        #region Item Events

        // ���
        public static event Action<EquipmentData> OnEquipmentObtained;
        public static event Action<EquipmentData> OnEquipmentEquipped;
        public static event Action<EquipmentType> OnEquipmentUnequipped;

        // ����
        public static event Action<RelicInstance> OnRelicObtained;
        public static event Action<RelicInstance> OnRelicLevelUp;
        public static event Action<RelicInstance, bool> OnRelicFusionAttempt; // relic, success

        #endregion

        #region Enhancement Events

        public static event Action<StatType, int> OnStatEnhanced; // stat, level
        public static event Action<StatType> OnStatEnhancementMaxed;

        #endregion

        #region UI Events

        public static event Action<string> OnPanelOpened;
        public static event Action<string> OnPanelClosed;
        public static event Action<string> OnNotificationShown;

        #endregion

        #region Achievement Events

        public static event Action<string> OnAchievementUnlocked; // achievementId
        public static event Action<string, float> OnAchievementProgress; // achievementId, progress

        #endregion

        #region Trigger Methods - Player

        public static void TriggerPlayerLevelUp(int newLevel)
        {
            OnPlayerLevelUp?.Invoke(newLevel);
            if (EnableDebugLog) Debug.Log($"[Event] Player Level Up: {newLevel}");
        }

        public static void TriggerPlayerExpGained(int exp)
        {
            OnPlayerExpGained?.Invoke(exp);
            if (EnableDebugLog) Debug.Log($"[Event] Player Exp Gained: {exp}");
        }

        public static void TriggerPlayerHealthChanged(int current, int max)
        {
            OnPlayerHealthChanged?.Invoke(current, max);
            if (EnableDebugLog) Debug.Log($"[Event] Player Health: {current}/{max}");
        }

        public static void TriggerPlayerDeath()
        {
            OnPlayerDeath?.Invoke();
            if (EnableDebugLog) Debug.Log("[Event] Player Death");
        }

        public static void TriggerPlayerHealed(int amount)
        {
            OnPlayerHealed?.Invoke(amount);
            if (EnableDebugLog) Debug.Log($"[Event] Player Healed: {amount}");
        }

        public static void TriggerPlayerStatChanged(StatType stat, float value)
        {
            OnPlayerStatChanged?.Invoke(stat, value);
            if (EnableDebugLog) Debug.Log($"[Event] Player Stat Changed: {stat} = {value}");
        }

        #endregion

        #region Trigger Methods - Combat

        public static void TriggerDamageDealt(int damage, bool isCritical)
        {
            OnDamageDealt?.Invoke(damage, isCritical);
            if (EnableDebugLog) Debug.Log($"[Event] Damage Dealt: {damage} (Crit: {isCritical})");
        }

        public static void TriggerDamageTaken(int damage)
        {
            OnDamageTaken?.Invoke(damage);
            if (EnableDebugLog) Debug.Log($"[Event] Damage Taken: {damage}");
        }

        #endregion

        #region Trigger Methods - Stage

        public static void TriggerStageStarted(StageData stage)
        {
            OnStageStarted?.Invoke(stage);
            if (EnableDebugLog) Debug.Log($"[Event] Stage Started: {stage.stageNumber} - {stage.stageName}");
        }

        public static void TriggerStageCleared(int stageNumber)
        {
            OnStageCleared?.Invoke(stageNumber);
            if (EnableDebugLog) Debug.Log($"[Event] Stage Cleared: {stageNumber}");
        }

        public static void TriggerStageProgress(float progress)
        {
            OnStageProgress?.Invoke(progress);
            if (EnableDebugLog && progress % 0.25f < 0.01f) // 25% �����θ� �α�
                Debug.Log($"[Event] Stage Progress: {progress:P0}");
        }

        public static void TriggerMonsterSpawned(MonsterData monster, int currentHp, int maxHp)
        {
            OnMonsterSpawned?.Invoke(monster, currentHp, maxHp);
            if (EnableDebugLog) Debug.Log($"[Event] Monster Spawned: {monster.monsterName}");
        }

        public static void TriggerMonsterHealthChanged(int current, int max)
        {
            OnMonsterHealthChanged?.Invoke(current, max);
        }

        public static void TriggerMonsterKilled(MonsterData monster)
        {
            OnMonsterKilled?.Invoke(monster);
            if (EnableDebugLog) Debug.Log($"[Event] Monster Killed: {monster.monsterName}");
        }

        public static void TriggerBossFailed(int stageNumber)
        {
            OnBossFailed?.Invoke(stageNumber);
            if (EnableDebugLog) Debug.Log($"[Event] Boss Failed: Stage {stageNumber}");
        }

        #endregion

        #region Trigger Methods - Currency

        public static void TriggerCurrencyChanged(CurrencyType type, long amount)
        {
            OnCurrencyChanged?.Invoke(type, amount);
            if (EnableDebugLog) Debug.Log($"[Event] Currency Changed: {type} = {amount}");
        }

        public static void TriggerCurrencySpent(CurrencyType type, long amount)
        {
            OnCurrencySpent?.Invoke(type, amount);
            if (EnableDebugLog) Debug.Log($"[Event] Currency Spent: {type} - {amount}");
        }

        #endregion

        #region Trigger Methods - Items

        public static void TriggerEquipmentObtained(EquipmentData equipment)
        {
            OnEquipmentObtained?.Invoke(equipment);
            if (EnableDebugLog) Debug.Log($"[Event] Equipment Obtained: {equipment.equipmentName}");
        }

        public static void TriggerEquipmentEquipped(EquipmentData equipment)
        {
            OnEquipmentEquipped?.Invoke(equipment);
            if (EnableDebugLog) Debug.Log($"[Event] Equipment Equipped: {equipment.equipmentName}");
        }

        public static void TriggerEquipmentUnequipped(EquipmentType type)
        {
            OnEquipmentUnequipped?.Invoke(type);
            if (EnableDebugLog) Debug.Log($"[Event] Equipment Unequipped: {type}");
        }

        public static void TriggerRelicObtained(RelicInstance relic)
        {
            OnRelicObtained?.Invoke(relic);
            if (EnableDebugLog) Debug.Log($"[Event] Relic Obtained: {relic.relicData.relicName}");
        }

        public static void TriggerRelicLevelUp(RelicInstance relic)
        {
            OnRelicLevelUp?.Invoke(relic);
            if (EnableDebugLog) Debug.Log($"[Event] Relic Level Up: {relic.relicData.relicName} Lv.{relic.level}");
        }

        public static void TriggerRelicFusionAttempt(RelicInstance relic, bool success)
        {
            OnRelicFusionAttempt?.Invoke(relic, success);
            if (EnableDebugLog) Debug.Log($"[Event] Relic Fusion: {relic.relicData.relicName} - {(success ? "Success" : "Failed")}");
        }

        #endregion

        #region Trigger Methods - Enhancement

        public static void TriggerStatEnhanced(StatType stat, int level)
        {
            OnStatEnhanced?.Invoke(stat, level);
            if (EnableDebugLog) Debug.Log($"[Event] Stat Enhanced: {stat} Lv.{level}");
        }

        public static void TriggerStatEnhancementMaxed(StatType stat)
        {
            OnStatEnhancementMaxed?.Invoke(stat);
            if (EnableDebugLog) Debug.Log($"[Event] Stat Enhancement Maxed: {stat}");
        }

        #endregion

        #region Trigger Methods - UI

        public static void TriggerPanelOpened(string panelName)
        {
            OnPanelOpened?.Invoke(panelName);
            if (EnableDebugLog) Debug.Log($"[Event] Panel Opened: {panelName}");
        }

        public static void TriggerPanelClosed(string panelName)
        {
            OnPanelClosed?.Invoke(panelName);
            if (EnableDebugLog) Debug.Log($"[Event] Panel Closed: {panelName}");
        }

        public static void TriggerNotificationShown(string message)
        {
            OnNotificationShown?.Invoke(message);
            if (EnableDebugLog) Debug.Log($"[Event] Notification: {message}");
        }

        #endregion

        #region Trigger Methods - Achievement

        public static void TriggerAchievementUnlocked(string achievementId)
        {
            OnAchievementUnlocked?.Invoke(achievementId);
            if (EnableDebugLog) Debug.Log($"[Event] Achievement Unlocked: {achievementId}");
        }

        public static void TriggerAchievementProgress(string achievementId, float progress)
        {
            OnAchievementProgress?.Invoke(achievementId, progress);
            if (EnableDebugLog && progress % 0.25f < 0.01f) // 25% �����θ� �α�
                Debug.Log($"[Event] Achievement Progress: {achievementId} - {progress:P0}");
        }

        #endregion

        #region Debug

        // ����� �α� Ȱ��ȭ
        public static bool EnableDebugLog { get; set; } = false;

        /// <summary>
        /// ��� �̺�Ʈ ������ �� Ȯ�� (�޸� ���� üũ��)
        /// </summary>
        public static void PrintEventSubscriberCount()
        {
            Debug.Log("========== Event Subscriber Count ==========");
            PrintSubscriberCount(nameof(OnPlayerLevelUp), OnPlayerLevelUp);
            PrintSubscriberCount(nameof(OnPlayerExpGained), OnPlayerExpGained);
            PrintSubscriberCount(nameof(OnPlayerHealthChanged), OnPlayerHealthChanged);
            PrintSubscriberCount(nameof(OnDamageDealt), OnDamageDealt);
            PrintSubscriberCount(nameof(OnMonsterKilled), OnMonsterKilled);
            PrintSubscriberCount(nameof(OnStageCleared), OnStageCleared);
            PrintSubscriberCount(nameof(OnCurrencyChanged), OnCurrencyChanged);
            PrintSubscriberCount(nameof(OnEquipmentObtained), OnEquipmentObtained);
            Debug.Log("==========================================");
        }

        private static void PrintSubscriberCount(string eventName, Delegate eventDelegate)
        {
            int count = eventDelegate?.GetInvocationList()?.Length ?? 0;
            if (count > 0)
            {
                Debug.Log($"{eventName}: {count} subscribers");
            }
        }

        #endregion
    }
}