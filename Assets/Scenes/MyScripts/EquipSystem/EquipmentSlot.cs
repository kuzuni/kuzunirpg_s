using System;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public class EquipmentSlot : IEquipmentSlot
{
    [ShowInInspector, ReadOnly]
    [PreviewField(50)]
    private Sprite SlotIcon => currentEquipment?.icon;

    [ShowInInspector, ReadOnly]
    private string SlotInfo => currentEquipment != null ?
        $"{currentEquipment.GetFullRarityName()} {currentEquipment.equipmentName}" : "빈 슬롯";

    [SerializeField, ReadOnly]
    [LabelText("슬롯 타입")]
    private EquipmentType slotType;

    [SerializeField]
    [ValueDropdown("GetAvailableEquipments")]
    [LabelText("장착 장비")]
    [OnValueChanged("OnEquipmentChanged")]
    private EquipmentData currentEquipment;

    public EquipmentData CurrentEquipment => currentEquipment;
    public bool IsEmpty => currentEquipment == null;
    public EquipmentType SlotType => slotType;

    // 인벤토리 참조를 위한 정적 변수
    private static InventorySystem inventoryReference;
    private static EquipmentSystem equipmentSystemReference;

    public EquipmentSlot(EquipmentType type)
    {
        slotType = type;
        currentEquipment = null;
    }

    // 인벤토리 설정 메서드
    public static void SetInventoryReference(InventorySystem inventory)
    {
        inventoryReference = inventory;
        Debug.Log($"InventoryReference 설정됨: {inventory != null}");
    }

    // EquipmentSystem 설정 메서드
    public static void SetEquipmentSystemReference(EquipmentSystem equipmentSystem)
    {
        equipmentSystemReference = equipmentSystem;
        Debug.Log($"EquipmentSystemReference 설정됨: {equipmentSystem != null}");
    }

    // 장비 변경 시 호출되는 메서드
    private void OnEquipmentChanged()
    {
        // Inspector에서 직접 변경한 경우 처리
        if (Application.isPlaying && equipmentSystemReference != null)
        {
            equipmentSystemReference.HandleSlotChange(slotType, currentEquipment);
        }
    }

    // 드롭다운에 표시할 장비 목록
    private ValueDropdownList<EquipmentData> GetAvailableEquipments()
    {
        var items = new ValueDropdownList<EquipmentData>();

        // 빈 슬롯 옵션
        items.Add("없음", null);

        // 디버그 로그
        Debug.Log($"GetAvailableEquipments 호출 - SlotType: {slotType}, InventoryRef: {inventoryReference != null}");

        // 인벤토리에서 해당 타입의 장비 가져오기
        if (inventoryReference != null)
        {
            var equipments = inventoryReference.GetEquipmentsByType(slotType);
            Debug.Log($"{slotType} 타입 장비 개수: {equipments.Count}");

            // 등급별로 정렬
            var sortedEquipments = equipments
                .OrderByDescending(e => e.rarity)
                .ThenByDescending(e => e.subGrade)
                .ThenBy(e => e.equipmentName);

            foreach (var equipment in sortedEquipments)
            {
                var displayName = $"[{equipment.GetFullRarityName()}] {equipment.equipmentName}";
                items.Add(displayName, equipment);
                Debug.Log($"드롭다운에 추가: {displayName}");
            }
        }
        else
        {
            Debug.LogWarning("InventoryReference가 null입니다!");
        }

        return items;
    }

    public bool Equip(EquipmentData equipment)
    {
        if (equipment != null && equipment.equipmentType != slotType)
        {
            Debug.LogError($"장비 타입 불일치! 슬롯: {slotType}, 장비: {equipment.equipmentType}");
            return false;
        }

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

