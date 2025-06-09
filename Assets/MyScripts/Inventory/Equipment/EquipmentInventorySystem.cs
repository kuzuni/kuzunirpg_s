using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Sirenix.OdinInspector;

public class EquipmentInventorySystem : BaseInventorySystem<EquipmentData, InventorySlot<EquipmentData>>
{
    [Title("장비 인벤토리 통계")]
    [ShowInInspector, ReadOnly]
    public override int TotalItems => inventory.Sum(slot => slot.quantity);

    [ShowInInspector, ReadOnly]
    [DictionaryDrawerSettings(KeyLabel = "등급", ValueLabel = "개수")]
    private Dictionary<EquipmentRarity, int> InventoryByRarity
    {
        get
        {
            var result = new Dictionary<EquipmentRarity, int>();
            foreach (EquipmentRarity rarity in System.Enum.GetValues(typeof(EquipmentRarity)))
            {
                result[rarity] = 0;
            }

            foreach (var slot in inventory)
            {
                if (slot.item != null)
                {
                    result[slot.item.rarity] += slot.quantity;
                }
            }

            return result;
        }
    }

    [ShowInInspector, ReadOnly]
    [DictionaryDrawerSettings(KeyLabel = "타입", ValueLabel = "개수")]
    private Dictionary<EquipmentType, int> InventoryByType
    {
        get
        {
            var result = new Dictionary<EquipmentType, int>();
            foreach (EquipmentType type in System.Enum.GetValues(typeof(EquipmentType)))
            {
                result[type] = 0;
            }

            foreach (var slot in inventory)
            {
                if (slot.item != null)
                {
                    result[slot.item.equipmentType] += slot.quantity;
                }
            }

            return result;
        }
    }

    // BaseInventorySystem 추상 메서드 구현
    protected override EquipmentData GetItemFromSlot(InventorySlot<EquipmentData> slot)
    {
        return slot?.item;
    }

    protected override void SetItemToSlot(InventorySlot<EquipmentData> slot, EquipmentData item)
    {
        if (slot != null)
            slot.item = item;
    }

    protected override int GetSlotQuantity(InventorySlot<EquipmentData> slot)
    {
        return slot?.quantity ?? 0;
    }

    protected override void SetSlotQuantity(InventorySlot<EquipmentData> slot, int quantity)
    {
        if (slot != null)
            slot.quantity = quantity;
    }

    protected override InventorySlot<EquipmentData> CreateNewSlot(EquipmentData item, int quantity)
    {
        return new InventorySlot<EquipmentData>(item, quantity);
    }

    protected override bool IsSameItem(EquipmentData item1, EquipmentData item2)
    {
        // 같은 장비인지 확인 (이름과 세부등급이 같으면 스택 가능)
        return item1.name == item2.name && item1.subGrade == item2.subGrade;
    }

    public override int GetTotalItemCount()
    {
        return TotalItems;
    }

    public override void SortInventory()
    {
        inventory = inventory
            .OrderByDescending(s => s.item.rarity)
            .ThenByDescending(s => s.item.subGrade)
            .ThenBy(s => s.item.equipmentType)
            .ThenBy(s => s.item.equipmentName)
            .ToList();

        Debug.Log("장비 인벤토리를 정렬했습니다.");
    }

    protected override void LogItemAdded(EquipmentData item, int quantity)
    {
        var color = ColorUtility.ToHtmlStringRGB(item.GetRarityColor());
        Debug.Log($"<color=#{color}>{item.GetFullRarityName()} {item.equipmentName} x{quantity}을(를) 획득했습니다!</color>");
    }

    public override void ShowInventoryStatus()
    {
        Debug.Log($"========== 장비 인벤토리 상태 ==========");
        Debug.Log($"보유 중: {UniqueItems}종류, 총 {TotalItems}개");

        foreach (var kvp in InventoryByRarity)
        {
            if (kvp.Value > 0)
            {
                var color = ColorUtility.ToHtmlStringRGB(RarityColors.GetRarityColor(kvp.Key));
                Debug.Log($"<color=#{color}>{RarityColors.GetRarityName(kvp.Key)}: {kvp.Value}개</color>");
            }
        }
        Debug.Log("====================================");
    }

    // 장비 전용 메서드들
    public List<EquipmentData> GetEquipmentsByType(EquipmentType type)
    {
        return inventory
            .Where(slot => slot.item != null && slot.item.equipmentType == type)
            .Select(slot => slot.item)
            .ToList();
    }

    public List<EquipmentData> GetEquipmentsByRarity(EquipmentRarity rarity)
    {
        return inventory
            .Where(slot => slot.item != null && slot.item.rarity == rarity)
            .Select(slot => slot.item)
            .ToList();
    }

    [Button("낮은 등급 일괄 판매", ButtonSizes.Large)]
    [ButtonGroup("Management")]
    [GUIColor(0.8f, 0.8f, 0.3f)]
    private void SellLowRarityItems()
    {
        var itemsToSell = inventory
            .Where(slot => slot.item != null && slot.item.rarity <= EquipmentRarity.Uncommon)
            .ToList();

        int totalGold = 0;
        foreach (var slot in itemsToSell)
        {
            totalGold += slot.item.sellPrice * slot.quantity;
            inventory.Remove(slot);
        }

        Debug.Log($"<color=yellow>{itemsToSell.Count}개 아이템을 판매하여 {totalGold} 골드를 획득했습니다!</color>");
    }
}