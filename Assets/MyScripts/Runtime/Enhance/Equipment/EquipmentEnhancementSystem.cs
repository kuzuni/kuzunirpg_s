using RPG.Common;
using RPG.Items.Equipment;
using RPG.Managers;
using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace RPG.Enhancement
{
    /// <summary>
    /// ��� ��ȭ �ý��� - BaseEnhancementSystem�� ������� ����
    /// </summary>
    public class EquipmentEnhancementSystem : MonoBehaviour
    {
        [Title("�ý��� ����")]
        [SerializeField]
        private CurrencyManager currencyManager;

        [Title("��ȭ ����")]
        [SerializeField]
        private int maxEquipmentLevel = 100;

        [SerializeField]
        private float levelBonusPerLevel = 0.01f; // ������ 1% ���ʽ�

        [SerializeField]
        private int baseEnhanceCost = 100;

        [SerializeField]
        private float costMultiplier = 1.5f;

        [Title("������ ����")]
        [SerializeField]
        private bool guaranteedSuccess = true;

        // �̺�Ʈ
        public event Action<EquipmentData, int, bool> OnEquipmentEnhanced;

        /// <summary>
        /// ��� ��ȭ �õ�
        /// </summary>
        public bool TryEnhanceEquipment(EquipmentData equipment)
        {
            if (equipment == null)
            {
                Debug.LogError("��ȭ�� ��� �����ϴ�!");
                return false;
            }

            if (equipment.level >= maxEquipmentLevel)
            {
                Debug.LogWarning($"{equipment.equipmentName}��(��) �̹� �ִ� �����Դϴ�!");
                return false;
            }

            // ��ȭ ��� ���
            int cost = CalculateEnhanceCost(equipment);

            // ��� Ȯ�� �� ����
            if (currencyManager != null && !currencyManager.CanAfford(CurrencyType.Gold, cost))
            {
                Debug.LogError("��尡 �����մϴ�!");
                return false;
            }

            if (currencyManager != null)
            {
                currencyManager.TrySpend(CurrencyType.Gold, cost);
            }

            // ������
            equipment.level++;
            Debug.Log($"<color=green>��ȭ ����! {equipment.GetFullRarityName()}</color>");

            // �̺�Ʈ �߻�
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