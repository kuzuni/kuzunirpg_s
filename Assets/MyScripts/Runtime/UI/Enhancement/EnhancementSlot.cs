using RPG.Common;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using RPG.UI.Components;

namespace RPG.UI.Enhancement
{
    [RequireComponent(typeof(RectTransform))]
    public class EnhancementSlot : MonoBehaviour
    {
        [Title("슬롯 설정")]
        [SerializeField, Required]
        private StatType statType;

        [SerializeField, Required]
        private Image statIcon;

        [Title("UI 참조")]
        [SerializeField, Required]
        private TextMeshProUGUI statNameText;

        [SerializeField, Required]
        private TextMeshProUGUI currentValueText;

        [SerializeField]
        private TextMeshProUGUI nextValueText;

        [SerializeField, Required]
        private TextMeshProUGUI currentLevelText;

        [SerializeField, Required]
        private TextMeshProUGUI maxLevelText;

        [SerializeField, Required]
        private Button enhanceButton;

        [SerializeField, Required]
        private TextMeshProUGUI costText;

        [SerializeField]
        private TextMeshProUGUI enhanceCountText; // 강화 횟수 표시

        [Title("색상 설정")]
        [SerializeField]
        private Color normalColor = new Color(0.2f, 0.8f, 1f);

        [SerializeField]
        private Color maxedColor = new Color(1f, 0.8f, 0.2f);

        [SerializeField]
        private Color cantAffordColor = new Color(0.5f, 0.5f, 0.5f);

        [SerializeField]
        private Color nextValueColor = new Color(0.2f, 1f, 0.2f);

        [SerializeField]
        private Color multiEnhanceColor = new Color(1f, 0.5f, 0.8f);

        // 상태
        private int currentLevel = 0;
        private int maxLevel = 10;
        private long enhanceCost = 1000;
        private float currentEnhancementValue = 0;
        private float baseEnhancementValue = 0; // 추가: 기본 강화값 저장
        private bool isPercentageStat = false;
        private EnhancementUI parentUI;
        private LongPressButton longPressButton;
        private EnhancementUI.EnhanceMode currentMode = EnhancementUI.EnhanceMode.x1;
        private int possibleEnhanceCount = 1;

        // Properties
        public StatType StatType => statType;
        public int CurrentLevel => currentLevel;
        public int MaxLevel => maxLevel;
        public long Cost => enhanceCost;

        public void Initialize(EnhancementUI ui, StatType type, Sprite icon)
        {
            parentUI = ui;
            statType = type;

            if (statIcon != null && icon != null)
            {
                statIcon.sprite = icon;
            }

            if (statNameText != null)
            {
                statNameText.text = GetStatDisplayName(statType);
            }

            if (enhanceButton != null)
            {
                enhanceButton.onClick.RemoveAllListeners();

                longPressButton = enhanceButton.GetComponent<LongPressButton>();
                if (longPressButton == null)
                {
                    longPressButton = enhanceButton.gameObject.AddComponent<LongPressButton>();
                }

                longPressButton.OnClick -= OnEnhanceClicked;
                longPressButton.OnLongPressRepeat -= OnEnhanceClicked;

                longPressButton.OnClick += OnEnhanceClicked;
                longPressButton.OnLongPressRepeat += OnEnhanceClicked;
            }
        }

        // 메서드 시그니처 수정 - baseValue 추가
        public void UpdateSlot(int level, int max, float enhancementValue, bool isPercentage,
            EnhancementUI.EnhanceMode mode = EnhancementUI.EnhanceMode.x1, int possibleCount = 1, float baseValue = 0)
        {
            currentLevel = level;
            maxLevel = max;
            currentEnhancementValue = enhancementValue;
            isPercentageStat = isPercentage;
            currentMode = mode;
            possibleEnhanceCount = possibleCount;
            baseEnhancementValue = baseValue;

            // 스탯 이름과 레벨을 함께 표시
            if (statNameText != null)
            {
                string displayName = GetStatDisplayName(statType);
                if (currentLevel >= maxLevel)
                {
                    statNameText.text = $"{displayName} <color=#{ColorUtility.ToHtmlStringRGB(maxedColor)}>MAX</color>";
                }
                else
                {
                    statNameText.text = $"{displayName} <color=white>Lv. {level}</color>";
                }
            }

            if (maxLevelText != null)
            {
                maxLevelText.text = $"Max Lv.{max}";
            }

            if (currentLevelText != null)
            {
                currentLevelText.gameObject.SetActive(false);
            }

            if (nextValueText != null)
            {
                nextValueText.gameObject.SetActive(false);
            }

            // 강화 횟수 표시
            if (enhanceCountText != null)
            {
                if (currentMode != EnhancementUI.EnhanceMode.x1 && possibleEnhanceCount > 0)
                {
                    enhanceCountText.gameObject.SetActive(true);
                    enhanceCountText.text = $"x{possibleEnhanceCount}";
                    enhanceCountText.color = possibleEnhanceCount < (int)currentMode ? Color.yellow : multiEnhanceColor;
                }
                else
                {
                    enhanceCountText.gameObject.SetActive(false);
                }
            }

            UpdateValueDisplay();

            if (currentLevel >= maxLevel)
            {
                if (enhanceButton != null)
                    enhanceButton.interactable = false;

                if (costText != null)
                    costText.text = "-";
            }
        }

        private void UpdateValueDisplay()
        {
            if (currentValueText == null) return;

            string currentValueStr = GetFormattedValue(currentEnhancementValue);

            if (currentLevel >= maxLevel)
            {
                currentValueText.text = currentValueStr;
                currentValueText.color = maxedColor;
            }
            else
            {
                // 정확한 다음 값 계산
                float nextValue;

                if (baseEnhancementValue > 0)
                {
                    // baseEnhancementValue가 설정된 경우 정확한 계산
                    nextValue = baseEnhancementValue * (currentLevel + possibleEnhanceCount);
                }
                else
                {
                    // baseEnhancementValue가 없는 경우 현재 값으로부터 추정
                    if (currentLevel > 0)
                    {
                        float estimatedBaseValue = currentEnhancementValue / currentLevel;
                        nextValue = estimatedBaseValue * (currentLevel + possibleEnhanceCount);
                    }
                    else
                    {
                        // 레벨 0인 경우 기본값 사용
                        nextValue = GetDefaultBaseValue(statType) * possibleEnhanceCount;
                    }
                }

                string nextValueStr = GetFormattedValue(nextValue);

                if (possibleEnhanceCount > 1)
                {
                    currentValueText.text = $"{currentValueStr}<color=#{ColorUtility.ToHtmlStringRGB(multiEnhanceColor)}>>{nextValueStr}</color>";
                }
                else
                {
                    currentValueText.text = $"{currentValueStr}<color=green>>>{nextValueStr}</color>";
                }
            }
        }

        // 스탯별 기본값 (EnhancementSystem의 값과 동일하게 설정)
        private float GetDefaultBaseValue(StatType type)
        {
            switch (type)
            {
                case StatType.MaxHp: return 20f;
                case StatType.AttackPower: return 3f;
                case StatType.CritChance: return 0.02f;
                case StatType.CritDamage: return 0.1f;
                case StatType.AttackSpeed: return 5f;
                case StatType.HpRegen: return 0.5f;
                default: return 1f;
            }
        }

        public void UpdateCost(long cost, bool canAfford)
        {
            enhanceCost = cost;

            if (currentLevel >= maxLevel)
            {
                costText.text = "-";
                enhanceButton.interactable = false;
            }
            else
            {
                if (currentMode != EnhancementUI.EnhanceMode.x1 && possibleEnhanceCount > 0)
                {
                    costText.text = $"{cost:N0} ({possibleEnhanceCount}x)";
                }
                else
                {
                    costText.text = $"{cost:N0}";
                }

                enhanceButton.interactable = canAfford;
                costText.color = canAfford ? Color.white : cantAffordColor;
            }
        }

        private string GetFormattedValue(float value)
        {
            switch (statType)
            {
                case StatType.MaxHp:
                    return $"{value:F0}";
                case StatType.AttackPower:
                    return $"{value:F0}";
                case StatType.CritChance:
                    return $"{value:F2}%";
                case StatType.CritDamage:
                    return $"{value:F1}%";
                case StatType.AttackSpeed:
                    return isPercentageStat ? $"{value:F0}%" : $"{value:F1}";
                case StatType.HpRegen:
                    return $"{value:F1}/s";
                default:
                    return $"{value:F0}";
            }
        }

        private void OnEnhanceClicked()
        {
            parentUI?.OnEnhanceRequested(statType, enhanceCost);
        }

        private Color buttonOriginalColor;
        private bool isButtonColorSaved = false;
        public void PlayEnhanceAnimation()
        {
            // 버튼 플래시 및 크기 애니메이션
            Image buttonImage = enhanceButton.GetComponent<Image>();
            if (buttonImage != null)
            {
                if (!isButtonColorSaved)
                {
                    buttonOriginalColor = buttonImage.color;
                    isButtonColorSaved = true;
                }

                buttonImage.DOKill(true);
                buttonImage.color = buttonOriginalColor;

                // 색상 애니메이션
                Color enhanceColor;
                if (possibleEnhanceCount >= 100)
                {
                    ColorUtility.TryParseHtmlString("#FF1744", out enhanceColor); // 빨간색
                }
                else if (possibleEnhanceCount >= 10)
                {
                    ColorUtility.TryParseHtmlString("#FFC107", out enhanceColor); // 주황색
                }
                else
                {
                    ColorUtility.TryParseHtmlString("#286F4C", out enhanceColor); // 녹색
                }

                buttonImage.DOColor(enhanceColor, 0.1f)
                    .SetEase(Ease.InOutQuad)
                    .OnComplete(() =>
                    {
                        buttonImage.DOColor(buttonOriginalColor, 0.1f)
                            .SetEase(Ease.InOutQuad);
                    });
            }

            // 버튼 크기 애니메이션 - 단일 펄스
            if (enhanceButton != null)
            {
                enhanceButton.transform.DOKill(true);
                enhanceButton.transform.localScale = Vector3.one;

                // 강화 횟수에 따라 펄스 강도만 조절
                float punchScale = possibleEnhanceCount >= 100 ? 0.4f :
                                  possibleEnhanceCount >= 10 ? 0.3f : 0.2f;

                enhanceButton.transform.DOPunchScale(Vector3.one * punchScale, 0.3f, 10, 1f);
            }

            // 값 텍스트 애니메이션 - 단일 펄스
            if (currentValueText != null)
            {
                currentValueText.transform.DOKill(true);
                currentValueText.transform.localScale = Vector3.one;

                float textPunchScale = possibleEnhanceCount >= 100 ? 0.4f :
                                      possibleEnhanceCount >= 10 ? 0.35f : 0.3f;

                currentValueText.transform.DOPunchScale(Vector3.one * textPunchScale, 0.3f, 10, 1f);
            }

            // 스탯 이름 텍스트 애니메이션 - 단일 펄스
            if (statNameText != null)
            {
                statNameText.transform.DOKill(true);
                statNameText.transform.localScale = Vector3.one;

                float namePunchScale = possibleEnhanceCount >= 100 ? 0.3f :
                                       possibleEnhanceCount >= 10 ? 0.25f : 0.2f;

                statNameText.transform.DOPunchScale(Vector3.one * namePunchScale, 0.3f, 10, 1f);
            }
        }
        private string GetStatDisplayName(StatType type)
        {
            switch (type)
            {
                case StatType.AttackPower: return "Attack";
                case StatType.MaxHp: return "Health";
                case StatType.CritChance: return "Crit Chance";
                case StatType.CritDamage: return "Crit Damage";
                case StatType.AttackSpeed: return "Attack Speed";
                case StatType.HpRegen: return "HP Regen";
                default: return type.ToString();
            }
        }

        private void OnDestroy()
        {
            if (longPressButton != null)
            {
                longPressButton.OnClick -= OnEnhanceClicked;
                longPressButton.OnLongPressRepeat -= OnEnhanceClicked;
            }

            if (enhanceButton != null)
            {
                Image buttonImage = enhanceButton.GetComponent<Image>();
                if (buttonImage != null)
                {
                    buttonImage.DOKill(true);
                    if (isButtonColorSaved)
                    {
                        buttonImage.color = buttonOriginalColor;
                    }
                }

                enhanceButton.transform.DOKill(true);
                enhanceButton.transform.localScale = Vector3.one;
            }

            if (currentValueText != null)
            {
                currentValueText.transform.DOKill(true);
                currentValueText.transform.localScale = Vector3.one;
            }

            if (statNameText != null)
            {
                statNameText.transform.DOKill(true);
                statNameText.transform.localScale = Vector3.one;
            }

            transform.DOKill(true);
        }
    }
}