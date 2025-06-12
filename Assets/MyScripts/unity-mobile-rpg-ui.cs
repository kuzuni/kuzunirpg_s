using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using DG.Tweening;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using TMPro;
using System;

public class MobileRPGUIManager : MonoBehaviour
{
    [Title("UI References")]
    [FoldoutGroup("Top Bar")]
    [SerializeField] private RectTransform topBarContainer;
    [FoldoutGroup("Top Bar")]
    [SerializeField] private TextMeshProUGUI playerNameText;
    [FoldoutGroup("Top Bar")]
    [SerializeField] private TextMeshProUGUI playerLevelText;

    [FoldoutGroup("Top Bar/Currency", expanded: false)]
    [SerializeField] private TextMeshProUGUI goldText;
    [FoldoutGroup("Top Bar/Currency")]
    [SerializeField] private TextMeshProUGUI gemText;
    [FoldoutGroup("Top Bar/Currency")]
    [SerializeField] private TextMeshProUGUI energyText;

    [Title("Game View")]
    [FoldoutGroup("Main Game")]
    [SerializeField] private RectTransform gameViewPanel;
    [FoldoutGroup("Main Game")]
    [SerializeField] private TextMeshProUGUI stageInfoText;
    [FoldoutGroup("Main Game")]
    [SerializeField] private Transform characterContainer;
    [FoldoutGroup("Main Game")]
    [SerializeField] private Transform enemyContainer;

    [FoldoutGroup("Damage Display")]
    [SerializeField] private GameObject damageTextPrefab;
    [FoldoutGroup("Damage Display")]
    [SerializeField] private Transform damageTextContainer;
    [FoldoutGroup("Damage Display")]
    [SerializeField] private MMFeedbacks damagePopupFeedback;

    [Title("Skills")]
    [FoldoutGroup("Skill Bar")]
    [TableList(ShowIndexLabels = true)]
    [SerializeField] private List<SkillButtonData> skillButtons = new List<SkillButtonData>();

    [Title("Navigation")]
    [FoldoutGroup("Bottom Navigation")]
    [SerializeField] private List<NavigationButton> navigationButtons = new List<NavigationButton>();

    [Title("Side Panels")]
    [FoldoutGroup("Equipment")]
    [SerializeField] private RectTransform equipmentPanel;
    [FoldoutGroup("Equipment")]
    [TableList]
    [SerializeField] private List<EquipmentSlot> equipmentSlots = new List<EquipmentSlot>();

    [FoldoutGroup("Chat")]
    [SerializeField] private RectTransform chatPanel;
    [FoldoutGroup("Chat")]
    [SerializeField] private ScrollRect chatScrollRect;
    [FoldoutGroup("Chat")]
    [SerializeField] private TMP_InputField chatInput;

    [System.Serializable]
    public class SkillButtonData
    {
        [HorizontalGroup("Skill", 70)]
        [PreviewField(70)]
        public Sprite icon;

        [VerticalGroup("Skill/Info")]
        [LabelWidth(80)]
        public string skillName;

        [VerticalGroup("Skill/Info")]
        [LabelWidth(80)]
        public float cooldown = 5f;

        [HideInInspector]
        public Button button;
        [HideInInspector]
        public Image cooldownImage;
        [HideInInspector]
        public TextMeshProUGUI cooldownText;
        [HideInInspector]
        public MMFeedbacks skillUseFeedback;
    }

    [System.Serializable]
    public class NavigationButton
    {
        [HorizontalGroup("Nav", 50)]
        [PreviewField(50)]
        public Sprite icon;

        [VerticalGroup("Nav/Info")]
        public string buttonName;

        [VerticalGroup("Nav/Info")]
        public Color iconColor = Color.white;

        [HideInInspector]
        public Button button;
        [HideInInspector]
        public RectTransform rectTransform;
    }

    [System.Serializable]
    public class EquipmentSlot
    {
        public enum SlotType { Weapon, Armor, Accessory, Ring, Boots, Gloves }

        [EnumToggleButtons]
        public SlotType slotType;

        [PreviewField(60)]
        public Sprite currentItem;

        [HideInInspector]
        public Button button;
        [HideInInspector]
        public Image itemImage;
    }

    [System.Serializable]
    public class UIAnimationSettings
    {
        [Title("General")]
        [Range(0.1f, 2f)]
        public float defaultDuration = 0.3f;

        [EnumToggleButtons]
        public Ease defaultEase = Ease.OutQuad;

        [Title("Specific Animations")]
        [BoxGroup("Top Bar")]
        public float topBarSlideInDelay = 0.2f;
        [BoxGroup("Top Bar")]
        public float topBarSlideDistance = 200f;

        [BoxGroup("Skills")]
        public float skillButtonStaggerDelay = 0.05f;
        [BoxGroup("Skills")]
        public float skillButtonScaleFrom = 0f;

        [BoxGroup("Damage")]
        public float damageTextRiseHeight = 100f;
        [BoxGroup("Damage")]
        public float damageTextDuration = 1.5f;

        [BoxGroup("Currency")]
        public float currencyCountDuration = 0.5f;
        [BoxGroup("Currency")]
        public AnimationCurve currencyCountCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    }

    [System.Serializable]
    public class UIStyleSettings
    {
        [Title("Colors")]
        [ColorUsage(true, true)]
        public Color primaryColor = new Color(0.2f, 0.2f, 0.8f);
        [ColorUsage(true, true)]
        public Color secondaryColor = new Color(0.8f, 0.2f, 0.2f);
        [ColorUsage(true, true)]
        public Color backgroundTint = new Color(0.1f, 0.1f, 0.1f, 0.9f);

        [Title("Fonts")]
        public TMP_FontAsset primaryFont;
        public TMP_FontAsset numberFont;

        [Title("Spacing")]
        [Range(5, 50)]
        public float elementSpacing = 10f;
        [Range(5, 50)]
        public float panelPadding = 20f;
    }

    [System.Serializable]
    public class UILayoutSettings
    {
        [Title("Top Bar")]
        [Range(0.05f, 0.2f)]
        public float topBarHeight = 0.07f;

        [Title("Main Game Area")]
        [Range(0.3f, 0.9f)]
        public float gameAreaTop = 0.85f;

        [Range(0.2f, 0.5f)]
        public float gameAreaBottom = 0.3f;

        [Title("Skill Bar")]
        [MinMaxSlider(0.05f, 0.4f, true)]
        public Vector2 skillBarVerticalRange = new Vector2(0.15f, 0.25f);

        [Range(0f, 0.2f)]
        public float skillBarHorizontalPadding = 0.1f;

        [Title("Bottom Navigation")]
        [Range(0.05f, 0.2f)]
        public float bottomNavHeight = 0.1f;

        [Title("Side Panels")]
        [Range(0.1f, 0.3f)]
        public float equipmentPanelWidth = 0.15f;

        [MinMaxSlider(0.1f, 0.9f, true)]
        public Vector2 equipmentPanelVerticalRange = new Vector2(0.3f, 0.7f);

        [Range(0.2f, 0.6f)]
        public float chatPanelWidth = 0.4f;

        [MinMaxSlider(0.05f, 0.5f, true)]
        public Vector2 chatPanelVerticalRange = new Vector2(0.1f, 0.3f);

        [Title("Element Sizes")]
        [Range(80f, 200f)]
        public float skillButtonSize = 120f;

        [Range(60f, 150f)]
        public float navButtonHeight = 100f;

        [Range(50f, 120f)]
        public float equipmentSlotSize = 80f;
    }

    [Title("UI Animation Settings")]
    [FoldoutGroup("Animations")]
    [SerializeField] private UIAnimationSettings animationSettings = new UIAnimationSettings();

    [Title("UI Style Settings")]
    [FoldoutGroup("Styling")]
    [SerializeField] private UIStyleSettings styleSettings = new UIStyleSettings();

    [Title("UI Layout Settings")]
    [FoldoutGroup("Layout", expanded: true)]
    [OnInspectorGUI("DrawLayoutPreview")]
    [SerializeField] private UILayoutSettings layoutSettings = new UILayoutSettings();

    private Dictionary<int, Tweener> skillCooldownTweeners = new Dictionary<int, Tweener>();
    private Sequence startupSequence;

    private void DrawLayoutPreview()
    {
#if UNITY_EDITOR
        if (GUILayout.Button("Update Layout", GUILayout.Height(25)))
        {
            ApplyLayoutSettings();
        }
#endif
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Auto update layout when values change in inspector
        if (topBarContainer != null && !Application.isPlaying)
        {
            UnityEditor.EditorApplication.delayCall += () =>
            {
                if (this != null)
                {
                    ApplyLayoutSettings();
                }
            };
        }
    }
#endif

    [Button("Apply Layout Settings", ButtonSizes.Large)]
    [PropertySpace(SpaceBefore = 10, SpaceAfter = 20)]
    private void ApplyLayoutSettings()
    {
        // Update Top Bar
        if (topBarContainer != null)
        {
            topBarContainer.anchorMin = new Vector2(0, 1f - layoutSettings.topBarHeight);
            topBarContainer.anchorMax = new Vector2(1, 1);
            topBarContainer.sizeDelta = Vector2.zero;
            topBarContainer.anchoredPosition = Vector2.zero;
        }

        // Update Game View Panel
        if (gameViewPanel != null)
        {
            gameViewPanel.anchorMin = new Vector2(0, layoutSettings.gameAreaBottom);
            gameViewPanel.anchorMax = new Vector2(1, layoutSettings.gameAreaTop);
            gameViewPanel.sizeDelta = Vector2.zero;
            gameViewPanel.anchoredPosition = Vector2.zero;
        }

        // Update Skill Bar
        var skillBar = transform.Find("SkillBar");
        if (skillBar != null)
        {
            var skillBarRect = skillBar.GetComponent<RectTransform>();
            skillBarRect.anchorMin = new Vector2(layoutSettings.skillBarHorizontalPadding, layoutSettings.skillBarVerticalRange.x);
            skillBarRect.anchorMax = new Vector2(1f - layoutSettings.skillBarHorizontalPadding, layoutSettings.skillBarVerticalRange.y);
            skillBarRect.sizeDelta = Vector2.zero;
            skillBarRect.anchoredPosition = Vector2.zero;
        }

        // Update Bottom Navigation
        var bottomNav = transform.Find("BottomNavigation");
        if (bottomNav != null)
        {
            var bottomNavRect = bottomNav.GetComponent<RectTransform>();
            bottomNavRect.anchorMin = new Vector2(0, 0);
            bottomNavRect.anchorMax = new Vector2(1, layoutSettings.bottomNavHeight);
            bottomNavRect.sizeDelta = Vector2.zero;
            bottomNavRect.anchoredPosition = Vector2.zero;
        }

        // Update Equipment Panel
        if (equipmentPanel != null)
        {
            equipmentPanel.anchorMin = new Vector2(0, layoutSettings.equipmentPanelVerticalRange.x);
            equipmentPanel.anchorMax = new Vector2(layoutSettings.equipmentPanelWidth, layoutSettings.equipmentPanelVerticalRange.y);
            equipmentPanel.sizeDelta = Vector2.zero;
            equipmentPanel.anchoredPosition = Vector2.zero;
        }

        // Update Chat Panel
        if (chatPanel != null)
        {
            chatPanel.anchorMin = new Vector2(0, layoutSettings.chatPanelVerticalRange.x);
            chatPanel.anchorMax = new Vector2(layoutSettings.chatPanelWidth, layoutSettings.chatPanelVerticalRange.y);
            chatPanel.sizeDelta = Vector2.zero;
            chatPanel.anchoredPosition = Vector2.zero;
        }

        // Force canvas update
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            UnityEditor.EditorUtility.SetDirty(gameObject);
        }
#endif
        Canvas.ForceUpdateCanvases();
    }

    [Button("Setup UI", ButtonSizes.Large)]
    [PropertySpace(SpaceBefore = 20)]
    private void SetupUI()
    {
        // Ensure we have a RectTransform
        if (!GetComponent<RectTransform>())
        {
            gameObject.AddComponent<RectTransform>();
        }

        CreateUIStructure();
        ApplyStyling();

        // Force Canvas update
        Canvas.ForceUpdateCanvases();

        if (Application.isPlaying)
        {
            AnimateUIEntrance();
        }
    }

    [ButtonGroup("Preview")]
    [Button("Preview Damage")]
    private void PreviewDamage()
    {
        ShowDamageText(UnityEngine.Random.Range(10000, 999999), DamageType.Critical);
    }

    [ButtonGroup("Preview")]
    [Button("Preview Skill Cooldown")]
    private void PreviewSkillCooldown()
    {
        if (skillButtons.Count > 0)
        {
            TriggerSkillCooldown(0);
        }
    }

    void Start()
    {
        if (Application.isPlaying)
        {
            AnimateUIEntrance();
            InitializeFeedbacks();
        }
    }

    void CreateUIStructure()
    {
        // Get or create Canvas
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            GameObject canvasGO = new GameObject("Canvas");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<GraphicRaycaster>();

            // Setup Canvas Scaler for mobile
            CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
        }

        // Set this GameObject as child of Canvas and setup RectTransform
        transform.SetParent(canvas.transform);

        // Setup UI Manager RectTransform to fill canvas
        RectTransform myRect = GetComponent<RectTransform>();
        myRect.anchorMin = Vector2.zero;
        myRect.anchorMax = Vector2.one;
        myRect.sizeDelta = Vector2.zero;
        myRect.anchoredPosition = Vector2.zero;
        myRect.localScale = Vector3.one;

        // Create Top Bar
        if (topBarContainer == null)
        {
            GameObject topBarGO = new GameObject("TopBar");
            topBarGO.transform.SetParent(transform);
            topBarContainer = topBarGO.AddComponent<RectTransform>();
            topBarContainer.anchorMin = new Vector2(0, 1f - layoutSettings.topBarHeight);
            topBarContainer.anchorMax = new Vector2(1, 1);
            topBarContainer.pivot = new Vector2(0.5f, 0.5f);
            topBarContainer.sizeDelta = Vector2.zero;
            topBarContainer.anchoredPosition = Vector2.zero;

            Image topBarBg = topBarGO.AddComponent<Image>();
            topBarBg.color = styleSettings.backgroundTint;

            // Create player info
            CreatePlayerInfo(topBarContainer);

            // Create currency display
            CreateCurrencyDisplay(topBarContainer);
        }

        // Create Main Game View
        if (gameViewPanel == null)
        {
            GameObject gameViewGO = new GameObject("GameViewPanel");
            gameViewGO.transform.SetParent(transform);
            gameViewPanel = gameViewGO.AddComponent<RectTransform>();
            gameViewPanel.anchorMin = new Vector2(0, 0.3f);
            gameViewPanel.anchorMax = new Vector2(1, 0.85f);
            gameViewPanel.sizeDelta = Vector2.zero;
            gameViewPanel.anchoredPosition = Vector2.zero;

            CreateGameViewContent(gameViewPanel);
        }

        // Create Skill Bar
        CreateSkillBar();

        // Create Bottom Navigation
        CreateBottomNavigation();

        // Create Side Panels
        CreateSidePanels();
    }

    void CreatePlayerInfo(RectTransform parent)
    {
        GameObject playerInfoGO = new GameObject("PlayerInfo");
        playerInfoGO.transform.SetParent(parent);
        RectTransform playerInfoRect = playerInfoGO.AddComponent<RectTransform>();
        playerInfoRect.anchorMin = new Vector2(0, 0);
        playerInfoRect.anchorMax = new Vector2(0.3f, 1);
        playerInfoRect.sizeDelta = Vector2.zero;
        playerInfoRect.anchoredPosition = Vector2.zero;

        // Player Name
        if (playerNameText == null)
        {
            GameObject nameGO = new GameObject("PlayerName");
            nameGO.transform.SetParent(playerInfoGO.transform);
            RectTransform nameRect = nameGO.AddComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0.1f, 0.5f);
            nameRect.anchorMax = new Vector2(0.9f, 0.8f);
            nameRect.sizeDelta = Vector2.zero;
            nameRect.anchoredPosition = Vector2.zero;

            playerNameText = nameGO.AddComponent<TextMeshProUGUI>();
            playerNameText.text = "Player";
            playerNameText.fontSize = 24;
            playerNameText.alignment = TextAlignmentOptions.Center;
        }

        // Player Level
        if (playerLevelText == null)
        {
            GameObject levelGO = new GameObject("PlayerLevel");
            levelGO.transform.SetParent(playerInfoGO.transform);
            RectTransform levelRect = levelGO.AddComponent<RectTransform>();
            levelRect.anchorMin = new Vector2(0.1f, 0.2f);
            levelRect.anchorMax = new Vector2(0.9f, 0.5f);
            levelRect.sizeDelta = Vector2.zero;
            levelRect.anchoredPosition = Vector2.zero;

            playerLevelText = levelGO.AddComponent<TextMeshProUGUI>();
            playerLevelText.text = "Lv. 1";
            playerLevelText.fontSize = 18;
            playerLevelText.alignment = TextAlignmentOptions.Center;
        }
    }

    void CreateCurrencyDisplay(RectTransform parent)
    {
        GameObject currencyContainer = new GameObject("CurrencyContainer");
        currencyContainer.transform.SetParent(parent);
        RectTransform currencyRect = currencyContainer.AddComponent<RectTransform>();
        currencyRect.anchorMin = new Vector2(0.3f, 0);
        currencyRect.anchorMax = new Vector2(0.9f, 1);
        currencyRect.sizeDelta = Vector2.zero;
        currencyRect.anchoredPosition = Vector2.zero;

        // Create Gold
        CreateCurrencyItem(currencyContainer.transform, "Gold", ref goldText, 0);

        // Create Gems
        CreateCurrencyItem(currencyContainer.transform, "Gems", ref gemText, 1);

        // Create Energy
        CreateCurrencyItem(currencyContainer.transform, "Energy", ref energyText, 2);
    }

    void CreateCurrencyItem(Transform parent, string currencyName, ref TextMeshProUGUI textRef, int index)
    {
        GameObject currencyItem = new GameObject(currencyName);
        currencyItem.transform.SetParent(parent);
        RectTransform itemRect = currencyItem.AddComponent<RectTransform>();
        itemRect.anchorMin = new Vector2(index * 0.33f, 0);
        itemRect.anchorMax = new Vector2((index + 1) * 0.33f, 1);
        itemRect.sizeDelta = Vector2.zero;
        itemRect.anchoredPosition = Vector2.zero;

        // Icon background
        GameObject iconGO = new GameObject("Icon");
        iconGO.transform.SetParent(currencyItem.transform);
        RectTransform iconRect = iconGO.AddComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0.1f, 0.2f);
        iconRect.anchorMax = new Vector2(0.3f, 0.8f);
        iconRect.sizeDelta = Vector2.zero;
        iconRect.anchoredPosition = Vector2.zero;

        Image iconImage = iconGO.AddComponent<Image>();
        iconImage.color = currencyName == "Gold" ? Color.yellow :
                         currencyName == "Gems" ? styleSettings.secondaryColor :
                         styleSettings.primaryColor;

        // Value text
        GameObject textGO = new GameObject("Value");
        textGO.transform.SetParent(currencyItem.transform);
        RectTransform textRect = textGO.AddComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.35f, 0);
        textRect.anchorMax = new Vector2(0.95f, 1);
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;

        textRef = textGO.AddComponent<TextMeshProUGUI>();
        textRef.text = "0";
        textRef.fontSize = 22;
        textRef.alignment = TextAlignmentOptions.MidlineLeft;
    }

    void CreateGameViewContent(RectTransform parent)
    {
        // Stage Info
        if (stageInfoText == null)
        {
            GameObject stageGO = new GameObject("StageInfo");
            stageGO.transform.SetParent(parent);
            RectTransform stageRect = stageGO.AddComponent<RectTransform>();
            stageRect.anchorMin = new Vector2(0.3f, 0.8f);
            stageRect.anchorMax = new Vector2(0.7f, 0.95f);
            stageRect.sizeDelta = Vector2.zero;
            stageRect.anchoredPosition = Vector2.zero;

            stageInfoText = stageGO.AddComponent<TextMeshProUGUI>();
            stageInfoText.text = "Stage 1-1";
            stageInfoText.fontSize = 32;
            stageInfoText.alignment = TextAlignmentOptions.Center;
        }

        // Character Container
        if (characterContainer == null)
        {
            GameObject charGO = new GameObject("CharacterContainer");
            charGO.transform.SetParent(parent);
            RectTransform charRect = charGO.AddComponent<RectTransform>();
            charRect.anchorMin = new Vector2(0, 0.2f);
            charRect.anchorMax = new Vector2(0.4f, 0.8f);
            charRect.sizeDelta = Vector2.zero;
            charRect.anchoredPosition = Vector2.zero;
            characterContainer = charGO.transform;
        }

        // Enemy Container
        if (enemyContainer == null)
        {
            GameObject enemyGO = new GameObject("EnemyContainer");
            enemyGO.transform.SetParent(parent);
            RectTransform enemyRect = enemyGO.AddComponent<RectTransform>();
            enemyRect.anchorMin = new Vector2(0.6f, 0.2f);
            enemyRect.anchorMax = new Vector2(1, 0.8f);
            enemyRect.sizeDelta = Vector2.zero;
            enemyRect.anchoredPosition = Vector2.zero;
            enemyContainer = enemyGO.transform;
        }

        // Damage Text Container
        if (damageTextContainer == null)
        {
            GameObject damageGO = new GameObject("DamageTextContainer");
            damageGO.transform.SetParent(parent);
            RectTransform damageRect = damageGO.AddComponent<RectTransform>();
            damageRect.anchorMin = new Vector2(0, 0);
            damageRect.anchorMax = new Vector2(1, 1);
            damageRect.sizeDelta = Vector2.zero;
            damageRect.anchoredPosition = Vector2.zero;
            damageTextContainer = damageGO.transform;
        }
    }

    void CreateSkillBar()
    {
        GameObject skillBarGO = new GameObject("SkillBar");
        skillBarGO.transform.SetParent(transform);
        RectTransform skillBarRect = skillBarGO.AddComponent<RectTransform>();
        skillBarRect.anchorMin = new Vector2(0.1f, 0.15f);
        skillBarRect.anchorMax = new Vector2(0.9f, 0.25f);
        skillBarRect.sizeDelta = Vector2.zero;
        skillBarRect.anchoredPosition = Vector2.zero;

        // Create skill buttons
        for (int i = 0; i < 6; i++)
        {
            if (i >= skillButtons.Count)
            {
                skillButtons.Add(new SkillButtonData());
            }

            CreateSkillButton(skillBarGO.transform, i);
        }
    }

    void CreateSkillButton(Transform parent, int index)
    {
        GameObject buttonGO = new GameObject($"SkillButton_{index}");
        buttonGO.transform.SetParent(parent);
        RectTransform buttonRect = buttonGO.AddComponent<RectTransform>();

        float buttonWidth = 1f / 6f;
        buttonRect.anchorMin = new Vector2(index * buttonWidth, 0);
        buttonRect.anchorMax = new Vector2((index + 1) * buttonWidth - 0.02f, 1);
        buttonRect.sizeDelta = Vector2.zero;
        buttonRect.anchoredPosition = Vector2.zero;

        Image buttonBg = buttonGO.AddComponent<Image>();
        buttonBg.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);

        Button button = buttonGO.AddComponent<Button>();
        skillButtons[index].button = button;

        // Skill Icon
        GameObject iconGO = new GameObject("Icon");
        iconGO.transform.SetParent(buttonGO.transform);
        RectTransform iconRect = iconGO.AddComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0.1f, 0.1f);
        iconRect.anchorMax = new Vector2(0.9f, 0.9f);
        iconRect.sizeDelta = Vector2.zero;
        iconRect.anchoredPosition = Vector2.zero;

        Image iconImage = iconGO.AddComponent<Image>();
        iconImage.color = new Color(0.5f, 0.5f, 1f, 1f);
        if (skillButtons[index].icon != null)
        {
            iconImage.sprite = skillButtons[index].icon;
        }

        // Cooldown overlay
        GameObject cooldownGO = new GameObject("Cooldown");
        cooldownGO.transform.SetParent(buttonGO.transform);
        RectTransform cooldownRect = cooldownGO.AddComponent<RectTransform>();
        cooldownRect.anchorMin = Vector2.zero;
        cooldownRect.anchorMax = Vector2.one;
        cooldownRect.sizeDelta = Vector2.zero;
        cooldownRect.anchoredPosition = Vector2.zero;

        Image cooldownImage = cooldownGO.AddComponent<Image>();
        cooldownImage.color = new Color(0, 0, 0, 0.7f);
        cooldownImage.type = Image.Type.Filled;
        cooldownImage.fillMethod = Image.FillMethod.Radial360;
        cooldownImage.fillOrigin = (int)Image.Origin360.Top;
        cooldownImage.fillClockwise = false;
        cooldownImage.fillAmount = 0;
        skillButtons[index].cooldownImage = cooldownImage;

        // Cooldown text
        GameObject cdTextGO = new GameObject("CooldownText");
        cdTextGO.transform.SetParent(buttonGO.transform);
        RectTransform cdTextRect = cdTextGO.AddComponent<RectTransform>();
        cdTextRect.anchorMin = Vector2.zero;
        cdTextRect.anchorMax = Vector2.one;
        cdTextRect.sizeDelta = Vector2.zero;
        cdTextRect.anchoredPosition = Vector2.zero;

        TextMeshProUGUI cdText = cdTextGO.AddComponent<TextMeshProUGUI>();
        cdText.text = "";
        cdText.fontSize = 24;
        cdText.alignment = TextAlignmentOptions.Center;
        skillButtons[index].cooldownText = cdText;
    }

    void CreateBottomNavigation()
    {
        GameObject navGO = new GameObject("BottomNavigation");
        navGO.transform.SetParent(transform);
        RectTransform navRect = navGO.AddComponent<RectTransform>();
        navRect.anchorMin = new Vector2(0, 0);
        navRect.anchorMax = new Vector2(1, 0.1f);
        navRect.sizeDelta = Vector2.zero;
        navRect.anchoredPosition = Vector2.zero;

        Image navBg = navGO.AddComponent<Image>();
        navBg.color = new Color(0.15f, 0.15f, 0.15f, 0.95f);

        string[] navNames = { "Inventory", "Skills", "Shop", "Summon", "Quest" };

        for (int i = 0; i < navNames.Length; i++)
        {
            if (i >= navigationButtons.Count)
            {
                navigationButtons.Add(new NavigationButton { buttonName = navNames[i] });
            }
            CreateNavButton(navGO.transform, i, navNames.Length);
        }
    }

    void CreateNavButton(Transform parent, int index, int totalButtons)
    {
        var navData = navigationButtons[index];

        GameObject buttonGO = new GameObject($"Nav_{navData.buttonName}");
        buttonGO.transform.SetParent(parent);
        RectTransform buttonRect = buttonGO.AddComponent<RectTransform>();

        float buttonWidth = 1f / totalButtons;
        buttonRect.anchorMin = new Vector2(index * buttonWidth, 0);
        buttonRect.anchorMax = new Vector2((index + 1) * buttonWidth, 1);
        buttonRect.sizeDelta = Vector2.zero;
        buttonRect.anchoredPosition = Vector2.zero;

        navData.rectTransform = buttonRect;

        Button button = buttonGO.AddComponent<Button>();
        navData.button = button;

        // Icon
        GameObject iconGO = new GameObject("Icon");
        iconGO.transform.SetParent(buttonGO.transform);
        RectTransform iconRect = iconGO.AddComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0.2f, 0.3f);
        iconRect.anchorMax = new Vector2(0.8f, 0.7f);
        iconRect.sizeDelta = Vector2.zero;
        iconRect.anchoredPosition = Vector2.zero;

        Image iconImage = iconGO.AddComponent<Image>();
        iconImage.color = navData.iconColor;
        if (navData.icon != null)
        {
            iconImage.sprite = navData.icon;
        }

        // Label
        GameObject labelGO = new GameObject("Label");
        labelGO.transform.SetParent(buttonGO.transform);
        RectTransform labelRect = labelGO.AddComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0, 0);
        labelRect.anchorMax = new Vector2(1, 0.3f);
        labelRect.sizeDelta = Vector2.zero;
        labelRect.anchoredPosition = Vector2.zero;

        TextMeshProUGUI labelText = labelGO.AddComponent<TextMeshProUGUI>();
        labelText.text = navData.buttonName;
        labelText.fontSize = 14;
        labelText.alignment = TextAlignmentOptions.Center;
    }

    void CreateSidePanels()
    {
        // Equipment Panel
        if (equipmentPanel == null)
        {
            GameObject equipGO = new GameObject("EquipmentPanel");
            equipGO.transform.SetParent(transform);
            equipmentPanel = equipGO.AddComponent<RectTransform>();
            equipmentPanel.anchorMin = new Vector2(0, 0.3f);
            equipmentPanel.anchorMax = new Vector2(0.15f, 0.7f);
            equipmentPanel.sizeDelta = Vector2.zero;
            equipmentPanel.anchoredPosition = Vector2.zero;

            Image equipBg = equipGO.AddComponent<Image>();
            equipBg.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);

            // Create equipment slots
            for (int i = 0; i < 6; i++)
            {
                if (i >= equipmentSlots.Count)
                {
                    equipmentSlots.Add(new EquipmentSlot());
                }
                CreateEquipmentSlot(equipmentPanel, i);
            }
        }

        // Chat Panel
        if (chatPanel == null)
        {
            GameObject chatGO = new GameObject("ChatPanel");
            chatGO.transform.SetParent(transform);
            chatPanel = chatGO.AddComponent<RectTransform>();
            chatPanel.anchorMin = new Vector2(0, 0.1f);
            chatPanel.anchorMax = new Vector2(0.4f, 0.3f);
            chatPanel.sizeDelta = Vector2.zero;
            chatPanel.anchoredPosition = Vector2.zero;

            Image chatBg = chatGO.AddComponent<Image>();
            chatBg.color = new Color(0, 0, 0, 0.7f);

            CreateChatContent(chatPanel);
        }
    }

    void CreateEquipmentSlot(RectTransform parent, int index)
    {
        GameObject slotGO = new GameObject($"EquipSlot_{index}");
        slotGO.transform.SetParent(parent);
        RectTransform slotRect = slotGO.AddComponent<RectTransform>();

        float slotHeight = 1f / 6f;
        slotRect.anchorMin = new Vector2(0.1f, 1 - (index + 1) * slotHeight);
        slotRect.anchorMax = new Vector2(0.9f, 1 - index * slotHeight - 0.02f);
        slotRect.sizeDelta = Vector2.zero;
        slotRect.anchoredPosition = Vector2.zero;

        Image slotBg = slotGO.AddComponent<Image>();
        slotBg.color = new Color(0.3f, 0.3f, 0.3f, 1f);

        Button slotButton = slotGO.AddComponent<Button>();
        equipmentSlots[index].button = slotButton;

        // Item image
        GameObject itemGO = new GameObject("ItemImage");
        itemGO.transform.SetParent(slotGO.transform);
        RectTransform itemRect = itemGO.AddComponent<RectTransform>();
        itemRect.anchorMin = new Vector2(0.1f, 0.1f);
        itemRect.anchorMax = new Vector2(0.9f, 0.9f);
        itemRect.sizeDelta = Vector2.zero;
        itemRect.anchoredPosition = Vector2.zero;

        Image itemImage = itemGO.AddComponent<Image>();
        itemImage.color = new Color(1f, 1f, 1f, 0.5f);
        equipmentSlots[index].itemImage = itemImage;

        if (equipmentSlots[index].currentItem != null)
        {
            itemImage.sprite = equipmentSlots[index].currentItem;
            itemImage.color = Color.white;
        }
    }

    void CreateChatContent(RectTransform parent)
    {
        // Chat scroll view
        GameObject scrollViewGO = new GameObject("ChatScrollView");
        scrollViewGO.transform.SetParent(parent);
        RectTransform scrollRect = scrollViewGO.AddComponent<RectTransform>();
        scrollRect.anchorMin = new Vector2(0, 0.2f);
        scrollRect.anchorMax = new Vector2(1, 1);
        scrollRect.sizeDelta = Vector2.zero;
        scrollRect.anchoredPosition = Vector2.zero;

        chatScrollRect = scrollViewGO.AddComponent<ScrollRect>();
        chatScrollRect.vertical = true;
        chatScrollRect.horizontal = false;

        // Viewport
        GameObject viewport = new GameObject("Viewport");
        viewport.transform.SetParent(scrollViewGO.transform);
        RectTransform viewportRect = viewport.AddComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.sizeDelta = Vector2.zero;
        viewportRect.anchoredPosition = Vector2.zero;

        Image viewportImage = viewport.AddComponent<Image>();
        viewportImage.color = new Color(1, 1, 1, 0.01f);
        Mask viewportMask = viewport.AddComponent<Mask>();
        viewportMask.showMaskGraphic = false;

        // Content
        GameObject content = new GameObject("Content");
        content.transform.SetParent(viewport.transform);
        RectTransform contentRect = content.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.sizeDelta = new Vector2(0, 300);
        contentRect.anchoredPosition = Vector2.zero;

        VerticalLayoutGroup contentLayout = content.AddComponent<VerticalLayoutGroup>();
        contentLayout.padding = new RectOffset(10, 10, 10, 10);
        contentLayout.spacing = 5;
        contentLayout.childControlHeight = false;
        contentLayout.childControlWidth = true;
        contentLayout.childForceExpandHeight = false;
        contentLayout.childForceExpandWidth = true;

        ContentSizeFitter contentFitter = content.AddComponent<ContentSizeFitter>();
        contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        chatScrollRect.viewport = viewportRect;
        chatScrollRect.content = contentRect;

        // Input field
        GameObject inputGO = new GameObject("ChatInput");
        inputGO.transform.SetParent(parent);
        RectTransform inputRect = inputGO.AddComponent<RectTransform>();
        inputRect.anchorMin = new Vector2(0, 0);
        inputRect.anchorMax = new Vector2(1, 0.2f);
        inputRect.sizeDelta = Vector2.zero;
        inputRect.anchoredPosition = Vector2.zero;

        Image inputBg = inputGO.AddComponent<Image>();
        inputBg.color = new Color(0.2f, 0.2f, 0.2f, 1f);

        chatInput = inputGO.AddComponent<TMP_InputField>();

        // Input text area
        GameObject textArea = new GameObject("Text Area");
        textArea.transform.SetParent(inputGO.transform);
        RectTransform textAreaRect = textArea.AddComponent<RectTransform>();
        textAreaRect.anchorMin = Vector2.zero;
        textAreaRect.anchorMax = Vector2.one;
        textAreaRect.sizeDelta = Vector2.zero;
        textAreaRect.anchoredPosition = Vector2.zero;

        RectMask2D textAreaMask = textArea.AddComponent<RectMask2D>();

        GameObject placeholder = new GameObject("Placeholder");
        placeholder.transform.SetParent(textArea.transform);
        RectTransform placeholderRect = placeholder.AddComponent<RectTransform>();
        placeholderRect.anchorMin = Vector2.zero;
        placeholderRect.anchorMax = Vector2.one;
        placeholderRect.sizeDelta = Vector2.zero;
        placeholderRect.anchoredPosition = Vector2.zero;

        TextMeshProUGUI placeholderText = placeholder.AddComponent<TextMeshProUGUI>();
        placeholderText.text = "Enter message...";
        placeholderText.fontSize = 14;
        placeholderText.color = new Color(1, 1, 1, 0.5f);

        GameObject inputText = new GameObject("Text");
        inputText.transform.SetParent(textArea.transform);
        RectTransform inputTextRect = inputText.AddComponent<RectTransform>();
        inputTextRect.anchorMin = Vector2.zero;
        inputTextRect.anchorMax = Vector2.one;
        inputTextRect.sizeDelta = Vector2.zero;
        inputTextRect.anchoredPosition = Vector2.zero;

        TextMeshProUGUI inputTextComponent = inputText.AddComponent<TextMeshProUGUI>();
        inputTextComponent.fontSize = 14;
        inputTextComponent.color = Color.white;

        chatInput.textViewport = textAreaRect;
        chatInput.textComponent = inputTextComponent;
        chatInput.placeholder = placeholderText;
    }

    void ApplyStyling()
    {
        // Apply colors and fonts from styleSettings
        if (styleSettings.primaryFont != null)
        {
            if (playerNameText) playerNameText.font = styleSettings.primaryFont;
            if (stageInfoText) stageInfoText.font = styleSettings.primaryFont;
        }

        // Apply background tints
        var images = GetComponentsInChildren<Image>();
        foreach (var img in images)
        {
            if (img.name.Contains("Background") || img.name.Contains("Bg"))
            {
                img.color = styleSettings.backgroundTint;
            }
        }
    }

    void InitializeFeedbacks()
    {
        // Initialize feedback system using DOTween for maximum compatibility
        foreach (var skill in skillButtons)
        {
            if (skill.button != null)
            {
                // Store MMFeedbacks reference for compatibility
                var mmFeedbacks = skill.button.gameObject.GetComponent<MMFeedbacks>();
                if (mmFeedbacks == null)
                {
                    mmFeedbacks = skill.button.gameObject.AddComponent<MMFeedbacks>();
                    mmFeedbacks.Initialization();
                }
                skill.skillUseFeedback = mmFeedbacks;

                // Create custom feedback using DOTween
                int skillIndex = skillButtons.IndexOf(skill);
                skill.button.onClick.AddListener(() => {
                    // Play custom animation sequence
                    PlaySkillAnimation(skill.button.transform);

                    // Play MMFeedbacks if configured in inspector
                    if (skill.skillUseFeedback != null)
                    {
                        skill.skillUseFeedback.PlayFeedbacks();
                    }

                    TriggerSkillCooldown(skillIndex);
                });
            }
        }
    }

    void PlaySkillAnimation(Transform buttonTransform)
    {
        // Kill any existing animations
        buttonTransform.DOKill();

        // Create a juice sequence
        var sequence = DOTween.Sequence();

        // Scale punch
        sequence.Append(buttonTransform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 10, 1f));

        // Rotation wiggle
        sequence.Join(buttonTransform.DOPunchRotation(new Vector3(0, 0, 15f), 0.3f, 10, 1f));

        // Optional: Add color flash if button has Image component
        var image = buttonTransform.GetComponent<Image>();
        if (image != null)
        {
            var originalColor = image.color;
            sequence.Join(image.DOColor(Color.white, 0.1f).OnComplete(() => {
                image.DOColor(originalColor, 0.2f);
            }));
        }

        // Optional: Add glow effect
        var outline = buttonTransform.GetComponent<Outline>();
        if (outline == null)
        {
            outline = buttonTransform.gameObject.AddComponent<Outline>();
            outline.effectColor = styleSettings.primaryColor;
            outline.effectDistance = new Vector2(2, 2);
            outline.enabled = false;
        }

        sequence.OnStart(() => outline.enabled = true);
        sequence.OnComplete(() => outline.enabled = false);
    }

    void AnimateUIEntrance()
    {
        if (startupSequence != null && startupSequence.IsActive())
        {
            startupSequence.Kill();
        }

        startupSequence = DOTween.Sequence();

        // Animate top bar slide in
        if (topBarContainer)
        {
            topBarContainer.anchoredPosition = new Vector2(0, animationSettings.topBarSlideDistance);
            startupSequence.Append(
                topBarContainer.DOAnchorPosY(0, animationSettings.defaultDuration)
                    .SetEase(animationSettings.defaultEase)
                    .SetDelay(animationSettings.topBarSlideInDelay)
            );
        }

        // Animate skill buttons with stagger
        for (int i = 0; i < skillButtons.Count; i++)
        {
            if (skillButtons[i].button != null)
            {
                var button = skillButtons[i].button;
                button.transform.localScale = Vector3.one * animationSettings.skillButtonScaleFrom;

                startupSequence.Insert(
                    animationSettings.topBarSlideInDelay + (i * animationSettings.skillButtonStaggerDelay),
                    button.transform.DOScale(1f, animationSettings.defaultDuration)
                        .SetEase(Ease.OutBack)
                );
            }
        }

        // Animate navigation buttons
        for (int i = 0; i < navigationButtons.Count; i++)
        {
            if (navigationButtons[i].rectTransform != null)
            {
                var navButton = navigationButtons[i].rectTransform;
                navButton.anchoredPosition = new Vector2(navButton.anchoredPosition.x, -100);

                startupSequence.Insert(
                    0.5f + (i * 0.05f),
                    navButton.DOAnchorPosY(0, animationSettings.defaultDuration)
                        .SetEase(Ease.OutBack)
                );
            }
        }
    }

    public void TriggerSkillCooldown(int skillIndex)
    {
        if (skillIndex < 0 || skillIndex >= skillButtons.Count) return;

        var skill = skillButtons[skillIndex];
        if (skill.cooldownImage == null) return;

        // Kill existing tween if any
        if (skillCooldownTweeners.ContainsKey(skillIndex))
        {
            skillCooldownTweeners[skillIndex]?.Kill();
        }

        // Start cooldown animation
        skill.cooldownImage.fillAmount = 1f;
        skill.button.interactable = false;

        var tween = DOTween.To(
            () => skill.cooldownImage.fillAmount,
            x => {
                skill.cooldownImage.fillAmount = x;
                if (skill.cooldownText != null)
                {
                    skill.cooldownText.text = Mathf.CeilToInt(x * skill.cooldown).ToString();
                }
            },
            0f,
            skill.cooldown
        ).SetEase(Ease.Linear)
        .OnComplete(() => {
            skill.button.interactable = true;
            if (skill.cooldownText != null)
            {
                skill.cooldownText.text = "";
            }
            skill.skillUseFeedback?.PlayFeedbacks();
        });

        skillCooldownTweeners[skillIndex] = tween;
    }

    public enum DamageType { Normal, Critical, Heal }

    public void ShowDamageText(long damage, DamageType damageType = DamageType.Normal)
    {
        if (damageTextPrefab == null || damageTextContainer == null) return;

        var damageGO = Instantiate(damageTextPrefab, damageTextContainer);
        var damageText = damageGO.GetComponent<TextMeshProUGUI>();

        if (damageText != null)
        {
            // Format damage number
            damageText.text = damage.ToString("N0");

            // Apply color based on type
            switch (damageType)
            {
                case DamageType.Critical:
                    damageText.color = styleSettings.secondaryColor;
                    damageText.fontSize *= 1.5f;
                    break;
                case DamageType.Heal:
                    damageText.color = Color.green;
                    damageText.text = "+" + damageText.text;
                    break;
                default:
                    damageText.color = Color.white;
                    break;
            }

            // Animate with more juice
            var rectTransform = damageGO.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = new Vector2(UnityEngine.Random.Range(-50f, 50f), 0);

            var sequence = DOTween.Sequence();

            // Rise animation
            sequence.Append(rectTransform.DOAnchorPosY(animationSettings.damageTextRiseHeight, animationSettings.damageTextDuration).SetEase(Ease.OutQuad));

            // Fade out
            sequence.Join(damageText.DOFade(0, animationSettings.damageTextDuration * 0.7f).SetDelay(animationSettings.damageTextDuration * 0.3f));

            // Scale effect for critical
            if (damageType == DamageType.Critical)
            {
                rectTransform.localScale = Vector3.zero;
                sequence.Join(rectTransform.DOScale(1.5f, 0.3f).SetEase(Ease.OutBack));
                sequence.Insert(0.3f, rectTransform.DOScale(1.2f, 0.2f));
            }
            else
            {
                rectTransform.localScale = Vector3.one * 0.8f;
                sequence.Join(rectTransform.DOScale(1f, 0.2f).SetEase(Ease.OutBack));
            }

            sequence.OnComplete(() => Destroy(damageGO));

            // Play feedback if available
            if (damagePopupFeedback != null)
            {
                damagePopupFeedback.PlayFeedbacks();
            }
        }
    }

    [Button("Update Currency with Animation", ButtonSizes.Large)]
    public void UpdateCurrency(int gold, int gems, int energy)
    {
        AnimateNumberChange(goldText, gold);
        AnimateNumberChange(gemText, gems);
        AnimateNumberChange(energyText, energy);

        // Visual feedback using DOTween (Feel의 Haptic은 모바일 전용이라 제거)
        if (goldText != null)
        {
            goldText.transform.DOKill();
            goldText.transform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 5, 0.5f);

            // Color flash
            goldText.DOColor(styleSettings.primaryColor, 0.1f).SetLoops(2, LoopType.Yoyo);
        }
        if (gemText != null)
        {
            gemText.transform.DOKill();
            gemText.transform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 5, 0.5f).SetDelay(0.1f);
            gemText.DOColor(styleSettings.secondaryColor, 0.1f).SetLoops(2, LoopType.Yoyo).SetDelay(0.1f);
        }
        if (energyText != null)
        {
            energyText.transform.DOKill();
            energyText.transform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 5, 0.5f).SetDelay(0.2f);
            energyText.DOColor(Color.yellow, 0.1f).SetLoops(2, LoopType.Yoyo).SetDelay(0.2f);
        }

        // Play Feel feedback if you have it set up in the inspector
        if (damagePopupFeedback != null)
        {
            damagePopupFeedback.PlayFeedbacks();
        }
    }

    void AnimateNumberChange(TextMeshProUGUI textComponent, int targetValue)
    {
        if (textComponent == null) return;

        int.TryParse(textComponent.text.Replace(",", ""), out int currentValue);

        DOTween.To(
            () => currentValue,
            x => textComponent.text = x.ToString("N0"),
            targetValue,
            animationSettings.currencyCountDuration
        ).SetEase(animationSettings.currencyCountCurve);
    }

    [Title("Debug Tools")]
    [ButtonGroup("Debug")]
    [Button("Fix UI Layout")]
    void FixUILayout()
    {
        RectTransform myRect = GetComponent<RectTransform>();
        if (myRect != null)
        {
            myRect.anchorMin = Vector2.zero;
            myRect.anchorMax = Vector2.one;
            myRect.offsetMin = Vector2.zero;
            myRect.offsetMax = Vector2.zero;
            myRect.pivot = new Vector2(0.5f, 0.5f);
            myRect.localScale = Vector3.one;
            myRect.localPosition = Vector3.zero;
        }

        Canvas.ForceUpdateCanvases();
    }

    [ButtonGroup("Debug")]
    [Button("Shake Screen")]
    void DebugShakeScreen()
    {
        var canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            canvas.transform.DOShakePosition(0.5f, 10f, 20);
        }
    }

    [ButtonGroup("Debug")]
    [Button("Flash UI")]
    void DebugFlashUI()
    {
        var canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        // DOTween flash with more juice
        var sequence = DOTween.Sequence();
        sequence.Append(canvasGroup.DOFade(0.3f, 0.1f));
        sequence.Append(canvasGroup.DOFade(1f, 0.1f));
        sequence.Join(transform.DOShakePosition(0.2f, 5f, 20, 90, false, true));

        // Alternative: Use color flash on all images
        var images = GetComponentsInChildren<Image>();
        foreach (var img in images)
        {
            img.DOColor(Color.white, 0.1f).SetLoops(2, LoopType.Yoyo);
        }
    }

    [Title("Advanced Animation Methods")]
    [ButtonGroup("Effects")]
    [Button("Combo Effect")]
    void PlayComboEffect()
    {
        int comboCount = 5;
        var sequence = DOTween.Sequence();

        for (int i = 0; i < skillButtons.Count && i < comboCount; i++)
        {
            if (skillButtons[i].button != null)
            {
                var button = skillButtons[i].button.transform;
                sequence.Insert(i * 0.1f, button.DOPunchScale(Vector3.one * 0.3f, 0.3f, 10, 1f));
                sequence.Insert(i * 0.1f, button.DOPunchRotation(new Vector3(0, 0, 30f), 0.3f, 10, 1f));
            }
        }

        // Screen flash
        var canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
        sequence.Insert(0.3f, canvasGroup.DOFade(0.8f, 0.05f));
        sequence.Insert(0.35f, canvasGroup.DOFade(1f, 0.1f));
    }

    [ButtonGroup("Effects")]
    [Button("Victory Animation")]
    void PlayVictoryAnimation()
    {
        var sequence = DOTween.Sequence();

        // Bounce all navigation buttons
        for (int i = 0; i < navigationButtons.Count; i++)
        {
            if (navigationButtons[i].rectTransform != null)
            {
                var nav = navigationButtons[i].rectTransform;
                nav.localScale = Vector3.one;
                sequence.Insert(i * 0.1f, nav.DOScale(1.2f, 0.2f));
                sequence.Insert(i * 0.1f + 0.2f, nav.DOScale(1f, 0.2f));
                sequence.Insert(i * 0.1f, nav.DOLocalRotate(new Vector3(0, 0, 360), 0.4f, RotateMode.FastBeyond360));
            }
        }

        // Fireworks effect with damage texts
        for (int i = 0; i < 5; i++)
        {
            sequence.InsertCallback(i * 0.2f, () => {
                ShowDamageText(UnityEngine.Random.Range(100000, 999999), DamageType.Critical);
            });
        }
    }

    void OnDestroy()
    {
        // Clean up tweens
        foreach (var tween in skillCooldownTweeners.Values)
        {
            tween?.Kill();
        }
        startupSequence?.Kill();
        DOTween.KillAll();
    }
}