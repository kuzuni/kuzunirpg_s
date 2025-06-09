// ===== EquipmentType.cs =====
using Sirenix.OdinInspector;
using System;
using UnityEngine;
// ===== EquipmentSlot.cs =====


[Serializable]
public class EquipmentSlot : IEquipmentSlot
{
    [ShowInInspector, ReadOnly]
    [PreviewField(50)]
    private Sprite SlotIcon => currentEquipment?.icon;

    [ShowInInspector, ReadOnly]
    private string SlotInfo => currentEquipment != null ? currentEquipment.equipmentName : "빈 슬롯";

    [SerializeField]
    private EquipmentType slotType;

    [SerializeField]
    private EquipmentData currentEquipment;

    public EquipmentData CurrentEquipment => currentEquipment;
    public bool IsEmpty => currentEquipment == null;
    public EquipmentType SlotType => slotType;

    public EquipmentSlot(EquipmentType type)
    {
        slotType = type;
        currentEquipment = null;
    }

    public bool Equip(EquipmentData equipment)
    {
        if (equipment == null || equipment.equipmentType != slotType)
            return false;

        currentEquipment = equipment;
        return true;
    }

    public EquipmentData Unequip()
    {
        var unequipped = currentEquipment;
        currentEquipment = null;
        return unequipped;
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