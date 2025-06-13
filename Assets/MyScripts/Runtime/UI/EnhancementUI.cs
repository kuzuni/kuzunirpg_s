// 메인 Enhancement UI 관리자
using RPG.Common;
using RPG.Core.Events;
using RPG.Enhancement;
using RPG.Managers;
using RPG.Player;
using RPG.UI.Enhancement;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System;
using UnityEngine;
using DG.Tweening;
public class EnhancementUI : MonoBehaviour
{
    [Title("슬롯 설정")]
    [SerializeField, Required]
    private Transform slotContainer;

    [SerializeField, Required]
    private GameObject enhancementSlotPrefab;

    [SerializeField, Required]
    private EnhancementStatConfig statConfig;

    [Title("비용 설정")]
    [SerializeField]
    private int baseCostGold = 1000;

    [SerializeField]
    private float costMultiplier = 1.5f;

    // 시스템 참조
    private EnhancementSystem enhancementSystem;
    private PlayerController playerController;
    private CurrencyManager currencyManager;

    // 슬롯 관리
    private Dictionary<StatType, EnhancementSlot> slots = new Dictionary<StatType, EnhancementSlot>();

    private void Start()
    {
        InitializeSystems();
        CreateSlots();
        SubscribeToEvents();
        RefreshAllSlots();
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    private void InitializeSystems()
    {
        playerController = FindObjectOfType<PlayerController>();
        if (playerController != null)
        {
            enhancementSystem = playerController.Enhancement;
        }

        currencyManager = FindObjectOfType<CurrencyManager>();
    }

    private void CreateSlots()
    {
        // 기존 슬롯 정리
        foreach (Transform child in slotContainer)
        {
            Destroy(child.gameObject);
        }
        slots.Clear();

        // 설정된 스탯들에 대해 슬롯 생성
        if (statConfig != null)
        {
            foreach (var statIcon in statConfig.statIcons)
            {
                CreateSlot(statIcon.statType, statIcon.icon);
            }
        }
        else
        {
            // 설정이 없으면 모든 스탯 타입에 대해 생성
            foreach (StatType statType in Enum.GetValues(typeof(StatType)))
            {
                CreateSlot(statType, null);
            }
        }
    }

    private void CreateSlot(StatType statType, Sprite icon)
    {
        GameObject slotObj = Instantiate(enhancementSlotPrefab, slotContainer);
        EnhancementSlot slot = slotObj.GetComponent<EnhancementSlot>();

        if (slot != null)
        {
            slot.Initialize(this, statType, icon);
            slots[statType] = slot;
            UpdateSlotData(slot);
        }
    }

    private void UpdateSlotData(EnhancementSlot slot)
    {
        if (enhancementSystem == null) return;

        var enhancementLevel = enhancementSystem.GetEnhancementLevel(slot.StatType);
        if (enhancementLevel != null)
        {
            slot.UpdateSlot(
                enhancementLevel.currentLevel,
                enhancementLevel.maxLevel,
                enhancementLevel.GetEnhancementValue(),
                enhancementLevel.isPercentage
            );

            // 비용 계산
            long cost = CalculateEnhanceCost(slot.StatType, enhancementLevel.currentLevel);
            bool canAfford = currencyManager != null && currencyManager.CanAfford(CurrencyType.Gold, cost);
            slot.UpdateCost(cost, canAfford);
        }
    }

    public void OnEnhanceRequested(StatType statType, long cost)
    {
        if (enhancementSystem == null || currencyManager == null) return;

        // 비용 지불
        if (!currencyManager.TrySpend(CurrencyType.Gold, cost))
        {
            ShowNotEnoughGoldMessage();
            return;
        }

        // 강화 실행
        bool success = enhancementSystem.EnhanceStat(statType);

        if (success && slots.ContainsKey(statType))
        {
            slots[statType].PlayEnhanceAnimation();
        }
    }

    private void SubscribeToEvents()
    {
        GameEventManager.OnStatEnhanced += OnStatEnhanced;
        GameEventManager.OnStatEnhancementMaxed += OnStatMaxed;
        GameEventManager.OnCurrencyChanged += OnCurrencyChanged;
    }

    private void UnsubscribeFromEvents()
    {
        GameEventManager.OnStatEnhanced -= OnStatEnhanced;
        GameEventManager.OnStatEnhancementMaxed -= OnStatMaxed;
        GameEventManager.OnCurrencyChanged -= OnCurrencyChanged;
    }

    private void OnStatEnhanced(StatType statType, int newLevel)
    {
        if (slots.ContainsKey(statType))
        {
            UpdateSlotData(slots[statType]);
        }
    }

    private void OnStatMaxed(StatType statType)
    {
        if (slots.ContainsKey(statType))
        {
            var slot = slots[statType];
            UpdateSlotData(slot);

            // MAX 달성 애니메이션
            // 기존 애니메이션 중지
            slot.transform.DOKill();
            slot.transform.localScale = Vector3.one;

            // 새 애니메이션 시작
            slot.transform.DOPunchScale(Vector3.one * 0.2f, 0.5f);
        }
    }

    private void OnCurrencyChanged(CurrencyType type, long amount)
    {
        if (type == CurrencyType.Gold)
        {
            RefreshAllSlots();
        }
    }

    private void RefreshAllSlots()
    {
        foreach (var slot in slots.Values)
        {
            UpdateSlotData(slot);
        }
    }

    private long CalculateEnhanceCost(StatType statType, int currentLevel)
    {
        return (long)(baseCostGold * Mathf.Pow(costMultiplier, currentLevel));
    }

    private void ShowNotEnoughGoldMessage()
    {
        Debug.Log("<color=red>골드가 부족합니다!</color>");
        // TODO: UI 알림 메시지 표시
    }

    [Title("디버그")]
    [Button("슬롯 재생성")]
    private void DebugRecreateSlots()
    {
        CreateSlots();
        RefreshAllSlots();
    }
}
