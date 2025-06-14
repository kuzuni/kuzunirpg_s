using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using RPG.Items.Equipment;
using RPG.Inventory;
using RPG.UI.Components;

namespace RPG.UI.Inventory
{
    /// <summary>
    /// 장비 인벤토리 UI 메인 컨트롤러
    /// </summary>
    public class EquipmentInventoryUI : MonoBehaviour
    {
        [Title("시스템 참조")]
        [SerializeField]
        [InfoBox("설정하지 않으면 자동으로 찾습니다", InfoMessageType.Info)]
        private EquipmentInventorySystem inventorySystem;

        [SerializeField]
        private EquipmentFusionSystem fusionSystem;

        [Title("UI 참조")]
        [BoxGroup("Tab")]
        [SerializeField, Required]
        private Toggle weaponTabToggle;

        [BoxGroup("Tab")]
        [SerializeField, Required]
        private Toggle armorTabToggle;

        [BoxGroup("Tab")]
        [SerializeField, Required]
        private Toggle ringTabToggle;

        [BoxGroup("Content")]
        [SerializeField, Required]
        private Transform itemContainer;

        [BoxGroup("Content")]
        [SerializeField, Required]
        private GameObject itemSlotPrefab;

        [BoxGroup("Content")]
        [SerializeField]
        private ScrollRect scrollRect;

        [Title("합성 UI")]
        [BoxGroup("Fusion")]
        [SerializeField, Required]
        private Button fusionButton;

        [BoxGroup("Fusion")]
        [SerializeField, Required]
        private Button autoFusionButton;

        [BoxGroup("Fusion")]
        [SerializeField, Required]
        private TextMeshProUGUI fusionInfoText;

        [BoxGroup("Fusion")]
        [SerializeField]
        private GameObject fusionPanel;

        [Title("정보 표시")]
        [BoxGroup("Info")]
        [SerializeField]
        private TextMeshProUGUI itemCountText;

        [BoxGroup("Info")]
        [SerializeField]
        private TextMeshProUGUI selectedCountText;

        [Title("설정")]
        [SerializeField]
        private int maxSelectableItems = 10;

        [SerializeField]
        private Color normalTabColor = Color.white;

        [SerializeField]
        private Color selectedTabColor = new Color(0.3f, 0.7f, 1f);

        [SerializeField]
        private bool animateTabSwitch = true;

        [SerializeField]
        private Color unownedItemColor = new Color(0.2f, 0.2f, 0.2f, 1f); // 미획득 아이템 색상

        [Title("상태")]
        [ShowInInspector, ReadOnly]
        private EquipmentType currentTab = EquipmentType.Weapon;

        [ShowInInspector, ReadOnly]
        private List<EquipmentInventorySlot> currentSlots = new List<EquipmentInventorySlot>();

        [ShowInInspector, ReadOnly]
        private EquipmentInventorySlot selectedSlot = null;

        [ShowInInspector, ReadOnly]
        private bool isInFusionMode = false;

        // Resources 폴더의 모든 장비 캐시
        private Dictionary<EquipmentType, List<EquipmentData>> allEquipmentCache = new Dictionary<EquipmentType, List<EquipmentData>>();

        // 이벤트
        public event System.Action<EquipmentData> OnItemSelected;
        public event System.Action<EquipmentData> OnItemDeselected;
        public event System.Action<List<EquipmentData>> OnFusionRequested;

        private void Start()
        {
            // 시스템 자동 찾기
            FindRequiredSystems();

            // Resources 폴더에서 모든 장비 로드
            LoadAllEquipments();

            InitializeTabs();
            InitializeButtons();
            RefreshInventory();

            // 이벤트 구독
            if (inventorySystem != null)
            {
                inventorySystem.OnItemAdded += OnInventoryChanged;
                inventorySystem.OnItemRemoved += OnInventoryChanged;
            }

            if (fusionSystem != null)
            {
                fusionSystem.OnFusionComplete += OnFusionComplete;
                fusionSystem.OnAutoFusionComplete += OnAutoFusionComplete;
            }
        }

        private void LoadAllEquipments()
        {
            // Resources 폴더에서 모든 장비 로드
            var allEquipments = Resources.LoadAll<EquipmentData>("Equipment");

            // 타입별로 분류
            allEquipmentCache.Clear();
            foreach (EquipmentType type in System.Enum.GetValues(typeof(EquipmentType)))
            {
                allEquipmentCache[type] = new List<EquipmentData>();
            }

            foreach (var equipment in allEquipments)
            {
                if (equipment != null)
                {
                    allEquipmentCache[equipment.equipmentType].Add(equipment);
                }
            }

            Debug.Log($"[LoadAllEquipments] 총 {allEquipments.Length}개 장비 로드 완료");
            foreach (var kvp in allEquipmentCache)
            {
                Debug.Log($"- {kvp.Key}: {kvp.Value.Count}개");
            }
        }

        private void FindRequiredSystems()
        {
            // 인벤토리 시스템 찾기
            if (inventorySystem == null)
            {
                inventorySystem = GetComponent<EquipmentInventorySystem>();

                if (inventorySystem == null)
                    inventorySystem = GetComponentInParent<EquipmentInventorySystem>();

                if (inventorySystem == null)
                    inventorySystem = GetComponentInChildren<EquipmentInventorySystem>();

                if (inventorySystem == null)
                    inventorySystem = FindObjectOfType<EquipmentInventorySystem>();
            }

            // 합성 시스템 찾기
            if (fusionSystem == null)
            {
                fusionSystem = GetComponent<EquipmentFusionSystem>();

                if (fusionSystem == null)
                    fusionSystem = GetComponentInParent<EquipmentFusionSystem>();

                if (fusionSystem == null)
                    fusionSystem = GetComponentInChildren<EquipmentFusionSystem>();

                if (fusionSystem == null)
                    fusionSystem = FindObjectOfType<EquipmentFusionSystem>();
            }

            // 결과 로그
            Debug.Log($"[EquipmentInventoryUI] 시스템 찾기 완료:\n" +
                     $"- InventorySystem: {(inventorySystem != null ? inventorySystem.name : "없음")}\n" +
                     $"- FusionSystem: {(fusionSystem != null ? fusionSystem.name : "없음")}");
        }

        private void OnEnable()
        {
            // 활성화될 때마다 시스템 확인
            if (inventorySystem == null || fusionSystem == null)
            {
                FindRequiredSystems();
            }

            // 인벤토리 새로고침
            RefreshInventory();
        }

        private void OnDestroy()
        {
            // 이벤트 구독 해제
            if (inventorySystem != null)
            {
                inventorySystem.OnItemAdded -= OnInventoryChanged;
                inventorySystem.OnItemRemoved -= OnInventoryChanged;
            }

            if (fusionSystem != null)
            {
                fusionSystem.OnFusionComplete -= OnFusionComplete;
                fusionSystem.OnAutoFusionComplete -= OnAutoFusionComplete;
            }
        }

        private void InitializeTabs()
        {
            // 탭 토글 이벤트 설정
            weaponTabToggle.onValueChanged.AddListener((isOn) =>
            {
                if (isOn) SwitchTab(EquipmentType.Weapon);
            });

            armorTabToggle.onValueChanged.AddListener((isOn) =>
            {
                if (isOn) SwitchTab(EquipmentType.Armor);
            });

            ringTabToggle.onValueChanged.AddListener((isOn) =>
            {
                if (isOn) SwitchTab(EquipmentType.Ring);
            });

            // 기본 탭 선택
            weaponTabToggle.isOn = true;
            UpdateTabVisuals();
        }

        private void InitializeButtons()
        {
            if (fusionButton != null)
            {
                fusionButton.onClick.AddListener(OnFusionButtonClicked);
            }

            if (autoFusionButton != null)
            {
                autoFusionButton.onClick.AddListener(OnAutoFusionButtonClicked);
            }
        }

        private void SwitchTab(EquipmentType newTab)
        {
            if (currentTab == newTab && currentSlots.Count > 0) return;

            currentTab = newTab;

            // 선택 초기화
            ClearSelection();

            // 탭 전환 애니메이션
            if (animateTabSwitch && itemContainer != null)
            {
                itemContainer.DOKill();

                // 페이드 아웃
                var canvasGroup = itemContainer.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = itemContainer.gameObject.AddComponent<CanvasGroup>();
                }

                canvasGroup.DOFade(0, 0.1f).OnComplete(() =>
                {
                    RefreshInventory();
                    canvasGroup.DOFade(1, 0.2f);
                });
            }
            else
            {
                RefreshInventory();
            }

            UpdateTabVisuals();
        }

        private void UpdateTabVisuals()
        {
            // 탭 색상 업데이트
            UpdateToggleVisual(weaponTabToggle, currentTab == EquipmentType.Weapon);
            UpdateToggleVisual(armorTabToggle, currentTab == EquipmentType.Armor);
            UpdateToggleVisual(ringTabToggle, currentTab == EquipmentType.Ring);
        }

        private void UpdateToggleVisual(Toggle toggle, bool isSelected)
        {
            if (toggle == null) return;

            var image = toggle.targetGraphic as Image;
            if (image != null)
            {
                image.color = isSelected ? selectedTabColor : normalTabColor;
            }

            // 텍스트 색상도 변경
            var text = toggle.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                text.fontStyle = isSelected ? FontStyles.Bold : FontStyles.Normal;
            }
        }

        private void RefreshInventory()
        {
            Debug.Log($"[RefreshInventory] 시작 - 현재 탭: {currentTab}");

            // 기존 슬롯 제거
            ClearSlots();

            // 현재 탭의 모든 장비 가져오기 (Resources)
            if (!allEquipmentCache.ContainsKey(currentTab))
            {
                Debug.LogError($"[RefreshInventory] {currentTab} 탭의 장비가 캐시에 없습니다!");
                return;
            }

            var allEquipments = allEquipmentCache[currentTab];
            Debug.Log($"[RefreshInventory] {currentTab} 탭 전체 장비 개수: {allEquipments.Count}");

            // 정렬 (등급 > 세부등급 > 이름)
            allEquipments = allEquipments.OrderByDescending(e => e.rarity)
                                        .ThenByDescending(e => e.subGrade)
                                        .ThenBy(e => e.equipmentName)
                                        .ToList();

            // 모든 장비에 대해 슬롯 생성
            foreach (var equipment in allEquipments)
            {
                CreateSlot(equipment);
            }

            Debug.Log($"[RefreshInventory] 생성된 슬롯 개수: {currentSlots.Count}");

            // UI 업데이트
            UpdateItemCount();
            UpdateFusionButton();

            // 스크롤 위치 초기화
            if (scrollRect != null)
            {
                scrollRect.verticalNormalizedPosition = 1f;
            }
        }

        private void CreateSlot(EquipmentData equipment)
        {
            if (itemSlotPrefab == null || itemContainer == null || equipment == null)
            {
                return;
            }

            // 프리팹 인스턴스 생성
            GameObject slotObj = Instantiate(itemSlotPrefab, itemContainer);

            // EquipmentInventorySlot 컴포넌트 확인
            EquipmentInventorySlot slot = slotObj.GetComponent<EquipmentInventorySlot>();

            if (slot == null)
            {
                Debug.LogError($"[CreateSlot] EquipmentInventorySlot 컴포넌트를 찾을 수 없습니다!");
                Destroy(slotObj);
                return;
            }

            // 보유 수량 확인
            int count = 0;
            bool isOwned = false;

            if (inventorySystem != null)
            {
                count = inventorySystem.GetItemCount(equipment);
                isOwned = count > 0;
            }

            // 슬롯 설정
            slot.Setup(equipment, count);

            // 미획득 아이템은 아이콘을 어둡게
            if (!isOwned)
            {
                slot.SetIconColor(unownedItemColor);
            }
            else
            {
                slot.SetIconColor(Color.white);
            }

            slot.OnSlotClicked += OnSlotClicked;
            slot.OnSlotRightClicked += OnSlotRightClicked;

            currentSlots.Add(slot);
        }

        private void ClearSlots()
        {
            foreach (var slot in currentSlots)
            {
                if (slot != null)
                {
                    slot.OnSlotClicked -= OnSlotClicked;
                    slot.OnSlotRightClicked -= OnSlotRightClicked;
                    Destroy(slot.gameObject);
                }
            }
            currentSlots.Clear();
            selectedSlot = null;
        }

        private void OnSlotClicked(EquipmentInventorySlot slot)
        {
            // 미보유 아이템은 선택 불가
            if (slot.Quantity <= 0)
            {
                ShowUnownedItemMessage(slot.Equipment);
                return;
            }

            // 이전 선택 해제
            if (selectedSlot != null && selectedSlot != slot)
            {
                selectedSlot.SetSelected(false);
            }

            // 현재 슬롯 선택/해제 토글
            if (selectedSlot == slot)
            {
                slot.SetSelected(false);
                selectedSlot = null;
                OnItemDeselected?.Invoke(slot.Equipment);
            }
            else
            {
                slot.SetSelected(true);
                selectedSlot = slot;
                OnItemSelected?.Invoke(slot.Equipment);

                // 장비 상세 정보 표시
                ShowEquipmentDetails(slot.Equipment);
            }

            UpdateFusionButton();
        }

        private void OnSlotRightClicked(EquipmentInventorySlot slot)
        {
            // 우클릭으로 빠른 장착
            if (slot.Quantity > 0)
            {
                // TODO: 장착 시스템과 연동
                Debug.Log($"장비 장착 시도: {slot.Equipment.equipmentName}");
            }
        }

        private void ClearSelection()
        {
            if (selectedSlot != null)
            {
                selectedSlot.SetSelected(false);
                selectedSlot = null;
            }
        }

        private void ShowEquipmentDetails(EquipmentData equipment)
        {
            // TODO: 장비 상세 정보 팝업 표시
            Debug.Log($"장비 정보: {equipment.GetFullRarityName()} {equipment.equipmentName}");
        }

        private void ShowUnownedItemMessage(EquipmentData equipment)
        {
            Debug.Log($"<color=gray>미획득 아이템: {equipment.GetFullRarityName()} {equipment.equipmentName}</color>");
            // TODO: UI 메시지 표시
        }

        private void OnInventoryChanged(EquipmentData item, int count)
        {
            // 해당 아이템의 슬롯 찾아서 업데이트
            var slot = currentSlots.FirstOrDefault(s => s.Equipment == item);
            if (slot != null)
            {
                slot.UpdateQuantity(count);

                // 보유 여부에 따라 아이콘 색상 변경
                if (count > 0)
                {
                    slot.SetIconColor(Color.white);
                }
                else
                {
                    slot.SetIconColor(unownedItemColor);
                }
            }
        }

        private void UpdateItemCount()
        {
            if (itemCountText == null) return;

            int totalCount = currentSlots.Count;
            int ownedCount = currentSlots.Count(s => s.Quantity > 0);

            itemCountText.text = $"{GetTabName(currentTab)} {ownedCount}/{totalCount}개 보유";
        }

        private void UpdateFusionButton()
        {
            if (fusionButton == null) return;

            // 선택한 아이템이 있고 5개 이상 보유중이면 합성 가능
            bool canFuse = selectedSlot != null && selectedSlot.Quantity >= 5;
            fusionButton.interactable = canFuse;

            if (fusionInfoText != null)
            {
                if (selectedSlot == null)
                {
                    fusionInfoText.text = "합성할 장비를 선택하세요";
                }
                else if (selectedSlot.Quantity < 5)
                {
                    fusionInfoText.text = $"합성하려면 5개 필요 (현재: {selectedSlot.Quantity}개)";
                }
                else
                {
                    // 합성 미리보기 정보
                    if (fusionSystem != null)
                    {
                        var preview = fusionSystem.GetFusionPreview(selectedSlot.Equipment);
                        if (preview.fusionType == FusionType.SubGradeUpgrade)
                        {
                            fusionInfoText.text = $"{preview.resultSubGrade}★로 강화 (100% 성공)";
                        }
                        else if (preview.fusionType == FusionType.RarityUpgrade)
                        {
                            fusionInfoText.text = $"{RarityColors.GetRarityName(preview.resultRarity)}로 승급 (100% 성공)";
                        }
                        else
                        {
                            fusionInfoText.text = "최고 등급입니다";
                        }
                    }
                    else
                    {
                        fusionInfoText.text = $"{selectedSlot.Equipment.equipmentName} x{selectedSlot.Quantity} 합성 가능";
                    }
                }
            }
        }

        private void OnFusionButtonClicked()
        {
            if (selectedSlot == null || selectedSlot.Quantity < 5 || fusionSystem == null)
            {
                return;
            }

            // 합성 미리보기
            var preview = fusionSystem.GetFusionPreview(selectedSlot.Equipment);

            ShowFusionConfirmPopup(preview, () =>
            {
                fusionSystem.TryFusion(selectedSlot.Equipment);

                // 선택 해제
                ClearSelection();
            });
        }

        private void OnAutoFusionButtonClicked()
        {
            if (fusionSystem == null)
            {
                Debug.LogError("FusionSystem이 없습니다!");
                return;
            }

            // 현재 탭의 자동 합성 실행
            fusionSystem.PerformAutoFusion(currentTab);
        }

        // 합성 결과 콜백
        private void OnFusionComplete(EquipmentData original, EquipmentData result, bool success)
        {
            if (success)
            {
                ShowFusionSuccessEffect(original, result);
            }

            // 원본 슬롯 업데이트
            var originalSlot = currentSlots.FirstOrDefault(s => s.Equipment == original);
            if (originalSlot != null)
            {
                int newCount = inventorySystem.GetItemCount(original);
                originalSlot.UpdateQuantity(newCount);
            }

            if (success && result != null)
            {
                // 결과 슬롯 찾기 - 속성으로 비교
                var resultSlot = currentSlots.FirstOrDefault(s =>
                    s.Equipment != null &&
                    s.Equipment.equipmentName == result.equipmentName &&
                    s.Equipment.rarity == result.rarity &&
                    s.Equipment.subGrade == result.subGrade);

                if (resultSlot != null)
                {
                    // 기존 슬롯 업데이트
                    int newCount = inventorySystem.GetItemCount(result);
                    Debug.Log($"[UI 업데이트] {result.GetFullRarityName()} {result.equipmentName}: {resultSlot.Quantity} → {newCount}");
                    resultSlot.UpdateQuantity(newCount);
                    resultSlot.SetIconColor(Color.white);
                }
                else
                {
                    // 슬롯이 없는 경우 (새로운 등급/세부등급)
                    Debug.Log($"[UI 업데이트] 새 슬롯이 필요함: {result.GetFullRarityName()} {result.equipmentName}");

                    // 옵션 1: 전체 새로고침 (간단하지만 비효율적)
                    // RefreshInventory();

                    // 옵션 2: 해당 슬롯만 추가 (효율적)
                    CreateSlot(result);

                    // 정렬 다시 하기
                    SortSlots();
                }
            }

            UpdateFusionButton();
            UpdateItemCount();
        }
        private void SortSlots()
        {
            // Transform 순서 재정렬
            var sortedSlots = currentSlots
                .OrderByDescending(s => s.Equipment.rarity)
                .ThenByDescending(s => s.Equipment.subGrade)
                .ThenBy(s => s.Equipment.equipmentName)
                .ToList();

            for (int i = 0; i < sortedSlots.Count; i++)
            {
                sortedSlots[i].transform.SetSiblingIndex(i);
            }
        }
        private void OnAutoFusionComplete(int totalCount, int successCount)
        {
            if (totalCount > 0)
            {
                ShowAutoFusionResult(totalCount, successCount);
                RefreshInventory(); // 자동 합성은 전체 새로고침
            }
            else
            {
                ShowNoFusionItemsMessage();
            }
        }

        // 합성 확인 팝업
        private void ShowFusionConfirmPopup(FusionPreview preview, System.Action onConfirm)
        {
            if (!preview.isValid)
            {
                Debug.LogError("유효하지 않은 합성입니다!");
                return;
            }

            // TODO: 실제 UI 팝업 구현
            Debug.Log($"=== 합성 확인 ===");
            Debug.Log($"재료: {preview.baseEquipment.GetFullRarityName()} {preview.baseEquipment.equipmentName} x{preview.requiredCount}");
            Debug.Log($"성공률: 100% (실패 없음)");

            if (preview.fusionType == FusionType.SubGradeUpgrade)
            {
                Debug.Log($"결과: {preview.resultSubGrade}★로 업그레이드");
            }
            else if (preview.fusionType == FusionType.RarityUpgrade)
            {
                Debug.Log($"결과: {RarityColors.GetRarityName(preview.resultRarity)} 1★로 등급 상승");
            }
            else if (preview.fusionType == FusionType.MaxLevel)
            {
                Debug.Log("이미 최고 등급입니다!");
                return;
            }

            // 임시로 바로 실행
            onConfirm?.Invoke();
        }

        // 효과 표시 메서드들
        private void ShowFusionSuccessEffect(EquipmentData original, EquipmentData result)
        {
            // TODO: 성공 이펙트 표시
            Debug.Log($"<color=green>합성 성공 효과 표시</color>");
        }

        private void ShowAutoFusionResult(int totalCount, int successCount)
        {
            Debug.Log($"<color=yellow>자동 합성 완료: {totalCount}회 시도, {successCount}회 성공 (성공률: 100%)</color>");
            // TODO: UI 팝업으로 결과 표시
        }

        private void ShowNoFusionItemsMessage()
        {
            Debug.Log("합성 가능한 장비가 없습니다!");
            // TODO: 메시지 표시
        }

        private string GetTabName(EquipmentType type)
        {
            switch (type)
            {
                case EquipmentType.Weapon: return "무기";
                case EquipmentType.Armor: return "갑옷";
                case EquipmentType.Ring: return "반지";
                default: return type.ToString();
            }
        }


        [Title("테스트")]
        [Button("디버그: 현재 상태 확인")]
        private void DebugCheckStatus()
        {
            Debug.Log("===== Equipment Inventory UI 상태 =====");
            Debug.Log($"InventorySystem: {(inventorySystem != null ? "연결됨" : "NULL")}");
            Debug.Log($"FusionSystem: {(fusionSystem != null ? "연결됨" : "NULL")}");
            Debug.Log($"현재 탭: {currentTab}");
            Debug.Log($"전체 슬롯 수: {currentSlots.Count}");
            Debug.Log($"보유 슬롯 수: {currentSlots.Count(s => s.Quantity > 0)}");
            Debug.Log($"선택된 슬롯: {(selectedSlot != null ? selectedSlot.Equipment.equipmentName : "없음")}");

            if (allEquipmentCache.Count > 0)
            {
                Debug.Log("=== 캐시된 장비 ===");
                foreach (var kvp in allEquipmentCache)
                {
                    Debug.Log($"{kvp.Key}: {kvp.Value.Count}개");
                }
            }
            Debug.Log("=====================================");
        }

        [Button("Resources 장비 다시 로드")]
        private void TestReloadEquipments()
        {
            LoadAllEquipments();
            RefreshInventory();
        }

        [Button("테스트: 랜덤 아이템 획득")]
        private void TestAddRandomItem()
        {
            if (inventorySystem == null || allEquipmentCache.Count == 0)
            {
                Debug.LogError("시스템이 준비되지 않았습니다!");
                return;
            }

            // 현재 탭에서 랜덤 아이템 선택
            var equipments = allEquipmentCache[currentTab];
            if (equipments.Count > 0)
            {
                var randomEquipment = equipments[Random.Range(0, equipments.Count)];
                inventorySystem.AddItem(randomEquipment, 1);
                Debug.Log($"획득: {randomEquipment.GetFullRarityName()} {randomEquipment.equipmentName}");
            }
        }
    }
}