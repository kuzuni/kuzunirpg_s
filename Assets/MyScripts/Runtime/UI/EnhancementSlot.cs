using RPG.Common;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

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
    private TextMeshProUGUI nextValueText;  // 이제 사용하지 않음 (숨기거나 제거 가능)

    [SerializeField, Required]
    private TextMeshProUGUI currentLevelText;  // 이제 사용하지 않음 (숨기거나 제거 가능)

    [SerializeField, Required]
    private TextMeshProUGUI maxLevelText;  // 최대 레벨만 표시

    [SerializeField, Required]
    private Button enhanceButton;

    [SerializeField, Required]
    private TextMeshProUGUI costText;

    [Title("색상 설정")]
    [SerializeField]
    private Color normalColor = new Color(0.2f, 0.8f, 1f);

    [SerializeField]
    private Color maxedColor = new Color(1f, 0.8f, 0.2f);

    [SerializeField]
    private Color cantAffordColor = new Color(0.5f, 0.5f, 0.5f);

    [SerializeField]
    private Color nextValueColor = new Color(0.2f, 1f, 0.2f);  // 강화 후 수치 색상

    // 상태
    private int currentLevel = 0;
    private int maxLevel = 10;
    private long enhanceCost = 1000;
    private float currentEnhancementValue = 0;
    private bool isPercentageStat = false;
    private EnhancementUI parentUI;
    private LongPressButton longPressButton;

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

        // 텍스트 설정 - 초기에는 레벨 없이 이름만
        if (statNameText != null)
        {
            statNameText.text = GetStatDisplayName(statType);
        }

        // 버튼 이벤트 설정
        if (enhanceButton != null)
        {
            enhanceButton.onClick.RemoveAllListeners();

            // LongPressButton 컴포넌트 추가 또는 가져오기
            longPressButton = enhanceButton.GetComponent<LongPressButton>();
            if (longPressButton == null)
            {
                longPressButton = enhanceButton.gameObject.AddComponent<LongPressButton>();
            }

            // 이벤트 연결
            longPressButton.OnClick -= OnEnhanceClicked;
            longPressButton.OnLongPressRepeat -= OnEnhanceClicked;

            longPressButton.OnClick += OnEnhanceClicked;
            longPressButton.OnLongPressRepeat += OnEnhanceClicked;
        }
    }

    public void UpdateSlot(int level, int max, float enhancementValue, bool isPercentage)
    {
        currentLevel = level;
        maxLevel = max;
        currentEnhancementValue = enhancementValue;
        isPercentageStat = isPercentage;

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
                statNameText.text = $"{displayName} Lv. {level}";
            }
        }

        // 최대 레벨 텍스트
        if (maxLevelText != null)
        {
            maxLevelText.text = $"Max Lv.{max}";
        }

        // currentLevelText와 nextValueText는 더 이상 사용하지 않으므로 비활성화
        if (currentLevelText != null)
        {
            currentLevelText.gameObject.SetActive(false);
        }

        if (nextValueText != null)
        {
            nextValueText.gameObject.SetActive(false);
        }

        // 현재값 → 다음값 형식으로 표시
        UpdateValueDisplay();

        // 최대 레벨 처리
        if (currentLevel >= maxLevel)
        {
            if (enhanceButton != null)
                enhanceButton.interactable = false;

            if (costText != null)
                costText.text = "-";
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
            costText.text = $"{cost:N0}";
            enhanceButton.interactable = canAfford;
            costText.color = canAfford ? Color.white : cantAffordColor;
        }
    }

    private void UpdateValueDisplay()
    {
        if (currentValueText == null) return;

        string currentValueStr = GetFormattedValue(currentEnhancementValue);

        if (currentLevel >= maxLevel)
        {
            // 최대 레벨일 때는 현재 값만 표시
            currentValueText.text = currentValueStr;
            currentValueText.color = maxedColor;
        }
        else
        {
            // 다음 레벨 값 계산 (20% 증가)
            float nextValue = currentEnhancementValue * 1.2f;
            string nextValueStr = GetFormattedValue(nextValue);

            // 현재값→다음값 형식으로 표시
            currentValueText.text = $"{currentValueStr}→{nextValueStr}";
        }
    }

    private string GetFormattedValue(float value)
    {
        switch (statType)
        {
            case StatType.MaxHp:
                return $"+{value:F0}";
            case StatType.AttackPower:
                return $"+{value:F0}";
            case StatType.CritChance:
                return $"+{value:F2}%";
            case StatType.CritDamage:
                return $"+{value:F1}%";
            case StatType.AttackSpeed:
                return isPercentageStat ? $"+{value:F0}%" : $"+{value:F1}";
            case StatType.HpRegen:
                return $"+{value:F1}/s";
            default:
                return $"+{value:F0}";
        }
    }

    private void OnEnhanceClicked()
    {
        parentUI?.OnEnhanceRequested(statType, enhanceCost);
    }

    // 버튼 원래 색상 저장
    private Color buttonOriginalColor;
    private bool isButtonColorSaved = false;

    public void PlayEnhanceAnimation()
    {
        // 버튼 플래시 및 크기 애니메이션
        Image buttonImage = enhanceButton.GetComponent<Image>();
        if (buttonImage != null)
        {
            // 원래 색상 저장 (처음 한 번만)
            if (!isButtonColorSaved)
            {
                buttonOriginalColor = buttonImage.color;
                isButtonColorSaved = true;
            }

            // 기존 애니메이션 중지하고 원래 색상으로 즉시 복원
            buttonImage.DOKill(true);
            buttonImage.color = buttonOriginalColor;

            // 색상 애니메이션
            Color enhanceColor;
            ColorUtility.TryParseHtmlString("#286F4C", out enhanceColor);
            buttonImage.DOColor(enhanceColor, 0.1f)
                .SetEase(Ease.InOutQuad)
                .OnComplete(() =>
                {
                    buttonImage.DOColor(buttonOriginalColor, 0.1f)
                        .SetEase(Ease.InOutQuad);
                });
        }

        // 버튼 크기 애니메이션
        if (enhanceButton != null)
        {
            enhanceButton.transform.DOKill(true);
            enhanceButton.transform.localScale = Vector3.one;
            enhanceButton.transform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 10, 1f);
        }

        // 값 텍스트 애니메이션
        if (currentValueText != null)
        {
            // 기존 애니메이션 중지하고 스케일 초기화
            currentValueText.transform.DOKill(true);
            currentValueText.transform.localScale = Vector3.one;

            // 새 애니메이션 시작
            currentValueText.transform.DOPunchScale(Vector3.one * 0.3f, 0.3f, 10, 1f);
        }

        // 스탯 이름 텍스트도 애니메이션 (레벨업 느낌)
        if (statNameText != null)
        {
            statNameText.transform.DOKill(true);
            statNameText.transform.localScale = Vector3.one;
            statNameText.transform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 10, 1f);
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
        // 이벤트 해제
        if (longPressButton != null)
        {
            longPressButton.OnClick -= OnEnhanceClicked;
            longPressButton.OnLongPressRepeat -= OnEnhanceClicked;
        }

        // 애니메이션 정리 - 완료 콜백까지 즉시 실행
        if (enhanceButton != null)
        {
            Image buttonImage = enhanceButton.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.DOKill(true);
                // 원래 색상으로 복원
                if (isButtonColorSaved)
                {
                    buttonImage.color = buttonOriginalColor;
                }
            }

            // 버튼 크기 애니메이션 정리
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