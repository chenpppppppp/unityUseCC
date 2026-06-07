using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Canvas 管理器 — 程序化构建全部 UGUI 界面
/// 替代所有 OnGUI 代码
/// </summary>
public class CanvasManager : MonoBehaviour
{
    public static CanvasManager Instance { get; private set; }

    // 面板引用
    [HideInInspector] public GameObject menuPanel;
    [HideInInspector] public GameObject levelPanel;
    [HideInInspector] public GameObject hudPanel;
    [HideInInspector] public GameObject upgradePanel;
    [HideInInspector] public GameObject pausePanel, resultPanel;

    // 菜单元素
    [HideInInspector] public Text menuGoldText, menuKillsText, menuBouncesText, menuGamesText;
    [HideInInspector] public Button menuStartBtn;

    // 关卡选择
    [HideInInspector] public LevelRow[] levelRows = new LevelRow[5];
    [HideInInspector] public Button levelBackBtn;

    // HUD 元素
    [HideInInspector] public Text hudHPText, hudShieldText, hudWaveText, hudXPText, hudGoldText, hudAbilitiesText;
    [HideInInspector] public Image hudXPBar;
    [HideInInspector] public GameObject hudBossBar;
    [HideInInspector] public Image hudBossBarFill;
    [HideInInspector] public Text hudBossNameText, hudBossHPText;

    // 升级面板
    [HideInInspector] public Text upgradeTitle;
    [HideInInspector] public UpgradeCard[] upgradeCards = new UpgradeCard[3];

    // 结算面板
    [HideInInspector] public Text resultTitle, resultWave, resultLevel, resultKills, resultGold, resultScore;
    [HideInInspector] public Button resultRestartBtn, resultMenuBtn;

    // 暂停/设置面板
    [HideInInspector] public Slider pauseMasterSlider, pauseSFXSlider, pauseBGMVSlder;
    [HideInInspector] public Text pauseMasterText, pauseSFXText, pauseBGMText;
    [HideInInspector] public Button pauseResumeBtn, pauseMenuBtn;

    private Canvas canvas;

    [System.Serializable]
    public class LevelRow
    {
        public GameObject root;
        public Text nameText, waveText, scoreText;
        public Button startBtn;
    }

    [System.Serializable]
    public class UpgradeCard
    {
        public GameObject root;
        public Text nameText, descText;
        public Button selectBtn;
    }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        BuildCanvas();
        Debug.Log("[CanvasManager] Canvas 构建完成，显示主菜单");
    }

    void BuildCanvas()
    {
        // Canvas
        GameObject canvasGO = new GameObject("Canvas");
        canvasGO.transform.SetParent(transform);
        canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasGO.AddComponent<GraphicRaycaster>();

        // EventSystem
        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        TryBuild("Menu", () => BuildMenuPanel(canvasGO.transform));
        TryBuild("Level", () => BuildLevelPanel(canvasGO.transform));
        TryBuild("HUD", () => BuildHUDPanel(canvasGO.transform));
        TryBuild("Upgrade", () => BuildUpgradePanel(canvasGO.transform));
        TryBuild("Pause", () => BuildPausePanel(canvasGO.transform));
        TryBuild("Result", () => BuildResultPanel(canvasGO.transform));

        ShowOnly(menuPanel);
    }

    // ========== 主菜单面板 ==========
    void BuildMenuPanel(Transform parent)
    {
        menuPanel = NewPanel("MenuPanel", parent);
        RectTransform r = menuPanel.GetComponent<RectTransform>();
        Stretch(r);

        // 背景
        Image bg = menuPanel.AddComponent<Image>();
        bg.color = new Color(0.06f, 0.06f, 0.12f);

        // 标题
        Text title = NewText("Title", menuPanel.transform, "弹珠守卫者", 52, Color.yellow);
        RectTransform tr = title.GetComponent<RectTransform>();
        tr.anchorMin = new Vector2(0.5f, 0.85f); tr.anchorMax = new Vector2(0.5f, 0.85f);
        tr.sizeDelta = new Vector2(500, 70); tr.anchoredPosition = Vector2.zero;

        // 副标题
        Text sub = NewText("Subtitle", menuPanel.transform,
            "Marble Defender  |  Roguelike 弹珠射击", 18, new Color(0.7f, 0.7f, 0.7f));
        RectTransform sr = sub.GetComponent<RectTransform>();
        sr.anchorMin = new Vector2(0.5f, 0.78f); sr.anchorMax = new Vector2(0.5f, 0.78f);
        sr.sizeDelta = new Vector2(500, 30); sr.anchoredPosition = Vector2.zero;

        // 金币
        menuGoldText = NewText("Gold", menuPanel.transform, "总金币: 0", 22, Color.yellow);
        RectTransform gr = menuGoldText.GetComponent<RectTransform>();
        gr.anchorMin = new Vector2(0.5f, 0.55f); gr.anchorMax = new Vector2(0.5f, 0.55f);
        gr.sizeDelta = new Vector2(400, 40); gr.anchoredPosition = Vector2.zero;

        // 统计
        menuKillsText = NewText("Stats", menuPanel.transform, "总击杀: 0  总弹射: 0  游戏: 0", 14, new Color(0.6f, 0.6f, 0.6f));
        RectTransform sr2 = menuKillsText.GetComponent<RectTransform>();
        sr2.anchorMin = new Vector2(0.5f, 0.50f); sr2.anchorMax = new Vector2(0.5f, 0.50f);
        sr2.sizeDelta = new Vector2(500, 25); sr2.anchoredPosition = Vector2.zero;

        // 开始按钮
        menuStartBtn = NewButton("StartBtn", menuPanel.transform, "开始游戏", 24);
        RectTransform br = menuStartBtn.GetComponent<RectTransform>();
        br.anchorMin = new Vector2(0.5f, 0.43f); br.anchorMax = new Vector2(0.5f, 0.43f);
        br.sizeDelta = new Vector2(220, 55); br.anchoredPosition = Vector2.zero;

        // 提示文字
        NewText("Hint", menuPanel.transform,
            "鼠标拖拽瞄准 → 松开发射弹珠 → 弹射消灭敌人 → 升级强化\n弹珠在墙壁间弹射，每次碰撞都是策略选择",
            14, new Color(0.5f, 0.5f, 0.5f))
            .GetComponent<RectTransform>().Let(x => {
                x.anchorMin = new Vector2(0.5f, 0.22f); x.anchorMax = new Vector2(0.5f, 0.22f);
                x.sizeDelta = new Vector2(600, 50); x.anchoredPosition = Vector2.zero;
            });

        // 版本
        NewText("Version", menuPanel.transform, "v1.0  |  俯视角弹珠射击", 12, new Color(0.4f, 0.4f, 0.4f))
            .GetComponent<RectTransform>().Let(x => {
                x.anchorMin = new Vector2(0.5f, 0.03f); x.anchorMax = new Vector2(0.5f, 0.03f);
                x.sizeDelta = new Vector2(200, 25); x.anchoredPosition = Vector2.zero;
            });
    }

    // ========== 关卡选择面板 ==========
    void BuildLevelPanel(Transform parent)
    {
        levelPanel = NewPanel("LevelPanel", parent);
        Image lbg = levelPanel.AddComponent<Image>();
        lbg.color = new Color(0.06f, 0.06f, 0.12f);

        // 标题
        Text title = NewText("LevelTitle", levelPanel.transform, "选择关卡", 36, Color.yellow);
        RectTransform tr = title.GetComponent<RectTransform>();
        tr.anchorMin = new Vector2(0.5f, 0.92f); tr.anchorMax = new Vector2(0.5f, 0.92f);
        tr.sizeDelta = new Vector2(300, 50); tr.anchoredPosition = Vector2.zero;

        // 5 关
        string[] names = { "教学关", "弹射入门", "弹珠大师", "混沌战场", "终极试炼" };
        int[] waves = { 5, 8, 10, 12, 15 };
        string[] hexColors = { "#4CAF50", "#2196F3", "#FF9800", "#E91E63", "#9C27B0" };

        for (int i = 0; i < 5; i++)
        {
            levelRows[i] = new LevelRow();
            BuildLevelRow(levelPanel.transform, i, names[i], waves[i], hexColors[i]);
        }

        // 返回
        levelBackBtn = NewButton("LevelBack", levelPanel.transform, "返回主菜单", 20);
        RectTransform lbr = levelBackBtn.GetComponent<RectTransform>();
        lbr.anchorMin = new Vector2(0.5f, 0.05f); lbr.anchorMax = new Vector2(0.5f, 0.05f);
        lbr.sizeDelta = new Vector2(160, 40); lbr.anchoredPosition = Vector2.zero;
    }

    void BuildLevelRow(Transform parent, int index, string name, int waves, string hexColor)
    {
        float y = 0.82f - index * 0.14f;
        float w = 500, h = 60;

        GameObject row = NewPanel($"LevelRow_{index}", parent);
        row.AddComponent<Image>().color = HexColor(hexColor, 0.5f);
        RectTransform rr = row.GetComponent<RectTransform>();
        rr.anchorMin = new Vector2(0.5f, y); rr.anchorMax = new Vector2(0.5f, y);
        rr.sizeDelta = new Vector2(w, h); rr.anchoredPosition = Vector2.zero;

        levelRows[index].root = row;

        // 编号+名称
        levelRows[index].nameText = NewText("Name", row.transform,
            $"#{index + 1}  {name}", 20, Color.white);
        levelRows[index].nameText.GetComponent<RectTransform>().Let(x => {
            x.anchorMin = Vector2.zero; x.anchorMax = Vector2.one;
            x.offsetMin = new Vector2(15, 0); x.offsetMax = new Vector2(-100, 0);
        });
        levelRows[index].nameText.alignment = TextAnchor.MiddleLeft;

        // 波次
        levelRows[index].waveText = NewText("Wave", row.transform,
            $"{waves} 波", 14, new Color(0.7f, 0.7f, 0.7f));
        levelRows[index].waveText.GetComponent<RectTransform>().Let(x => {
            x.anchorMin = new Vector2(1, 0); x.anchorMax = new Vector2(1, 1);
            x.offsetMin = new Vector2(-140, 0); x.offsetMax = new Vector2(-100, 0);
        });

        // 最高分
        levelRows[index].scoreText = NewText("Score", row.transform, "—", 14, new Color(0.4f, 0.4f, 0.4f));
        levelRows[index].scoreText.GetComponent<RectTransform>().Let(x => {
            x.anchorMin = new Vector2(1, 0); x.anchorMax = new Vector2(1, 1);
            x.offsetMin = new Vector2(-100, 0); x.offsetMax = new Vector2(-20, 0);
        });

        // 开始按钮
        levelRows[index].startBtn = NewButton("StartBtn", row.transform, "开始", 16);
        levelRows[index].startBtn.GetComponent<RectTransform>().Let(x => {
            x.anchorMin = new Vector2(1, 0.5f); x.anchorMax = new Vector2(1, 0.5f);
            x.sizeDelta = new Vector2(80, 35); x.anchoredPosition = new Vector2(-55, 0);
        });
    }

    // ========== HUD 面板 ==========
    void BuildHUDPanel(Transform parent)
    {
        hudPanel = NewPanel("HUDPanel", parent);
        Image hudImg = hudPanel.AddComponent<Image>();
        hudImg.color = Color.clear;
        hudImg.raycastTarget = false;

        // 左上：生命
        hudShieldText = NewText("Shield", hudPanel.transform, "■ 护盾", 18, Color.white);
        hudShieldText.GetComponent<RectTransform>().Let(x => {
            x.anchorMin = new Vector2(0, 1); x.anchorMax = new Vector2(0, 1);
            x.pivot = new Vector2(0, 1); x.anchoredPosition = new Vector2(10, -10);
            x.sizeDelta = new Vector2(120, 28);
        });

        hudHPText = NewText("HP", hudPanel.transform, "♥ ♥ ♥", 20, Color.white);
        hudHPText.GetComponent<RectTransform>().Let(x => {
            x.anchorMin = new Vector2(0, 1); x.anchorMax = new Vector2(0, 1);
            x.pivot = new Vector2(0, 1); x.anchoredPosition = new Vector2(130, -10);
            x.sizeDelta = new Vector2(180, 28);
        });

        // 右上：波次
        hudWaveText = NewText("Wave", hudPanel.transform, "Wave 1/10", 18, Color.white);
        hudWaveText.GetComponent<RectTransform>().Let(x => {
            x.anchorMin = new Vector2(1, 1); x.anchorMax = new Vector2(1, 1);
            x.pivot = new Vector2(1, 1); x.anchoredPosition = new Vector2(-10, -10);
            x.sizeDelta = new Vector2(180, 28);
        });
        hudWaveText.alignment = TextAnchor.MiddleRight;

        // 能力文字
        hudAbilitiesText = NewText("Abilities", hudPanel.transform, "", 13, Color.cyan);
        hudAbilitiesText.GetComponent<RectTransform>().Let(x => {
            x.anchorMin = new Vector2(0, 1); x.anchorMax = new Vector2(0, 1);
            x.pivot = new Vector2(0, 1); x.anchoredPosition = new Vector2(10, -40);
            x.sizeDelta = new Vector2(400, 22);
        });

        // Boss 血条（默认隐藏）
        hudBossBar = NewPanel("BossBar", hudPanel.transform);
        hudBossBar.AddComponent<Image>().color = new Color(0, 0, 0, 0.5f);
        hudBossBar.GetComponent<RectTransform>().Let(x => {
            x.anchorMin = new Vector2(0.5f, 1); x.anchorMax = new Vector2(0.5f, 1);
            x.pivot = new Vector2(0.5f, 1); x.anchoredPosition = new Vector2(0, -55);
            x.sizeDelta = new Vector2(400, 28);
        });

        GameObject bossFill = NewPanel("BossFill", hudBossBar.transform);
        hudBossBarFill = bossFill.AddComponent<Image>();
        hudBossBarFill.color = Color.red;
        hudBossBarFill.GetComponent<RectTransform>().Let(x => {
            x.anchorMin = Vector2.zero; x.anchorMax = new Vector2(1, 1);
            x.offsetMin = new Vector2(2, 2); x.offsetMax = new Vector2(-2, -2);
        });

        hudBossNameText = NewText("BossName", hudBossBar.transform, "巨石傀儡", 14, Color.white);
        hudBossNameText.GetComponent<RectTransform>().Let(x => {
            x.anchorMin = Vector2.zero; x.anchorMax = Vector2.one;
            x.offsetMin = new Vector2(10, 0); x.offsetMax = new Vector2(-80, 0);
        });
        hudBossNameText.alignment = TextAnchor.MiddleLeft;

        hudBossHPText = NewText("BossHP", hudBossBar.transform, "500/500", 14, Color.white);
        hudBossHPText.GetComponent<RectTransform>().Let(x => {
            x.anchorMin = Vector2.zero; x.anchorMax = Vector2.one;
            x.offsetMin = new Vector2(0, 0); x.offsetMax = new Vector2(-10, 0);
        });
        hudBossHPText.alignment = TextAnchor.MiddleRight;
        hudBossBar.SetActive(false);

        // 底部：经验条
        GameObject xpBarBg = NewPanel("XPBarBg", hudPanel.transform);
        xpBarBg.AddComponent<Image>().color = new Color(0, 0, 0, 0.5f);
        xpBarBg.GetComponent<RectTransform>().Let(x => {
            x.anchorMin = new Vector2(0.5f, 0); x.anchorMax = new Vector2(0.5f, 0);
            x.pivot = new Vector2(0.5f, 0); x.anchoredPosition = new Vector2(0, 10);
            x.sizeDelta = new Vector2(600, 22);
        });

        GameObject xpFill = NewPanel("XPFill", xpBarBg.transform);
        hudXPBar = xpFill.AddComponent<Image>();
        hudXPBar.color = Color.green;
        hudXPBar.GetComponent<RectTransform>().Let(x => {
            x.anchorMin = Vector2.zero; x.anchorMax = new Vector2(1, 1);
            x.offsetMin = new Vector2(2, 2); x.offsetMax = new Vector2(-2, -2);
            x.pivot = Vector2.zero;
        });

        hudXPText = NewText("XPText", xpBarBg.transform, "Lv.1  XP: 0/10  金币: 0", 14, Color.white);
        hudXPText.GetComponent<RectTransform>().Let(x => {
            x.anchorMin = Vector2.zero; x.anchorMax = Vector2.one;
            x.offsetMin = Vector2.zero; x.offsetMax = Vector2.zero;
        });
        hudXPText.alignment = TextAnchor.MiddleCenter;

        // 金币
        hudGoldText = NewText("Gold", hudPanel.transform, "", 14, Color.white);
        hudGoldText.gameObject.SetActive(false); // 合并到 XP 行
    }

    // ========== 升级面板 ==========
    void BuildUpgradePanel(Transform parent)
    {
        upgradePanel = NewPanel("UpgradePanel", parent);
        Image ubg = upgradePanel.AddComponent<Image>();
        ubg.color = new Color(0, 0, 0, 0.75f);

        // 标题
        upgradeTitle = NewText("UpgradeTitle", upgradePanel.transform, "Lv.X — 选择强化", 28, Color.yellow);
        upgradeTitle.GetComponent<RectTransform>().Let(x => {
            x.anchorMin = new Vector2(0.5f, 0.85f); x.anchorMax = new Vector2(0.5f, 0.85f);
            x.sizeDelta = new Vector2(500, 45); x.anchoredPosition = Vector2.zero;
        });

        // 3 张卡片
        for (int i = 0; i < 3; i++)
        {
            upgradeCards[i] = new UpgradeCard();
            BuildUpgradeCard(upgradePanel.transform, i);
        }
    }

    void BuildUpgradeCard(Transform parent, int index)
    {
        float x = -170 + index * 170;
        float w = 155, h = 220;

        GameObject card = NewPanel($"Card_{index}", parent);
        card.AddComponent<Image>().color = new Color(0.15f, 0.15f, 0.2f, 0.9f);
        RectTransform cr = card.GetComponent<RectTransform>();
        cr.anchorMin = new Vector2(0.5f, 0.5f); cr.anchorMax = new Vector2(0.5f, 0.5f);
        cr.sizeDelta = new Vector2(w, h); cr.anchoredPosition = new Vector2(x, 10);

        upgradeCards[index].root = card;

        upgradeCards[index].nameText = NewText("Name", card.transform, "", 16, Color.cyan);
        upgradeCards[index].nameText.GetComponent<RectTransform>().Let(r => {
            r.anchorMin = new Vector2(0, 1); r.anchorMax = new Vector2(1, 1);
            r.pivot = new Vector2(0.5f, 1); r.anchoredPosition = new Vector2(0, -10);
            r.sizeDelta = new Vector2(-10, 45);
        });

        upgradeCards[index].descText = NewText("Desc", card.transform, "", 13, Color.white);
        upgradeCards[index].descText.GetComponent<RectTransform>().Let(r => {
            r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one;
            r.offsetMin = new Vector2(8, 50); r.offsetMax = new Vector2(-8, -45);
        });

        upgradeCards[index].selectBtn = NewButton("SelectBtn", card.transform, $"选择 [{index + 1}]", 16);
        upgradeCards[index].selectBtn.GetComponent<RectTransform>().Let(r => {
            r.anchorMin = new Vector2(0.5f, 0); r.anchorMax = new Vector2(0.5f, 0);
            r.pivot = new Vector2(0.5f, 0); r.anchoredPosition = new Vector2(0, 10);
            r.sizeDelta = new Vector2(w - 30, 32);
        });
    }

    // ========== 暂停/设置面板 ==========
    void BuildPausePanel(Transform parent)
    {
        pausePanel = NewPanel("PausePanel", parent);
        pausePanel.AddComponent<Image>().color = new Color(0, 0, 0, 0.75f);

        // 内部卡片
        GameObject inner = NewPanel("PauseInner", pausePanel.transform);
        inner.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.15f, 0.95f);
        RectTransform ir = inner.GetComponent<RectTransform>();
        ir.anchorMin = new Vector2(0.5f, 0.5f); ir.anchorMax = new Vector2(0.5f, 0.5f);
        ir.sizeDelta = new Vector2(360, 340); ir.anchoredPosition = Vector2.zero;

        // 标题
        Text title = NewText("PauseTitle", inner.transform, "游戏暂停", 32, Color.yellow);
        title.GetComponent<RectTransform>().Let(x => {
            x.anchorMin = new Vector2(0.5f, 1); x.anchorMax = new Vector2(0.5f, 1);
            x.pivot = new Vector2(0.5f, 1); x.anchoredPosition = new Vector2(0, -15);
            x.sizeDelta = new Vector2(300, 45);
        });
        title.fontStyle = FontStyle.Bold;

        // --- 音量滑块 ---
        float yStart = 0.78f;
        BuildVolumeSlider(inner.transform, "主音量", ref pauseMasterSlider, ref pauseMasterText, yStart);
        BuildVolumeSlider(inner.transform, "音效",   ref pauseSFXSlider,   ref pauseSFXText,   yStart - 0.14f);
        BuildVolumeSlider(inner.transform, "音乐",   ref pauseBGMVSlder,   ref pauseBGMText,   yStart - 0.28f);

        // 按钮
        pauseResumeBtn = NewButton("ResumeBtn", inner.transform, "继续游戏", 20);
        pauseResumeBtn.GetComponent<RectTransform>().Let(x => {
            x.anchorMin = new Vector2(0.5f, 0.22f); x.anchorMax = new Vector2(0.5f, 0.22f);
            x.sizeDelta = new Vector2(200, 40); x.anchoredPosition = Vector2.zero;
        });

        pauseMenuBtn = NewButton("PauseMenuBtn", inner.transform, "返回主菜单", 18);
        pauseMenuBtn.GetComponent<RectTransform>().Let(x => {
            x.anchorMin = new Vector2(0.5f, 0.08f); x.anchorMax = new Vector2(0.5f, 0.08f);
            x.sizeDelta = new Vector2(200, 40); x.anchoredPosition = Vector2.zero;
        });
        pauseMenuBtn.GetComponent<Image>().color = new Color(0.7f, 0.3f, 0.3f, 0.5f);
    }

    void BuildVolumeSlider(Transform parent, string label, ref Slider slider, ref Text text, float y)
    {
        GameObject row = NewPanel($"Vol_{label}", parent);
        row.AddComponent<Image>().color = Color.clear;
        row.GetComponent<RectTransform>().Let(x => {
            x.anchorMin = new Vector2(0.5f, y); x.anchorMax = new Vector2(0.5f, y);
            x.sizeDelta = new Vector2(300, 30); x.anchoredPosition = Vector2.zero;
        });

        // 标签
        Text lbl = NewText("Label", row.transform, label, 16, Color.white);
        lbl.GetComponent<RectTransform>().Let(x => {
            x.anchorMin = new Vector2(0, 0); x.anchorMax = new Vector2(0.25f, 1);
            x.offsetMin = new Vector2(10, 0); x.offsetMax = Vector2.zero;
        });
        lbl.alignment = TextAnchor.MiddleLeft;

        // 滑块
        GameObject sliderGO = new GameObject("Slider", typeof(RectTransform));
        sliderGO.transform.SetParent(row.transform, false);
        slider = sliderGO.AddComponent<Slider>();
        RectTransform sr = sliderGO.GetComponent<RectTransform>();
        sr.anchorMin = new Vector2(0.25f, 0); sr.anchorMax = new Vector2(0.8f, 1);
        sr.offsetMin = Vector2.zero; sr.offsetMax = Vector2.zero;

        // 滑块背景
        GameObject bgGO = new GameObject("Background", typeof(RectTransform));
        bgGO.transform.SetParent(sliderGO.transform, false);
        bgGO.AddComponent<Image>().color = new Color(0.3f, 0.3f, 0.3f);
        bgGO.GetComponent<RectTransform>().Let(x => { x.anchorMin = Vector2.zero; x.anchorMax = Vector2.one;
            x.offsetMin = Vector2.zero; x.offsetMax = Vector2.zero; });

        // 滑块填充
        GameObject fillGO = new GameObject("Fill", typeof(RectTransform));
        fillGO.transform.SetParent(sliderGO.transform, false);
        fillGO.AddComponent<Image>().color = new Color(0.3f, 0.6f, 0.9f);
        RectTransform fr = fillGO.GetComponent<RectTransform>();
        fr.anchorMin = Vector2.zero; fr.anchorMax = Vector2.one;
        fr.offsetMin = Vector2.zero; fr.offsetMax = Vector2.zero;

        // 手柄
        GameObject handleGO = new GameObject("Handle", typeof(RectTransform));
        handleGO.transform.SetParent(sliderGO.transform, false);
        handleGO.AddComponent<Image>().color = Color.white;
        handleGO.GetComponent<RectTransform>().Let(x => {
            x.anchorMin = new Vector2(0.5f, 0); x.anchorMax = new Vector2(0.5f, 1);
            x.sizeDelta = new Vector2(10, 0); x.anchoredPosition = Vector2.zero;
        });

        slider.fillRect = fr;
        slider.handleRect = handleGO.GetComponent<RectTransform>();
        slider.targetGraphic = handleGO.GetComponent<Image>();
        slider.minValue = 0; slider.maxValue = 1; slider.value = 1;

        // 数值文字
        text = NewText("Value", row.transform, "100%", 14, Color.white);
        text.GetComponent<RectTransform>().Let(x => {
            x.anchorMin = new Vector2(0.8f, 0); x.anchorMax = new Vector2(1, 1);
            x.offsetMin = Vector2.zero; x.offsetMax = new Vector2(-5, 0);
        });
    }

    // ========== 结算面板 ==========
    void BuildResultPanel(Transform parent)
    {
        resultPanel = NewPanel("ResultPanel", parent);
        Image rbg = resultPanel.AddComponent<Image>();
        rbg.color = new Color(0, 0, 0, 0.8f);

        // 内部卡片
        GameObject inner = NewPanel("ResultInner", resultPanel.transform);
        inner.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.15f, 0.95f);
        RectTransform ir = inner.GetComponent<RectTransform>();
        ir.anchorMin = new Vector2(0.5f, 0.5f); ir.anchorMax = new Vector2(0.5f, 0.5f);
        ir.sizeDelta = new Vector2(400, 380); ir.anchoredPosition = Vector2.zero;

        resultTitle = NewText("ResultTitle", inner.transform, "", 36, Color.yellow);
        resultTitle.GetComponent<RectTransform>().Let(x => {
            x.anchorMin = new Vector2(0.5f, 1); x.anchorMax = new Vector2(0.5f, 1);
            x.pivot = new Vector2(0.5f, 1); x.anchoredPosition = new Vector2(0, -15);
            x.sizeDelta = new Vector2(350, 55);
        });
        resultTitle.fontStyle = FontStyle.Bold;

        resultWave = NewText("ResultWave", inner.transform, "", 18, Color.white);
        resultLevel = NewText("ResultLevel", inner.transform, "", 18, Color.white);
        resultKills = NewText("ResultKills", inner.transform, "", 18, Color.white);
        resultGold = NewText("ResultGold", inner.transform, "", 18, Color.white);
        resultScore = NewText("ResultScore", inner.transform, "", 26, Color.yellow);

        float sy = 0.82f;
        foreach (Text t in new[] { resultWave, resultLevel, resultKills, resultGold })
        {
            t.GetComponent<RectTransform>().Let(x => {
                x.anchorMin = new Vector2(0.5f, sy); x.anchorMax = new Vector2(0.5f, sy);
                x.sizeDelta = new Vector2(300, 30); x.anchoredPosition = Vector2.zero;
            });
            t.alignment = TextAnchor.MiddleCenter;
            sy -= 0.08f;
        }
        resultScore.GetComponent<RectTransform>().Let(x => {
            x.anchorMin = new Vector2(0.5f, 0.35f); x.anchorMax = new Vector2(0.5f, 0.35f);
            x.sizeDelta = new Vector2(300, 40); x.anchoredPosition = Vector2.zero;
        });
        resultScore.fontStyle = FontStyle.Bold;

        // 按钮
        resultRestartBtn = NewButton("RestartBtn", inner.transform, "重新开始", 20);
        resultRestartBtn.GetComponent<RectTransform>().Let(x => {
            x.anchorMin = new Vector2(0.35f, 0.08f); x.anchorMax = new Vector2(0.35f, 0.08f);
            x.sizeDelta = new Vector2(140, 45); x.anchoredPosition = Vector2.zero;
        });

        resultMenuBtn = NewButton("MenuBtn", inner.transform, "返回菜单", 20);
        resultMenuBtn.GetComponent<RectTransform>().Let(x => {
            x.anchorMin = new Vector2(0.65f, 0.08f); x.anchorMax = new Vector2(0.65f, 0.08f);
            x.sizeDelta = new Vector2(140, 45); x.anchoredPosition = Vector2.zero;
        });
    }

    // ========== 工具方法 ==========

    void TryBuild(string name, System.Action build)
    {
        try { build(); }
        catch (System.Exception e) { Debug.LogError($"[CanvasManager] 构建{name}面板失败: {e.Message}"); }
    }

    public void ShowOnly(GameObject panel)
    {
        menuPanel.SetActive(panel == menuPanel);
        levelPanel.SetActive(panel == levelPanel);
        hudPanel.SetActive(panel == hudPanel);
        upgradePanel.SetActive(panel == upgradePanel);
        pausePanel.SetActive(panel == pausePanel);
        resultPanel.SetActive(panel == resultPanel);
    }

    Font _font;
    Font GetFont()
    {
        if (_font != null) return _font;
        _font = Font.CreateDynamicFontFromOSFont("Arial", 14);
        if (_font == null)
            _font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        return _font;
    }

    GameObject NewPanel(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        // 默认拉伸
        RectTransform r = go.GetComponent<RectTransform>();
        r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one;
        r.offsetMin = Vector2.zero; r.offsetMax = Vector2.zero;
        return go;
    }

    void Stretch(RectTransform r)
    {
        r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one;
        r.offsetMin = Vector2.zero; r.offsetMax = Vector2.zero;
    }

    Text NewText(string name, Transform parent, string content, int fontSize, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        Text text = go.AddComponent<Text>();
        text.text = content;
        text.fontSize = fontSize;
        text.color = color;
        text.font = GetFont();
        text.alignment = TextAnchor.MiddleCenter;
        text.raycastTarget = false;
        return text;
    }

    Button NewButton(string name, Transform parent, string label, int fontSize)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);

        Image img = go.AddComponent<Image>();
        img.color = Color.clear;

        Button btn = go.AddComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.normalColor = Color.clear;
        cb.highlightedColor = new Color(1f, 1f, 1f, 0.2f);
        cb.pressedColor = new Color(1f, 1f, 1f, 0.1f);
        btn.colors = cb;

        // 标签
        GameObject labelGO = new GameObject("Label", typeof(RectTransform));
        labelGO.transform.SetParent(go.transform, false);
        Text txt = labelGO.AddComponent<Text>();
        txt.text = label;
        txt.fontSize = fontSize;
        txt.color = new Color(0.6f, 0.85f, 1f);
        txt.font = GetFont();
        txt.alignment = TextAnchor.MiddleCenter;
        txt.raycastTarget = false;

        RectTransform lr = labelGO.GetComponent<RectTransform>();
        Stretch(lr);

        return btn;
    }

    Color HexColor(string hex, float alpha)
    {
        Color c;
        ColorUtility.TryParseHtmlString(hex, out c);
        c.a = alpha;
        return c;
    }
}
