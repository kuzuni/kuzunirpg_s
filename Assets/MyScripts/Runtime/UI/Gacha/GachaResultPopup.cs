using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using DG.Tweening;
using RPG.UI.Popup;
using RPG.Items.Equipment;
using RPG.Items.Relic;
using System.Linq;
using System;

namespace RPG.UI.Gacha
{
    /// <summary>
    /// 가챠 결과를 표시하는 팝업 UI
    /// </summary>
    public class GachaResultPopup : PopupUI
    {
        [Title("UI 참조")]
        [SerializeField, Required]
        private Transform resultContainer;

        [SerializeField, Required]
        private GameObject resultItemPrefab;

        // titleText 제거됨

        [SerializeField]
        private Button confirmButton;

        [SerializeField]
        private Button pullAgainButton;

        [SerializeField]
        private TextMeshProUGUI pullAgainCostText;

        [Title("레이아웃 설정")]
        [SerializeField]
        private int maxItemsPerRow = 5;

        [SerializeField]
        private float itemSpacing = 10f;

        [SerializeField]
        private bool autoArrangeGrid = true;

        [Title("특수 효과")]
        [SerializeField]
        private GameObject epicEffectPrefab; // Epic 이상 획득 시 효과

        [SerializeField]
        private GameObject legendaryEffectPrefab; // Legendary 이상 획득 시 효과

        [SerializeField]
        private ParticleSystem confettiEffect; // 축하 효과

        [Title("사운드")]
        [SerializeField]
        private AudioClip normalRevealSound;

        [SerializeField]
        private AudioClip rareRevealSound;

        [SerializeField]
        private AudioClip epicRevealSound;

        [SerializeField]
        private AudioClip legendaryRevealSound;

        // 상태
        private List<GachaResultItem> currentResultItems = new List<GachaResultItem>();
        private Action onPullAgainCallback;
        private AudioSource audioSource;
        private bool isShowingResults = false;

        protected override void Awake()
        {
            base.Awake();

            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }

            // 버튼 이벤트 설정
            if (confirmButton != null)
            {
                confirmButton.onClick.RemoveAllListeners();
                confirmButton.onClick.AddListener(OnConfirmClicked);
            }

            if (pullAgainButton != null)
            {
                pullAgainButton.onClick.RemoveAllListeners();
                pullAgainButton.onClick.AddListener(OnPullAgainClicked);
            }
        }

        /// <summary>
        /// 장비 가챠 결과 표시
        /// </summary>
        public void ShowEquipmentResults(List<EquipmentData> results, Action onPullAgain = null)
        {
            if (results == null || results.Count == 0) return;

            ClearPreviousResults();

            // titleText 설정 제거됨

            onPullAgainCallback = onPullAgain;
            isShowingResults = true;

            // 등급별로 정렬 (높은 등급부터)
            var sortedResults = results.OrderByDescending(e => e.rarity)
                                     .ThenByDescending(e => e.subGrade)
                                     .ToList();

            // 결과 아이템 생성
            StartCoroutine(CreateEquipmentResultItems(sortedResults));

            // 특수 효과 체크
            CheckAndPlaySpecialEffects(sortedResults);
        }

        /// <summary>
        /// 유물 가챠 결과 표시
        /// </summary>
        public void ShowRelicResults(List<RelicData> results, Action onPullAgain = null)
        {
            if (results == null || results.Count == 0) return;

            ClearPreviousResults();

            // titleText 설정 제거됨

            onPullAgainCallback = onPullAgain;
            isShowingResults = true;

            // 등급별로 정렬 (높은 등급부터)
            var sortedResults = results.OrderByDescending(r => r.rarity).ToList();

            // 결과 아이템 생성
            StartCoroutine(CreateRelicResultItems(sortedResults));

            // 특수 효과 체크
            CheckAndPlaySpecialEffectsForRelics(sortedResults);
        }

        /// <summary>
        /// 장비 결과 아이템 생성
        /// </summary>
        private System.Collections.IEnumerator CreateEquipmentResultItems(List<EquipmentData> equipments)
        {
            // 버튼 비활성화
            SetButtonsInteractable(false);

            // 그리드 레이아웃 설정
            if (autoArrangeGrid)
            {
                SetupGridLayout(equipments.Count);
            }

            int index = 0;
            foreach (var equipment in equipments)
            {
                GameObject itemObj = Instantiate(resultItemPrefab, resultContainer);
                GachaResultItem resultItem = itemObj.GetComponent<GachaResultItem>();

                if (resultItem != null)
                {
                    resultItem.SetupEquipment(equipment, index);
                    currentResultItems.Add(resultItem);

                    // 사운드 재생
                    PlayRevealSound(equipment.rarity);
                }

                index++;
                yield return new WaitForSeconds(0.1f); // 순차적 표시
            }

            // 모든 아이템 표시 완료
            yield return new WaitForSeconds(0.5f);

            // 버튼 활성화
            SetButtonsInteractable(true);

            // 다시 뽑기 버튼 설정
            UpdatePullAgainButton();
        }

        /// <summary>
        /// 유물 결과 아이템 생성
        /// </summary>
        private System.Collections.IEnumerator CreateRelicResultItems(List<RelicData> relics)
        {
            // 버튼 비활성화
            SetButtonsInteractable(false);

            // 그리드 레이아웃 설정
            if (autoArrangeGrid)
            {
                SetupGridLayout(relics.Count);
            }

            int index = 0;
            foreach (var relic in relics)
            {
                GameObject itemObj = Instantiate(resultItemPrefab, resultContainer);
                GachaResultItem resultItem = itemObj.GetComponent<GachaResultItem>();

                if (resultItem != null)
                {
                    resultItem.SetupRelic(relic, index);
                    currentResultItems.Add(resultItem);

                    // 사운드 재생
                    PlayRevealSoundForRelic(relic.rarity);
                }

                index++;
                yield return new WaitForSeconds(0.1f); // 순차적 표시
            }

            // 모든 아이템 표시 완료
            yield return new WaitForSeconds(0.5f);

            // 버튼 활성화
            SetButtonsInteractable(true);

            // 다시 뽑기 버튼 설정
            UpdatePullAgainButton();
        }

        /// <summary>
        /// 그리드 레이아웃 자동 설정
        /// </summary>
        private void SetupGridLayout(int itemCount)
        {
            GridLayoutGroup gridLayout = resultContainer.GetComponent<GridLayoutGroup>();
            if (gridLayout == null)
            {
                gridLayout = resultContainer.gameObject.AddComponent<GridLayoutGroup>();
            }

            // 아이템 개수에 따라 그리드 설정
            if (itemCount <= 1)
            {
                gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                gridLayout.constraintCount = 1;
                gridLayout.cellSize = new Vector2(200, 250);
            }
            else if (itemCount <= 5)
            {
                gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                gridLayout.constraintCount = itemCount;
                gridLayout.cellSize = new Vector2(150, 200);
            }
            else if (itemCount <= 11)
            {
                gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                gridLayout.constraintCount = Math.Min(itemCount, maxItemsPerRow);
                gridLayout.cellSize = new Vector2(120, 160);
            }
            else
            {
                gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                gridLayout.constraintCount = maxItemsPerRow;
                gridLayout.cellSize = new Vector2(100, 140);
            }

            gridLayout.spacing = new Vector2(itemSpacing, itemSpacing);
            gridLayout.childAlignment = TextAnchor.MiddleCenter;
        }

        /// <summary>
        /// 특수 효과 체크 및 재생 (장비)
        /// </summary>
        private void CheckAndPlaySpecialEffects(List<EquipmentData> equipments)
        {
            bool hasEpic = equipments.Any(e => e.rarity >= EquipmentRarity.Epic);
            bool hasLegendary = equipments.Any(e => e.rarity >= EquipmentRarity.Legendary);

            if (hasLegendary && legendaryEffectPrefab != null)
            {
                Instantiate(legendaryEffectPrefab, transform);
                PlayConfettiEffect();
            }
            else if (hasEpic && epicEffectPrefab != null)
            {
                Instantiate(epicEffectPrefab, transform);
            }
        }

        /// <summary>
        /// 특수 효과 체크 및 재생 (유물)
        /// </summary>
        private void CheckAndPlaySpecialEffectsForRelics(List<RelicData> relics)
        {
            bool hasEpic = relics.Any(r => r.rarity >= RelicRarity.Epic);
            bool hasLegendary = relics.Any(r => r.rarity >= RelicRarity.Legendary);

            if (hasLegendary && legendaryEffectPrefab != null)
            {
                Instantiate(legendaryEffectPrefab, transform);
                PlayConfettiEffect();
            }
            else if (hasEpic && epicEffectPrefab != null)
            {
                Instantiate(epicEffectPrefab, transform);
            }
        }

        /// <summary>
        /// 축하 효과 재생
        /// </summary>
        private void PlayConfettiEffect()
        {
            if (confettiEffect != null)
            {
                confettiEffect.Play();
            }
        }

        /// <summary>
        /// 등급별 사운드 재생 (장비)
        /// </summary>
        private void PlayRevealSound(EquipmentRarity rarity)
        {
            AudioClip soundToPlay = null;

            switch (rarity)
            {
                case EquipmentRarity.Common:
                case EquipmentRarity.Uncommon:
                    soundToPlay = normalRevealSound;
                    break;
                case EquipmentRarity.Rare:
                    soundToPlay = rareRevealSound;
                    break;
                case EquipmentRarity.Epic:
                    soundToPlay = epicRevealSound;
                    break;
                case EquipmentRarity.Legendary:
                case EquipmentRarity.Mythic:
                case EquipmentRarity.Celestial:
                    soundToPlay = legendaryRevealSound;
                    break;
            }

            if (soundToPlay != null && audioSource != null)
            {
                audioSource.PlayOneShot(soundToPlay);
            }
        }

        /// <summary>
        /// 등급별 사운드 재생 (유물)
        /// </summary>
        private void PlayRevealSoundForRelic(RelicRarity rarity)
        {
            AudioClip soundToPlay = null;

            switch (rarity)
            {
                case RelicRarity.Common:
                    soundToPlay = normalRevealSound;
                    break;
                case RelicRarity.Rare:
                    soundToPlay = rareRevealSound;
                    break;
                case RelicRarity.Epic:
                    soundToPlay = epicRevealSound;
                    break;
                case RelicRarity.Legendary:
                    soundToPlay = legendaryRevealSound;
                    break;
            }

            if (soundToPlay != null && audioSource != null)
            {
                audioSource.PlayOneShot(soundToPlay);
            }
        }

        /// <summary>
        /// 이전 결과 정리
        /// </summary>
        private void ClearPreviousResults()
        {
            foreach (var item in currentResultItems)
            {
                if (item != null)
                {
                    Destroy(item.gameObject);
                }
            }
            currentResultItems.Clear();
        }

        /// <summary>
        /// 버튼 활성화/비활성화
        /// </summary>
        private void SetButtonsInteractable(bool interactable)
        {
            if (confirmButton != null)
            {
                confirmButton.interactable = interactable;
            }

            if (pullAgainButton != null)
            {
                pullAgainButton.interactable = interactable;
            }
        }

        /// <summary>
        /// 다시 뽑기 버튼 업데이트
        /// </summary>
        private void UpdatePullAgainButton()
        {
            if (pullAgainButton == null) return;

            // 콜백이 있으면 버튼 표시
            pullAgainButton.gameObject.SetActive(onPullAgainCallback != null);

            // TODO: 비용 표시 업데이트
            if (pullAgainCostText != null)
            {
                pullAgainCostText.text = "1,000"; // 실제 비용으로 변경 필요
            }
        }

        /// <summary>
        /// 확인 버튼 클릭
        /// </summary>
        private void OnConfirmClicked()
        {
            if (!isShowingResults) return;

            // 버튼 애니메이션
            confirmButton.transform.DOKill();
            confirmButton.transform.DOPunchScale(Vector3.one * 0.1f, 0.2f, 10, 1f);

            // 팝업 닫기
            Close();
        }

        /// <summary>
        /// 다시 뽑기 버튼 클릭
        /// </summary>
        private void OnPullAgainClicked()
        {
            if (!isShowingResults) return;

            // 버튼 애니메이션
            pullAgainButton.transform.DOKill();
            pullAgainButton.transform.DOPunchScale(Vector3.one * 0.1f, 0.2f, 10, 1f);

            // 콜백 실행
            onPullAgainCallback?.Invoke();

            // 팝업 닫기
            Close();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            // 버튼 이벤트 해제
            if (confirmButton != null)
            {
                confirmButton.onClick.RemoveListener(OnConfirmClicked);
            }

            if (pullAgainButton != null)
            {
                pullAgainButton.onClick.RemoveListener(OnPullAgainClicked);
            }

            // 애니메이션 정리
            if (confirmButton != null) confirmButton.transform.DOKill();
            if (pullAgainButton != null) pullAgainButton.transform.DOKill();
        }

        [Title("테스트")]
        [Button("테스트 결과 표시 (장비)", ButtonSizes.Large)]
        private void TestShowEquipmentResults()
        {
            if (!Application.isPlaying) return;

            // 테스트용 장비 생성
            var testResults = new List<EquipmentData>();
            // 실제로는 Resources에서 로드하거나 생성해야 함

            ShowEquipmentResults(testResults);
        }
    }
}