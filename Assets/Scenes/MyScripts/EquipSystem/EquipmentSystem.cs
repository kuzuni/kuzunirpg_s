using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections;

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
                    info.Add($"<color=#{rarityColor}>[{eq.GetFullRarityName()}] {eq.equipmentName}</color>");
                }
            }
            return info;
        }
    }

    // ����
    private PlayerStatus playerStatus;
    private InventorySystem inventorySystem;
    private bool isInitialized = false;

    // �̺�Ʈ
    public event Action<EquipmentData, EquipmentType> OnEquipmentChanged;
    public event Action<EquipmentType> OnEquipmentRemoved;

    void Awake()
    {
        // �κ��丮 �ý��� ã��
        inventorySystem = GetComponent<InventorySystem>();
        if (inventorySystem == null)
        {
            inventorySystem = FindObjectOfType<InventorySystem>();
        }

        // ��� ��� �� ���� (�κ��丮�� ���� �ʱ�ȭ�ǵ���)
        StartCoroutine(SetupReferences());
    }

    IEnumerator SetupReferences()
    {
        yield return null; // �� ������ ���

        if (inventorySystem != null)
        {
            EquipmentSlot.SetInventoryReference(inventorySystem);
            EquipmentSlot.SetEquipmentSystemReference(this);
            Debug.Log("EquipmentSlot ���� ���� �Ϸ�");
        }
        else
        {
            Debug.LogError("InventorySystem�� ã�� �� �����ϴ�!");
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

    // Inspector���� ���� ���� �� ó���ϴ� �޼���
    public void HandleSlotChange(EquipmentType slotType, EquipmentData newEquipment)
    {
        if (!isInitialized || inventorySystem == null)
        {
            Debug.LogWarning("�ý����� �ʱ�ȭ���� �ʾҰų� �κ��丮�� �����ϴ�.");
            return;
        }

        var slot = equipmentSlots[slotType];
        var previousEquipment = slot.CurrentEquipment;

        // ���� ��� �־��ٸ� �κ��丮�� ��ȯ
        if (previousEquipment != null && previousEquipment != newEquipment)
        {
            ApplyEquipmentStats(previousEquipment, false);
            inventorySystem.AddItem(previousEquipment);
        }

        // �� ��� ����
        if (newEquipment != null)
        {
            ApplyEquipmentStats(newEquipment, true);
            inventorySystem.RemoveItem(newEquipment, 1);
            OnEquipmentChanged?.Invoke(newEquipment, slotType);

            var color = ColorUtility.ToHtmlStringRGB(RarityColors.GetRarityColor(newEquipment.rarity));
            Debug.Log($"<color=#{color}>[{newEquipment.GetFullRarityName()}] {newEquipment.equipmentName}��(��) �����߽��ϴ�!</color>");
        }
        else
        {
            OnEquipmentRemoved?.Invoke(slotType);
            Debug.Log($"{slotType} ������ ������ϴ�.");
        }

        UpdateTotalBonuses();
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
            OnEquipmentRemoved?.Invoke(slotType);
            UpdateTotalBonuses();

            Debug.Log($"<color=yellow>{unequipped.equipmentName}��(��) �����߽��ϴ�!</color>");
        }

        return unequipped;
    }

    // ��� ���� �� �κ��丮���� �����ϴ� �޼���
    public bool EquipItemFromInventory(EquipmentData equipment)
    {
        if (!isInitialized || equipment == null || inventorySystem == null)
        {
            Debug.LogError("�ý����� �ʱ�ȭ���� �ʾҰų� ���/�κ��丮�� null�Դϴ�.");
            return false;
        }

        var slot = equipmentSlots[equipment.equipmentType];

        // �̹� ������ ��� ������ �κ��丮�� ��ȯ
        if (!slot.IsEmpty)
        {
            var unequipped = slot.CurrentEquipment;
            inventorySystem.AddItem(unequipped);
        }

        // �� ��� ����
        if (slot.Equip(equipment))
        {
            ApplyEquipmentStats(equipment, true);
            OnEquipmentChanged?.Invoke(equipment, equipment.equipmentType);
            UpdateTotalBonuses();

            // �κ��丮���� ����
            inventorySystem.RemoveItem(equipment, 1);

            var color = ColorUtility.ToHtmlStringRGB(RarityColors.GetRarityColor(equipment.rarity));
            Debug.Log($"<color=#{color}>[{equipment.GetFullRarityName()}] {equipment.equipmentName}��(��) �����߽��ϴ�!</color>");
            return true;
        }

        return false;
    }

    // ��� ���� �� �κ��丮�� ��ȯ�ϴ� �޼���
    public EquipmentData UnequipItemToInventory(EquipmentType slotType)
    {
        if (!isInitialized || inventorySystem == null)
        {
            Debug.LogError("�ý����� �ʱ�ȭ���� �ʾҰų� �κ��丮�� null�Դϴ�.");
            return null;
        }

        var slot = equipmentSlots[slotType];
        var unequipped = slot.Unequip();

        if (unequipped != null)
        {
            ApplyEquipmentStats(unequipped, false);
            OnEquipmentRemoved?.Invoke(slotType);
            UpdateTotalBonuses();

            // �κ��丮�� ��ȯ
            inventorySystem.AddItem(unequipped);

            Debug.Log($"<color=yellow>{unequipped.equipmentName}��(��) �����ϰ� �κ��丮�� ��ȯ�߽��ϴ�!</color>");
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