using RPG.Common;
using RPG.Items.Equipment;
using RPG.Managers;
using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace RPG.Enhancement
{
    /// <summary>
    /// 장비 강화 시스템 - BaseEnhancementSystem을 상속하지 않음
    /// </summary>
    public class EquipmentEnhancementSystem : MonoBehaviour
    {
        [Title("시스템 참조")]
        [SerializeField]
        private CurrencyManager currencyManager;

        [Title("강화 설정")]
        [SerializeField]
        private int maxEquipmentLevel = 100;

        [SerializeField]
        private float levelBonusPerLevel = 0.01f; // 레벨당 1% 보너스

        [SerializeField]
        private int baseEnhanceCost = 100;

        [SerializeField]
        private float costMultiplier = 1.5f;

        [Title("성공률 설정")]
        [SerializeField]
        private bool guaranteedSuccess = true;

        // 이벤트
        public event Action<EquipmentData, int, bool> OnEquipmentEnhanced;

        /// <summary>
        /// 장비 강화 시도
        /// </summary>
        public bool TryEnhanceEquipment(EquipmentData equipment)
        {
            if (equipment == null)
            {
                Debug.LogError("강화할 장비가 없습니다!");
                return false;
            }

            if (equipment.level >= maxEquipmentLevel)
            {
                Debug.LogWarning($"{equipment.equipmentName}은(는) 이미 최대 레벨입니다!");
                return false;
            }

            // 강화 비용 계산
            int cost = CalculateEnhanceCost(equipment);

            // 비용 확인 및 차감
            if (currencyManager != null && !currencyManager.CanAfford(CurrencyType.Gold, cost))
            {
                Debug.LogError("골드가 부족합니다!");
                return false;
            }

            if (currencyManager != null)
            {
                currencyManager.TrySpend(CurrencyType.Gold, cost);
            }

            // 레벨업
            equipment.level++;
            Debug.Log($"<color=green>강화 성공! {equipment.GetFullRarityName()}</color>");

            // 이벤트 발생
            OnEquipmentEnhanced?.Invoke(equipment, equipment.level, true);

            return true;
        }

        private int CalculateEnhanceCost(EquipmentData equipment)
        {
            float rarityMultiplier = 1f + (int)equipment.rarity * 0.5f;
            return Mathf.RoundToInt(baseEnhanceCost * Mathf.Pow(costMultiplier, equipment.level) * rarityMultiplier);
        }
    }
}