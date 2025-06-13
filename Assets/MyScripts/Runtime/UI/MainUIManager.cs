using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using RPG.Core.Events;
using RPG.Common;
using RPG.UI.Base;
using RPG.Stage;
using RPG.Items.Equipment;
using RPG.Items.Relic;

namespace RPG.UI
{
    public class MainUIManager : MonoBehaviour
    {
        [Title("UI 참조", "메인 화면 UI 요소들")]
        [TabGroup("Main", "플레이어 정보")]
        [HorizontalGroup("Main/플레이어 정보/Top", 0.3f)]
        [VerticalGroup("Main/플레이어 정보/Top/Profile")]
        [PreviewField(80), HideLabel]
        [SerializeField] private Image playerAvatar;

        [VerticalGroup("Main/플레이어 정보/Top/Info")]
        [LabelText("플레이어 이름")]
        [SerializeField] private Text playerNameText;

        [VerticalGroup("Main/플레이어 정보/Top/Info")]
        [LabelText("플레이어 레벨")]
        [SerializeField] private Text playerLevelText;

        [VerticalGroup("Main/플레이어 정보/Top/Info")]
        [LabelText("경험치 바")]
        [SerializeField] private Slider experienceBar;

        [TabGroup("Main", "자원 표시")]
        [BoxGroup("Main/자원 표시/골드")]
        [HorizontalGroup("Main/자원 표시/골드/H")]
        [LabelText("아이콘"), PreviewField(40)]
        [SerializeField] private Image goldIcon;

        [HorizontalGroup("Main/자원 표시/골드/H")]
        [LabelText("텍스트")]
        [SerializeField] private Text goldText;

        [BoxGroup("Main/자원 표시/다이아몬드")]
        [HorizontalGroup("Main/자원 표시/다이아몬드/H")]
        [LabelText("아이콘"), PreviewField(40)]
        [SerializeField] private Image diamondIcon;

        [HorizontalGroup("Main/자원 표시/다이아몬드/H")]
        [LabelText("텍스트")]
        [SerializeField] private Text diamondText;

        [Title("전투 화면", "현재 스테이지 정보")]
        [TabGroup("Combat", "스테이지")]
        [BoxGroup("Combat/스테이지/현재 스테이지")]
        [SerializeField] private Text stageNumberText;
        [SerializeField] private Text stageName;
        [SerializeField] private Slider stageProgressBar;

        [BoxGroup("Combat/스테이지/몬스터 정보")]
        [SerializeField] private Text monsterNameText;
        [SerializeField] private Slider monsterHealthBar;
        [SerializeField] private GameObject monsterHealthBarGroup;

        [TabGroup("Combat", "전투 통계")]
        [ShowInInspector, ReadOnly]
        [LabelText("현재 DPS")]
        [SuffixLabel("DMG/초", true)]
        private float currentDPS;

        [ShowInInspector, ReadOnly]
        [LabelText("처치한 몬스터")]
        [SuffixLabel("마리", true)]
        private int monstersKilled;

        [ShowInInspector, ReadOnly]
        [LabelText("획득한 골드")]
        [SuffixLabel("G", true)]
        private long goldEarned;

        [Title("메뉴 버튼", "하단 네비게이션")]
        [TabGroup("Navigation", "주요 메뉴")]
        [HorizontalGroup("Navigation/주요 메뉴/Buttons")]
        [GUIColor(0.8f, 0.3f, 0.3f)]
        [SerializeField] private Button characterButton;

        [HorizontalGroup("Navigation/주요 메뉴/Buttons")]
        [GUIColor(0.3f, 0.8f, 0.3f)]
        [SerializeField] private Button inventoryButton;

        [HorizontalGroup("Navigation/주요 메뉴/Buttons")]
        [GUIColor(0.3f, 0.3f, 0.8f)]
        [SerializeField] private Button skillButton;

        [HorizontalGroup("Navigation/주요 메뉴/Buttons")]
        [GUIColor(0.8f, 0.8f, 0.3f)]
        [SerializeField] private Button shopButton;

        [TabGroup("Navigation", "보조 메뉴")]
        [HorizontalGroup("Navigation/보조 메뉴/SubButtons")]
        [SerializeField] private Button questButton;
        [SerializeField] private Button achievementButton;
        [SerializeField] private Button rankingButton;
        [SerializeField] private Button settingsButton;

        [Title("팝업 패널", "각종 UI 패널들")]
        [SerializeField] private GameObject characterPanel;
        [SerializeField] private GameObject inventoryPanel;
        [SerializeField] private GameObject skillPanel;
        [SerializeField] private GameObject shopPanel;

        [Title("이펙트 & 애니메이션")]
        [SerializeField] private GameObject levelUpEffectPrefab;
        [SerializeField] private GameObject expGainEffectPrefab;
        [SerializeField] private GameObject stageClearEffectPrefab;
        [SerializeField] private Transform effectContainer;

        // UI 패널 관리
        private Dictionary<string, IUIPanel> uiPanels = new Dictionary<string, IUIPanel>();
        private IUIPanel currentActivePanel;

        // 캐시된 데이터
        private int cachedPlayerLevel = 1;
        private int cachedMaxExp = 100;
        private int cachedCurrentExp = 0;
        private long cachedGold = 0;
        private long cachedDiamond = 0;
        private StageData cachedStageData;
        private MonsterData cachedMonsterData;
        private int cachedMonsterCurrentHp;
        private int cachedMonsterMaxHp;

        // DPS 계산용
        private float dpsCalculationTime = 0f;
        private int damageInCurrentSecond = 0;
        private Queue<DamageRecord> damageRecords = new Queue<DamageRecord>();

        private struct DamageRecord
        {
            public float time;
            public int damage;
        }

        #region Unity Lifecycle

        private void Awake()
        {
            InitializeButtons();
            RegisterPanels();
        }

        private void OnEnable()
        {
            SubscribeToEvents();
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }

        private void Start()
        {
            InitializeUI();
        }

        private void Update()
        {
            UpdateDPS();
        }

        #endregion

        #region Initialization

        private void InitializeButtons()
        {
            // 메인 메뉴 버튼
            characterButton?.onClick.AddListener(() => TogglePanel("Character"));
            inventoryButton?.onClick.AddListener(() => TogglePanel("Inventory"));
            skillButton?.onClick.AddListener(() => TogglePanel("Skill"));
            shopButton?.onClick.AddListener(() => TogglePanel("Shop"));

            // 보조 메뉴 버튼
            questButton?.onClick.AddListener(() => TogglePanel("Quest"));
            achievementButton?.onClick.AddListener(() => TogglePanel("Achievement"));
            rankingButton?.onClick.AddListener(() => TogglePanel("Ranking"));
            settingsButton?.onClick.AddListener(() => TogglePanel("Settings"));
        }

        private void RegisterPanels()
        {
            // 패널 컴포넌트 등록
            if (characterPanel) RegisterPanel("Character", characterPanel.GetComponent<IUIPanel>());
            if (inventoryPanel) RegisterPanel("Inventory", inventoryPanel.GetComponent<IUIPanel>());
            if (skillPanel) RegisterPanel("Skill", skillPanel.GetComponent<IUIPanel>());
            if (shopPanel) RegisterPanel("Shop", shopPanel.GetComponent<IUIPanel>());
        }

        private void RegisterPanel(string panelName, IUIPanel panel)
        {
            if (panel != null)
            {
                uiPanels[panelName] = panel;
                panel.Hide();
            }
        }

        private void InitializeUI()
        {
            // 초기 UI 상태 설정
            if (monsterHealthBarGroup) monsterHealthBarGroup.SetActive(false);
            UpdateAllUI();
        }

        #endregion

        #region Event Subscription

        private void SubscribeToEvents()
        {
            // 플레이어 이벤트
            GameEventManager.OnPlayerLevelUp += OnPlayerLevelUp;
            GameEventManager.OnPlayerExpGained += OnPlayerExpGained;
            GameEventManager.OnPlayerHealthChanged += OnPlayerHealthChanged;
            GameEventManager.OnPlayerDeath += OnPlayerDeath;

            // 전투 이벤트
            GameEventManager.OnDamageDealt += OnDamageDealt;
            GameEventManager.OnMonsterKilled += OnMonsterKilled;
            GameEventManager.OnStageCleared += OnStageCleared;

            // 스테이지 이벤트
            GameEventManager.OnStageStarted += OnStageStarted;
            GameEventManager.OnMonsterSpawned += OnMonsterSpawned;
            GameEventManager.OnStageProgress += OnStageProgress;

            // 화폐 이벤트
            GameEventManager.OnCurrencyChanged += OnCurrencyChanged;

            // 아이템 이벤트
            GameEventManager.OnEquipmentObtained += OnEquipmentObtained;
            GameEventManager.OnRelicObtained += OnRelicObtained;
        }

        private void UnsubscribeFromEvents()
        {
            // 플레이어 이벤트
            GameEventManager.OnPlayerLevelUp -= OnPlayerLevelUp;
            GameEventManager.OnPlayerExpGained -= OnPlayerExpGained;
            GameEventManager.OnPlayerHealthChanged -= OnPlayerHealthChanged;
            GameEventManager.OnPlayerDeath -= OnPlayerDeath;

            // 전투 이벤트
            GameEventManager.OnDamageDealt -= OnDamageDealt;
            GameEventManager.OnMonsterKilled -= OnMonsterKilled;
            GameEventManager.OnStageCleared -= OnStageCleared;

            // 스테이지 이벤트
            GameEventManager.OnStageStarted -= OnStageStarted;
            GameEventManager.OnMonsterSpawned -= OnMonsterSpawned;
            GameEventManager.OnStageProgress -= OnStageProgress;

            // 화폐 이벤트
            GameEventManager.OnCurrencyChanged -= OnCurrencyChanged;

            // 아이템 이벤트
            GameEventManager.OnEquipmentObtained -= OnEquipmentObtained;
            GameEventManager.OnRelicObtained -= OnRelicObtained;
        }

        #endregion

        #region Event Handlers

        private void OnPlayerLevelUp(int newLevel)
        {
            cachedPlayerLevel = newLevel;

            if (playerLevelText)
                playerLevelText.text = $"Lv.{newLevel}";

            ShowLevelUpEffect();

            // 레벨업 시 경험치 바 리셋
            UpdateExperienceBar();
        }

        private void OnPlayerExpGained(int exp)
        {
            cachedCurrentExp += exp;

            // 레벨업 체크는 PlayerStatus에서 처리하므로 여기서는 UI만 업데이트
            UpdateExperienceBar();
            ShowExpGainEffect(exp);
        }

        private void OnPlayerHealthChanged(int current, int max)
        {
            // 플레이어 체력 UI 업데이트 (필요시)
        }

        private void OnPlayerDeath()
        {
            // 사망 UI 표시
            Debug.Log("플레이어 사망!");
        }

        private void OnDamageDealt(int damage, bool isCritical)
        {
            // DPS 계산을 위한 데미지 기록
            var record = new DamageRecord
            {
                time = Time.time,
                damage = damage
            };

            damageRecords.Enqueue(record);
            damageInCurrentSecond += damage;

            // 데미지 텍스트 표시
            ShowDamageText(damage, isCritical);
        }

        private void OnMonsterKilled(MonsterData monster)
        {
            monstersKilled++;
            goldEarned += monster.goldReward;

            // 몬스터 체력바 숨기기
            if (monsterHealthBarGroup)
                monsterHealthBarGroup.SetActive(false);
        }

        private void OnStageCleared(int stageNumber)
        {
            ShowStageClearEffect(stageNumber);
        }

        private void OnStageStarted(StageData stage)
        {
            cachedStageData = stage;
            UpdateStageInfo();
        }

        private void OnMonsterSpawned(MonsterData monster, int currentHp, int maxHp)
        {
            cachedMonsterData = monster;
            cachedMonsterCurrentHp = currentHp;
            cachedMonsterMaxHp = maxHp;

            UpdateMonsterInfo();

            if (monsterHealthBarGroup)
                monsterHealthBarGroup.SetActive(true);
        }

        private void OnStageProgress(float progress)
        {
            if (stageProgressBar)
                stageProgressBar.value = progress;
        }

        private void OnCurrencyChanged(CurrencyType type, long amount)
        {
            switch (type)
            {
                case CurrencyType.Gold:
                    cachedGold = amount;
                    if (goldText) goldText.text = FormatNumber(amount);
                    break;

                case CurrencyType.Diamond:
                    cachedDiamond = amount;
                    if (diamondText) diamondText.text = FormatNumber(amount);
                    break;
            }
        }

        private void OnEquipmentObtained(EquipmentData equipment)
        {
            // 장비 획득 알림
            ShowItemObtainedNotification(equipment.Icon, equipment.equipmentName, equipment.GetRarityColor());
        }

        private void OnRelicObtained(RelicInstance relic)
        {
            // 유물 획득 알림
            ShowItemObtainedNotification(relic.Icon, relic.relicData.relicName, relic.relicData.GetRarityColor());
        }

        #endregion

        #region UI Updates

        private void UpdateAllUI()
        {
            UpdatePlayerInfo();
            UpdateCurrency();
            UpdateStageInfo();
        }

        private void UpdatePlayerInfo()
        {
            if (playerNameText) playerNameText.text = "Player";
            if (playerLevelText) playerLevelText.text = $"Lv.{cachedPlayerLevel}";

            UpdateExperienceBar();
        }

        private void UpdateExperienceBar()
        {
            if (experienceBar && cachedMaxExp > 0)
            {
                experienceBar.value = (float)cachedCurrentExp / cachedMaxExp;
            }
        }

        private void UpdateCurrency()
        {
            if (goldText) goldText.text = FormatNumber(cachedGold);
            if (diamondText) diamondText.text = FormatNumber(cachedDiamond);
        }

        private void UpdateStageInfo()
        {
            if (cachedStageData == null) return;

            if (stageNumberText)
                stageNumberText.text = $"Stage {cachedStageData.stageNumber}";

            if (stageName)
                stageName.text = cachedStageData.stageName;
        }

        private void UpdateMonsterInfo()
        {
            if (cachedMonsterData == null) return;

            if (monsterNameText)
                monsterNameText.text = cachedMonsterData.monsterName;

            if (monsterHealthBar && cachedMonsterMaxHp > 0)
            {
                monsterHealthBar.value = (float)cachedMonsterCurrentHp / cachedMonsterMaxHp;
            }
        }

        private void UpdateDPS()
        {
            // 1초마다 DPS 계산
            dpsCalculationTime += Time.deltaTime;

            if (dpsCalculationTime >= 1f)
            {
                // 오래된 기록 제거
                while (damageRecords.Count > 0 && Time.time - damageRecords.Peek().time > 5f)
                {
                    damageRecords.Dequeue();
                }

                // DPS 계산
                float totalDamage = 0;
                foreach (var record in damageRecords)
                {
                    totalDamage += record.damage;
                }

                currentDPS = damageRecords.Count > 0 ? totalDamage / 5f : 0;

                dpsCalculationTime = 0f;
                damageInCurrentSecond = 0;
            }
        }

        #endregion

        #region Panel Management

        public void TogglePanel(string panelName)
        {
            if (!uiPanels.ContainsKey(panelName)) return;

            var panel = uiPanels[panelName];

            if (currentActivePanel == panel)
            {
                CloseCurrentPanel();
            }
            else
            {
                CloseCurrentPanel();
                OpenPanel(panelName);
            }
        }

        private void OpenPanel(string panelName)
        {
            if (!uiPanels.ContainsKey(panelName)) return;

            var panel = uiPanels[panelName];
            panel.Show();
            currentActivePanel = panel;

            GameEventManager.TriggerPanelOpened(panelName);
        }

        private void CloseCurrentPanel()
        {
            if (currentActivePanel != null)
            {
                currentActivePanel.Hide();

                // 패널 이름 찾기
                foreach (var kvp in uiPanels)
                {
                    if (kvp.Value == currentActivePanel)
                    {
                        GameEventManager.TriggerPanelClosed(kvp.Key);
                        break;
                    }
                }

                currentActivePanel = null;
            }
        }

        #endregion

        #region Effects & Notifications

        private void ShowLevelUpEffect()
        {
            if (levelUpEffectPrefab && effectContainer)
            {
                var effect = Instantiate(levelUpEffectPrefab, effectContainer);
                Destroy(effect, 2f);
            }

            Debug.Log($"<color=yellow>레벨업! Lv.{cachedPlayerLevel}</color>");
        }

        private void ShowExpGainEffect(int exp)
        {
            if (expGainEffectPrefab && effectContainer)
            {
                var effect = Instantiate(expGainEffectPrefab, effectContainer);
                // 경험치 텍스트 설정
                var textComponent = effect.GetComponentInChildren<Text>();
                if (textComponent) textComponent.text = $"+{exp} EXP";

                Destroy(effect, 1.5f);
            }
        }

        private void ShowStageClearEffect(int stageNumber)
        {
            if (stageClearEffectPrefab && effectContainer)
            {
                var effect = Instantiate(stageClearEffectPrefab, effectContainer);
                Destroy(effect, 3f);
            }

            Debug.Log($"<color=green>Stage {stageNumber} Clear!</color>");
        }

        private void ShowDamageText(int damage, bool isCritical)
        {
            // TODO: 데미지 텍스트 UI 구현
            if (isCritical)
            {
                Debug.Log($"<color=yellow>크리티컬! {damage}</color>");
            }
            else
            {
                Debug.Log($"데미지: {damage}");
            }
        }

        private void ShowItemObtainedNotification(Sprite icon, string itemName, Color rarityColor)
        {
            // TODO: 아이템 획득 알림 UI 구현
            var colorHex = ColorUtility.ToHtmlStringRGB(rarityColor);
            Debug.Log($"<color=#{colorHex}>획득: {itemName}</color>");
        }

        #endregion

        #region Utility

        private string FormatNumber(long number)
        {
            if (number >= 1000000000000) return $"{number / 1000000000000f:F1}T";
            if (number >= 1000000000) return $"{number / 1000000000f:F1}B";
            if (number >= 1000000) return $"{number / 1000000f:F1}M";
            if (number >= 1000) return $"{number / 1000f:F1}K";
            return number.ToString();
        }

        #endregion

        #region Debug

        [Title("디버그 기능")]
        [Button("모든 UI 새로고침", ButtonSizes.Large)]
        [GUIColor(0.3f, 0.8f, 0.8f)]
        private void DebugRefreshAllUI()
        {
            UpdateAllUI();
            Debug.Log("모든 UI가 새로고침되었습니다.");
        }

        [Button("DPS 시뮬레이션", ButtonSizes.Medium)]
        private void SimulateDPS()
        {
            for (int i = 0; i < 10; i++)
            {
                int damage = UnityEngine.Random.Range(100, 500);
                bool isCrit = UnityEngine.Random.value < 0.3f;

                if (isCrit) damage *= 2;

                OnDamageDealt(damage, isCrit);
            }

            Debug.Log($"현재 DPS: {currentDPS:F0}");
        }

        [Button("레벨업 테스트", ButtonSizes.Medium)]
        private void TestLevelUp()
        {
            OnPlayerLevelUp(cachedPlayerLevel + 1);
        }

        [Button("스테이지 클리어 테스트", ButtonSizes.Medium)]
        private void TestStageClear()
        {
            if (cachedStageData != null)
            {
                OnStageCleared(cachedStageData.stageNumber);
            }
        }

        #endregion
    }
}