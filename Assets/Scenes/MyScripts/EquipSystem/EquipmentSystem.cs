// ===== EquipmentType.cs =====
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

// ===== EquipmentSystem.cs =====


public class EquipmentSystem : MonoBehaviour
{
    [Title("장비 슬롯")]
    [DictionaryDrawerSettings(KeyLabel = "슬롯 타입", ValueLabel = "장착 장비")]
    [ShowInInspector]
    private Dictionary<EquipmentType, EquipmentSlot> equipmentSlots;

    [Title("장비 효과")]
    [ShowInInspector, ReadOnly]
    [ProgressBar(0, 100, 0.8f, 0.3f, 0.3f)]
    private int TotalAttackBonus { get; set; }

    [ShowInInspector, ReadOnly]
    [ProgressBar(0, 500, 0.3f, 0.8f, 0.3f)]
    private int TotalMaxHpBonus { get; set; }

    [ShowInInspector, ReadOnly]
    [ProgressBar(0, 10, 0.3f, 0.8f, 0.8f)]
    private float TotalHpRegenBonus { get; set; }

    // 참조
    private PlayerStatus playerStatus;
    private bool isInitialized = false;

    // 이벤트
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

    [Title("장비 관리")]
    [Button("장비 장착", ButtonSizes.Large), GUIColor(0.3f, 0.8f, 0.3f)]
    public bool EquipItem(EquipmentData equipment)
    {
        if (!isInitialized || equipment == null)
        {
            Debug.LogError("시스템이 초기화되지 않았거나 장비가 null입니다.");
            return false;
        }

        var slot = equipmentSlots[equipment.equipmentType];

        // 이미 장착된 장비가 있으면 해제
        if (!slot.IsEmpty)
        {
            UnequipItem(equipment.equipmentType);
        }

        // 새 장비 장착
        if (slot.Equip(equipment))
        {
            ApplyEquipmentStats(equipment, true);
            OnEquipmentChanged?.Invoke(equipment, equipment.equipmentType);
            UpdateTotalBonuses();

            Debug.Log($"<color=green>{equipment.equipmentName}을(를) 장착했습니다!</color>");
            return true;
        }

        return false;
    }

    [Button("장비 해제", ButtonSizes.Large), GUIColor(0.8f, 0.3f, 0.3f)]
    public EquipmentData UnequipItem(EquipmentType slotType)
    {
        if (!isInitialized)
        {
            Debug.LogError("시스템이 초기화되지 않았습니다.");
            return null;
        }

        var slot = equipmentSlots[slotType];
        var unequipped = slot.Unequip();

        if (unequipped != null)
        {
            ApplyEquipmentStats(unequipped, false);
            OnEquipmentRemoved?.Invoke(slotType);
            UpdateTotalBonuses();

            Debug.Log($"<color=yellow>{unequipped.equipmentName}을(를) 해제했습니다!</color>");
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
                    playerStatus.CurrentHp += equipment.maxHpBonus; // 장착 시 현재 체력도 증가
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

    // 장비 조회
    public EquipmentData GetEquipment(EquipmentType slotType)
    {
        return equipmentSlots[slotType].CurrentEquipment;
    }

    public bool IsSlotEmpty(EquipmentType slotType)
    {
        return equipmentSlots[slotType].IsEmpty;
    }

    [Title("디버그")]
    [Button("모든 장비 해제", ButtonSizes.Large), GUIColor(0.8f, 0.8f, 0.3f)]
    public void UnequipAll()
    {
        foreach (var slotType in equipmentSlots.Keys)
        {
            UnequipItem(slotType);
        }
    }

    [Button("장비 상태 출력", ButtonSizes.Large)]
    public void DebugEquipmentStatus()
    {
        Debug.Log("========== 장비 상태 ==========");
        foreach (var kvp in equipmentSlots)
        {
            var slot = kvp.Value;
            if (slot.IsEmpty)
            {
                Debug.Log($"{kvp.Key}: <color=gray>비어있음</color>");
            }
            else
            {
                var eq = slot.CurrentEquipment;
                Debug.Log($"{kvp.Key}: <color=cyan>{eq.equipmentName}</color>");
            }
        }
        Debug.Log($"총 공격력 보너스: <color=red>+{TotalAttackBonus}</color>");
        Debug.Log($"총 최대체력 보너스: <color=green>+{TotalMaxHpBonus}</color>");
        Debug.Log($"총 체력회복 보너스: <color=cyan>+{TotalHpRegenBonus}/초</color>");
        Debug.Log("================================");
    }
}

// ===== PlayerManager.cs 수정 =====
// PlayerManager의 Awake 메서드에 추가:
/*
private EquipmentSystem equipmentSystem;
public EquipmentSystem Equipment => equipmentSystem;

void Awake()
{
    // 기존 코드...
    
    // 장비 시스템 추가
    equipmentSystem = gameObject.AddComponent<EquipmentSystem>();
    equipmentSystem.Initialize(playerStatus);
}
*/