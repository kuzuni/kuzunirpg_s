using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using RPG.Player;
using RPG.Core.Events;

namespace RPG.Items.Equipment
{
    public class EquipmentSystem : MonoBehaviour
    {
        [Title("��� ����")]
        [DictionaryDrawerSettings(KeyLabel = "���� Ÿ��", ValueLabel = "���� ���")]
        [ShowInInspector]
        private Dictionary<EquipmentType, EquipmentSlot> equipmentSlots;

        [Title("��� ȿ��")]
        [ShowInInspector, ReadOnly]
        [ProgressBar(0, 200, 0.8f, 0.3f, 0.3f)]
        private int TotalAttackBonus { get; set; }

        [ShowInInspector, ReadOnly]
        [ProgressBar(0, 1000, 0.3f, 0.8f, 0.3f)]
        private int TotalMaxHpBonus { get; set; }

        [ShowInInspector, ReadOnly]
        [ProgressBar(0, 20, 0.3f, 0.8f, 0.8f)]
        private float TotalHpRegenBonus { get; set; }

        [Title("���� ��� ��")]
        [ShowInInspector, ReadOnly]
        [ListDrawerSettings(ShowFoldout = false, Expanded = true)]
        private List<string> EquippedItemsInfo
        {
            get
            {
                var info = new List<string>();
                foreach (var slot in equipmentSlots.Values)
                {
                    if (!slot.IsEmpty)
                    {
                        var eq = slot.CurrentEquipment;
                        var rarityColor = ColorUtility.ToHtmlStringRGB(RarityColors.GetRarityColor(eq.rarity));
                        var rarityName = RarityColors.GetRarityName(eq.rarity);
                        info.Add($"<color=#{rarityColor}>[{rarityName}] {eq.equipmentName}</color>");
                    }
                }
                return info;
            }
        }

        // ����
        private PlayerStatus playerStatus;
        private bool isInitialized = false;

        public void Initialize(PlayerStatus status)
        {
            playerStatus = status;
            InitializeSlots();
            isInitialized = true;
        }

        private void InitializeSlots()
        {
            equipmentSlots = new Dictionary<EquipmentType, EquipmentSlot>
            {
                { EquipmentType.Weapon, new EquipmentSlot(EquipmentType.Weapon) },
                { EquipmentType.Armor, new EquipmentSlot(EquipmentType.Armor) },
                { EquipmentType.Ring, new EquipmentSlot(EquipmentType.Ring) }
            };
        }

        [Title("��� ����")]
        [Button("��� ����", ButtonSizes.Large), GUIColor(0.3f, 0.8f, 0.3f)]
        public bool EquipItem(EquipmentData equipment)
        {
            if (!isInitialized || equipment == null)
            {
                Debug.LogError("�ý����� �ʱ�ȭ���� �ʾҰų� ��� null�Դϴ�.");
                return false;
            }

            var slot = equipmentSlots[equipment.equipmentType];

            // �̹� ������ ��� ������ ����
            if (!slot.IsEmpty)
            {
                UnequipItem(equipment.equipmentType);
            }

            // �� ��� ����
            if (slot.Equip(equipment))
            {
                ApplyEquipmentStats(equipment, true);
                UpdateTotalBonuses();

                // �̺�Ʈ �߻� (���� OnEquipmentChanged ��ü)
                GameEventManager.TriggerEquipmentEquipped(equipment);

                // ��޺� �������� �α� ���
                var color = ColorUtility.ToHtmlStringRGB(RarityColors.GetRarityColor(equipment.rarity));
                Debug.Log($"<color=#{color}>[{equipment.GetFullRarityName()}] {equipment.equipmentName}��(��) �����߽��ϴ�!</color>");
                return true;
            }

            return false;
        }

        [Button("��� ����", ButtonSizes.Large), GUIColor(0.8f, 0.3f, 0.3f)]
        public EquipmentData UnequipItem(EquipmentType slotType)
        {
            if (!isInitialized)
            {
                Debug.LogError("�ý����� �ʱ�ȭ���� �ʾҽ��ϴ�.");
                return null;
            }

            var slot = equipmentSlots[slotType];
            var unequipped = slot.Unequip();

            if (unequipped != null)
            {
                ApplyEquipmentStats(unequipped, false);
                UpdateTotalBonuses();

                // �̺�Ʈ �߻� (���� OnEquipmentRemoved ��ü)
                GameEventManager.TriggerEquipmentUnequipped(slotType);

                Debug.Log($"<color=yellow>{unequipped.equipmentName}��(��) �����߽��ϴ�!</color>");
            }

            return unequipped;
        }

        private void ApplyEquipmentStats(EquipmentData equipment, bool isEquipping)
        {
            int multiplier = isEquipping ? 1 : -1;

            // ��� ���ʽ��� ����� ���� ���� ���
            switch (equipment.equipmentType)
            {
                case EquipmentType.Weapon:
                    playerStatus.AttackPower += equipment.GetFinalAttackPower() * multiplier;
                    break;

                case EquipmentType.Armor:
                    int hpBonus = equipment.GetFinalMaxHp() * multiplier;
                    playerStatus.MaxHp += hpBonus;
                    if (isEquipping)
                    {
                        playerStatus.CurrentHp += hpBonus; // ���� �� ���� ü�µ� ����
                    }
                    break;

                case EquipmentType.Ring:
                    playerStatus.HpRegen += equipment.GetFinalHpRegen() * multiplier;
                    break;
            }

            // ���� ���� �̺�Ʈ �߻�
            NotifyStatChanges();
        }

        private void NotifyStatChanges()
        {
            GameEventManager.TriggerPlayerStatChanged(Common.StatType.MaxHp, playerStatus.MaxHp);
            GameEventManager.TriggerPlayerStatChanged(Common.StatType.AttackPower, playerStatus.AttackPower);
            GameEventManager.TriggerPlayerStatChanged(Common.StatType.HpRegen, playerStatus.HpRegen);
        }

        private void UpdateTotalBonuses()
        {
            TotalAttackBonus = 0;
            TotalMaxHpBonus = 0;
            TotalHpRegenBonus = 0;

            foreach (var slot in equipmentSlots.Values)
            {
                if (!slot.IsEmpty)
                {
                    var equipment = slot.CurrentEquipment;
                    switch (equipment.equipmentType)
                    {
                        case EquipmentType.Weapon:
                            TotalAttackBonus += equipment.GetFinalAttackPower();
                            break;
                        case EquipmentType.Armor:
                            TotalMaxHpBonus += equipment.GetFinalMaxHp();
                            break;
                        case EquipmentType.Ring:
                            TotalHpRegenBonus += equipment.GetFinalHpRegen();
                            break;
                    }
                }
            }
        }

        // ��� ��ȸ
        public EquipmentData GetEquipment(EquipmentType slotType)
        {
            return equipmentSlots[slotType].CurrentEquipment;
        }

        public bool IsSlotEmpty(EquipmentType slotType)
        {
            return equipmentSlots[slotType].IsEmpty;
        }

        [Title("�����")]
        [Button("��� ��� ����", ButtonSizes.Large), GUIColor(0.8f, 0.8f, 0.3f)]
        public void UnequipAll()
        {
            foreach (var slotType in equipmentSlots.Keys)
            {
                UnequipItem(slotType);
            }
        }

        [Button("��� ���� ���", ButtonSizes.Large)]
        public void DebugEquipmentStatus()
        {
            Debug.Log("========== ��� ���� ==========");
            foreach (var kvp in equipmentSlots)
            {
                var slot = kvp.Value;
                if (slot.IsEmpty)
                {
                    Debug.Log($"{kvp.Key}: <color=gray>�������</color>");
                }
                else
                {
                    var eq = slot.CurrentEquipment;
                    var color = ColorUtility.ToHtmlStringRGB(RarityColors.GetRarityColor(eq.rarity));

                    string statInfo = "";
                    switch (eq.equipmentType)
                    {
                        case EquipmentType.Weapon:
                            statInfo = $"���ݷ� +{eq.GetFinalAttackPower()}";
                            break;
                        case EquipmentType.Armor:
                            statInfo = $"�ִ�ü�� +{eq.GetFinalMaxHp()}";
                            break;
                        case EquipmentType.Ring:
                            statInfo = $"ü��ȸ�� +{eq.GetFinalHpRegen():F1}/��";
                            break;
                    }

                    Debug.Log($"{kvp.Key}: <color=#{color}>[{eq.GetFullRarityName()}] {eq.equipmentName}</color> ({statInfo})");
                }
            }
            Debug.Log($"�� ���ݷ� ���ʽ�: <color=red>+{TotalAttackBonus}</color>");
            Debug.Log($"�� �ִ�ü�� ���ʽ�: <color=green>+{TotalMaxHpBonus}</color>");
            Debug.Log($"�� ü��ȸ�� ���ʽ�: <color=cyan>+{TotalHpRegenBonus:F1}/��</color>");
            Debug.Log("================================");
        }

        [Title("��޺� ���")]
        [ShowInInspector, ReadOnly]
        private Dictionary<EquipmentRarity, int> RarityCount
        {
            get
            {
                var count = new Dictionary<EquipmentRarity, int>();
                foreach (EquipmentRarity rarity in Enum.GetValues(typeof(EquipmentRarity)))
                {
                    count[rarity] = 0;
                }

                foreach (var slot in equipmentSlots.Values)
                {
                    if (!slot.IsEmpty)
                    {
                        count[slot.CurrentEquipment.rarity]++;
                    }
                }

                return count;
            }
        }
    }
}