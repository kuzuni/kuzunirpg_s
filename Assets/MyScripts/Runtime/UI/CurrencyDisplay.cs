using UnityEngine;
using TMPro;
using RPG.Common;
using RPG.Managers;
using RPG.Core.Events;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using DG.Tweening;

namespace RPG.UI
{
    // 화폐 UI 표시 컴포넌트 - 단일 또는 다중 화폐 표시 가능
    public class CurrencyDisplay : MonoBehaviour
    {
        [System.Serializable]
        public class CurrencyUIElement
        {
            [HorizontalGroup("Currency", 0.3f)]
            public CurrencyType currencyType;

            [HorizontalGroup("Currency", 0.7f)]
            public TextMeshProUGUI currencyText;

            [FoldoutGroup("추가 설정")]
            public string iconPrefix = ""; // 예: "💰 ", "💎 "

            [FoldoutGroup("추가 설정")]
            public bool useCustomFormat = false;

            [FoldoutGroup("추가 설정")]
            [ShowIf("useCustomFormat")]
            public string customFormat = "{0:N0}";

            [HideInInspector]
            public long lastValue = 0;
        }

        [Title("표시 모드")]
        [EnumToggleButtons]
        [SerializeField] private DisplayMode displayMode = DisplayMode.Single;

        public enum DisplayMode
        {
            Single,     // 단일 화폐만 표시
            Multiple    // 여러 화폐 표시
        }

        [Title("단일 화폐 설정")]
        [ShowIf("displayMode", DisplayMode.Single)]
        [SerializeField] private CurrencyType singleCurrencyType;

        [ShowIf("displayMode", DisplayMode.Single)]
        [SerializeField] private TextMeshProUGUI singleCurrencyText;

        [ShowIf("displayMode", DisplayMode.Single)]
        [SerializeField] private string singleIconPrefix = "";

        [Title("다중 화폐 설정")]
        [ShowIf("displayMode", DisplayMode.Multiple)]
        [ListDrawerSettings(ShowIndexLabels = true, Expanded = true)]
        [SerializeField] private List<CurrencyUIElement> currencyElements = new List<CurrencyUIElement>();

        [Title("공통 표시 형식")]
        [SerializeField] private string defaultFormat = "{0:N0}"; // 천 단위 구분 기호
        [SerializeField] private bool useAbbreviation = true; // K, M, B 사용

        [Title("애니메이션 설정")]
        [SerializeField] private bool useCountAnimation = true;
        [SerializeField] private float animationDuration = 0.5f;
        [SerializeField] private Ease animationEase = Ease.OutQuad;

        private CurrencyManager currencyManager;
        private Dictionary<CurrencyType, Tween> activeTweens = new Dictionary<CurrencyType, Tween>();

        private void Start()
        {
            // CurrencyManager 찾기
            currencyManager = FindObjectOfType<CurrencyManager>();

            if (currencyManager == null)
            {
                Debug.LogError("CurrencyManager를 찾을 수 없습니다!");
                return;
            }

            // 단일 모드에서 텍스트 컴포넌트 자동 찾기
            if (displayMode == DisplayMode.Single && singleCurrencyText == null)
            {
                singleCurrencyText = GetComponent<TextMeshProUGUI>();
            }

            // 초기 값 표시
            UpdateDisplay();

            // 이벤트 구독
            GameEventManager.OnCurrencyChanged += OnCurrencyChanged;
        }

        private void OnDestroy()
        {
            // 이벤트 구독 해제
            GameEventManager.OnCurrencyChanged -= OnCurrencyChanged;

            // 모든 트윈 정리
            foreach (var tween in activeTweens.Values)
            {
                tween?.Kill();
            }
            activeTweens.Clear();
        }

        private void OnCurrencyChanged(CurrencyType type, long newAmount)
        {
            if (displayMode == DisplayMode.Single)
            {
                // 단일 모드: 해당 화폐 타입만 업데이트
                if (type == singleCurrencyType)
                {
                    UpdateSingleCurrency(newAmount);
                }
            }
            else
            {
                // 다중 모드: 해당하는 모든 요소 업데이트
                foreach (var element in currencyElements)
                {
                    if (element.currencyType == type && element.currencyText != null)
                    {
                        UpdateCurrencyElement(element, newAmount);
                    }
                }
            }
        }

        private void UpdateDisplay()
        {
            if (currencyManager == null) return;

            if (displayMode == DisplayMode.Single)
            {
                long amount = GetCurrencyAmount(singleCurrencyType);
                UpdateSingleCurrency(amount);
            }
            else
            {
                foreach (var element in currencyElements)
                {
                    if (element.currencyText != null)
                    {
                        long amount = GetCurrencyAmount(element.currencyType);
                        element.lastValue = amount;
                        UpdateCurrencyElement(element, amount);
                    }
                }
            }
        }

        private void UpdateSingleCurrency(long newAmount)
        {
            if (singleCurrencyText == null) return;

            if (useCountAnimation && Application.isPlaying)
            {
                AnimateCurrencyChange(singleCurrencyType, 0, newAmount, (value) =>
                {
                    string text = FormatCurrency(value, defaultFormat);
                    if (!string.IsNullOrEmpty(singleIconPrefix))
                    {
                        text = singleIconPrefix + text;
                    }
                    singleCurrencyText.text = text;
                });
            }
            else
            {
                string text = FormatCurrency(newAmount, defaultFormat);
                if (!string.IsNullOrEmpty(singleIconPrefix))
                {
                    text = singleIconPrefix + text;
                }
                singleCurrencyText.text = text;
            }
        }

        private void UpdateCurrencyElement(CurrencyUIElement element, long newAmount)
        {
            if (useCountAnimation && Application.isPlaying)
            {
                AnimateCurrencyChange(element.currencyType, element.lastValue, newAmount, (value) =>
                {
                    string format = element.useCustomFormat ? element.customFormat : defaultFormat;
                    string text = FormatCurrency(value, format);
                    if (!string.IsNullOrEmpty(element.iconPrefix))
                    {
                        text = element.iconPrefix + text;
                    }
                    element.currencyText.text = text;
                });
                element.lastValue = newAmount;
            }
            else
            {
                string format = element.useCustomFormat ? element.customFormat : defaultFormat;
                string text = FormatCurrency(newAmount, format);
                if (!string.IsNullOrEmpty(element.iconPrefix))
                {
                    text = element.iconPrefix + text;
                }
                element.currencyText.text = text;
                element.lastValue = newAmount;
            }
        }

        private void AnimateCurrencyChange(CurrencyType type, long fromValue, long toValue, System.Action<long> onUpdate)
        {
            // 기존 트윈이 있으면 중지
            if (activeTweens.ContainsKey(type))
            {
                activeTweens[type]?.Kill();
            }

            // 새 트윈 생성
            long currentValue = fromValue;
            var tween = DOTween.To(() => currentValue, x => currentValue = x, toValue, animationDuration)
                .SetEase(animationEase)
                .OnUpdate(() => onUpdate(currentValue))
                .OnComplete(() => activeTweens.Remove(type));

            activeTweens[type] = tween;
        }

        private long GetCurrencyAmount(CurrencyType type)
        {
            switch (type)
            {
                case CurrencyType.Gold:
                    return currencyManager.Gold;
                case CurrencyType.Diamond:
                    return currencyManager.Diamond;
                case CurrencyType.Energy:
                    return currencyManager.Energy;
                case CurrencyType.SoulStone:
                    return currencyManager.SoulStone;
                default:
                    return 0;
            }
        }

        private string FormatCurrency(long amount, string format)
        {
            if (useAbbreviation)
            {
                // K, M, B 형식으로 표시
                if (amount >= 1000000000)
                {
                    return $"{amount / 1000000000f:F1}B";
                }
                else if (amount >= 1000000)
                {
                    return $"{amount / 1000000f:F1}M";
                }
                else if (amount >= 10000)
                {
                    return $"{amount / 1000f:F1}K";
                }
            }

            // 기본 형식 사용
            return string.Format(format, amount);
        }

        [Title("디버그")]
        [Button("표시 갱신")]
        private void DebugRefreshDisplay()
        {
            UpdateDisplay();
        }

        [ShowIf("displayMode", DisplayMode.Multiple)]
        [Button("모든 화폐 타입 추가")]
        private void AddAllCurrencyTypes()
        {
            currencyElements.Clear();
            foreach (CurrencyType type in System.Enum.GetValues(typeof(CurrencyType)))
            {
                currencyElements.Add(new CurrencyUIElement
                {
                    currencyType = type,
                    iconPrefix = GetDefaultIcon(type)
                });
            }
        }

        private string GetDefaultIcon(CurrencyType type)
        {
            switch (type)
            {
                case CurrencyType.Gold:
                    return "💰 ";
                case CurrencyType.Diamond:
                    return "💎 ";
                case CurrencyType.Energy:
                    return "⚡ ";
                case CurrencyType.SoulStone:
                    return "🔮 ";
                default:
                    return "";
            }
        }
    }
}