using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using DG.Tweening;
using RPG.Gacha;
using RPG.Items.Equipment;
using RPG.Items.Relic;
using RPG.UI.Popup;
using RPG.Managers;
using RPG.Common;
using RPG.UI.Components;

namespace RPG.UI.Gacha
{
    /// <summary>
    /// 가챠 UI를 관리하는 메인 컨트롤러
    /// </summary>
    public class GachaUIController : MonoBehaviour
    {
        [Title("가챠 시스템 참조")]
        [SerializeField]
        [InfoBox("프리팹인 경우 자동으로 찾습니다", InfoMessageType.Info)]
        private EquipmentGachaSystem equipmentGachaSystem;

        [SerializeField]
        private RelicGachaSystem relicGachaSystem;

        [Title("UI 참조")]
        [TabGroup("Tab", "장비 가챠")]
        [SerializeField, Required]
        private Button equipmentPull1Button;

        [TabGroup("Tab", "장비 가챠")]
        [SerializeField, Required]
        private Button equipmentPull11Button;

        [TabGroup("Tab", "장비 가챠")]
        [SerializeField, Required]
        private Button equipmentPull55Button;

        [TabGroup("Tab", "유물 가챠")]
        [SerializeField, Required]
        private Button relicPull1Button;

        [TabGroup("Tab", "유물 가챠")]
        [SerializeField, Required]
        private Button relicPull10Button;

        [Title("비용 텍스트")]
        [TabGroup("Tab", "장비 가챠")]
        [SerializeField]
        private TextMeshProUGUI equipmentCost1Text;

        [TabGroup("Tab", "장비 가챠")]
        [SerializeField]
        private TextMeshProUGUI equipmentCost11Text;

        [TabGroup("Tab", "장비 가챠")]
        [SerializeField]
        private TextMeshProUGUI equipmentCost55Text;

        [TabGroup("Tab", "유물 가챠")]
        [SerializeField]
        private TextMeshProUGUI relicCost1Text;

        [TabGroup("Tab", "유물 가챠")]
        [SerializeField]
        private TextMeshProUGUI relicCost10Text;

        [Title("천장 표시")]
        [SerializeField]
        private Slider equipmentPitySlider;

        [SerializeField]
        private TextMeshProUGUI equipmentPityText;

        [SerializeField]
        private Slider relicPitySlider;

        [SerializeField]
        private TextMeshProUGUI relicPityText;

        [Title("팝업 설정")]
        [SerializeField]
        [InfoBox("프리팹인 경우 자동으로 찾습니다", InfoMessageType.Info)]
        private PopupManager popupManager;

        [SerializeField, Required]
        private GameObject gachaResultPopupPrefab;

        [Title("가챠 비용 설정")]
        [BoxGroup("Cost")]
        [SerializeField]
        private int equipmentCost1 = 300;

        [BoxGroup("Cost")]
        [SerializeField]
        private int equipmentCost11 = 3000;

        [BoxGroup("Cost")]
        [SerializeField]
        private int equipmentCost55 = 14000;

        [BoxGroup("Cost")]
        [SerializeField]
        private int relicCost1 = 200;

        [BoxGroup("Cost")]
        [SerializeField]
        private int relicCost10 = 1800;

        [Title("애니메이션 설정")]
        [SerializeField]
        private bool animateButtons = true;

        [SerializeField]
        private float buttonPressScale = 0.95f;

        // 매니저 참조
        private CurrencyManager currencyManager;
        private GachaResultPopup currentResultPopup;

        // 상태
        private bool isPulling = false;

        private void Start()
        {
            // 시스템 자동 찾기
            FindRequiredSystems();

            // 버튼 이벤트 설정
            SetupButtons();

            // 비용 텍스트 업데이트
            UpdateCostTexts();

            // 천장 표시 업데이트
            UpdatePityDisplays();
        }

        private void FindRequiredSystems()
        {
            // 가챠 시스템 찾기
            if (equipmentGachaSystem == null)
            {
                equipmentGachaSystem = FindObjectOfType<EquipmentGachaSystem>();
                if (equipmentGachaSystem == null)
                {
                    Debug.LogError("[GachaUIController] EquipmentGachaSystem을 찾을 수 없습니다!");
                }
            }

            if (relicGachaSystem == null)
            {
                relicGachaSystem = FindObjectOfType<RelicGachaSystem>();
                if (relicGachaSystem == null)
                {
                    Debug.LogError("[GachaUIController] RelicGachaSystem을 찾을 수 없습니다!");
                }
            }

            // PopupManager 찾기
            if (popupManager == null)
            {
                popupManager = FindObjectOfType<PopupManager>();
                if (popupManager == null)
                {
                    Debug.LogError("[GachaUIController] PopupManager를 찾을 수 없습니다!");
                }
            }

            // CurrencyManager 찾기
            if (currencyManager == null)
            {
                currencyManager = FindObjectOfType<CurrencyManager>();
                if (currencyManager == null)
                {
                    Debug.LogError("[GachaUIController] CurrencyManager를 찾을 수 없습니다!");
                }
            }

            // 찾은 시스템 로그
            Debug.Log($"[GachaUIController] 시스템 찾기 완료:\n" +
                     $"- EquipmentGachaSystem: {(equipmentGachaSystem != null ? "찾음" : "없음")}\n" +
                     $"- RelicGachaSystem: {(relicGachaSystem != null ? "찾음" : "없음")}\n" +
                     $"- PopupManager: {(popupManager != null ? "찾음" : "없음")}\n" +
                     $"- CurrencyManager: {(currencyManager != null ? "찾음" : "없음")}");
        }

        private void SetupButtons()
        {
            // 장비 가챠 버튼
            if (equipmentPull1Button != null)
            {
                equipmentPull1Button.onClick.RemoveAllListeners();
                equipmentPull1Button.onClick.AddListener(() => OnEquipmentPull(1));
                AddButtonAnimation(equipmentPull1Button);
            }

            if (equipmentPull11Button != null)
            {
                equipmentPull11Button.onClick.RemoveAllListeners();
                equipmentPull11Button.onClick.AddListener(() => OnEquipmentPull(11));
                AddButtonAnimation(equipmentPull11Button);
            }

            if (equipmentPull55Button != null)
            {
                equipmentPull55Button.onClick.RemoveAllListeners();
                equipmentPull55Button.onClick.AddListener(() => OnEquipmentPull(55));
                AddButtonAnimation(equipmentPull55Button);
            }

            // 유물 가챠 버튼
            if (relicPull1Button != null)
            {
                relicPull1Button.onClick.RemoveAllListeners();
                relicPull1Button.onClick.AddListener(() => OnRelicPull(1));
                AddButtonAnimation(relicPull1Button);
            }

            if (relicPull10Button != null)
            {
                relicPull10Button.onClick.RemoveAllListeners();
                relicPull10Button.onClick.AddListener(() => OnRelicPull(10));
                AddButtonAnimation(relicPull10Button);
            }
        }

        /// <summary>
        /// 버튼에 애니메이션 추가
        /// </summary>
        private void AddButtonAnimation(Button button)
        {
            if (!animateButtons || button == null) return;

            // 기존 컴포넌트 확인
            var pressEffect = button.GetComponent<ButtonPressEffect>();
            if (pressEffect == null)
            {
                pressEffect = button.gameObject.AddComponent<ButtonPressEffect>();
                pressEffect.pressScale = buttonPressScale;
            }
        }

        /// <summary>
        /// 장비 가챠 실행
        /// </summary>
        private void OnEquipmentPull(int count)
        {
            if (isPulling) return;

            // 시스템 체크
            if (equipmentGachaSystem == null)
            {
                Debug.LogError("[GachaUIController] EquipmentGachaSystem이 없습니다!");
                return;
            }

            // 비용 확인
            int cost = GetEquipmentCost(count);
            if (!CanAfford(cost))
            {
                ShowNotEnoughMoneyPopup();
                return;
            }

            isPulling = true;

            // 비용 차감
            currencyManager.TrySpend(CurrencyType.Diamond, cost);

            // 가챠 실행
            List<EquipmentData> results = null;
            switch (count)
            {
                case 1:
                    var single = equipmentGachaSystem.PullSingle();
                    results = new List<EquipmentData> { single };
                    break;
                case 11:
                    results = equipmentGachaSystem.Pull11();
                    break;
                case 55:
                    results = equipmentGachaSystem.Pull55();
                    break;
            }

            // 결과 표시
            ShowEquipmentResults(results, count);

            // 천장 업데이트
            UpdatePityDisplays();
        }

        /// <summary>
        /// 유물 가챠 실행
        /// </summary>
        private void OnRelicPull(int count)
        {
            if (isPulling) return;

            // 시스템 체크
            if (relicGachaSystem == null)
            {
                Debug.LogError("[GachaUIController] RelicGachaSystem이 없습니다!");
                return;
            }

            // 비용 확인
            int cost = GetRelicCost(count);
            if (!CanAfford(cost))
            {
                ShowNotEnoughMoneyPopup();
                return;
            }

            isPulling = true;

            // 비용 차감
            currencyManager.TrySpend(CurrencyType.Diamond, cost);

            // 가챠 실행
            List<RelicData> results = null;
            switch (count)
            {
                case 1:
                    var single = relicGachaSystem.PullSingle();
                    results = new List<RelicData> { single };
                    break;
                case 10:
                    results = relicGachaSystem.Pull10();
                    break;
            }

            // 결과 표시
            ShowRelicResults(results, count);

            // 천장 업데이트
            UpdatePityDisplays();
        }

        /// <summary>
        /// 장비 가챠 결과 표시
        /// </summary>
        private void ShowEquipmentResults(List<EquipmentData> results, int pullCount)
        {
            if (results == null || results.Count == 0)
            {
                isPulling = false;
                return;
            }

            // PopupManager 사용 가능한 경우
            if (popupManager != null)
            {
                Debug.Log("[GachaUIController] PopupManager로 가챠 결과 팝업 표시");

                var popup = popupManager.Pop(
                    PopupManager.PopupType.GachaResult,
                    onOpen: () =>
                    {
                        // 생성된 팝업에서 GachaResultPopup 컴포넌트 찾기
                        currentResultPopup = popupManager.popupContainer.GetComponentInChildren<GachaResultPopup>();

                        if (currentResultPopup != null)
                        {
                            string title = GetPullTitle("장비", pullCount);
                            currentResultPopup.ShowEquipmentResults(
                                results,
                                title,
                                onPullAgain: () => OnEquipmentPull(pullCount)
                            );
                            Debug.Log("[GachaUIController] 장비 결과 표시 완료");
                        }
                        else
                        {
                            Debug.LogError("[GachaUIController] GachaResultPopup 컴포넌트를 찾을 수 없습니다!");
                        }
                    },
                    onClose: () =>
                    {
                        isPulling = false;
                        currentResultPopup = null;
                        Debug.Log("[GachaUIController] 가챠 결과 팝업 닫기 완료");
                    }
                );
            }
            else if (gachaResultPopupPrefab != null)
            {
                // PopupManager가 없으면 직접 생성
                Debug.Log("[GachaUIController] 직접 가챠 결과 팝업 생성");

                Canvas canvas = FindObjectOfType<Canvas>();
                if (canvas != null)
                {
                    GameObject popupObj = Instantiate(gachaResultPopupPrefab, canvas.transform);
                    currentResultPopup = popupObj.GetComponent<GachaResultPopup>();

                    if (currentResultPopup != null)
                    {
                        string title = GetPullTitle("장비", pullCount);
                        currentResultPopup.ShowEquipmentResults(
                            results,
                            title,
                            onPullAgain: () => OnEquipmentPull(pullCount)
                        );

                        currentResultPopup.Open(onClose: () =>
                        {
                            isPulling = false;
                            currentResultPopup = null;
                        });
                    }
                }
                else
                {
                    Debug.LogError("[GachaUIController] Canvas를 찾을 수 없습니다!");
                    isPulling = false;
                }
            }
            else
            {
                Debug.LogError("[GachaUIController] 팝업을 표시할 방법이 없습니다!");
                isPulling = false;
            }
        }

        /// <summary>
        /// 유물 가챠 결과 표시
        /// </summary>
        private void ShowRelicResults(List<RelicData> results, int pullCount)
        {
            if (results == null || results.Count == 0)
            {
                isPulling = false;
                return;
            }

            // 결과 팝업 표시
            if (gachaResultPopupPrefab != null)
            {
                Debug.Log($"[GachaUIController] 유물 가챠 결과 표시 시작 - 아이템 개수: {results.Count}");

                // PopupManager 사용 가능한 경우
                if (popupManager != null)
                {
                    // PopupManager의 컨테이너에 생성
                    GameObject popupObj = Instantiate(gachaResultPopupPrefab, popupManager.popupContainer);
                    currentResultPopup = popupObj.GetComponent<GachaResultPopup>();
                }
                else
                {
                    // PopupManager가 없으면 Canvas 찾아서 생성
                    Canvas canvas = FindObjectOfType<Canvas>();
                    if (canvas != null)
                    {
                        GameObject popupObj = Instantiate(gachaResultPopupPrefab, canvas.transform);
                        currentResultPopup = popupObj.GetComponent<GachaResultPopup>();
                        Debug.Log($"[GachaUIController] Canvas에 직접 팝업 생성: {canvas.name}");
                    }
                    else
                    {
                        Debug.LogError("[GachaUIController] Canvas를 찾을 수 없습니다!");
                        isPulling = false;
                        return;
                    }
                }

                if (currentResultPopup != null)
                {
                    string title = GetPullTitle("유물", pullCount);

                    // 결과 표시
                    currentResultPopup.ShowRelicResults(
                        results,
                        title,
                        onPullAgain: () => OnRelicPull(pullCount) // 다시 뽑기 콜백
                    );

                    // 팝업 열기
                    currentResultPopup.Open(onClose: () =>
                    {
                        isPulling = false;
                        currentResultPopup = null;
                    });

                    Debug.Log("[GachaUIController] 유물 가챠 팝업 열기 완료");
                }
                else
                {
                    Debug.LogError("[GachaUIController] GachaResultPopup 컴포넌트를 찾을 수 없습니다!");
                }
            }
            else
            {
                Debug.LogError("[GachaUIController] gachaResultPopupPrefab이 할당되지 않았습니다!");
            }

            isPulling = false;
        }

        /// <summary>
        /// 뽑기 제목 생성
        /// </summary>
        private string GetPullTitle(string type, int count)
        {
            if (count == 1)
                return $"{type} 뽑기 결과";
            else
                return $"{type} {count}연차 결과";
        }

        /// <summary>
        /// 비용 확인
        /// </summary>
        private bool CanAfford(int cost)
        {
            return currencyManager != null && currencyManager.CanAfford(CurrencyType.Diamond, cost);
        }

        /// <summary>
        /// 장비 가챠 비용 계산
        /// </summary>
        private int GetEquipmentCost(int count)
        {
            switch (count)
            {
                case 1: return equipmentCost1;
                case 11: return equipmentCost11;
                case 55: return equipmentCost55;
                default: return 0;
            }
        }

        /// <summary>
        /// 유물 가챠 비용 계산
        /// </summary>
        private int GetRelicCost(int count)
        {
            switch (count)
            {
                case 1: return relicCost1;
                case 10: return relicCost10;
                default: return 0;
            }
        }

        /// <summary>
        /// 비용 텍스트 업데이트
        /// </summary>
        private void UpdateCostTexts()
        {
            // 장비 가챠 비용
            if (equipmentCost1Text != null)
                equipmentCost1Text.text = $"{equipmentCost1:N0}";

            if (equipmentCost11Text != null)
                equipmentCost11Text.text = $"{equipmentCost11:N0}";

            if (equipmentCost55Text != null)
                equipmentCost55Text.text = $"{equipmentCost55:N0}";

            // 유물 가챠 비용
            if (relicCost1Text != null)
                relicCost1Text.text = $"{relicCost1:N0}";

            if (relicCost10Text != null)
                relicCost10Text.text = $"{relicCost10:N0}";
        }

        /// <summary>
        /// 천장 표시 업데이트
        /// </summary>
        private void UpdatePityDisplays()
        {
            // 장비 천장
            if (equipmentGachaSystem != null)
            {
                float equipmentPityProgress = equipmentGachaSystem.GetPityProgress();
                int equipmentPityCount = equipmentGachaSystem.GetCurrentPityCount();

                if (equipmentPitySlider != null)
                {
                    equipmentPitySlider.value = equipmentPityProgress;
                }

                if (equipmentPityText != null)
                {
                    equipmentPityText.text = $"{equipmentPityCount}/90";
                }
            }

            // 유물 천장
            if (relicGachaSystem != null)
            {
                float relicPityProgress = relicGachaSystem.GetPityProgress();
                int relicPityCount = relicGachaSystem.GetCurrentPityCount();

                if (relicPitySlider != null)
                {
                    relicPitySlider.value = relicPityProgress;
                }

                if (relicPityText != null)
                {
                    relicPityText.text = $"{relicPityCount}/50";
                }
            }
        }

        /// <summary>
        /// 돈 부족 팝업 표시
        /// </summary>
        private void ShowNotEnoughMoneyPopup()
        {
            Debug.Log("다이아몬드가 부족합니다!");
            // TODO: 실제 팝업 표시
        }

        private void OnDestroy()
        {
            // 버튼 이벤트 해제
            if (equipmentPull1Button != null) equipmentPull1Button.onClick.RemoveAllListeners();
            if (equipmentPull11Button != null) equipmentPull11Button.onClick.RemoveAllListeners();
            if (equipmentPull55Button != null) equipmentPull55Button.onClick.RemoveAllListeners();
            if (relicPull1Button != null) relicPull1Button.onClick.RemoveAllListeners();
            if (relicPull10Button != null) relicPull10Button.onClick.RemoveAllListeners();
        }

        [Title("디버그")]
        [Button("시스템 다시 찾기", ButtonSizes.Large)]
        [GUIColor(0.3f, 0.8f, 0.8f)]
        private void RefindSystems()
        {
            FindRequiredSystems();
        }

        [Button("장비 1회 뽑기 테스트", ButtonSizes.Large)]
        private void TestEquipment1Pull()
        {
            if (!Application.isPlaying) return;
            OnEquipmentPull(1);
        }

        [Button("장비 11회 뽑기 테스트", ButtonSizes.Large)]
        private void TestEquipment11Pull()
        {
            if (!Application.isPlaying) return;
            OnEquipmentPull(11);
        }

        [Button("유물 10회 뽑기 테스트", ButtonSizes.Large)]
        private void TestRelic10Pull()
        {
            if (!Application.isPlaying) return;
            OnRelicPull(10);
        }
    }

    /// <summary>
    /// 버튼 클릭 애니메이션 컴포넌트
    /// </summary>
    public class ButtonPressEffect : MonoBehaviour, UnityEngine.EventSystems.IPointerDownHandler, UnityEngine.EventSystems.IPointerUpHandler
    {
        public float pressScale = 0.95f;
        public float duration = 0.1f;

        private Vector3 originalScale;

        private void Awake()
        {
            originalScale = transform.localScale;
        }

        public void OnPointerDown(UnityEngine.EventSystems.PointerEventData eventData)
        {
            transform.DOScale(originalScale * pressScale, duration).SetEase(Ease.OutQuad);
        }

        public void OnPointerUp(UnityEngine.EventSystems.PointerEventData eventData)
        {
            transform.DOScale(originalScale, duration).SetEase(Ease.OutBack);
        }
    }
}