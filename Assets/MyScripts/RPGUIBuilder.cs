using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using RPG.UI;
using System.Reflection;

public class RPGUIBuilder : EditorWindow
{
    private Color goldColor = new Color(1f, 0.84f, 0f);
    private Color diamondColor = new Color(0.4f, 0.8f, 1f);
    private Color energyColor = new Color(0.4f, 1f, 0.4f);
    private Color darkBG = new Color(0.1f, 0.1f, 0.1f, 0.95f);

    private Font defaultFont;
    private TMP_FontAsset cafe24TMPFont;

    // UI 요소들 캐시
    private struct UIElements
    {
        // 플레이어 정보
        public Image playerAvatar;
        public Text playerNameText;
        public Text playerLevelText;
        public Slider experienceBar;

        // 자원
        public Image goldIcon;
        public Text goldText;
        public Image diamondIcon;
        public Text diamondText;

        // 스테이지
        public Text stageNumberText;
        public Text stageName;
        public Slider stageProgressBar;

        // 몬스터
        public Text monsterNameText;
        public Slider monsterHealthBar;
        public GameObject monsterHealthBarGroup;

        // 버튼들
        public Button characterButton;
        public Button inventoryButton;
        public Button skillButton;
        public Button shopButton;
        public Button questButton;
        public Button achievementButton;
        public Button rankingButton;
        public Button settingsButton;

        // 패널들
        public GameObject characterPanel;
        public GameObject inventoryPanel;
        public GameObject skillPanel;
        public GameObject shopPanel;

        // 이펙트
        public Transform effectContainer;
    }

    private UIElements uiElements;

    [MenuItem("Tools/Create RPG UI")]
    public static void ShowWindow()
    {
        GetWindow<RPGUIBuilder>("RPG UI Builder");
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField("RPG UI Builder", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        if (GUILayout.Button("Create Complete UI with MainUIManager (TextMeshPro)", GUILayout.Height(50)))
        {
            CreateCompleteUIWithTMP();
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Create Complete UI with MainUIManager (Legacy Text)", GUILayout.Height(50)))
        {
            CreateCompleteUIWithLegacyText();
        }
    }

    void CreateCompleteUIWithTMP()
    {
        if (!TMPro.TMP_Settings.instance)
        {
            EditorUtility.DisplayDialog("TextMeshPro",
                "TextMeshPro Essential Resources가 필요합니다.\n" +
                "Window > TextMeshPro > Import TMP Essential Resources를 선택해주세요.",
                "OK");
            return;
        }

        // Cafe24Ssurround TMP 폰트 찾기
        FindCafe24TMPFont();

        CreateCompleteUI(true);
    }

    void CreateCompleteUIWithLegacyText()
    {
        // Cafe24Ssurround 폰트 찾기
        FindCafe24Font();

        CreateCompleteUI(false);
    }

    void FindCafe24TMPFont()
    {
        // Cafe24Ssurround TMP Font Asset 검색
        string[] fontGuids = AssetDatabase.FindAssets("Cafe24Ssurround t:TMP_FontAsset");
        if (fontGuids.Length > 0)
        {
            string fontPath = AssetDatabase.GUIDToAssetPath(fontGuids[0]);
            cafe24TMPFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fontPath);
            Debug.Log($"<color=green>Cafe24Ssurround TMP Font found: {fontPath}</color>");
        }
        else
        {
            // TMP Font Asset이 없으면 TTF에서 생성 시도
            string[] ttfGuids = AssetDatabase.FindAssets("Cafe24Ssurround t:Font");
            if (ttfGuids.Length > 0)
            {
                EditorUtility.DisplayDialog("Font Asset 생성 필요",
                    "Cafe24Ssurround TMP Font Asset이 없습니다.\n" +
                    "Window > TextMeshPro > Font Asset Creator에서 생성해주세요.\n" +
                    "또는 폰트 파일 우클릭 > Create > TextMeshPro > Font Asset",
                    "OK");
            }

            // 기본 폰트 사용
            cafe24TMPFont = TMP_Settings.defaultFontAsset;
            Debug.LogWarning("Cafe24Ssurround TMP Font not found. Using default font.");
        }
    }

    void FindCafe24Font()
    {
        // Cafe24Ssurround 폰트 검색
        string[] fontGuids = AssetDatabase.FindAssets("Cafe24Ssurround t:Font");
        if (fontGuids.Length > 0)
        {
            string fontPath = AssetDatabase.GUIDToAssetPath(fontGuids[0]);
            defaultFont = AssetDatabase.LoadAssetAtPath<Font>(fontPath);
            Debug.Log($"<color=green>Cafe24Ssurround Font found: {fontPath}</color>");
        }
        else
        {
            // 못 찾으면 기본 폰트 사용
            defaultFont = Font.CreateDynamicFontFromOSFont("Arial", 14);
            if (defaultFont == null)
            {
                string[] anyFontGuids = AssetDatabase.FindAssets("t:Font");
                if (anyFontGuids.Length > 0)
                {
                    string fontPath = AssetDatabase.GUIDToAssetPath(anyFontGuids[0]);
                    defaultFont = AssetDatabase.LoadAssetAtPath<Font>(fontPath);
                }
            }
            Debug.LogWarning("Cafe24Ssurround Font not found. Using fallback font.");
        }
    }

    void CreateCompleteUI(bool useTMP)
    {
        // UI 요소 초기화
        uiElements = new UIElements();

        // Canvas 생성
        GameObject canvasObj = new GameObject("GameCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();

        // MainUIManager 추가
        MainUIManager mainUIManager = canvasObj.AddComponent<MainUIManager>();

        // 메인 컨테이너 (SafeArea를 위해)
        GameObject mainContainer = CreateUIElement("MainContainer", canvasObj.transform);
        RectTransform mainRect = mainContainer.GetComponent<RectTransform>();
        SetFullScreen(mainRect);

        // 배경
        CreateBackground(mainContainer.transform);

        // 상단 UI
        CreateTopUI(mainContainer.transform, useTMP);

        // 게임 영역
        CreateGameArea(mainContainer.transform, useTMP);

        // 중앙 퀵슬롯
        CreateQuickSlots(mainContainer.transform, useTMP);

        // 하단 UI
        CreateBottomUI(mainContainer.transform, useTMP);

        // 팝업 패널들
        CreatePopupPanels(mainContainer.transform, useTMP);

        // 이펙트 컨테이너
        uiElements.effectContainer = CreateUIElement("EffectContainer", mainContainer.transform).transform;

        // MainUIManager에 UI 요소들 할당
        AssignUIElementsToManager(mainUIManager);

        Debug.Log($"RPG UI Created Successfully with MainUIManager! (Using {(useTMP ? "TextMeshPro" : "Legacy Text")})");
    }

    void AssignUIElementsToManager(MainUIManager manager)
    {
        if (manager == null) return;

        System.Type managerType = typeof(MainUIManager);
        BindingFlags bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance;

        // 플레이어 정보 할당
        SetFieldValue(managerType, manager, "playerAvatar", uiElements.playerAvatar, bindingFlags);
        SetFieldValue(managerType, manager, "playerNameText", uiElements.playerNameText, bindingFlags);
        SetFieldValue(managerType, manager, "playerLevelText", uiElements.playerLevelText, bindingFlags);
        SetFieldValue(managerType, manager, "experienceBar", uiElements.experienceBar, bindingFlags);

        // 자원 표시 할당
        SetFieldValue(managerType, manager, "goldIcon", uiElements.goldIcon, bindingFlags);
        SetFieldValue(managerType, manager, "goldText", uiElements.goldText, bindingFlags);
        SetFieldValue(managerType, manager, "diamondIcon", uiElements.diamondIcon, bindingFlags);
        SetFieldValue(managerType, manager, "diamondText", uiElements.diamondText, bindingFlags);

        // 스테이지 정보 할당
        SetFieldValue(managerType, manager, "stageNumberText", uiElements.stageNumberText, bindingFlags);
        SetFieldValue(managerType, manager, "stageName", uiElements.stageName, bindingFlags);
        SetFieldValue(managerType, manager, "stageProgressBar", uiElements.stageProgressBar, bindingFlags);

        // 몬스터 정보 할당
        SetFieldValue(managerType, manager, "monsterNameText", uiElements.monsterNameText, bindingFlags);
        SetFieldValue(managerType, manager, "monsterHealthBar", uiElements.monsterHealthBar, bindingFlags);
        SetFieldValue(managerType, manager, "monsterHealthBarGroup", uiElements.monsterHealthBarGroup, bindingFlags);

        // 메뉴 버튼 할당
        SetFieldValue(managerType, manager, "characterButton", uiElements.characterButton, bindingFlags);
        SetFieldValue(managerType, manager, "inventoryButton", uiElements.inventoryButton, bindingFlags);
        SetFieldValue(managerType, manager, "skillButton", uiElements.skillButton, bindingFlags);
        SetFieldValue(managerType, manager, "shopButton", uiElements.shopButton, bindingFlags);
        SetFieldValue(managerType, manager, "questButton", uiElements.questButton, bindingFlags);
        SetFieldValue(managerType, manager, "achievementButton", uiElements.achievementButton, bindingFlags);
        SetFieldValue(managerType, manager, "rankingButton", uiElements.rankingButton, bindingFlags);
        SetFieldValue(managerType, manager, "settingsButton", uiElements.settingsButton, bindingFlags);

        // 패널 할당
        SetFieldValue(managerType, manager, "characterPanel", uiElements.characterPanel, bindingFlags);
        SetFieldValue(managerType, manager, "inventoryPanel", uiElements.inventoryPanel, bindingFlags);
        SetFieldValue(managerType, manager, "skillPanel", uiElements.skillPanel, bindingFlags);
        SetFieldValue(managerType, manager, "shopPanel", uiElements.shopPanel, bindingFlags);

        // 이펙트 컨테이너 할당
        SetFieldValue(managerType, manager, "effectContainer", uiElements.effectContainer, bindingFlags);

        // Dirty 표시로 변경사항 저장
        UnityEditor.EditorUtility.SetDirty(manager);

        Debug.Log("<color=green>All UI elements successfully assigned to MainUIManager!</color>");
    }

    void SetFieldValue(System.Type type, object obj, string fieldName, object value, BindingFlags bindingFlags)
    {
        FieldInfo field = type.GetField(fieldName, bindingFlags);
        if (field != null && value != null)
        {
            field.SetValue(obj, value);
            Debug.Log($"<color=cyan>Assigned {fieldName} to MainUIManager</color>");
        }
        else if (field == null)
        {
            Debug.LogWarning($"<color=yellow>Field {fieldName} not found in MainUIManager</color>");
        }
    }

    void SetFullScreen(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;
        rect.anchoredPosition = Vector2.zero;
    }

    void CreateBackground(Transform parent)
    {
        GameObject bg = CreateUIElement("Background", parent);
        Image bgImage = bg.AddComponent<Image>();
        bgImage.color = new Color(0.05f, 0.05f, 0.05f, 1f);

        RectTransform rect = bg.GetComponent<RectTransform>();
        SetFullScreen(rect);
    }

    void CreateTopUI(Transform parent, bool useTMP)
    {
        GameObject topUI = CreateUIElement("TopUI", parent);
        RectTransform topRect = topUI.GetComponent<RectTransform>();
        topRect.anchorMin = new Vector2(0, 0.92f);  // 상단 UI 높이 축소 (10% -> 8%)
        topRect.anchorMax = Vector2.one;
        topRect.offsetMin = new Vector2(0, 0);
        topRect.offsetMax = new Vector2(0, 0);

        // 플레이어 정보
        CreatePlayerInfo(topUI.transform, useTMP);

        // 자원 바
        CreateResourceBar(topUI.transform, useTMP);

        // 상단 메뉴 버튼
        CreateTopMenuButtons(topUI.transform);
    }

    void CreatePlayerInfo(Transform parent, bool useTMP)
    {
        GameObject playerInfo = CreateUIElement("PlayerInfo", parent);
        RectTransform rect = playerInfo.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 0);
        rect.anchorMax = new Vector2(0.3f, 1);
        rect.offsetMin = new Vector2(10, 0);
        rect.offsetMax = new Vector2(-10, 0);

        // 아바타 프레임
        GameObject avatarFrame = CreateUIElement("AvatarFrame", playerInfo.transform);
        Image frameImg = avatarFrame.AddComponent<Image>();
        frameImg.color = new Color(0.8f, 0.6f, 0.3f);
        uiElements.playerAvatar = frameImg; // 캐시에 저장

        RectTransform frameRect = avatarFrame.GetComponent<RectTransform>();
        frameRect.anchorMin = new Vector2(0, 0.5f);
        frameRect.anchorMax = new Vector2(0, 0.5f);
        frameRect.sizeDelta = new Vector2(60, 60);  // 아바타 크기 축소
        frameRect.anchoredPosition = new Vector2(40, 0);

        // 플레이어 이름
        GameObject nameObj = CreateUIElement("PlayerName", playerInfo.transform);
        RectTransform nameRect = nameObj.GetComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0.4f, 0.6f);
        nameRect.anchorMax = new Vector2(0.9f, 0.9f);
        nameRect.offsetMin = Vector2.zero;
        nameRect.offsetMax = Vector2.zero;

        if (useTMP)
        {
            TextMeshProUGUI tmpName = nameObj.AddComponent<TextMeshProUGUI>();
            tmpName.text = "Player";
            tmpName.font = cafe24TMPFont;
            tmpName.fontSize = 18;
            tmpName.alignment = TextAlignmentOptions.MidlineLeft;
        }
        else
        {
            Text nameText = nameObj.AddComponent<Text>();
            nameText.text = "Player";
            nameText.font = defaultFont;
            nameText.fontSize = 18;
            nameText.alignment = TextAnchor.MiddleLeft;
            nameText.color = Color.white;
            uiElements.playerNameText = nameText;
        }

        // 레벨 표시
        GameObject levelObj = CreateUIElement("PlayerLevel", playerInfo.transform);
        RectTransform levelRect = levelObj.GetComponent<RectTransform>();
        levelRect.anchorMin = new Vector2(0.4f, 0.3f);
        levelRect.anchorMax = new Vector2(0.9f, 0.6f);
        levelRect.offsetMin = Vector2.zero;
        levelRect.offsetMax = Vector2.zero;

        if (useTMP)
        {
            TextMeshProUGUI tmpLevel = levelObj.AddComponent<TextMeshProUGUI>();
            tmpLevel.text = "Lv.1";
            tmpLevel.font = cafe24TMPFont;
            tmpLevel.fontSize = 22;
            tmpLevel.alignment = TextAlignmentOptions.MidlineLeft;
            tmpLevel.color = Color.yellow;
        }
        else
        {
            Text levelText = levelObj.AddComponent<Text>();
            levelText.text = "Lv.1";
            levelText.font = defaultFont;
            levelText.fontSize = 22;
            levelText.alignment = TextAnchor.MiddleLeft;
            levelText.color = Color.yellow;
            uiElements.playerLevelText = levelText;
        }

        // 경험치 바
        GameObject expBar = CreateUIElement("ExperienceBar", playerInfo.transform);
        RectTransform expRect = expBar.GetComponent<RectTransform>();
        expRect.anchorMin = new Vector2(0.4f, 0.1f);
        expRect.anchorMax = new Vector2(0.9f, 0.3f);
        expRect.offsetMin = Vector2.zero;
        expRect.offsetMax = Vector2.zero;

        Slider expSlider = expBar.AddComponent<Slider>();
        uiElements.experienceBar = expSlider;

        // 슬라이더 배경
        GameObject expBg = CreateUIElement("Background", expBar.transform);
        Image expBgImg = expBg.AddComponent<Image>();
        expBgImg.color = new Color(0.2f, 0.2f, 0.2f);
        RectTransform expBgRect = expBg.GetComponent<RectTransform>();
        SetFullScreen(expBgRect);

        // 슬라이더 채우기
        GameObject expFillArea = CreateUIElement("Fill Area", expBar.transform);
        RectTransform fillAreaRect = expFillArea.GetComponent<RectTransform>();
        SetFullScreen(fillAreaRect);

        GameObject expFill = CreateUIElement("Fill", expFillArea.transform);
        Image expFillImg = expFill.AddComponent<Image>();
        expFillImg.color = new Color(0.2f, 0.8f, 1f);
        RectTransform expFillRect = expFill.GetComponent<RectTransform>();
        SetFullScreen(expFillRect);

        expSlider.fillRect = expFillRect;
        expSlider.value = 0.3f;
    }

    void CreateResourceBar(Transform parent, bool useTMP)
    {
        GameObject resourceBar = CreateUIElement("ResourceBar", parent);
        RectTransform rect = resourceBar.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.3f, 0);
        rect.anchorMax = new Vector2(0.7f, 1);
        rect.offsetMin = new Vector2(10, 10);
        rect.offsetMax = new Vector2(-10, -10);

        HorizontalLayoutGroup layout = resourceBar.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 20;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        // 골드
        CreateResourceItem(resourceBar.transform, "Gold", goldColor, "34,168", useTMP, true);

        // 다이아몬드  
        CreateResourceItem(resourceBar.transform, "Diamond", diamondColor, "22,000", useTMP, false);
    }

    void CreateResourceItem(Transform parent, string name, Color color, string amount, bool useTMP, bool isGold)
    {
        GameObject container = CreateUIElement(name + "Container", parent);

        HorizontalLayoutGroup layout = container.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 5;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
        layout.padding = new RectOffset(10, 10, 5, 5);

        // 배경
        Image bg = container.AddComponent<Image>();
        bg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

        // 아이콘
        GameObject icon = CreateUIElement(name + "Icon", container.transform);
        Image iconImg = icon.AddComponent<Image>();
        iconImg.color = color;
        RectTransform iconRect = icon.GetComponent<RectTransform>();
        iconRect.sizeDelta = new Vector2(30, 30);

        // 아이콘 캐시에 저장
        if (isGold)
            uiElements.goldIcon = iconImg;
        else
            uiElements.diamondIcon = iconImg;

        // 텍스트
        GameObject textObj = CreateUIElement(name + "Text", container.transform);
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.sizeDelta = new Vector2(80, 30);

        if (useTMP)
        {
            TextMeshProUGUI tmpText = textObj.AddComponent<TextMeshProUGUI>();
            tmpText.text = amount;
            tmpText.font = cafe24TMPFont;
            tmpText.fontSize = 18;
            tmpText.color = Color.white;
            tmpText.alignment = TextAlignmentOptions.MidlineLeft;
        }
        else
        {
            Text amountText = textObj.AddComponent<Text>();
            amountText.text = amount;
            amountText.font = defaultFont;
            amountText.fontSize = 18;
            amountText.color = Color.white;
            amountText.alignment = TextAnchor.MiddleLeft;

            // 텍스트 캐시에 저장
            if (isGold)
                uiElements.goldText = amountText;
            else
                uiElements.diamondText = amountText;
        }
    }

    void CreateTopMenuButtons(Transform parent)
    {
        GameObject menuButtons = CreateUIElement("TopMenuButtons", parent);
        RectTransform rect = menuButtons.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.7f, 0);
        rect.anchorMax = new Vector2(1, 1);
        rect.offsetMin = new Vector2(10, 10);
        rect.offsetMax = new Vector2(-10, -10);

        HorizontalLayoutGroup layout = menuButtons.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 10;
        layout.childAlignment = TextAnchor.MiddleRight;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        // 퀘스트 버튼
        uiElements.questButton = CreateTopMenuButton(menuButtons.transform, "QuestBtn", new Color(0.8f, 0.6f, 0.2f));

        // 업적 버튼
        uiElements.achievementButton = CreateTopMenuButton(menuButtons.transform, "AchievementBtn", new Color(0.2f, 0.6f, 0.8f));

        // 랭킹 버튼
        uiElements.rankingButton = CreateTopMenuButton(menuButtons.transform, "RankingBtn", new Color(0.8f, 0.8f, 0.2f));

        // 설정 버튼
        uiElements.settingsButton = CreateTopMenuButton(menuButtons.transform, "SettingsBtn", new Color(0.5f, 0.5f, 0.5f));
    }

    Button CreateTopMenuButton(Transform parent, string name, Color color)
    {
        GameObject btn = CreateUIElement(name, parent);
        Button button = btn.AddComponent<Button>();
        Image img = btn.AddComponent<Image>();
        img.color = color;

        RectTransform rect = btn.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(50, 50);

        return button;
    }

    void CreateGameArea(Transform parent, bool useTMP)
    {
        GameObject gameArea = CreateUIElement("GameArea", parent);
        RectTransform rect = gameArea.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 0.35f);  // 게임 영역 확대
        rect.anchorMax = new Vector2(1, 0.92f);  // 상단 UI에 맞춰 조정
        rect.offsetMin = new Vector2(0, 0);
        rect.offsetMax = new Vector2(0, 0);

        // 스테이지 정보
        CreateStageInfo(gameArea.transform, useTMP);

        // 전투 필드
        CreateBattleField(gameArea.transform, useTMP);
    }

    void CreateStageInfo(Transform parent, bool useTMP)
    {
        GameObject stageInfo = CreateUIElement("StageInfo", parent);
        RectTransform rect = stageInfo.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.3f, 0.8f);
        rect.anchorMax = new Vector2(0.7f, 0.95f);
        rect.offsetMin = new Vector2(0, 0);
        rect.offsetMax = new Vector2(0, 0);

        // 배경
        Image bg = stageInfo.AddComponent<Image>();
        bg.color = new Color(0.5f, 0.3f, 0.1f);

        // STAGE 텍스트
        GameObject stageText = CreateUIElement("StageText", stageInfo.transform);
        RectTransform stageRect = stageText.GetComponent<RectTransform>();
        stageRect.anchorMin = new Vector2(0.5f, 0.5f);
        stageRect.anchorMax = new Vector2(0.5f, 0.5f);
        stageRect.sizeDelta = new Vector2(200, 40);
        stageRect.anchoredPosition = new Vector2(0, 20);

        if (useTMP)
        {
            TextMeshProUGUI tmpStage = stageText.AddComponent<TextMeshProUGUI>();
            tmpStage.text = "STAGE";
            tmpStage.font = cafe24TMPFont;
            tmpStage.fontSize = 24;
            tmpStage.alignment = TextAlignmentOptions.Center;
            tmpStage.color = Color.white;
        }
        else
        {
            Text stage = stageText.AddComponent<Text>();
            stage.text = "STAGE";
            stage.font = defaultFont;
            stage.fontSize = 24;
            stage.alignment = TextAnchor.MiddleCenter;
            stage.color = Color.white;
            uiElements.stageName = stage;
        }

        // 스테이지 번호
        GameObject numberText = CreateUIElement("StageNumber", stageInfo.transform);
        RectTransform numRect = numberText.GetComponent<RectTransform>();
        numRect.anchorMin = new Vector2(0.5f, 0.5f);
        numRect.anchorMax = new Vector2(0.5f, 0.5f);
        numRect.sizeDelta = new Vector2(200, 50);
        numRect.anchoredPosition = new Vector2(0, -20);

        if (useTMP)
        {
            TextMeshProUGUI tmpNumber = numberText.AddComponent<TextMeshProUGUI>();
            tmpNumber.text = "1";
            tmpNumber.font = cafe24TMPFont;
            tmpNumber.fontSize = 36;
            tmpNumber.fontStyle = FontStyles.Bold;
            tmpNumber.alignment = TextAlignmentOptions.Center;
            tmpNumber.color = Color.yellow;
        }
        else
        {
            Text number = numberText.AddComponent<Text>();
            number.text = "1";
            number.font = defaultFont;
            number.fontSize = 36;
            number.fontStyle = FontStyle.Bold;
            number.alignment = TextAnchor.MiddleCenter;
            number.color = Color.yellow;
            uiElements.stageNumberText = number;
        }

        // 스테이지 진행도 바
        GameObject progressBar = CreateUIElement("StageProgressBar", parent);
        RectTransform progressRect = progressBar.GetComponent<RectTransform>();
        progressRect.anchorMin = new Vector2(0.2f, 0.75f);
        progressRect.anchorMax = new Vector2(0.8f, 0.78f);
        progressRect.offsetMin = Vector2.zero;
        progressRect.offsetMax = Vector2.zero;

        Slider progressSlider = progressBar.AddComponent<Slider>();
        uiElements.stageProgressBar = progressSlider;

        // 진행도 바 배경
        GameObject progressBg = CreateUIElement("Background", progressBar.transform);
        Image progressBgImg = progressBg.AddComponent<Image>();
        progressBgImg.color = new Color(0.2f, 0.2f, 0.2f);
        RectTransform progressBgRect = progressBg.GetComponent<RectTransform>();
        SetFullScreen(progressBgRect);

        // 진행도 바 채우기
        GameObject progressFillArea = CreateUIElement("Fill Area", progressBar.transform);
        RectTransform progressFillAreaRect = progressFillArea.GetComponent<RectTransform>();
        SetFullScreen(progressFillAreaRect);

        GameObject progressFill = CreateUIElement("Fill", progressFillArea.transform);
        Image progressFillImg = progressFill.AddComponent<Image>();
        progressFillImg.color = new Color(0.2f, 1f, 0.2f);
        RectTransform progressFillRect = progressFill.GetComponent<RectTransform>();
        SetFullScreen(progressFillRect);

        progressSlider.fillRect = progressFillRect;
        progressSlider.value = 0f;
    }

    void CreateBattleField(Transform parent, bool useTMP)
    {
        GameObject battleField = CreateUIElement("BattleField", parent);
        RectTransform rect = battleField.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 0);
        rect.anchorMax = new Vector2(1, 0.8f);
        rect.offsetMin = new Vector2(20, 0);
        rect.offsetMax = new Vector2(-20, 0);

        // 플레이어 위치
        GameObject player = CreateUIElement("Player", battleField.transform);
        RectTransform playerRect = player.GetComponent<RectTransform>();
        playerRect.anchorMin = new Vector2(0.2f, 0.3f);
        playerRect.anchorMax = new Vector2(0.2f, 0.3f);
        playerRect.sizeDelta = new Vector2(100, 100);
        playerRect.anchoredPosition = Vector2.zero;

        Image playerImg = player.AddComponent<Image>();
        playerImg.color = new Color(0.2f, 0.5f, 0.8f);

        // 몬스터 위치
        GameObject monster = CreateUIElement("Monster", battleField.transform);
        RectTransform monsterRect = monster.GetComponent<RectTransform>();
        monsterRect.anchorMin = new Vector2(0.8f, 0.3f);
        monsterRect.anchorMax = new Vector2(0.8f, 0.3f);
        monsterRect.sizeDelta = new Vector2(120, 120);
        monsterRect.anchoredPosition = Vector2.zero;

        Image monsterImg = monster.AddComponent<Image>();
        monsterImg.color = new Color(0.8f, 0.2f, 0.2f);

        // 몬스터 체력바 그룹
        uiElements.monsterHealthBarGroup = CreateMonsterHealthBar(monster.transform, useTMP);
    }

    GameObject CreateMonsterHealthBar(Transform parent, bool useTMP)
    {
        GameObject healthBarGroup = CreateUIElement("HealthBarGroup", parent);
        RectTransform groupRect = healthBarGroup.GetComponent<RectTransform>();
        groupRect.anchorMin = new Vector2(0.5f, 0);
        groupRect.anchorMax = new Vector2(0.5f, 0);
        groupRect.sizeDelta = new Vector2(150, 40);
        groupRect.anchoredPosition = new Vector2(0, -100);

        // 몬스터 이름
        GameObject monsterName = CreateUIElement("MonsterName", healthBarGroup.transform);
        RectTransform nameRect = monsterName.GetComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0.5f, 1);
        nameRect.anchorMax = new Vector2(0.5f, 1);
        nameRect.sizeDelta = new Vector2(150, 20);
        nameRect.anchoredPosition = new Vector2(0, 10);

        if (useTMP)
        {
            TextMeshProUGUI tmpName = monsterName.AddComponent<TextMeshProUGUI>();
            tmpName.text = "슬라임";
            tmpName.font = cafe24TMPFont;
            tmpName.fontSize = 16;
            tmpName.alignment = TextAlignmentOptions.Center;
            tmpName.color = Color.white;
        }
        else
        {
            Text nameText = monsterName.AddComponent<Text>();
            nameText.text = "슬라임";
            nameText.font = defaultFont;
            nameText.fontSize = 16;
            nameText.alignment = TextAnchor.MiddleCenter;
            nameText.color = Color.white;
            uiElements.monsterNameText = nameText;
        }

        // 체력바
        GameObject healthBar = CreateUIElement("HealthBar", healthBarGroup.transform);
        RectTransform rect = healthBar.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0);
        rect.anchorMax = new Vector2(0.5f, 0);
        rect.sizeDelta = new Vector2(100, 20);
        rect.anchoredPosition = new Vector2(0, 0);

        Slider healthSlider = healthBar.AddComponent<Slider>();
        uiElements.monsterHealthBar = healthSlider;

        // 배경
        GameObject bg = CreateUIElement("Background", healthBar.transform);
        Image bgImg = bg.AddComponent<Image>();
        bgImg.color = Color.black;
        RectTransform bgRect = bg.GetComponent<RectTransform>();
        SetFullScreen(bgRect);

        // 체력바 채우기
        GameObject fillArea = CreateUIElement("Fill Area", healthBar.transform);
        RectTransform fillAreaRect = fillArea.GetComponent<RectTransform>();
        SetFullScreen(fillAreaRect);

        GameObject fill = CreateUIElement("Fill", fillArea.transform);
        Image fillImg = fill.AddComponent<Image>();
        fillImg.color = Color.red;
        RectTransform fillRect = fill.GetComponent<RectTransform>();
        SetFullScreen(fillRect);

        healthSlider.fillRect = fillRect;
        healthSlider.value = 1f;

        return healthBarGroup;
    }

    void CreateQuickSlots(Transform parent, bool useTMP)
    {
        GameObject quickSlots = CreateUIElement("QuickSlots", parent);
        RectTransform rect = quickSlots.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 0.27f);  // 퀵슬롯 위치 조정
        rect.anchorMax = new Vector2(1, 0.35f);  // 높이 조정
        rect.offsetMin = new Vector2(20, 0);
        rect.offsetMax = new Vector2(-20, 0);

        HorizontalLayoutGroup layout = quickSlots.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 10;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        // 6개의 스킬 슬롯
        for (int i = 0; i < 6; i++)
        {
            CreateSkillSlot(quickSlots.transform, i, useTMP);
        }
    }

    void CreateSkillSlot(Transform parent, int index, bool useTMP)
    {
        GameObject slot = CreateUIElement($"Slot{index + 1}", parent);

        // 슬롯 버튼
        Image slotImg = slot.AddComponent<Image>();
        slotImg.color = new Color(0.3f, 0.3f, 0.3f);

        Button btn = slot.AddComponent<Button>();

        RectTransform rect = slot.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(60, 60);  // 슬롯 크기 축소

        // + 아이콘
        GameObject icon = CreateUIElement("Icon", slot.transform);
        RectTransform iconRect = icon.GetComponent<RectTransform>();
        SetFullScreen(iconRect);

        if (useTMP)
        {
            TextMeshProUGUI tmpPlus = icon.AddComponent<TextMeshProUGUI>();
            tmpPlus.text = "+";
            tmpPlus.font = cafe24TMPFont;
            tmpPlus.fontSize = 40;
            tmpPlus.alignment = TextAlignmentOptions.Center;
            tmpPlus.color = Color.white;
        }
        else
        {
            Text plus = icon.AddComponent<Text>();
            plus.text = "+";
            plus.font = defaultFont;
            plus.fontSize = 40;
            plus.alignment = TextAnchor.MiddleCenter;
            plus.color = Color.white;
        }
    }

    void CreateBottomUI(Transform parent, bool useTMP)
    {
        GameObject bottomUI = CreateUIElement("BottomUI", parent);
        RectTransform rect = bottomUI.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = new Vector2(1, 0.27f);  // 하단 UI 높이 축소
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        // 스탯 업그레이드 패널
        CreateStatUpgradePanel(bottomUI.transform, useTMP);

        // 하단 메뉴
        CreateBottomMenu(bottomUI.transform, useTMP);
    }

    void CreateStatUpgradePanel(Transform parent, bool useTMP)
    {
        GameObject statPanel = CreateUIElement("StatUpgradePanel", parent);
        RectTransform rect = statPanel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 0.15f);  // 메뉴 버튼 공간 확보
        rect.anchorMax = new Vector2(1, 0.85f);  // 비율 조정
        rect.offsetMin = new Vector2(10, 0);
        rect.offsetMax = new Vector2(-10, 0);

        VerticalLayoutGroup layout = statPanel.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 5;
        layout.padding = new RectOffset(10, 10, 10, 10);
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        // 스탯 아이템
        CreateStatItem(statPanel.transform, "공격력", "10 → 11", goldColor, useTMP);
        CreateStatItem(statPanel.transform, "방어력", "200 → 210", goldColor, useTMP);
        CreateStatItem(statPanel.transform, "체력", "20 → 21", goldColor, useTMP);
        CreateStatItem(statPanel.transform, "치명타", "0% → 0.1%", goldColor, useTMP);
    }

    void CreateStatItem(Transform parent, string statName, string value, Color iconColor, bool useTMP)
    {
        GameObject item = CreateUIElement(statName + "Item", parent);
        RectTransform rect = item.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(0, 45);  // 스탯 아이템 높이 축소

        // 배경
        Image bg = item.AddComponent<Image>();
        bg.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);

        // 아이콘
        GameObject icon = CreateUIElement("Icon", item.transform);
        Image iconImg = icon.AddComponent<Image>();
        iconImg.color = iconColor;

        RectTransform iconRect = icon.GetComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0, 0.5f);
        iconRect.anchorMax = new Vector2(0, 0.5f);
        iconRect.sizeDelta = new Vector2(40, 40);
        iconRect.anchoredPosition = new Vector2(30, 0);

        // 스탯 이름
        GameObject nameObj = CreateUIElement("Name", item.transform);
        RectTransform nameRect = nameObj.GetComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0, 0.5f);
        nameRect.anchorMax = new Vector2(0, 0.5f);
        nameRect.sizeDelta = new Vector2(150, 25);
        nameRect.anchoredPosition = new Vector2(100, 10);

        if (useTMP)
        {
            TextMeshProUGUI tmpName = nameObj.AddComponent<TextMeshProUGUI>();
            tmpName.text = statName + " Lv.0";
            tmpName.font = cafe24TMPFont;
            tmpName.fontSize = 16;
            tmpName.color = Color.white;
            tmpName.alignment = TextAlignmentOptions.MidlineLeft;
        }
        else
        {
            Text name = nameObj.AddComponent<Text>();
            name.text = statName + " Lv.0";
            name.font = defaultFont;
            name.fontSize = 16;
            name.color = Color.white;
            name.alignment = TextAnchor.MiddleLeft;
        }

        // 값
        GameObject valueObj = CreateUIElement("Value", item.transform);
        RectTransform valRect = valueObj.GetComponent<RectTransform>();
        valRect.anchorMin = new Vector2(0, 0.5f);
        valRect.anchorMax = new Vector2(0, 0.5f);
        valRect.sizeDelta = new Vector2(150, 20);
        valRect.anchoredPosition = new Vector2(100, -10);

        if (useTMP)
        {
            TextMeshProUGUI tmpVal = valueObj.AddComponent<TextMeshProUGUI>();
            tmpVal.text = value;
            tmpVal.font = cafe24TMPFont;
            tmpVal.fontSize = 14;
            tmpVal.color = Color.green;
            tmpVal.alignment = TextAlignmentOptions.MidlineLeft;
        }
        else
        {
            Text val = valueObj.AddComponent<Text>();
            val.text = value;
            val.font = defaultFont;
            val.fontSize = 14;
            val.color = Color.green;
            val.alignment = TextAnchor.MiddleLeft;
        }

        // 업그레이드 버튼
        GameObject upgradeBtn = CreateUIElement("UpgradeBtn", item.transform);
        Button btn = upgradeBtn.AddComponent<Button>();
        Image btnImg = upgradeBtn.AddComponent<Image>();
        btnImg.color = new Color(0.8f, 0.6f, 0.2f);

        RectTransform btnRect = upgradeBtn.GetComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(1, 0.5f);
        btnRect.anchorMax = new Vector2(1, 0.5f);
        btnRect.sizeDelta = new Vector2(100, 35);
        btnRect.anchoredPosition = new Vector2(-60, 0);

        GameObject btnText = CreateUIElement("Text", upgradeBtn.transform);
        RectTransform btnTextRect = btnText.GetComponent<RectTransform>();
        SetFullScreen(btnTextRect);

        if (useTMP)
        {
            TextMeshProUGUI tmpTxt = btnText.AddComponent<TextMeshProUGUI>();
            tmpTxt.text = "업그레이드";
            tmpTxt.font = cafe24TMPFont;
            tmpTxt.fontSize = 14;
            tmpTxt.alignment = TextAlignmentOptions.Center;
            tmpTxt.color = Color.white;
        }
        else
        {
            Text txt = btnText.AddComponent<Text>();
            txt.text = "업그레이드";
            txt.font = defaultFont;
            txt.fontSize = 14;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = Color.white;
        }
    }

    void CreateBottomMenu(Transform parent, bool useTMP)
    {
        GameObject menu = CreateUIElement("BottomMenu", parent);
        RectTransform rect = menu.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 0);
        rect.anchorMax = new Vector2(1, 0.15f);  // 하단 메뉴 높이 축소
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        // 배경
        Image bg = menu.AddComponent<Image>();
        bg.color = new Color(0.15f, 0.15f, 0.15f);

        HorizontalLayoutGroup layout = menu.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 5;
        layout.padding = new RectOffset(10, 10, 10, 10);
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = true;

        // 메뉴 버튼들
        uiElements.characterButton = CreateMenuButton(menu.transform, "캐릭터", new Color(0.8f, 0.5f, 0.2f), useTMP);
        uiElements.inventoryButton = CreateMenuButton(menu.transform, "인벤토리", new Color(0.2f, 0.5f, 0.8f), useTMP);
        uiElements.skillButton = CreateMenuButton(menu.transform, "스킬", new Color(0.5f, 0.4f, 0.3f), useTMP);
        uiElements.shopButton = CreateMenuButton(menu.transform, "상점", new Color(0.8f, 0.8f, 0.2f), useTMP);
    }

    Button CreateMenuButton(Transform parent, string text, Color color, bool useTMP)
    {
        GameObject btn = CreateUIElement(text + "Btn", parent);

        Button button = btn.AddComponent<Button>();
        Image img = btn.AddComponent<Image>();
        img.color = color;

        GameObject textObj = CreateUIElement("Text", btn.transform);
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        SetFullScreen(textRect);

        if (useTMP)
        {
            TextMeshProUGUI tmpText = textObj.AddComponent<TextMeshProUGUI>();
            tmpText.text = text;
            tmpText.font = cafe24TMPFont;
            tmpText.fontSize = 16;
            tmpText.alignment = TextAlignmentOptions.Center;
            tmpText.color = Color.white;
        }
        else
        {
            Text btnText = textObj.AddComponent<Text>();
            btnText.text = text;
            btnText.font = defaultFont;
            btnText.fontSize = 16;
            btnText.alignment = TextAnchor.MiddleCenter;
            btnText.color = Color.white;
        }

        return button;
    }

    void CreatePopupPanels(Transform parent, bool useTMP)
    {
        // 캐릭터 패널
        uiElements.characterPanel = CreatePopupPanel(parent, "CharacterPanel", "캐릭터 정보", useTMP);

        // 인벤토리 패널
        uiElements.inventoryPanel = CreatePopupPanel(parent, "InventoryPanel", "인벤토리", useTMP);

        // 스킬 패널
        uiElements.skillPanel = CreatePopupPanel(parent, "SkillPanel", "스킬", useTMP);

        // 상점 패널
        uiElements.shopPanel = CreatePopupPanel(parent, "ShopPanel", "상점", useTMP);
    }

    GameObject CreatePopupPanel(Transform parent, string panelName, string title, bool useTMP)
    {
        GameObject panel = CreateUIElement(panelName, parent);
        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.1f, 0.1f);
        rect.anchorMax = new Vector2(0.9f, 0.9f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        // 패널 배경
        Image bg = panel.AddComponent<Image>();
        bg.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);

        // 패널 헤더
        GameObject header = CreateUIElement("Header", panel.transform);
        RectTransform headerRect = header.GetComponent<RectTransform>();
        headerRect.anchorMin = new Vector2(0, 0.9f);
        headerRect.anchorMax = new Vector2(1, 1);
        headerRect.offsetMin = Vector2.zero;
        headerRect.offsetMax = Vector2.zero;

        Image headerBg = header.AddComponent<Image>();
        headerBg.color = new Color(0.2f, 0.2f, 0.2f);

        // 패널 제목
        GameObject titleObj = CreateUIElement("Title", header.transform);
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.1f, 0);
        titleRect.anchorMax = new Vector2(0.9f, 1);
        titleRect.offsetMin = Vector2.zero;
        titleRect.offsetMax = Vector2.zero;

        if (useTMP)
        {
            TextMeshProUGUI tmpTitle = titleObj.AddComponent<TextMeshProUGUI>();
            tmpTitle.text = title;
            tmpTitle.font = cafe24TMPFont;
            tmpTitle.fontSize = 24;
            tmpTitle.alignment = TextAlignmentOptions.Center;
            tmpTitle.color = Color.white;
        }
        else
        {
            Text titleText = titleObj.AddComponent<Text>();
            titleText.text = title;
            titleText.font = defaultFont;
            titleText.fontSize = 24;
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.color = Color.white;
        }

        // 닫기 버튼
        GameObject closeBtn = CreateUIElement("CloseBtn", header.transform);
        Button closeBtnComp = closeBtn.AddComponent<Button>();
        Image closeBtnImg = closeBtn.AddComponent<Image>();
        closeBtnImg.color = new Color(0.8f, 0.2f, 0.2f);

        RectTransform closeBtnRect = closeBtn.GetComponent<RectTransform>();
        closeBtnRect.anchorMin = new Vector2(0.9f, 0.1f);
        closeBtnRect.anchorMax = new Vector2(0.98f, 0.9f);
        closeBtnRect.offsetMin = Vector2.zero;
        closeBtnRect.offsetMax = Vector2.zero;

        GameObject closeBtnText = CreateUIElement("Text", closeBtn.transform);
        RectTransform closeBtnTextRect = closeBtnText.GetComponent<RectTransform>();
        SetFullScreen(closeBtnTextRect);

        if (useTMP)
        {
            TextMeshProUGUI tmpClose = closeBtnText.AddComponent<TextMeshProUGUI>();
            tmpClose.text = "X";
            tmpClose.font = cafe24TMPFont;
            tmpClose.fontSize = 20;
            tmpClose.alignment = TextAlignmentOptions.Center;
            tmpClose.color = Color.white;
        }
        else
        {
            Text closeText = closeBtnText.AddComponent<Text>();
            closeText.text = "X";
            closeText.font = defaultFont;
            closeText.fontSize = 20;
            closeText.alignment = TextAnchor.MiddleCenter;
            closeText.color = Color.white;
        }

        // 패널 비활성화
        panel.SetActive(false);

        return panel;
    }

    GameObject CreateUIElement(string name, Transform parent)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        RectTransform rect = go.AddComponent<RectTransform>();
        return go;
    }
}