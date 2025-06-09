// ===== InventorySystem.cs =====
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Sirenix.OdinInspector;

[System.Serializable]
public class InventorySlot
{
    public EquipmentData equipment;
    public int quantity = 1;

    public InventorySlot(EquipmentData equipment, int quantity = 1)
    {
        this.equipment = equipment;
        this.quantity = quantity;
    }
}

public class InventorySystem : MonoBehaviour
{
    [Title("인벤토리 내용")]
    [SerializeField]
    [ListDrawerSettings(ShowFoldout = true, ShowPaging = true, NumberOfItemsPerPage = 10)]
    private List<InventorySlot> inventory = new List<InventorySlot>();

    [Title("인벤토리 통계")]
    [ShowInInspector, ReadOnly]
    private int TotalItems => inventory.Sum(slot => slot.quantity);

    [ShowInInspector, ReadOnly]
    private int UniqueItems => inventory.Count;

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
                if (slot.equipment != null)
                {
                    result[slot.equipment.rarity] += slot.quantity;
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
                if (slot.equipment != null)
                {
                    result[slot.equipment.equipmentType] += slot.quantity;
                }
            }

            return result;
        }
    }

    // 이벤트
    public event System.Action<EquipmentData> OnItemAdded;
    public event System.Action<EquipmentData> OnItemRemoved;

    // 아이템 추가
    public bool AddItem(EquipmentData equipment)
    {
        if (equipment == null) return false;

        // 같은 장비가 있는지 확인 (스택 가능한 경우)
        var existingSlot = inventory.FirstOrDefault(slot =>
            slot.equipment.name == equipment.name &&
            slot.equipment.subGrade == equipment.subGrade);

        if (existingSlot != null)
        {
            existingSlot.quantity++;
        }
        else
        {
            inventory.Add(new InventorySlot(equipment));
        }

        OnItemAdded?.Invoke(equipment);
        Debug.Log($"<color=green>{equipment.GetFullRarityName()} {equipment.equipmentName}을(를) 인벤토리에 추가했습니다!</color>");
        return true;
    }

    // 여러 아이템 추가
    public int AddItems(List<EquipmentData> equipments)
    {
        int addedCount = 0;
        foreach (var equipment in equipments)
        {
            if (AddItem(equipment))
            {
                addedCount++;
            }
        }
        return addedCount;
    }

    // 아이템 제거
    public bool RemoveItem(EquipmentData equipment, int quantity = 1)
    {
        var slot = inventory.FirstOrDefault(s => s.equipment == equipment);
        if (slot == null) return false;

        if (slot.quantity > quantity)
        {
            slot.quantity -= quantity;
        }
        else
        {
            inventory.Remove(slot);
        }

        OnItemRemoved?.Invoke(equipment);
        return true;
    }

    // 특정 타입의 장비 목록 가져오기
    public List<EquipmentData> GetEquipmentsByType(EquipmentType type)
    {
        return inventory
            .Where(slot => slot.equipment.equipmentType == type)
            .Select(slot => slot.equipment)
            .ToList();
    }

    // 특정 등급의 장비 목록 가져오기
    public List<EquipmentData> GetEquipmentsByRarity(EquipmentRarity rarity)
    {
        return inventory
            .Where(slot => slot.equipment.rarity == rarity)
            .Select(slot => slot.equipment)
            .ToList();
    }

    [Title("인벤토리 관리")]
    [Button("인벤토리 정렬", ButtonSizes.Large)]
    [ButtonGroup("Management")]
    private void SortInventory()
    {
        inventory = inventory
            .OrderByDescending(s => s.equipment.rarity)
            .ThenByDescending(s => s.equipment.subGrade)
            .ThenBy(s => s.equipment.equipmentType)
            .ThenBy(s => s.equipment.equipmentName)
            .ToList();

        Debug.Log("인벤토리를 정렬했습니다.");
    }

    [Button("낮은 등급 일괄 판매", ButtonSizes.Large)]
    [ButtonGroup("Management")]
    [GUIColor(0.8f, 0.8f, 0.3f)]
    private void SellLowRarityItems()
    {
        var itemsToSell = inventory
            .Where(slot => slot.equipment.rarity <= EquipmentRarity.Uncommon)
            .ToList();

        int totalGold = 0;
        foreach (var slot in itemsToSell)
        {
            totalGold += slot.equipment.sellPrice * slot.quantity;
            inventory.Remove(slot);
        }

        Debug.Log($"<color=yellow>{itemsToSell.Count}개 아이템을 판매하여 {totalGold} 골드를 획득했습니다!</color>");
    }

    [Button("인벤토리 상태", ButtonSizes.Medium)]
    [ButtonGroup("Debug")]
    private void ShowInventoryStatus()
    {
        Debug.Log($"========== 인벤토리 상태 ==========");
        Debug.Log($"보유 중: {UniqueItems}종류, 총 {TotalItems}개");

        foreach (var kvp in InventoryByRarity)
        {
            if (kvp.Value > 0)
            {
                var color = ColorUtility.ToHtmlStringRGB(RarityColors.GetRarityColor(kvp.Key));
                Debug.Log($"<color=#{color}>{RarityColors.GetRarityName(kvp.Key)}: {kvp.Value}개</color>");
            }
        }
        Debug.Log("===================================");
    }

    [Button("인벤토리 초기화", ButtonSizes.Medium)]
    [ButtonGroup("Debug")]
    [GUIColor(0.8f, 0.3f, 0.3f)]
    private void ClearInventory()
    {
        inventory.Clear();
        Debug.Log("인벤토리를 초기화했습니다.");
    }
}
