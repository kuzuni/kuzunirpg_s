// ===== EquipmentType.cs =====
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

// ===== EquipmentSystem.cs =====


public class EquipmentSystem : MonoBehaviour
{
    [Title("��� ����")]
    [DictionaryDrawerSettings(KeyLabel = "���� Ÿ��", ValueLabel = "���� ���")]
    [ShowInInspector]
    private Dictionary<EquipmentType, EquipmentSlot> equipmentSlots;

    [Title("��� ȿ��")]
    [ShowInInspector, ReadOnly]
    [ProgressBar(0, 100, 0.8f, 0.3f, 0.3f)]
    private int TotalAttackBonus { get; set; }

    [ShowInInspector, ReadOnly]
    [ProgressBar(0, 500, 0.3f, 0.8f, 0.3f)]
    private int TotalMaxHpBonus { get; set; }

    [ShowInInspector, ReadOnly]
    [ProgressBar(0, 10, 0.3f, 0.8f, 0.8f)]
    private float TotalHpRegenBonus { get; set; }

    // ����
    private PlayerStatus playerStatus;
    private bool isInitialized = false;

    // �̺�Ʈ
    public event Action<EquipmentData, EquipmentType> OnEquipmentChanged;
    public event Action<EquipmentType> OnEquipmentRemoved;

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
            OnEquipmentChanged?.Invoke(equipment, equipment.equipmentType);
            UpdateTotalBonuses();

            Debug.Log($"<color=green>{equipment.equipmentName}��(��) �����߽��ϴ�!</color>");
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
            OnEquipmentRemoved?.Invoke(slotType);
            UpdateTotalBonuses();

            Debug.Log($"<color=yellow>{unequipped.equipmentName}��(��) �����߽��ϴ�!</color>");
        }

        return unequipped;
    }

    private void ApplyEquipmentStats(EquipmentData equipment, bool isEquipping)
    {
        int multiplier = isEquipping ? 1 : -1;

        switch (equipment.equipmentType)
        {
            case EquipmentType.Weapon:
                playerStatus.AttackPower += equipment.attackPowerBonus * multiplier;
                break;

            case EquipmentType.Armor:
                playerStatus.MaxHp += equipment.maxHpBonus * multiplier;
                if (isEquipping)
                {
                    playerStatus.CurrentHp += equipment.maxHpBonus; // ���� �� ���� ü�µ� ����
                }
                break;

            case EquipmentType.Ring:
                playerStatus.HpRegen += equipment.hpRegenBonus * multiplier;
                break;
        }
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
                        TotalAttackBonus += equipment.attackPowerBonus;
                        break;
                    case EquipmentType.Armor:
                        TotalMaxHpBonus += equipment.maxHpBonus;
                        break;
                    case EquipmentType.Ring:
                        TotalHpRegenBonus += equipment.hpRegenBonus;
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
                Debug.Log($"{kvp.Key}: <color=cyan>{eq.equipmentName}</color>");
            }
        }
        Debug.Log($"�� ���ݷ� ���ʽ�: <color=red>+{TotalAttackBonus}</color>");
        Debug.Log($"�� �ִ�ü�� ���ʽ�: <color=green>+{TotalMaxHpBonus}</color>");
        Debug.Log($"�� ü��ȸ�� ���ʽ�: <color=cyan>+{TotalHpRegenBonus}/��</color>");
        Debug.Log("================================");
    }
}

// ===== PlayerManager.cs ���� =====
// PlayerManager�� Awake �޼��忡 �߰�:
/*
private EquipmentSystem equipmentSystem;
public EquipmentSystem Equipment => equipmentSystem;

void Awake()
{
    // ���� �ڵ�...
    
    // ��� �ý��� �߰�
    equipmentSystem = gameObject.AddComponent<EquipmentSystem>();
    equipmentSystem.Initialize(playerStatus);
}
*/