using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;

// UI 상태 관리 인터페이스 (Single Responsibility)
public interface IUIState
{
    void Enter();
    void Exit();
    void UpdateState();
}

// UI 패널 기본 인터페이스 (Interface Segregation)
public interface IUIPanel
{
    void Show();
    void Hide();
    void UpdatePanel();
    bool IsVisible { get; }
}


// 메인 UI 관리자
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
    [ShowInInspector, ReadOnly]
    [ProgressBar(0, "@currentMonsterMaxHp", ColorGetter = "GetMonsterHealthColor")]
    private int currentMonsterHp = 100;
    
    [ShowInInspector, ReadOnly]
    private int currentMonsterMaxHp = 100;
    
    [SerializeField] private Text monsterNameText;
    [SerializeField] private Slider monsterHealthBar;
    
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
    
    [Title("시스템 참조")]
    [Required]
    [SerializeField] private PlayerController playerController;
    
    [Required]
    [SerializeField] private StageManager stageManager;
    
    [Required]
    [SerializeField] private CurrencyManager currencyManager;
    
    // UI 패널 관리
    private Dictionary<string, IUIPanel> uiPanels = new Dictionary<string, IUIPanel>();
    private IUIPanel currentActivePanel;

    public Button InventoryButton { get => inventoryButton; set => inventoryButton = value; }

    // 이벤트
    public event Action<string> OnPanelOpened;
    public event Action<string> OnPanelClosed;
    
    private void Awake()
    {
        InitializeButtons();
        RegisterPanels();
    }
    
    private void Start()
    {
        SubscribeToEvents();
        UpdateAllUI();
    }
    
    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }
    
    private void InitializeButtons()
    {
        // 메인 메뉴 버튼
        characterButton?.onClick.AddListener(() => TogglePanel("Character"));
        InventoryButton?.onClick.AddListener(() => TogglePanel("Inventory"));
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
    
    private void SubscribeToEvents()
    {
        if (playerController != null)
        {
            playerController.Status.OnLevelUp += OnPlayerLevelUp;
            playerController.Status.OnExpChanged += OnExpChanged;
        }
        
        if (currencyManager != null)
        {
            currencyManager.OnCurrencyChanged += OnCurrencyChanged;
        }
        
        if (stageManager != null)
        {
            stageManager.OnStageChanged += OnStageChanged;
            stageManager.OnMonsterKilled += OnMonsterKilled;
            stageManager.OnStageProgress += OnStageProgress;
        }
    }
    
    private void UnsubscribeFromEvents()
    {
        if (playerController != null)
        {
            playerController.Status.OnLevelUp -= OnPlayerLevelUp;
            playerController.Status.OnExpChanged -= OnExpChanged;
        }
        
        if (currencyManager != null)
        {
            currencyManager.OnCurrencyChanged -= OnCurrencyChanged;
        }
        
        if (stageManager != null)
        {
            stageManager.OnStageChanged -= OnStageChanged;
            stageManager.OnMonsterKilled -= OnMonsterKilled;
            stageManager.OnStageProgress -= OnStageProgress;
        }
    }
    
    // UI 업데이트 메서드들
    private void UpdateAllUI()
    {
        UpdatePlayerInfo();
        UpdateCurrency();
        UpdateStageInfo();
    }
    
    private void UpdatePlayerInfo()
    {
        if (playerController == null) return;
        
        var status = playerController.Status;
        if (playerNameText) playerNameText.text = "Player"; // 나중에 실제 이름으로 변경
        if (playerLevelText) playerLevelText.text = $"Lv.{status.Level}";
        if (experienceBar) experienceBar.value = status.GetExpProgress();
    }
    
    private void UpdateCurrency()
    {
        if (currencyManager == null) return;
        
        if (goldText) goldText.text = FormatNumber(currencyManager.Gold);
        if (diamondText) diamondText.text = FormatNumber(currencyManager.Diamond);
    }
    
    private void UpdateStageInfo()
    {
        if (stageManager == null) return;
        
        var currentStage = stageManager.CurrentStage;
        if (stageNumberText) stageNumberText.text = $"Stage {currentStage.stageNumber}";
        if (stageName) stageName.text = currentStage.stageName;
        if (stageProgressBar) stageProgressBar.value = stageManager.GetStageProgress();
        
        UpdateMonsterInfo();
    }
    
    private void UpdateMonsterInfo()
    {
        var currentMonster = stageManager.CurrentMonster;
        if (currentMonster != null)
        {
            currentMonsterHp = currentMonster.CurrentHp;
            currentMonsterMaxHp = currentMonster.MaxHp;
            
            if (monsterNameText) monsterNameText.text = currentMonster.MonsterName;
            if (monsterHealthBar) 
            {
                monsterHealthBar.value = (float)currentMonsterHp / currentMonsterMaxHp;
            }
        }
    }
    
    // 패널 토글
    public void TogglePanel(string panelName)
    {
        if (!uiPanels.ContainsKey(panelName)) return;
        
        var panel = uiPanels[panelName];
        
        if (currentActivePanel == panel)
        {
            // 같은 패널 클릭 시 닫기
            CloseCurrentPanel();
        }
        else
        {
            // 다른 패널 열기
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
        OnPanelOpened?.Invoke(panelName);
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
                    OnPanelClosed?.Invoke(kvp.Key);
                    break;
                }
            }
            
            currentActivePanel = null;
        }
    }
    
    // 이벤트 핸들러들
    private void OnPlayerLevelUp(int newLevel)
    {
        UpdatePlayerInfo();
        // 레벨업 이펙트 표시
        ShowLevelUpEffect();
    }
    
    private void OnExpChanged(int currentExp, int maxExp)
    {
        if (experienceBar) experienceBar.value = (float)currentExp / maxExp;
    }
    
    private void OnCurrencyChanged(CurrencyType type, long amount)
    {
        UpdateCurrency();
    }
    
    private void OnStageChanged(StageData newStage)
    {
        UpdateStageInfo();
    }
    
    private void OnMonsterKilled(int killCount)
    {
        monstersKilled = killCount;
        UpdateMonsterInfo();
    }
    
    private void OnStageProgress(float progress)
    {
        if (stageProgressBar) stageProgressBar.value = progress;
    }
    
    // 유틸리티 메서드
    private string FormatNumber(long number)
    {
        if (number >= 1000000000000) return $"{number / 1000000000000f:F1}T";
        if (number >= 1000000000) return $"{number / 1000000000f:F1}B";
        if (number >= 1000000) return $"{number / 1000000f:F1}M";
        if (number >= 1000) return $"{number / 1000f:F1}K";
        return number.ToString();
    }
    
    private Color GetMonsterHealthColor(float value)
    {
        float healthPercent = currentMonsterMaxHp > 0 ? (float)currentMonsterHp / currentMonsterMaxHp : 0;
        if (healthPercent > 0.6f) return Color.green;
        if (healthPercent > 0.3f) return Color.yellow;
        return Color.red;
    }
    
    private void ShowLevelUpEffect()
    {
        Debug.Log("레벨업! 축하 이펙트 표시");
        // TODO: 실제 레벨업 이펙트 구현
    }
    
    // 디버그 기능
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
        currentDPS = UnityEngine.Random.Range(1000f, 50000f);
        Debug.Log($"현재 DPS: {currentDPS:F0}");
    }
}