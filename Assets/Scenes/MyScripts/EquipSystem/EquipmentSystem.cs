using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections;

public class EquipmentSystem : MonoBehaviour
{
    [Title("장비 슬롯")]
    [DictionaryDrawerSettings(KeyLabel = "슬롯 타입", ValueLabel = "장착 장비")]
    [ShowInInspector]
    private Dictionary<EquipmentType, EquipmentSlot> equipmentSlots;

    [Title("장비 효과")]
    [ShowInInspector, ReadOnly]
    [ProgressBar(0, 200, 0.8f, 0.3f, 0.3f)]
    private int TotalAttackBonus { get; set; }

    [ShowInInspector, ReadOnly]
    [ProgressBar(0, 1000, 0.3f, 0.8f, 0.3f)]
    private int TotalMaxHpBonus { get; set; }

    [ShowInInspector, ReadOnly]
    [ProgressBar(0, 20, 0.3f, 0.8f, 0.8f)]
    private float TotalHpRegenBonus { get; set; }

    [Title("장착 장비 상세")]
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
                    info.Add($"<color=#{rarityColor}>[{eq.GetFullRarityName()}] {eq.equipmentName}</color>");
                }
            }
            return info;
        }
    }

    // 참조
    private PlayerStatus playerStatus;
    private InventorySystem inventorySystem;
    private bool isInitialized = false;

    // 이벤트
    public event Action<EquipmentData, EquipmentType> OnEquipmentChanged;
    public event Action<EquipmentType> OnEquipmentRemoved;

    void Awake()
    {
        // 인벤토리 시스템 찾기
        inventorySystem = GetComponent<InventorySystem>();
        if (inventorySystem == null)
        {
            inventorySystem = FindObjectOfType<InventorySystem>();
        }

        // 잠시 대기 후 설정 (인벤토리가 먼저 초기화되도록)
        StartCoroutine(SetupReferences());
    }

    IEnumerator SetupReferences()
    {
        yield return null; // 한 프레임 대기

        if (inventorySystem != null)
        {
            EquipmentSlot.SetInventoryReference(inventorySystem);
            EquipmentSlot.SetEquipmentSystemReference(this);
            Debug.Log("EquipmentSlot 참조 설정 완료");
        }
        else
        {
            Debug.LogError("InventorySystem을 찾을 수 없습니다!");
        }
    }

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

    // Inspector에서 슬롯 변경 시 처리하는 메서드
    public void HandleSlotChange(EquipmentType slotType, EquipmentData newEquipment)
    {
        if (!isInitialized || inventorySystem == null)
        {
            Debug.LogWarning("시스템이 초기화되지 않았거나 인벤토리가 없습니다.");
            return;
        }

        var slot = equipmentSlots[slotType];
        var previousEquipment = slot.CurrentEquipment;

        // 이전 장비가 있었다면 인벤토리로 반환
        if (previousEquipment != null && previousEquipment != newEquipment)
        {
            ApplyEquipmentStats(previousEquipment, false);
            inventorySystem.AddItem(previousEquipment);
        }

        // 새 장비 장착
        if (newEquipment != null)
        {
            ApplyEquipmentStats(newEquipment, true);
            inventorySystem.RemoveItem(newEquipment, 1);
            OnEquipmentChanged?.Invoke(newEquipment, slotType);

            var color = ColorUtility.ToHtmlStringRGB(RarityColors.GetRarityColor(newEquipment.rarity));
            Debug.Log($"<color=#{color}>[{newEquipment.GetFullRarityName()}] {newEquipment.equipmentName}을(를) 장착했습니다!</color>");
        }
        else
        {
            OnEquipmentRemoved?.Invoke(slotType);
            Debug.Log($"{slotType} 슬롯을 비웠습니다.");
        }

        UpdateTotalBonuses();
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

            var color = ColorUtility.ToHtmlStringRGB(RarityColors.GetRarityColor(equipment.rarity));
            Debug.Log($"<color=#{color}>[{equipment.GetFullRarityName()}] {equipment.equipmentName}을(를) 장착했습니다!</color>");
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

    // 장비 장착 시 인벤토리에서 제거하는 메서드
    public bool EquipItemFromInventory(EquipmentData equipment)
    {
        if (!isInitialized || equipment == null || inventorySystem == null)
        {
            Debug.LogError("시스템이 초기화되지 않았거나 장비/인벤토리가 null입니다.");
            return false;
        }

        var slot = equipmentSlots[equipment.equipmentType];

        // 이미 장착된 장비가 있으면 인벤토리로 반환
        if (!slot.IsEmpty)
        {
            var unequipped = slot.CurrentEquipment;
            inventorySystem.AddItem(unequipped);
        }

        // 새 장비 장착
        if (slot.Equip(equipment))
        {
            ApplyEquipmentStats(equipment, true);
            OnEquipmentChanged?.Invoke(equipment, equipment.equipmentType);
            UpdateTotalBonuses();

            // 인벤토리에서 제거
            inventorySystem.RemoveItem(equipment, 1);

            var color = ColorUtility.ToHtmlStringRGB(RarityColors.GetRarityColor(equipment.rarity));
            Debug.Log($"<color=#{color}>[{equipment.GetFullRarityName()}] {equipment.equipmentName}을(를) 장착했습니다!</color>");
            return true;
        }

        return false;
    }

    // 장비 해제 시 인벤토리로 반환하는 메서드
    public EquipmentData UnequipItemToInventory(EquipmentType slotType)
    {
        if (!isInitialized || inventorySystem == null)
        {
            Debug.LogError("시스템이 초기화되지 않았거나 인벤토리가 null입니다.");
            return null;
        }

        var slot = equipmentSlots[slotType];
        var unequipped = slot.Unequip();

        if (unequipped != null)
        {
            ApplyEquipmentStats(unequipped, false);
            OnEquipmentRemoved?.Invoke(slotType);
            UpdateTotalBonuses();

            // 인벤토리로 반환
            inventorySystem.AddItem(unequipped);

            Debug.Log($"<color=yellow>{unequipped.equipmentName}을(를) 해제하고 인벤토리로 반환했습니다!</color>");
        }

        return unequipped;
    }

    private void ApplyEquipmentStats(EquipmentData equipment, bool isEquipping)
    {
        int multiplier = isEquipping ? 1 : -1;

        // 등급 보너스가 적용된 최종 스탯 사용
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
                    playerStatus.CurrentHp += hpBonus; // 장착 시 현재 체력도 증가
                }
                break;

            case EquipmentType.Ring:
                playerStatus.HpRegen += equipment.GetFinalHpRegen() * multiplier;
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
                var color = ColorUtility.ToHtmlStringRGB(RarityColors.GetRarityColor(eq.rarity));

                string statInfo = "";
                switch (eq.equipmentType)
                {
                    case EquipmentType.Weapon:
                        statInfo = $"공격력 +{eq.GetFinalAttackPower()}";
                        break;
                    case EquipmentType.Armor:
                        statInfo = $"최대체력 +{eq.GetFinalMaxHp()}";
                        break;
                    case EquipmentType.Ring:
                        statInfo = $"체력회복 +{eq.GetFinalHpRegen():F1}/초";
                        break;
                }

                Debug.Log($"{kvp.Key}: <color=#{color}>[{eq.GetFullRarityName()}] {eq.equipmentName}</color> ({statInfo})");
            }
        }
        Debug.Log($"총 공격력 보너스: <color=red>+{TotalAttackBonus}</color>");
        Debug.Log($"총 최대체력 보너스: <color=green>+{TotalMaxHpBonus}</color>");
        Debug.Log($"총 체력회복 보너스: <color=cyan>+{TotalHpRegenBonus:F1}/초</color>");
        Debug.Log("================================");
    }

    [Title("등급별 통계")]
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