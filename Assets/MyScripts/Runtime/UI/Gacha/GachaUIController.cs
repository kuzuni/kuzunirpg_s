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
        [Title("가챠 모드")]
        [EnumToggleButtons]
        [OnValueChanged("OnGachaModeChanged")]
        [SerializeField]
        private GachaMode currentGachaMode = GachaMode.Equipment;

        public enum GachaMode
        {
            Equipment,
            Relic
        }

        [Title("가챠 시스템 참조")]
        [SerializeField]
        [InfoBox("프리팹인 경우 자동으로 찾습니다", InfoMessageType.Info)]
        private EquipmentGachaSystem equipmentGachaSystem;

        [SerializeField]
        private RelicGachaSystem relicGachaSystem;

        [Title("장비 가챠 설정")]
        [ShowIf("currentGachaMode", GachaMode.Equipment)]
        [SerializeField]
        [EnumToggleButtons]
        private EquipmentType currentEquipmentType = EquipmentType.Weapon;

        [ShowIf("currentGachaMode", GachaMode.Equipment)]
        [SerializeField]
        [InfoBox("All Types를 선택하면 모든 타입이 랜덤하게 나옵니다")]
        private bool useAllTypes = false;

        [Title("공통 UI 참조")]
        [SerializeField, Required]
        private Button pull1Button;

        [SerializeField, Required]
        private Button pull11Button;

        [SerializeField, Required]
        private Button pull55Button;

        [Title("비용 텍스트")]
        [SerializeField]
        private TextMeshProUGUI cost1Text;

        [SerializeField]
        private TextMeshProUGUI cost11Text;

        [SerializeField]
        private TextMeshProUGUI cost55Text;

        [Title("천장 표시")]
        [SerializeField]
        private Slider pitySlider;

        [SerializeField]
        private TextMeshProUGUI pityText;

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
        private int relicCost11 = 2000;

        [BoxGroup("Cost")]
        [SerializeField]
        private int relicCost55 = 10000;

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

            // UI 업데이트
            UpdateUI();
        }

        private void OnGachaModeChanged()
        {
            UpdateUI();
        }

        private void OnEquipmentTypeChanged()
        {
            // 장비 타입 변경 시 처리할 내용이 있으면 여기에
        }

        private void UpdateUI()
        {
            // 비용 텍스트 업데이트
            UpdateCostTexts();

            // 천장 표시 업데이트
            UpdatePityDisplay();
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
            // 1회 뽑기 버튼
            if (pull1Button != null)
            {
                pull1Button.onClick.RemoveAllListeners();
                pull1Button.onClick.AddListener(() => OnPull(1));
                AddButtonAnimation(pull1Button);
            }

            // 11회 뽑기 버튼
            if (pull11Button != null)
            {
                pull11Button.onClick.RemoveAllListeners();
                pull11Button.onClick.AddListener(() => OnPull(11));
                AddButtonAnimation(pull11Button);
            }

            // 55회 뽑기 버튼
            if (pull55Button != null)
            {
                pull55Button.onClick.RemoveAllListeners();
                pull55Button.onClick.AddListener(() => OnPull(55));
                AddButtonAnimation(pull55Button);
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
        /// 가챠 실행 (모드에 따라 분기)
        /// </summary>
        private void OnPull(int count)
        {
            if (currentGachaMode == GachaMode.Equipment)
            {
                OnEquipmentPull(count);
            }
            else
            {
                OnRelicPull(count);
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

            if (useAllTypes)
            {
                // 모든 타입 랜덤
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
            }
            else
            {
                // 특정 타입만
                results = new List<EquipmentData>();
                for (int i = 0; i < count; i++)
                {
                    var equipment = equipmentGachaSystem.PullSingleByType(currentEquipmentType);
                    if (equipment != null)
                    {
                        results.Add(equipment);
                    }
                }
            }

            // 결과 표시
            ShowEquipmentResults(results, count);

            // 천장 업데이트
            UpdatePityDisplay();
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
                case 11:
                    results = new List<RelicData>();
                    // 10회 일반 뽑기
                    for (int i = 0; i < 10; i++)
                    {
                        results.Add(relicGachaSystem.PullSingle());
                    }
                    // 11번째는 희귀 이상 보장
                    bool hasRareOrBetter = results.Exists(r => r.rarity >= RelicRarity.Rare);
                    if (!hasRareOrBetter)
                    {
                        // TODO: 희귀 이상 보장 로직 구현 필요
                        results.Add(relicGachaSystem.PullSingle());
                    }
                    else
                    {
                        results.Add(relicGachaSystem.PullSingle());
                    }
                    break;
                case 55:
                    results = new List<RelicData>();
                    // 54회 뽑기
                    for (int i = 0; i < 54; i++)
                    {
                        results.Add(relicGachaSystem.PullSingle());
                    }
                    // 55번째는 영웅 이상 보장
                    bool hasEpicOrBetter = results.Exists(r => r.rarity >= RelicRarity.Epic);
                    if (!hasEpicOrBetter)
                    {
                        // TODO: 영웅 이상 보장 로직 구현 필요
                        results.Add(relicGachaSystem.PullSingle());
                    }
                    else
                    {
                        results.Add(relicGachaSystem.PullSingle());
                    }
                    break;
            }

            // 결과 표시
            ShowRelicResults(results, count);

            // 천장 업데이트
            UpdatePityDisplay();
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
                            // title 매개변수 제거됨
                            currentResultPopup.ShowEquipmentResults(
                                results,
                                () => OnEquipmentPull(pullCount)  // onPullAgain 콜백
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
                        // title 매개변수 제거됨
                        currentResultPopup.ShowEquipmentResults(
                            results,
                            () => OnEquipmentPull(pullCount)  // onPullAgain 콜백
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

            // PopupManager 사용 가능한 경우
            if (popupManager != null)
            {
                Debug.Log("[GachaUIController] PopupManager로 유물 가챠 결과 팝업 표시");

                var popup = popupManager.Pop(
                    PopupManager.PopupType.GachaResult,
                    onOpen: () =>
                    {
                        // 생성된 팝업에서 GachaResultPopup 컴포넌트 찾기
                        currentResultPopup = popupManager.popupContainer.GetComponentInChildren<GachaResultPopup>();

                        if (currentResultPopup != null)
                        {
                            // title 매개변수 제거됨
                            currentResultPopup.ShowRelicResults(
                                results,
                                () => OnRelicPull(pullCount)  // onPullAgain 콜백
                            );
                            Debug.Log("[GachaUIController] 유물 결과 표시 완료");
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
                        Debug.Log("[GachaUIController] 유물 가챠 결과 팝업 닫기 완료");
                    }
                );
            }
            else if (gachaResultPopupPrefab != null)
            {
                // PopupManager가 없으면 직접 생성
                Debug.Log("[GachaUIController] 직접 유물 가챠 결과 팝업 생성");

                Canvas canvas = FindObjectOfType<Canvas>();
                if (canvas != null)
                {
                    GameObject popupObj = Instantiate(gachaResultPopupPrefab, canvas.transform);
                    currentResultPopup = popupObj.GetComponent<GachaResultPopup>();

                    if (currentResultPopup != null)
                    {
                        // title 매개변수 제거됨
                        currentResultPopup.ShowRelicResults(
                            results,
                            () => OnRelicPull(pullCount)  // onPullAgain 콜백
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
        /// 장비 타입 한글 변환
        /// </summary>
        private string GetEquipmentTypeKorean(EquipmentType type)
        {
            switch (type)
            {
                case EquipmentType.Weapon: return "무기";
                case EquipmentType.Armor: return "방어구";
                case EquipmentType.Ring: return "반지";
                default: return type.ToString();
            }
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
                case 11: return relicCost11;
                case 55: return relicCost55;
                default: return 0;
            }
        }

        /// <summary>
        /// 비용 텍스트 업데이트
        /// </summary>
        private void UpdateCostTexts()
        {
            if (currentGachaMode == GachaMode.Equipment)
            {
                // 장비 가챠 비용
                if (cost1Text != null)
                    cost1Text.text = $"{equipmentCost1:N0}";

                if (cost11Text != null)
                    cost11Text.text = $"{equipmentCost11:N0}";

                if (cost55Text != null)
                    cost55Text.text = $"{equipmentCost55:N0}";
            }
            else
            {
                // 유물 가챠 비용
                if (cost1Text != null)
                    cost1Text.text = $"{relicCost1:N0}";

                if (cost11Text != null)
                    cost11Text.text = $"{relicCost11:N0}";

                if (cost55Text != null)
                    cost55Text.text = $"{relicCost55:N0}";
            }
        }

        /// <summary>
        /// 천장 표시 업데이트
        /// </summary>
        private void UpdatePityDisplay()
        {
            if (currentGachaMode == GachaMode.Equipment)
            {
                // 장비 천장
                if (equipmentGachaSystem != null)
                {
                    float pityProgress = equipmentGachaSystem.GetPityProgress();
                    int pityCount = equipmentGachaSystem.GetCurrentPityCount();

                    if (pitySlider != null)
                    {
                        pitySlider.value = pityProgress;
                    }

                    if (pityText != null)
                    {
                        pityText.text = $"{pityCount}/90";
                    }
                }
            }
            else
            {
                // 유물 천장
                if (relicGachaSystem != null)
                {
                    float pityProgress = relicGachaSystem.GetPityProgress();
                    int pityCount = relicGachaSystem.GetCurrentPityCount();

                    if (pitySlider != null)
                    {
                        pitySlider.value = pityProgress;
                    }

                    if (pityText != null)
                    {
                        pityText.text = $"{pityCount}/50";
                    }
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
            if (pull1Button != null) pull1Button.onClick.RemoveAllListeners();
            if (pull11Button != null) pull11Button.onClick.RemoveAllListeners();
            if (pull55Button != null) pull55Button.onClick.RemoveAllListeners();
        }

        [Title("디버그")]
        [Button("시스템 다시 찾기", ButtonSizes.Large)]
        [GUIColor(0.3f, 0.8f, 0.8f)]
        private void RefindSystems()
        {
            FindRequiredSystems();
        }

        [Title("가챠 모드 전환")]
        [ButtonGroup("ModeSwitch")]
        [Button("장비 가챠", ButtonSizes.Large)]
        [GUIColor(0.5f, 0.5f, 1f)]
        private void SwitchToEquipment()
        {
            currentGachaMode = GachaMode.Equipment;
            UpdateUI();
        }

        [ButtonGroup("ModeSwitch")]
        [Button("유물 가챠", ButtonSizes.Large)]
        [GUIColor(1f, 0.5f, 0.5f)]
        private void SwitchToRelic()
        {
            currentGachaMode = GachaMode.Relic;
            UpdateUI();
        }

        [Title("가챠 테스트")]
        [Button("1회 뽑기 테스트", ButtonSizes.Large)]
        private void Test1Pull()
        {
            if (!Application.isPlaying) return;
            OnPull(1);
        }

        [Button("11회 뽑기 테스트", ButtonSizes.Large)]
        private void Test11Pull()
        {
            if (!Application.isPlaying) return;
            OnPull(11);
        }

        [Button("55회 뽑기 테스트", ButtonSizes.Large)]
        private void Test55Pull()
        {
            if (!Application.isPlaying) return;
            OnPull(55);
        }
    }
}