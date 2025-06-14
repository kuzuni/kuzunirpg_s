using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using RPG.Items.Equipment;
using RPG.UI.Popup;

namespace RPG.UI.Inventory
{
    /// <summary>
    /// 장비 합성 팝업 UI
    /// </summary>
    public class EquipmentFusionPopup : PopupUI
    {
        [Title("합성 재료 슬롯")]
        [SerializeField, Required]
        private Transform materialSlotsContainer;

        [SerializeField, Required]
        private GameObject materialSlotPrefab;

        [SerializeField]
        private int maxMaterialSlots = 5;

        [Title("결과 표시")]
        [SerializeField, Required]
        private Image resultIconImage;

        [SerializeField, Required]
        private TextMeshProUGUI resultNameText;

        [SerializeField, Required]
        private TextMeshProUGUI resultStatsText;

        [SerializeField]
        private GameObject resultQuestionMark;

        [SerializeField]
        private GameObject resultPreview;

        [Title("확률 표시")]
        [SerializeField, Required]
        private TextMeshProUGUI successRateText;

        [SerializeField]
        private Slider successRateSlider;

        [SerializeField]
        private Image successRateFillImage;

        [Title("버튼")]
        [SerializeField, Required]
        private Button fusionButton;

        [SerializeField, Required]
        private Button cancelButton;

        [SerializeField]
        private TextMeshProUGUI fusionButtonText;

        [Title("비용")]
        [SerializeField, Required]
        private TextMeshProUGUI costText;

        [SerializeField]
        private Image costIcon;

        [Title("애니메이션")]
        [SerializeField]
        private GameObject fusionEffectPrefab;

        [SerializeField]
        private float fusionAnimationDuration = 1.5f;

        [SerializeField]
        private ParticleSystem successParticle;

        [SerializeField]
        private ParticleSystem failParticle;

        // 상태
        private List<EquipmentData> materialEquipments = new List<EquipmentData>();
        private List<FusionMaterialSlot> materialSlots = new List<FusionMaterialSlot>();
        private EquipmentData baseEquipment;
        private int fusionCost = 0;
        private float successRate = 0f;
        private bool isFusing = false;

        // 콜백
        private System.Action<List<EquipmentData>> onFusionConfirm;
        private System.Action onCancel;

        protected override void Awake()
        {
            base.Awake();

            // 버튼 이벤트 설정
            if (fusionButton != null)
            {
                fusionButton.onClick.AddListener(OnFusionButtonClicked);
            }

            if (cancelButton != null)
            {
                cancelButton.onClick.AddListener(OnCancelButtonClicked);
            }

            CreateMaterialSlots();
        }

        /// <summary>
        /// 합성 팝업 초기화
        /// </summary>
        public void Initialize(List<EquipmentData> materials, System.Action<List<EquipmentData>> onConfirm, System.Action onCancelCallback = null)
        {
            materialEquipments = materials;
            onFusionConfirm = onConfirm;
            onCancel = onCancelCallback;

            if (materials.Count > 0)
            {
                baseEquipment = materials[0];
            }

            UpdateMaterialSlots();
            CalculateFusionResult();
            UpdateUI();
        }

        private void CreateMaterialSlots()
        {
            if (materialSlotPrefab == null || materialSlotsContainer == null) return;

            // 기존 슬롯 제거
            foreach (Transform child in materialSlotsContainer)
            {
                Destroy(child.gameObject);
            }
            materialSlots.Clear();

            // 새 슬롯 생성
            for (int i = 0; i < maxMaterialSlots; i++)
            {
                GameObject slotObj = Instantiate(materialSlotPrefab, materialSlotsContainer);
                FusionMaterialSlot slot = slotObj.GetComponent<FusionMaterialSlot>();
                
                if (slot != null)
                {
                    slot.OnSlotClicked += OnMaterialSlotClicked;
                    materialSlots.Add(slot);
                }
            }
        }

        private void UpdateMaterialSlots()
        {
            // 모든 슬롯 초기화
            foreach (var slot in materialSlots)
            {
                slot.Clear();
            }

            // 재료 장비 표시
            for (int i = 0; i < Mathf.Min(materialEquipments.Count, materialSlots.Count); i++)
            {
                materialSlots[i].SetEquipment(materialEquipments[i]);
            }
        }

        private void CalculateFusionResult()
        {
            if (materialEquipments.Count < 2)
            {
                ShowEmptyResult();
                return;
            }

            // 기본 장비 (첫 번째 아이템)
            baseEquipment = materialEquipments[0];

            // 성공 확률 계산
            successRate = CalculateSuccessRate();

            // 예상 결과 계산
            CalculateExpectedResult();

            // 비용 계산
            fusionCost = CalculateFusionCost();
        }

        private float CalculateSuccessRate()
        {
            // 기본 성공률
            float baseRate = 0.8f; // 80%

            // 재료 개수에 따른 보너스
            float materialBonus = (materialEquipments.Count - 2) * 0.05f; // 추가 재료당 5%

            // 등급에 따른 패널티
            float rarityPenalty = 0f;
            switch (baseEquipment.rarity)
            {
                case EquipmentRarity.Rare:
                    rarityPenalty = 0.1f;
                    break;
                case EquipmentRarity.Epic:
                    rarityPenalty = 0.2f;
                    break;
                case EquipmentRarity.Legendary:
                    rarityPenalty = 0.3f;
                    break;
                case EquipmentRarity.Mythic:
                    rarityPenalty = 0.4f;
                    break;
                case EquipmentRarity.Celestial:
                    rarityPenalty = 0.5f;
                    break;
            }

            // 같은 세부등급 재료 사용 시 보너스
            float sameGradeBonus = 0f;
            if (materialEquipments.All(e => e.subGrade == baseEquipment.subGrade))
            {
                sameGradeBonus = 0.1f; // 10%
            }

            return Mathf.Clamp01(baseRate + materialBonus - rarityPenalty + sameGradeBonus);
        }

        private void CalculateExpectedResult()
        {
            if (resultPreview != null)
            {
                resultPreview.SetActive(true);
            }

            if (resultQuestionMark != null)
            {
                resultQuestionMark.SetActive(false);
            }

            // 결과 아이콘
            if (resultIconImage != null && baseEquipment.icon != null)
            {
                resultIconImage.sprite = baseEquipment.icon;
                resultIconImage.enabled = true;
            }

            // 결과 이름
            if (resultNameText != null)
            {
                // 성공 시 세부등급 상승
                int expectedSubGrade = Mathf.Min(baseEquipment.subGrade + 1, 5);
                string stars = new string('★', expectedSubGrade);
                
                resultNameText.text = $"{baseEquipment.equipmentName} {stars}";
                resultNameText.color = baseEquipment.GetRarityColor();
            }

            // 예상 스탯
            if (resultStatsText != null)
            {
                string statsText = "예상 결과:\n";
                
                switch (baseEquipment.equipmentType)
                {
                    case EquipmentType.Weapon:
                        int currentAtk = baseEquipment.GetFinalAttackPower();
                        int expectedAtk = Mathf.RoundToInt(currentAtk * 1.2f); // 20% 증가
                        statsText += $"공격력: {currentAtk} → <color=green>{expectedAtk}</color>";
                        break;
                        
                    case EquipmentType.Armor:
                        int currentHp = baseEquipment.GetFinalMaxHp();
                        int expectedHp = Mathf.RoundToInt(currentHp * 1.2f);
                        statsText += $"최대 체력: {currentHp} → <color=green>{expectedHp}</color>";
                        break;
                        
                    case EquipmentType.Ring:
                        float currentRegen = baseEquipment.GetFinalHpRegen();
                        float expectedRegen = currentRegen * 1.2f;
                        statsText += $"체력 재생: {currentRegen:F1} → <color=green>{expectedRegen:F1}</color>";
                        break;
                }

                resultStatsText.text = statsText;
            }
        }

        private int CalculateFusionCost()
        {
            // 기본 비용
            int baseCost = 1000;

            // 등급별 배수
            int rarityMultiplier = (int)baseEquipment.rarity + 1;

            // 재료 개수별 추가 비용
            int materialCost = materialEquipments.Count * 500;

            return baseCost * rarityMultiplier + materialCost;
        }

        private void ShowEmptyResult()
        {
            if (resultPreview != null)
            {
                resultPreview.SetActive(false);
            }

            if (resultQuestionMark != null)
            {
                resultQuestionMark.SetActive(true);
            }

            successRate = 0;
            fusionCost = 0;
        }

        private void UpdateUI()
        {
            // 성공률 표시
            UpdateSuccessRateDisplay();

            // 비용 표시
            UpdateCostDisplay();

            // 버튼 상태
            UpdateButtonState();
        }

        private void UpdateSuccessRateDisplay()
        {
            if (successRateText != null)
            {
                successRateText.text = $"{(successRate * 100):F0}%";
                
                // 색상 설정
                if (successRate >= 0.8f)
                {
                    successRateText.color = Color.green;
                }
                else if (successRate >= 0.5f)
                {
                    successRateText.color = Color.yellow;
                }
                else
                {
                    successRateText.color = Color.red;
                }
            }

            if (successRateSlider != null)
            {
                successRateSlider.value = successRate;
            }

            if (successRateFillImage != null)
            {
                // 그라데이션 색상
                successRateFillImage.color = Color.Lerp(Color.red, Color.green, successRate);
            }
        }

        private void UpdateCostDisplay()
        {
            if (costText != null)
            {
                costText.text = $"{fusionCost:N0}";
                
                // TODO: 실제 골드와 비교하여 색상 설정
                bool canAfford = true; // 임시
                costText.color = canAfford ? Color.white : Color.red;
            }
        }

        private void UpdateButtonState()
        {
            if (fusionButton != null)
            {
                bool canFuse = materialEquipments.Count >= 2 && !isFusing;
                fusionButton.interactable = canFuse;

                if (fusionButtonText != null)
                {
                    fusionButtonText.text = isFusing ? "합성 중..." : "합성";
                }
            }
        }

        private void OnMaterialSlotClicked(FusionMaterialSlot slot)
        {
            // 재료 슬롯 클릭 시 제거
            int index = materialSlots.IndexOf(slot);
            if (index >= 0 && index < materialEquipments.Count)
            {
                RemoveMaterial(index);
            }
        }

        private void RemoveMaterial(int index)
        {
            if (index < 0 || index >= materialEquipments.Count) return;

            materialEquipments.RemoveAt(index);
            UpdateMaterialSlots();
            CalculateFusionResult();
            UpdateUI();
        }

        private void OnFusionButtonClicked()
        {
            if (isFusing || materialEquipments.Count < 2) return;

            // 합성 시작
            StartFusion();
        }

        private void StartFusion()
        {
            isFusing = true;
            UpdateButtonState();

            // 합성 애니메이션
            PlayFusionAnimation(() =>
            {
                // 성공/실패 판정
                bool success = Random.Range(0f, 1f) <= successRate;

                if (success)
                {
                    OnFusionSuccess();
                }
                else
                {
                    OnFusionFailed();
                }
            });
        }

        private void PlayFusionAnimation(System.Action onComplete)
        {
            // 재료 슬롯들이 중앙으로 모이는 애니메이션
            Sequence fusionSequence = DOTween.Sequence();

            // 각 재료 슬롯 애니메이션
            for (int i = 0; i < materialSlots.Count; i++)
            {
                if (i < materialEquipments.Count)
                {
                    Transform slotTransform = materialSlots[i].transform;
                    Vector3 targetPos = resultIconImage.transform.position;

                    fusionSequence.Join(
                        slotTransform.DOMove(targetPos, fusionAnimationDuration * 0.6f)
                            .SetEase(Ease.InQuad)
                    );

                    fusionSequence.Join(
                        slotTransform.DOScale(0f, fusionAnimationDuration * 0.6f)
                            .SetEase(Ease.InQuad)
                    );
                }
            }

            // 이펙트 생성
            if (fusionEffectPrefab != null && resultIconImage != null)
            {
                GameObject effect = Instantiate(fusionEffectPrefab, resultIconImage.transform.position, Quaternion.identity, transform);
                Destroy(effect, fusionAnimationDuration * 2f);
            }

            // 결과 표시
            fusionSequence.AppendInterval(0.5f);
            fusionSequence.AppendCallback(() =>
            {
                onComplete?.Invoke();
            });
        }

        private void OnFusionSuccess()
        {
            Debug.Log("합성 성공!");

            // 성공 파티클
            if (successParticle != null)
            {
                successParticle.Play();
            }

            // 성공 애니메이션
            if (resultIconImage != null)
            {
                resultIconImage.transform.DOKill();
                resultIconImage.transform.DOPunchScale(Vector3.one * 0.3f, 0.5f, 10, 1f);
            }

            // 콜백 실행
            onFusionConfirm?.Invoke(materialEquipments);

            // 1초 후 자동 닫기
            DOVirtual.DelayedCall(1.5f, () =>
            {
                Close();
            });
        }

        private void OnFusionFailed()
        {
            Debug.Log("합성 실패!");

            // 실패 파티클
            if (failParticle != null)
            {
                failParticle.Play();
            }

            // 실패 애니메이션
            if (resultIconImage != null)
            {
                resultIconImage.transform.DOKill();
                resultIconImage.transform.DOShakePosition(0.5f, 10f, 20);
            }

            // 실패 텍스트 표시
            if (resultNameText != null)
            {
                resultNameText.text = "합성 실패!";
                resultNameText.color = Color.red;
            }

            // 재료는 소멸
            onFusionConfirm?.Invoke(materialEquipments);

            // 2초 후 자동 닫기
            DOVirtual.DelayedCall(2f, () =>
            {
                Close();
            });
        }

        private void OnCancelButtonClicked()
        {
            onCancel?.Invoke();
            Close();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            // 버튼 이벤트 해제
            if (fusionButton != null)
            {
                fusionButton.onClick.RemoveListener(OnFusionButtonClicked);
            }

            if (cancelButton != null)
            {
                cancelButton.onClick.RemoveListener(OnCancelButtonClicked);
            }

            // 재료 슬롯 이벤트 해제
            foreach (var slot in materialSlots)
            {
                if (slot != null)
                {
                    slot.OnSlotClicked -= OnMaterialSlotClicked;
                }
            }
        }
    }

    /// <summary>
    /// 합성 재료 슬롯
    /// </summary>
    public class FusionMaterialSlot : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private Image rarityFrame;
        [SerializeField] private TextMeshProUGUI gradeText;
        [SerializeField] private Button removeButton;
        [SerializeField] private GameObject emptyState;

        private EquipmentData equipment;

        public event System.Action<FusionMaterialSlot> OnSlotClicked;

        private void Awake()
        {
            if (removeButton != null)
            {
                removeButton.onClick.AddListener(() => OnSlotClicked?.Invoke(this));
            }
        }

        public void SetEquipment(EquipmentData equipmentData)
        {
            equipment = equipmentData;

            if (equipment != null)
            {
                if (iconImage != null && equipment.icon != null)
                {
                    iconImage.sprite = equipment.icon;
                    iconImage.enabled = true;
                }

                if (rarityFrame != null)
                {
                    rarityFrame.color = equipment.GetRarityColor();
                }

                if (gradeText != null)
                {
                    gradeText.text = new string('★', equipment.subGrade);
                    gradeText.color = equipment.GetRarityColor();
                }

                if (emptyState != null)
                {
                    emptyState.SetActive(false);
                }
            }
        }

        public void Clear()
        {
            equipment = null;

            if (iconImage != null)
            {
                iconImage.enabled = false;
            }

            if (gradeText != null)
            {
                gradeText.text = "";
            }

            if (emptyState != null)
            {
                emptyState.SetActive(true);
            }
        }
    }
}