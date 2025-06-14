using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sirenix.OdinInspector;
using UnityEngine.EventSystems;
using DG.Tweening;
using RPG.Items.Equipment;
using RPG.Inventory;

namespace RPG.UI.Inventory
{
    /// <summary>
    /// 장비 인벤토리 슬롯 UI
    /// </summary>
    public class EquipmentInventorySlot : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Title("UI 참조")]
        [SerializeField, Required]
        private Image iconImage;

        [SerializeField, Required]
        private Image rarityFrame;

        [SerializeField]
        private Image rarityBackground;

        [SerializeField, Required]
        private TextMeshProUGUI nameText;

        [SerializeField, Required]
        private TextMeshProUGUI subGradeText;

        [SerializeField]
        private TextMeshProUGUI quantityText;

        [Title("합성 진행도")]
        [SerializeField]
        private Slider fusionProgressSlider;

        [SerializeField]
        private Image fusionProgressFill;

        // fusionRequiredCount를 제거하고 FusionSystem에서 가져오도록 변경
        private int fusionRequiredCount = 5; // 기본값 5, Setup에서 업데이트됨

        [Title("상태 표시")]
        [SerializeField]
        private GameObject selectedFrame;

        [SerializeField]
        private GameObject equippedBadge;

        [SerializeField]
        private GameObject newBadge;

        [SerializeField]
        private GameObject fusionableMark;

        [Title("색상 설정")]
        [SerializeField]
        private Color normalBackgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);

        [SerializeField]
        private Color selectedBackgroundColor = new Color(0.3f, 0.5f, 0.8f, 0.9f);

        [SerializeField]
        private Color hoverColor = new Color(0.3f, 0.3f, 0.3f, 0.9f);

        [Title("애니메이션")]
        [SerializeField]
        private float hoverScale = 1.05f;

        [SerializeField]
        private float clickScale = 0.95f;

        [SerializeField]
        private float animationDuration = 0.2f;

        // 상태
        private EquipmentData equipment;
        private int quantity;
        private bool isSelected = false;
        private bool isEquipped = false;
        private bool isNew = false;
        private bool isFusionMode = false;

        // 이벤트
        public event System.Action<EquipmentInventorySlot> OnSlotClicked;
        public event System.Action<EquipmentInventorySlot> OnSlotRightClicked;

        // Properties
        public EquipmentData Equipment => equipment;
        public bool IsSelected => isSelected;
        public int Quantity => quantity;

        private Button button;
        private Image backgroundImage;
        private CanvasGroup canvasGroup;
        private Vector3 originalScale;

        private void Awake()
        {
            button = GetComponent<Button>();
            if (button == null)
            {
                button = gameObject.AddComponent<Button>();
            }

            backgroundImage = GetComponent<Image>();
            if (backgroundImage == null)
            {
                backgroundImage = gameObject.AddComponent<Image>();
            }

            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            originalScale = transform.localScale;

            // 버튼 전환 효과 비활성화 (자체 애니메이션 사용)
            button.transition = Button.Transition.None;

            // 슬라이더 초기 설정
            if (fusionProgressSlider != null)
            {
                fusionProgressSlider.minValue = 0f;
                fusionProgressSlider.maxValue = 1f;
                fusionProgressSlider.value = 0f;

                // 슬라이더 인터랙션 비활성화
                fusionProgressSlider.interactable = false;
            }

            // Start 시점에 FusionSystem에서 값 가져오기
            UpdateFusionRequiredCount();
        }

        private void Start()
        {
            // Start에서 한 번 더 업데이트
            UpdateFusionRequiredCount();
        }

        /// <summary>
        /// FusionSystem에서 필요 개수 가져오기
        /// </summary>
        private void UpdateFusionRequiredCount()
        {
            var fusionSystem = FindObjectOfType<EquipmentFusionSystem>();
            if (fusionSystem != null)
            {
                fusionRequiredCount = fusionSystem.FusionRequiredCount;
                Debug.Log($"[EquipmentInventorySlot] fusionRequiredCount 업데이트: {fusionRequiredCount}");
            }
        }

        /// <summary>
        /// 슬롯 초기화
        /// </summary>
        public void Setup(EquipmentData equipmentData, int count = 1, bool equipped = false, bool newItem = false)
        {
            equipment = equipmentData;
            quantity = count;
            isEquipped = equipped;
            isNew = newItem;

            // Setup 시점에 다시 한 번 FusionSystem 값 확인
            UpdateFusionRequiredCount();

            UpdateVisuals();
            UpdateFusionProgress();
        }

        /// <summary>
        /// 아이콘 색상 설정 (미획득 아이템 표시용)
        /// </summary>
        public void SetIconColor(Color color)
        {
            if (iconImage != null)
            {
                iconImage.color = color;
            }
        }

        private void UpdateVisuals()
        {
            if (equipment == null) return;

            // 아이콘
            if (iconImage != null && equipment.icon != null)
            {
                iconImage.sprite = equipment.icon;
                iconImage.enabled = true;
            }

            // 이름
            if (nameText != null)
            {
                nameText.text = equipment.equipmentName;
                nameText.color = equipment.GetRarityColor();
            }

            // 세부 등급
            if (subGradeText != null)
            {
                string stars = new string('★', equipment.subGrade);
                subGradeText.text = stars;
                subGradeText.color = equipment.GetRarityColor();
            }

            // 수량
            if (quantityText != null)
            {
                // "현재/필요" 형식으로 표시
                quantityText.gameObject.SetActive(true);
                quantityText.text = $"{quantity}/{fusionRequiredCount}";

                // 합성 가능하면 색상 변경
                if (quantity >= fusionRequiredCount)
                {
                    quantityText.color = Color.green;
                }
                else
                {
                    quantityText.color = Color.white;
                }
            }

            // 등급 프레임
            if (rarityFrame != null)
            {
                rarityFrame.color = equipment.GetRarityColor();
            }

            // 등급 배경
            if (rarityBackground != null)
            {
                var bgColor = equipment.GetRarityColor();
                bgColor.a = 0.2f;
                rarityBackground.color = bgColor;
            }

            // 상태 배지
            if (equippedBadge != null)
            {
                equippedBadge.SetActive(isEquipped);
            }

            if (newBadge != null)
            {
                newBadge.SetActive(isNew);
            }

            // 배경색
            UpdateBackgroundColor();
        }

        private void UpdateBackgroundColor()
        {
            if (backgroundImage == null) return;

            Color targetColor = isSelected ? selectedBackgroundColor : normalBackgroundColor;
            backgroundImage.color = targetColor;
        }

        /// <summary>
        /// 선택 상태 설정
        /// </summary>
        public void SetSelected(bool selected)
        {
            if (isSelected == selected) return;

            isSelected = selected;

            // 선택 프레임
            if (selectedFrame != null)
            {
                selectedFrame.SetActive(selected);
            }

            // 배경색 변경
            UpdateBackgroundColor();

            // 선택 애니메이션
            if (selected)
            {
                transform.DOKill();
                transform.DOPunchScale(Vector3.one * 0.1f, 0.3f, 10, 1f);
            }
        }

        /// <summary>
        /// 합성 진행도 업데이트
        /// </summary>
        private void UpdateFusionProgress()
        {
            if (fusionProgressSlider != null)
            {
                // 진행도 계산 - 최대값은 1로 제한
                float progress = Mathf.Clamp01((float)quantity / fusionRequiredCount);
                fusionProgressSlider.value = progress;

                // 슬라이더 색상 변경
                if (fusionProgressFill != null)
                {
                    if (quantity >= fusionRequiredCount)
                    {
                        // 합성 가능 - 녹색 (100% 이상)
                        fusionProgressFill.color = new Color(0.2f, 0.8f, 0.2f);
                    }
                    else if (quantity >= fusionRequiredCount / 2f)
                    {
                        // 절반 이상 - 노란색
                        fusionProgressFill.color = new Color(0.8f, 0.8f, 0.2f);
                    }
                    else
                    {
                        // 부족 - 빨간색
                        fusionProgressFill.color = new Color(0.8f, 0.2f, 0.2f);
                    }
                }

                // 합성 가능 마크 표시
                if (fusionableMark != null)
                {
                    fusionableMark.SetActive(quantity >= fusionRequiredCount);
                }

                // 디버그 로그
                Debug.Log($"[FusionProgress] {equipment?.equipmentName}: {quantity}/{fusionRequiredCount} = {progress:P0}");
            }
        }

        /// <summary>
        /// 합성 모드 설정
        /// </summary>
        public void SetFusionMode(bool enabled)
        {
            isFusionMode = enabled;

            // 투명도 조절 (합성 불가능한 아이템)
            if (enabled && quantity < fusionRequiredCount)
            {
                canvasGroup.alpha = 0.5f;
            }
            else
            {
                canvasGroup.alpha = 1f;
            }
        }

        /// <summary>
        /// 수량 업데이트
        /// </summary>
        public void UpdateQuantity(int newQuantity)
        {
            quantity = newQuantity;

            // 수량 텍스트 업데이트
            if (quantityText != null)
            {
                quantityText.text = $"{quantity}/{fusionRequiredCount}";
                quantityText.color = quantity >= fusionRequiredCount ? Color.green : Color.white;
            }

            // 진행도 업데이트
            UpdateFusionProgress();
        }

        /// <summary>
        /// 합성 필요 개수 설정
        /// </summary>
        public void SetFusionRequiredCount(int count)
        {
            fusionRequiredCount = count;
            UpdateQuantity(quantity); // 표시 갱신
        }

        /// <summary>
        /// 장착 상태 설정
        /// </summary>
        public void SetEquipped(bool equipped)
        {
            isEquipped = equipped;

            if (equippedBadge != null)
            {
                equippedBadge.SetActive(equipped);
            }
        }

        /// <summary>
        /// New 상태 설정
        /// </summary>
        public void SetNew(bool newItem)
        {
            isNew = newItem;

            if (newBadge != null)
            {
                newBadge.SetActive(newItem);
            }
        }

        // ... 나머지 코드는 동일 ...

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                // 클릭 애니메이션
                transform.DOKill();
                transform.localScale = originalScale;
                transform.DOScale(clickScale, animationDuration * 0.5f)
                    .OnComplete(() => transform.DOScale(originalScale, animationDuration * 0.5f));

                OnSlotClicked?.Invoke(this);
            }
            else if (eventData.button == PointerEventData.InputButton.Right)
            {
                OnSlotRightClicked?.Invoke(this);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            // 호버 효과
            if (!isSelected)
            {
                transform.DOKill();
                transform.DOScale(originalScale * hoverScale, animationDuration);

                if (backgroundImage != null)
                {
                    backgroundImage.DOColor(hoverColor, animationDuration);
                }
            }

            // 툴팁 표시
            ShowTooltip();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            // 호버 효과 제거
            if (!isSelected)
            {
                transform.DOKill();
                transform.DOScale(originalScale, animationDuration);

                UpdateBackgroundColor();
            }

            // 툴팁 숨김
            HideTooltip();
        }

        private void ShowTooltip()
        {
            // TODO: 툴팁 시스템과 연동
            if (equipment != null)
            {
                string tooltip = GetTooltipText();
                Debug.Log($"Tooltip: {tooltip}");
            }
        }

        private void HideTooltip()
        {
            // TODO: 툴팁 숨기기
        }

        private string GetTooltipText()
        {
            if (equipment == null) return "";

            string tooltip = $"<color=#{ColorUtility.ToHtmlStringRGB(equipment.GetRarityColor())}>{equipment.GetFullRarityName()}</color>\n";
            tooltip += $"{equipment.equipmentName}\n\n";

            // 스탯 표시
            switch (equipment.equipmentType)
            {
                case EquipmentType.Weapon:
                    tooltip += $"공격력: +{equipment.GetFinalAttackPower()}\n";
                    break;
                case EquipmentType.Armor:
                    tooltip += $"최대 체력: +{equipment.GetFinalMaxHp()}\n";
                    break;
                case EquipmentType.Ring:
                    tooltip += $"체력 재생: +{equipment.GetFinalHpRegen():F1}/초\n";
                    break;
            }

            if (!string.IsNullOrEmpty(equipment.description))
            {
                tooltip += $"\n{equipment.description}";
            }

            tooltip += $"\n\n보유: {quantity}개";

            if (quantity < fusionRequiredCount)
            {
                tooltip += $"\n합성까지: {fusionRequiredCount - quantity}개 더 필요";
            }
            else
            {
                int fusionCount = quantity / fusionRequiredCount;
                tooltip += $"\n<color=green>합성 가능: {fusionCount}회</color>";
            }

            return tooltip;
        }

        private void OnDestroy()
        {
            transform.DOKill();
            if (backgroundImage != null)
            {
                backgroundImage.DOKill();
            }
        }

        [Title("테스트")]
        [Button("진행도 테스트")]
        private void TestFusionProgress()
        {
            Debug.Log($"===== 합성 진행도 테스트 =====");
            Debug.Log($"장비: {equipment?.equipmentName ?? "NULL"}");
            Debug.Log($"현재 수량: {quantity}");
            Debug.Log($"필요 수량: {fusionRequiredCount}");
            Debug.Log($"진행도: {(float)quantity / fusionRequiredCount:P0}");

            if (fusionProgressSlider != null)
            {
                Debug.Log($"슬라이더 값: {fusionProgressSlider.value}");
                Debug.Log($"슬라이더 Min: {fusionProgressSlider.minValue}");
                Debug.Log($"슬라이더 Max: {fusionProgressSlider.maxValue}");
            }
            else
            {
                Debug.LogError("fusionProgressSlider가 null입니다!");
            }
            Debug.Log("==============================");
        }

        [Button("수량 증가 테스트")]
        private void TestIncreaseQuantity()
        {
            UpdateQuantity(quantity + 1);
        }

        [Button("선택 토글")]
        private void TestToggleSelection()
        {
            SetSelected(!isSelected);
        }

        /// <summary>
        /// 슬롯 상호작용 가능 여부 설정
        /// </summary>
        public void SetInteractable(bool interactable)
        {
            if (button != null)
            {
                button.interactable = interactable;
            }

            if (canvasGroup != null)
            {
                // 비활성화 시 투명도 조절
                canvasGroup.alpha = interactable ? 1f : 0.4f;
            }

            // 비활성화 시 시각적 표시
            if (!interactable)
            {
                if (backgroundImage != null)
                {
                    var color = backgroundImage.color;
                    color.a = 0.3f;
                    backgroundImage.color = color;
                }
            }
            else
            {
                UpdateBackgroundColor();
            }
        }
    }
}