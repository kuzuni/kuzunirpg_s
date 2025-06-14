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
using TMPro;

namespace RPG.UI.Enhancement
{
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

        [Title("강화 모드 설정")]
        [SerializeField, Required]
        private GameObject enhanceModeContainer;

        [SerializeField, Required]
        private UnityEngine.UI.Button mode1xButton;

        [SerializeField, Required]
        private UnityEngine.UI.Button mode10xButton;

        [SerializeField, Required]
        private UnityEngine.UI.Button mode100xButton;

        [Title("모드별 색상 설정")]
        [SerializeField]
        private Color mode1xColor = new Color(0.3f, 0.69f, 0.31f); // 녹색

        [SerializeField]
        private Color mode10xColor = new Color(1f, 0.76f, 0.03f); // 주황색

        [SerializeField]
        private Color mode100xColor = new Color(1f, 0.09f, 0.27f); // 빨간색

        [SerializeField]
        private Color normalModeColor = Color.white;

        [SerializeField]
        private Color normalTextColor = Color.black;

        [SerializeField]
        private Color selectedTextColor = Color.white;

        // 강화 모드 enum
        public enum EnhanceMode
        {
            x1 = 1,
            x10 = 10,
            x100 = 100
        }

        [ShowInInspector, ReadOnly]
        private EnhanceMode currentMode = EnhanceMode.x1;

        // 시스템 참조
        private PlayerEnhancementSystem enhancementSystem;
        private PlayerController playerController;
        private CurrencyManager currencyManager;

        // 슬롯 관리
        private Dictionary<StatType, EnhancementSlot> slots = new Dictionary<StatType, EnhancementSlot>();

        private void Start()
        {
            InitializeSystems();
            CreateSlots();
            SetupModeButtons();
            SubscribeToEvents();
            RefreshAllSlots();
            UpdateModeButtonVisuals();
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

        private void SetupModeButtons()
        {
            if (mode1xButton != null)
            {
                mode1xButton.onClick.RemoveAllListeners();
                mode1xButton.onClick.AddListener(() => SetEnhanceMode(EnhanceMode.x1));
            }

            if (mode10xButton != null)
            {
                mode10xButton.onClick.RemoveAllListeners();
                mode10xButton.onClick.AddListener(() => SetEnhanceMode(EnhanceMode.x10));
            }

            if (mode100xButton != null)
            {
                mode100xButton.onClick.RemoveAllListeners();
                mode100xButton.onClick.AddListener(() => SetEnhanceMode(EnhanceMode.x100));
            }
        }

        private void SetEnhanceMode(EnhanceMode mode)
        {
            // 이전 모드 버튼 애니메이션
            var previousButton = GetModeButton(currentMode);
            if (previousButton != null)
            {
                previousButton.transform.DOScale(1f, 0.1f);
            }

            currentMode = mode;
            UpdateModeButtonVisuals();
            RefreshAllSlots();

            // 새로운 모드 버튼 애니메이션
            var newButton = GetModeButton(mode);
            if (newButton != null)
            {
                newButton.transform.DOKill();
                newButton.transform.DOPunchScale(Vector3.one * 0.2f, 0.2f, 10, 1f);
            }

            Debug.Log($"<color=cyan>강화 모드 변경: {mode}</color>");
        }

        private UnityEngine.UI.Button GetModeButton(EnhanceMode mode)
        {
            switch (mode)
            {
                case EnhanceMode.x1: return mode1xButton;
                case EnhanceMode.x10: return mode10xButton;
                case EnhanceMode.x100: return mode100xButton;
                default: return null;
            }
        }

        private void UpdateModeButtonVisuals()
        {
            // 1x 버튼
            if (mode1xButton != null)
            {
                var image = mode1xButton.GetComponent<UnityEngine.UI.Image>();
                if (image != null)
                {
                    image.color = currentMode == EnhanceMode.x1 ? mode1xColor : normalModeColor;
                }

                var text = mode1xButton.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null)
                {
                    text.fontStyle = currentMode == EnhanceMode.x1 ? FontStyles.Bold : FontStyles.Normal;
                    text.color = currentMode == EnhanceMode.x1 ? selectedTextColor : normalTextColor;
                }

                var outline = mode1xButton.GetComponent<UnityEngine.UI.Outline>();
                if (outline != null)
                {
                    outline.enabled = currentMode == EnhanceMode.x1;
                    outline.effectColor = Color.white;
                    outline.effectDistance = new Vector2(2, 2);
                }
            }

            // 10x 버튼
            if (mode10xButton != null)
            {
                var image = mode10xButton.GetComponent<UnityEngine.UI.Image>();
                if (image != null)
                {
                    image.color = currentMode == EnhanceMode.x10 ? mode10xColor : normalModeColor;
                }

                var text = mode10xButton.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null)
                {
                    text.fontStyle = currentMode == EnhanceMode.x10 ? FontStyles.Bold : FontStyles.Normal;
                    text.color = currentMode == EnhanceMode.x10 ? selectedTextColor : normalTextColor;
                }

                var outline = mode10xButton.GetComponent<UnityEngine.UI.Outline>();
                if (outline != null)
                {
                    outline.enabled = currentMode == EnhanceMode.x10;
                    outline.effectColor = Color.white;
                    outline.effectDistance = new Vector2(2, 2);
                }
            }

            // 100x 버튼
            if (mode100xButton != null)
            {
                var image = mode100xButton.GetComponent<UnityEngine.UI.Image>();
                if (image != null)
                {
                    image.color = currentMode == EnhanceMode.x100 ? mode100xColor : normalModeColor;
                }

                var text = mode100xButton.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null)
                {
                    text.fontStyle = currentMode == EnhanceMode.x100 ? FontStyles.Bold : FontStyles.Normal;
                    text.color = currentMode == EnhanceMode.x100 ? selectedTextColor : normalTextColor;
                }

                var outline = mode100xButton.GetComponent<UnityEngine.UI.Outline>();
                if (outline != null)
                {
                    outline.enabled = currentMode == EnhanceMode.x100;
                    outline.effectColor = Color.white;
                    outline.effectDistance = new Vector2(2, 2);
                }
            }
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
                // 다중 강화 시 표시할 레벨 계산
                int displayLevel = enhancementLevel.currentLevel;
                int modeValue = (int)currentMode;

                // 현재 레벨 + 강화 횟수가 최대치를 넘지 않도록
                int possibleLevels = Math.Min(modeValue, enhancementLevel.maxLevel - enhancementLevel.currentLevel);

                // 슬롯 업데이트 시 현재 모드 정보와 baseValue도 전달
                slot.UpdateSlot(
                    enhancementLevel.currentLevel,
                    enhancementLevel.maxLevel,
                    enhancementLevel.GetEnhancementValue(),
                    enhancementLevel.isPercentage,
                    currentMode,
                    possibleLevels,
                    enhancementLevel.baseEnhancementValue
                );

                // 비용 계산 (다중 강화 비용)
                long totalCost = CalculateMultiEnhanceCost(slot.StatType, enhancementLevel.currentLevel, possibleLevels);
                bool canAfford = currencyManager != null && currencyManager.CanAfford(CurrencyType.Gold, totalCost);
                slot.UpdateCost(totalCost, canAfford);
            }
        }

        public void OnEnhanceRequested(StatType statType, long cost)
        {
            if (enhancementSystem == null || currencyManager == null) return;

            var enhancementLevel = enhancementSystem.GetEnhancementLevel(statType);
            if (enhancementLevel == null) return;

            // 실제로 강화 가능한 횟수 계산
            int modeValue = (int)currentMode;
            int possibleLevels = Math.Min(modeValue, enhancementLevel.maxLevel - enhancementLevel.currentLevel);

            if (possibleLevels <= 0)
            {
                Debug.Log("<color=yellow>이미 최대 레벨입니다!</color>");
                return;
            }

            // 다중 강화 비용 재계산
            long totalCost = CalculateMultiEnhanceCost(statType, enhancementLevel.currentLevel, possibleLevels);

            // 비용 지불
            if (!currencyManager.TrySpend(CurrencyType.Gold, totalCost))
            {
                ShowNotEnoughGoldMessage();
                return;
            }

            // 다중 강화 실행
            int successCount = 0;
            for (int i = 0; i < possibleLevels; i++)
            {
                bool success = enhancementSystem.EnhanceStat(statType);
                if (success)
                {
                    successCount++;
                }
                else
                {
                    break; // 더 이상 강화 불가능
                }
            }

            if (successCount > 0 && slots.ContainsKey(statType))
            {
                slots[statType].PlayEnhanceAnimation();

                // 다중 강화 시 추가 이펙트
                if (successCount > 1)
                {
                    ShowMultiEnhanceEffect(statType, successCount);
                }
            }

            Debug.Log($"<color=green>{statType} {successCount}회 강화 완료!</color>");
        }

        private long CalculateMultiEnhanceCost(StatType statType, int currentLevel, int enhanceCount)
        {
            long totalCost = 0;

            for (int i = 0; i < enhanceCount; i++)
            {
                totalCost += CalculateEnhanceCost(statType, currentLevel + i);
            }

            return totalCost;
        }

        private void ShowMultiEnhanceEffect(StatType statType, int count)
        {
            if (!slots.ContainsKey(statType)) return;

            var slot = slots[statType];

            // 다중 강화 시 더 강한 애니메이션
            slot.transform.DOKill();
            slot.transform.localScale = Vector3.one;

            // 단일 펄스 효과 (강도만 다르게)
            float pulseScale = count >= 100 ? 0.4f : count >= 10 ? 0.3f : 0.25f;
            slot.transform.DOPunchScale(Vector3.one * pulseScale, 0.3f, 10, 1f);
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
                slot.transform.DOKill();
                slot.transform.localScale = Vector3.one;
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

        [ButtonGroup("Debug Mode")]
        [Button("1x 모드", ButtonSizes.Medium)]
        private void SetMode1x() => SetEnhanceMode(EnhanceMode.x1);

        [ButtonGroup("Debug Mode")]
        [Button("10x 모드", ButtonSizes.Medium)]
        private void SetMode10x() => SetEnhanceMode(EnhanceMode.x10);

        [ButtonGroup("Debug Mode")]
        [Button("100x 모드", ButtonSizes.Medium)]
        private void SetMode100x() => SetEnhanceMode(EnhanceMode.x100);
    }
}