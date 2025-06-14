using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sirenix.OdinInspector;
using DG.Tweening;
using RPG.Items.Equipment;
using RPG.Items.Relic;
using RPG.Gacha.Base;

namespace RPG.UI.Gacha
{
    /// <summary>
    /// 가챠 결과 아이템 개별 표시 컴포넌트
    /// </summary>
    public class GachaResultItem : MonoBehaviour
    {
        [Title("UI 참조")]
        [SerializeField, Required]
        private Image itemIcon;

        [SerializeField, Required]
        private Image rarityFrame;

        [SerializeField, Required]
        private Image rarityBackground;

        [SerializeField]
        private Image glowEffect;

        [SerializeField, Required]
        private TextMeshProUGUI subGradeText; // 등급 + 세부등급 표시용

        [SerializeField]
        private GameObject newBadge;

        [Title("등급별 설정")]
        [SerializeField]
        private Sprite[] rarityFrameSprites; // 등급별 프레임 스프라이트

        [SerializeField]
        private Material rarityGlowMaterial; // 등급별 글로우 효과

        [Title("애니메이션 설정")]
        [SerializeField]
        private float revealDelay = 0.1f;

        [SerializeField]
        private float revealDuration = 0.5f;

        [SerializeField]
        private bool useRarityEffects = true;

        [ShowInInspector, ReadOnly]
        private IGachaItem currentItem;

        private CanvasGroup canvasGroup;
        private Vector3 originalScale;

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            originalScale = transform.localScale;

            // 초기 상태 숨김
            canvasGroup.alpha = 0;
            transform.localScale = Vector3.zero;

            // 아이콘 초기화
            if (itemIcon != null)
            {
                itemIcon.preserveAspect = true; // 아이콘 비율 유지
            }
        }

        /// <summary>
        /// 장비 아이템 설정
        /// </summary>
        public void SetupEquipment(EquipmentData equipment, int index = 0)
        {
            if (equipment == null)
            {
                Debug.LogError("[GachaResultItem] equipment이 null입니다!");
                return;
            }

            currentItem = equipment;

            // 아이콘 설정
            if (itemIcon != null)
            {
                if (equipment.icon != null)
                {
                    itemIcon.sprite = equipment.icon;
                    itemIcon.enabled = true;
                    itemIcon.color = Color.white;
                    Debug.Log($"[GachaResultItem] 아이콘 설정 완료: {equipment.equipmentName}");
                }
                else
                {
                    Debug.LogWarning($"[GachaResultItem] {equipment.equipmentName}의 아이콘이 없습니다!");
                    itemIcon.enabled = false;
                }
            }
            else
            {
                Debug.LogError("[GachaResultItem] itemIcon UI 컴포넌트가 할당되지 않았습니다!");
            }

            // 등급 + 세부등급 표시
            if (subGradeText != null)
            {
                // "영웅 3★" 형식으로 표시
                string rarityName = RarityColors.GetRarityName(equipment.rarity);
                subGradeText.text = $"{rarityName} {equipment.subGrade}★";
                subGradeText.color = equipment.GetRarityColor();
                subGradeText.gameObject.SetActive(true);
            }

            // 등급별 프레임 색상
            SetRarityVisuals(equipment.GetRarityColor(), (int)equipment.rarity);

            // 표시 애니메이션
            ShowWithAnimation(index * revealDelay);
        }

        /// <summary>
        /// 유물 아이템 설정
        /// </summary>
        public void SetupRelic(RelicData relic, int index = 0)
        {
            if (relic == null)
            {
                Debug.LogError("[GachaResultItem] relic이 null입니다!");
                return;
            }

            currentItem = relic;

            // 아이콘 설정
            if (itemIcon != null)
            {
                if (relic.icon != null)
                {
                    itemIcon.sprite = relic.icon;
                    itemIcon.enabled = true;
                    itemIcon.color = Color.white;
                    Debug.Log($"[GachaResultItem] 아이콘 설정 완료: {relic.relicName}");
                }
                else
                {
                    Debug.LogWarning($"[GachaResultItem] {relic.relicName}의 아이콘이 없습니다!");
                    itemIcon.enabled = false;
                }
            }
            else
            {
                Debug.LogError("[GachaResultItem] itemIcon UI 컴포넌트가 할당되지 않았습니다!");
            }

            // 유물은 등급만 표시 (세부 등급 없음)
            if (subGradeText != null)
            {
                string rarityName = RelicRarityColors.GetRarityName(relic.rarity);
                subGradeText.text = rarityName;
                subGradeText.color = relic.GetRarityColor();
                subGradeText.gameObject.SetActive(true);
            }

            // 등급별 프레임 색상
            SetRarityVisuals(relic.GetRarityColor(), (int)relic.rarity);

            // 표시 애니메이션
            ShowWithAnimation(index * revealDelay);
        }

        /// <summary>
        /// 등급별 비주얼 설정
        /// </summary>
        private void SetRarityVisuals(Color rarityColor, int rarityLevel)
        {
            // 프레임 색상
            if (rarityFrame != null)
            {
                rarityFrame.color = rarityColor;

                // 등급별 프레임 스프라이트가 있으면 사용
                if (rarityFrameSprites != null && rarityLevel < rarityFrameSprites.Length)
                {
                    var sprite = rarityFrameSprites[rarityLevel];
                    if (sprite != null)
                    {
                        rarityFrame.sprite = sprite;
                    }
                }
            }

            // 배경 색상 (투명도 적용)
            if (rarityBackground != null)
            {
                var bgColor = rarityColor;
                bgColor.a = 0.2f;
                rarityBackground.color = bgColor;
            }

            // 글로우 효과
            if (glowEffect != null && useRarityEffects)
            {
                glowEffect.color = rarityColor;

                // 고등급일수록 강한 글로우
                bool showGlow = rarityLevel >= 2; // Epic 이상
                glowEffect.gameObject.SetActive(showGlow);

                if (showGlow)
                {
                    // 글로우 애니메이션
                    glowEffect.DOFade(0.8f, 1f)
                        .SetLoops(-1, LoopType.Yoyo)
                        .SetEase(Ease.InOutSine);
                }
            }

            // NEW 배지 (확률적으로 표시)
            if (newBadge != null)
            {
                bool isNew = Random.Range(0f, 1f) < 0.3f; // 30% 확률로 NEW 표시
                newBadge.SetActive(isNew);
            }
        }

        /// <summary>
        /// 표시 애니메이션
        /// </summary>
        private void ShowWithAnimation(float delay)
        {
            // 초기 상태
            canvasGroup.alpha = 0;
            transform.localScale = Vector3.zero;

            // 시퀀스 생성
            Sequence showSequence = DOTween.Sequence();
            showSequence.SetDelay(delay);

            // 스케일 애니메이션
            showSequence.Append(transform.DOScale(originalScale * 1.2f, revealDuration * 0.6f)
                .SetEase(Ease.OutBack));

            showSequence.Join(canvasGroup.DOFade(1f, revealDuration * 0.4f));

            showSequence.Append(transform.DOScale(originalScale, revealDuration * 0.4f)
                .SetEase(Ease.OutElastic));

            // 고등급 아이템 추가 효과
            if (currentItem != null && currentItem.GetRarityLevel() >= 3) // Epic 이상
            {
                showSequence.OnComplete(() =>
                {
                    // 반짝임 효과
                    transform.DOPunchScale(originalScale * 0.1f, 0.3f, 10, 1f);

                    // 회전 효과
                    if (itemIcon != null)
                    {
                        itemIcon.transform.DORotate(new Vector3(0, 0, 360), 1f, RotateMode.FastBeyond360)
                            .SetEase(Ease.OutQuad);
                    }
                });
            }
        }


        /// <summary>
        /// 아이템 클릭 시 상세 정보 표시
        /// </summary>
        public void OnItemClicked()
        {
            if (currentItem == null) return;

            // 클릭 애니메이션
            transform.DOKill();
            transform.DOPunchScale(originalScale * 0.1f, 0.2f, 10, 1f);

            // TODO: 아이템 상세 정보 팝업 표시
            Debug.Log($"아이템 클릭: {currentItem.ItemName}");
        }

        private void OnDestroy()
        {
            transform.DOKill();
            if (canvasGroup != null) canvasGroup.DOKill();
            if (glowEffect != null) glowEffect.DOKill();
            if (itemIcon != null) itemIcon.transform.DOKill();
        }
    }
}