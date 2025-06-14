using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using RPG.Items.Equipment;
using RPG.Inventory;
using Sirenix.OdinInspector;

namespace RPG.Inventory
{
    /// <summary>
    /// ��� �ռ� �ý��� - ���� ��� �ռ� ó��
    /// </summary>
    public class EquipmentFusionSystem : MonoBehaviour
    {
        [Title("�ý��� ����")]
        [SerializeField, Required]
        private EquipmentInventorySystem inventorySystem;

        [Title("�ռ� ����")]
        [SerializeField]
        private int fusionRequiredCount = 5; // �ռ��� �ʿ��� ������ 5���� ����� ����
        /// <summary>
        /// �ռ��� �ʿ��� ������ �ܺο��� ���� �����ϵ���
        /// </summary>
        public int FusionRequiredCount => fusionRequiredCount;

        // �ռ� ��� �ݹ�
        public System.Action<EquipmentData, EquipmentData, bool> OnFusionComplete;
        public System.Action<int, int> OnAutoFusionComplete;

        private void Start()
        {
            // �κ��丮 �ý��� �ڵ� ã��
            if (inventorySystem == null)
            {
                inventorySystem = FindObjectOfType<EquipmentInventorySystem>();
            }

            // Start���� �� �� �� Ȯ��
            Debug.Log($"[EquipmentFusionSystem] �ռ� �ʿ� ����: {fusionRequiredCount}��");
        }

        /// <summary>
        /// ��� �ռ� �õ�
        /// </summary>
        public bool TryFusion(EquipmentData equipment, int count = -1)
        {
            // count�� -1�̸� �⺻�� ���
            if (count == -1)
            {
                count = fusionRequiredCount;
            }

            if (equipment == null || inventorySystem == null)
            {
                Debug.LogError("��� �Ǵ� �κ��丮 �ý����� �����ϴ�!");
                return false;
            }

            // ���� Ȯ��
            int currentCount = inventorySystem.GetItemCount(equipment);
            if (currentCount < count)
            {
                Debug.LogError($"��� ����! ����: {currentCount}, �ʿ�: {count}");
                return false;
            }

            // �ռ� Ÿ�� ����
            bool isMaxSubGrade = equipment.subGrade >= 5;

            if (isMaxSubGrade && equipment.rarity < EquipmentRarity.Celestial)
            {
                // ��� �±� �ռ�
                return PerformRarityUpgradeFusion(equipment);
            }
            else if (!isMaxSubGrade)
            {
                // ���ε�� ��ȭ �ռ�
                return PerformSubGradeUpgradeFusion(equipment);
            }
            else
            {
                Debug.Log("�̹� �ְ� ����Դϴ�!");
                return false;
            }
        }

        /// <summary>
        /// ���ε�� ��ȭ �ռ� (���� ����)
        /// </summary>
        private bool PerformSubGradeUpgradeFusion(EquipmentData baseEquipment)
        {
            Debug.Log($"[�ռ� ����] {baseEquipment.GetFullRarityName()} {baseEquipment.equipmentName}");
            Debug.Log($"[�ռ� ����] baseEquipment �ν��Ͻ� ID: {baseEquipment.GetInstanceID()}");

            // �ռ� �� ���� Ȯ��
            int beforeCount = inventorySystem.GetItemCount(baseEquipment);
            Debug.Log($"[�ռ� ��] ����: {beforeCount}��");

            // ��� �Ҹ�
            bool removed = inventorySystem.RemoveItem(baseEquipment, fusionRequiredCount);
            if (!removed)
            {
                Debug.LogError($"������ ���� ����!");
                return false;
            }

            // �ռ� �� ���� Ȯ��
            int afterCount = inventorySystem.GetItemCount(baseEquipment);
            Debug.Log($"[��� �Ҹ� ��] ����: {afterCount}��");

            // �κ��丮�� ��� ������ Ȯ��
            var allEquipments = inventorySystem.GetAllItems();
            Debug.Log($"[�κ��丮] �� ������ ����: {allEquipments.Count}");

            // ���׷��̵�� ��� ã��
            var upgradedEquipment = allEquipments.FirstOrDefault(e =>
                e.equipmentName == baseEquipment.equipmentName &&
                e.equipmentType == baseEquipment.equipmentType &&
                e.rarity == baseEquipment.rarity &&
                e.subGrade == baseEquipment.subGrade + 1);

            if (upgradedEquipment != null)
            {
                Debug.Log($"[���� ��� �߰�] {upgradedEquipment.GetFullRarityName()} {upgradedEquipment.equipmentName}");
                Debug.Log($"[���� ���] �ν��Ͻ� ID: {upgradedEquipment.GetInstanceID()}");

                // �߰� �� ����
                int beforeAddCount = inventorySystem.GetItemCount(upgradedEquipment);
                Debug.Log($"[�߰� ��] {upgradedEquipment.GetFullRarityName()} ����: {beforeAddCount}");

                // ���� ��� �߰�
                inventorySystem.AddItem(upgradedEquipment, 1);

                // �߰� �� ����
                int afterAddCount = inventorySystem.GetItemCount(upgradedEquipment);
                Debug.Log($"[�߰� ��] {upgradedEquipment.GetFullRarityName()} ����: {afterAddCount}");
            }
            else
            {
                Debug.Log($"[�� ��� ���� �ʿ�] {baseEquipment.equipmentName} {baseEquipment.rarity} {baseEquipment.subGrade + 1}��");

                // ���� ����
                upgradedEquipment = CreateUpgradedEquipment(baseEquipment, baseEquipment.subGrade + 1, baseEquipment.rarity);
                Debug.Log($"[�� ��� ������] �ν��Ͻ� ID: {upgradedEquipment.GetInstanceID()}");

                inventorySystem.AddItem(upgradedEquipment, 1);

                // �߰� Ȯ��
                int newItemCount = inventorySystem.GetItemCount(upgradedEquipment);
                Debug.Log($"[�� ��� �߰� ��] ����: {newItemCount}");
            }

            Debug.Log($"[�ռ� �Ϸ�] {upgradedEquipment.GetFullRarityName()} {upgradedEquipment.equipmentName}");

            OnFusionComplete?.Invoke(baseEquipment, upgradedEquipment, true);
            return true;
        }
        /// <summary>
        /// ��� �±� �ռ� (���� ����)
        /// </summary>
        private bool PerformRarityUpgradeFusion(EquipmentData baseEquipment)
        {
            Debug.Log($"��� �±� �ռ� �õ� - 100% ���� (�ʿ� ����: {fusionRequiredCount}��)");

            // �ռ� �� ���� Ȯ��
            int beforeCount = inventorySystem.GetItemCount(baseEquipment);
            Debug.Log($"�ռ� �� ���� ����: {beforeCount}��");

            // ��� �Ҹ� - ��������� fusionRequiredCount ���
            bool removed = inventorySystem.RemoveItem(baseEquipment, fusionRequiredCount);
            if (!removed)
            {
                Debug.LogError($"������ ���� ����! �����Ϸ��� ����: {fusionRequiredCount}");
                return false;
            }

            // �ռ� �� ���� Ȯ��
            int afterCount = inventorySystem.GetItemCount(baseEquipment);
            Debug.Log($"�ռ� �� ���� ����: {afterCount}�� (��� �Ҹ�: {beforeCount - afterCount}��)");

            // ���� ��� ��������
            EquipmentRarity nextRarity = GetNextRarity(baseEquipment.rarity);

            // �ռ� �� �κ��丮���� ���׷��̵�� ��� ã��
            var allEquipments = inventorySystem.GetAllItems();
            var upgradedEquipment = allEquipments.FirstOrDefault(e =>
                e.equipmentName == baseEquipment.equipmentName &&
                e.equipmentType == baseEquipment.equipmentType &&
                e.rarity == nextRarity &&
                e.subGrade == 1);

            if (upgradedEquipment != null)
            {
                // ���� ��� ���
                inventorySystem.AddItem(upgradedEquipment, 1);
            }
            else
            {
                // ���� ����
                upgradedEquipment = CreateUpgradedEquipment(baseEquipment, 1, nextRarity);
                inventorySystem.AddItem(upgradedEquipment, 1);
            }

            Debug.Log($"<color=yellow>�ڵ�� �±� ����!�� {upgradedEquipment.GetFullRarityName()} {upgradedEquipment.equipmentName} ȹ��!</color>");
            OnFusionComplete?.Invoke(baseEquipment, upgradedEquipment, true);
            return true;
        }

        /// <summary>
        /// �ڵ� �ռ� ����
        /// </summary>
        public void PerformAutoFusion(EquipmentType equipmentType)
        {
            if (inventorySystem == null) return;

            Debug.Log($"[�ڵ� �ռ� ����] �ʿ� ����: {fusionRequiredCount}��");

            int totalFusionCount = 0;
            int successCount = 0;
            bool hasMoreFusions = true;

            // �� �̻� �ռ��� ���� ���� ������ �ݺ�
            while (hasMoreFusions)
            {
                hasMoreFusions = false;

                // �Ź� ���� ��� ����� ������
                var allEquipments = inventorySystem.GetEquipmentsByType(equipmentType);

                // ��ް� ���ε���� ���� �ͺ��� ���� (���� �ͺ��� �ռ�)
                var sortedEquipments = allEquipments
                    .OrderBy(e => e.rarity)
                    .ThenBy(e => e.subGrade)
                    .ThenBy(e => e.equipmentName)
                    .ToList();

                // �� ��񺰷� �׷�ȭ
                var equipmentGroups = sortedEquipments
                    .GroupBy(e => new { e.equipmentName, e.subGrade, e.rarity })
                    .ToList();

                foreach (var group in equipmentGroups)
                {
                    var equipment = group.First();
                    int count = inventorySystem.GetItemCount(equipment);

                    if (count >= fusionRequiredCount)
                    {
                        Debug.Log($"[�ڵ� �ռ�] {equipment.GetFullRarityName()} {equipment.equipmentName}: {count}�� ����");

                        // �� ���� ������ ��� �ռ� ����
                        while (count >= fusionRequiredCount)
                        {
                            totalFusionCount++;

                            if (TryFusion(equipment, fusionRequiredCount))
                            {
                                successCount++;
                                hasMoreFusions = true; // �ռ������Ƿ� �ٽ� Ȯ�� �ʿ�
                            }
                            else
                            {
                                break; // �ռ� ���� �� �ߴ�
                            }

                            // ���� ���� ��Ȯ��
                            count = inventorySystem.GetItemCount(equipment);
                        }
                    }
                }
            }

            if (totalFusionCount > 0)
            {
                Debug.Log($"<color=yellow>�ڵ� �ռ� �Ϸ�: {totalFusionCount}ȸ �õ�, {successCount}ȸ ����</color>");
                OnAutoFusionComplete?.Invoke(totalFusionCount, successCount);
            }
            else
            {
                Debug.Log("�ռ� ������ ��� �����ϴ�!");
                OnAutoFusionComplete?.Invoke(0, 0);
            }
        }
        /// <summary>
        /// ���׷��̵�� ��� ����
        /// </summary>
        private EquipmentData CreateUpgradedEquipment(EquipmentData baseEquipment, int newSubGrade, EquipmentRarity newRarity)
        {
            // �� ��� ���� (ScriptableObject ����)
            var upgradedEquipment = ScriptableObject.CreateInstance<EquipmentData>();

            // �⺻ ���� ����
            upgradedEquipment.name = baseEquipment.name;
            upgradedEquipment.equipmentName = baseEquipment.equipmentName;
            upgradedEquipment.equipmentType = baseEquipment.equipmentType;
            upgradedEquipment.icon = baseEquipment.icon;
            upgradedEquipment.description = baseEquipment.description;

            // ���ο� ��� ����
            upgradedEquipment.rarity = newRarity;
            upgradedEquipment.subGrade = newSubGrade;

            // ���� ��� (��� ��ȭ �� ���� ����)
            float statMultiplier = GetRarityStatMultiplier(newRarity) / GetRarityStatMultiplier(baseEquipment.rarity);

            switch (baseEquipment.equipmentType)
            {
                case EquipmentType.Weapon:
                    upgradedEquipment.attackPowerBonus = Mathf.RoundToInt(baseEquipment.attackPowerBonus * statMultiplier);
                    break;
                case EquipmentType.Armor:
                    upgradedEquipment.maxHpBonus = Mathf.RoundToInt(baseEquipment.maxHpBonus * statMultiplier);
                    break;
                case EquipmentType.Ring:
                    upgradedEquipment.hpRegenBonus = baseEquipment.hpRegenBonus * statMultiplier;
                    break;
            }

            // ���ݵ� ����
            upgradedEquipment.buyPrice = Mathf.RoundToInt(baseEquipment.buyPrice * statMultiplier);
            upgradedEquipment.sellPrice = upgradedEquipment.buyPrice / 2;

            return upgradedEquipment;
        }

        #region ��ƿ��Ƽ �޼���

        private EquipmentRarity GetNextRarity(EquipmentRarity current)
        {
            switch (current)
            {
                case EquipmentRarity.Common: return EquipmentRarity.Uncommon;
                case EquipmentRarity.Uncommon: return EquipmentRarity.Rare;
                case EquipmentRarity.Rare: return EquipmentRarity.Epic;
                case EquipmentRarity.Epic: return EquipmentRarity.Legendary;
                case EquipmentRarity.Legendary: return EquipmentRarity.Mythic;
                case EquipmentRarity.Mythic: return EquipmentRarity.Celestial;
                default: return current;
            }
        }

        private float GetRarityStatMultiplier(EquipmentRarity rarity)
        {
            switch (rarity)
            {
                case EquipmentRarity.Common: return 1.0f;
                case EquipmentRarity.Uncommon: return 1.5f;
                case EquipmentRarity.Rare: return 2.5f;
                case EquipmentRarity.Epic: return 4.0f;
                case EquipmentRarity.Legendary: return 6.5f;
                case EquipmentRarity.Mythic: return 10.0f;
                case EquipmentRarity.Celestial: return 15.0f;
                default: return 1.0f;
            }
        }

        /// <summary>
        /// �ռ� ���� ���� Ȯ��
        /// </summary>
        public bool CanFuse(EquipmentData equipment)
        {
            if (equipment == null || inventorySystem == null) return false;

            int count = inventorySystem.GetItemCount(equipment);
            return count >= fusionRequiredCount;
        }

        /// <summary>
        /// �ռ� �̸�����
        /// </summary>
        public FusionPreview GetFusionPreview(EquipmentData equipment)
        {
            var preview = new FusionPreview();

            if (equipment == null)
            {
                preview.isValid = false;
                return preview;
            }

            preview.isValid = true;
            preview.baseEquipment = equipment;
            preview.requiredCount = fusionRequiredCount;

            // �ռ� Ÿ�� �Ǵ�
            if (equipment.subGrade >= 5 && equipment.rarity < EquipmentRarity.Celestial)
            {
                // ��� �±�
                preview.fusionType = FusionType.RarityUpgrade;
                preview.successRate = 1.0f; // 100% ����
                preview.resultRarity = GetNextRarity(equipment.rarity);
                preview.resultSubGrade = 1;
            }
            else if (equipment.subGrade < 5)
            {
                // ���ε�� ��ȭ
                preview.fusionType = FusionType.SubGradeUpgrade;
                preview.successRate = 1.0f; // 100% ����
                preview.resultRarity = equipment.rarity;
                preview.resultSubGrade = equipment.subGrade + 1;
            }
            else
            {
                // �ְ� ���
                preview.fusionType = FusionType.MaxLevel;
                preview.successRate = 0f;
            }

            return preview;
        }

        #endregion

        [Title("�����")]
        [Button("�ռ� ���� ���")]
        private void DebugPrintFusionInfo()
        {
            Debug.Log("===== ��� �ռ� �ý��� =====");
            Debug.Log($"�ʿ� ����: {fusionRequiredCount}��");
            Debug.Log("������: 100% (���� ����)");
            Debug.Log("\n���ε�� ��ȭ: 1�� �� 2�� �� 3�� �� 4�� �� 5��");
            Debug.Log("��� �±�: 5�� ��� �� ���� ��� 1��");
            Debug.Log("=============================");
        }

        [Button("���� ���� Ȯ��")]
        private void CheckCurrentSettings()
        {
            Debug.Log($"[EquipmentFusionSystem] ���� fusionRequiredCount: {fusionRequiredCount}");

            if (inventorySystem != null)
            {
                Debug.Log("[EquipmentFusionSystem] InventorySystem �����");
            }
            else
            {
                Debug.LogError("[EquipmentFusionSystem] InventorySystem�� �����ϴ�!");
            }
        }
    }


    /// <summary>
    /// �ռ� �̸����� ����
    /// </summary>
    [System.Serializable]
    public class FusionPreview
    {
        public bool isValid;
        public EquipmentData baseEquipment;
        public FusionType fusionType;
        public int requiredCount;
        public float successRate;
        public EquipmentRarity resultRarity;
        public int resultSubGrade;
    }

    public enum FusionType
    {
        SubGradeUpgrade,    // ���ε�� ��ȭ
        RarityUpgrade,      // ��� �±�
        MaxLevel           // �ְ� ���
    }
}