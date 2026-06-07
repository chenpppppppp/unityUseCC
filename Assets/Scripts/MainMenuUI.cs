using UnityEngine;

/// <summary>
/// 主菜单 + 关卡选择 — UGUI 版
/// </summary>
public class MainMenuUI : MonoBehaviour
{
    private GameManager gameManager;
    private PlayerHealth player;
    private CanvasManager cm;

    private readonly string[] levelNames = { "教学关", "弹射入门", "弹珠大师", "混沌战场", "终极试炼" };
    private readonly int[] levelWaves = { 5, 8, 10, 12, 15 };

    void Start()
    {
        gameManager = GameManager.Instance;
        player = FindObjectOfType<PlayerHealth>();

        // 确保 CanvasManager 存在
        cm = CanvasManager.Instance;
        if (cm == null)
        {
            GameObject go = new GameObject("CanvasManager");
            cm = go.AddComponent<CanvasManager>();
        }

        if (player != null) player.enabled = false;
        if (SoundManager.Instance) SoundManager.Instance.PlayMenuBGM();

        // 绑定按钮
        if (cm != null && cm.menuStartBtn != null)
        {
            cm.menuStartBtn.onClick.AddListener(() => {
                if (SoundManager.Instance) SoundManager.Instance.Play("button_click");
                cm.ShowOnly(cm.levelPanel);
                RefreshLevelPanel();
            });

            if (cm.levelBackBtn) cm.levelBackBtn.onClick.AddListener(() => {
                if (SoundManager.Instance) SoundManager.Instance.Play("button_click");
                cm.ShowOnly(cm.menuPanel);
            });

            for (int i = 0; i < cm.levelRows.Length; i++)
            {
                int index = i;
                if (cm.levelRows[i]?.startBtn != null)
                    cm.levelRows[i].startBtn.onClick.AddListener(() => StartLevel(index));
            }

            if (cm.resultRestartBtn) cm.resultRestartBtn.onClick.AddListener(() => {
                Time.timeScale = 1f;
                UnityEngine.SceneManagement.SceneManager.LoadScene(
                    UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
            });

            if (cm.resultMenuBtn) cm.resultMenuBtn.onClick.AddListener(() => {
                Time.timeScale = 1f;
                UnityEngine.SceneManagement.SceneManager.LoadScene(
                    UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
            });

            cm.ShowOnly(cm.menuPanel);
            RefreshMenuStats();
        }
    }

    void Update()
    {
        // 每秒刷新一次菜单统计
        if (Time.frameCount % 60 == 0 && cm != null && cm.menuPanel.activeSelf)
            RefreshMenuStats();
    }

    void RefreshMenuStats()
    {
        cm.menuGoldText.text = $"💰 总金币: {SaveData.TotalGold}";
        cm.menuKillsText.text = $"总击杀: {SaveData.TotalKills}  总弹射: {SaveData.TotalBounces}  游戏次数: {SaveData.GamesPlayed}";
    }

    void RefreshLevelPanel()
    {
        for (int i = 0; i < cm.levelRows.Length; i++)
        {
            bool unlocked = (i == 0) || SaveData.GetLevelHighScore(i - 1) > 0;
            int hs = SaveData.GetLevelHighScore(i);

            cm.levelRows[i].nameText.color = unlocked ? Color.white : new Color(0.5f, 0.5f, 0.5f);
            cm.levelRows[i].waveText.text = $"{levelWaves[i]} 波";

            if (hs > 0)
            {
                cm.levelRows[i].scoreText.text = $"{hs} 分";
                cm.levelRows[i].scoreText.color = Color.cyan;
            }
            else
            {
                cm.levelRows[i].scoreText.text = "—";
                cm.levelRows[i].scoreText.color = new Color(0.4f, 0.4f, 0.4f);
            }

            cm.levelRows[i].startBtn.interactable = unlocked;
            if (!unlocked)
                cm.levelRows[i].startBtn.GetComponentInChildren<UnityEngine.UI.Text>().text = "🔒";
            else
                cm.levelRows[i].startBtn.GetComponentInChildren<UnityEngine.UI.Text>().text = "开始";
        }
    }

    void StartLevel(int index)
    {
        if (SoundManager.Instance) SoundManager.Instance.Play("button_click");

        if (gameManager != null)
        {
            gameManager.totalWaves = levelWaves[index];
            gameManager.currentWave = 1;
            gameManager.currentLevelIndex = index;
            gameManager.StartGame();
        }

        if (player != null) player.enabled = true;

        if (cm != null) cm.ShowOnly(cm.hudPanel);

        gameObject.SetActive(false);
    }

    public void ShowMenu()
    {
        gameObject.SetActive(true);
        if (cm != null)
        {
            cm.ShowOnly(cm.levelPanel);
            RefreshLevelPanel();
        }
    }
}
