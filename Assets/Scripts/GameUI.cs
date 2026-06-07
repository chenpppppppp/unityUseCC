using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 游戏 UI — UGUI 版：HUD、升级面板、结算面板
/// </summary>
public class GameUI : MonoBehaviour
{
    private PlayerHealth player;
    private GameManager gameManager;
    private PlayerAim playerAim;
    private CanvasManager cm;

    private bool showingUpgrade;
    private List<UpgradeOption> currentOptions;
    private GameState state = GameState.Playing;

    private enum GameState { Playing, Victory, GameOver }

    private int finalLevel, finalGold, finalWave, finalKills, finalScore;

    public struct UpgradeOption
    {
        public string name;
        public string description;
        public System.Action onSelect;
    }

    void Start()
    {
        player = FindObjectOfType<PlayerHealth>();
        gameManager = GameManager.Instance;
        playerAim = FindObjectOfType<PlayerAim>();

        // 确保 CanvasManager 存在
        cm = CanvasManager.Instance;
        if (cm == null)
        {
            GameObject go = new GameObject("CanvasManager");
            cm = go.AddComponent<CanvasManager>();
        }

        // 通过 EventBus 订阅，替代直接持有 PlayerHealth 引用
        if (EventBus.Instance != null)
        {
            EventBus.Instance.OnPlayerLevelUp += ShowUpgradePanel;
            EventBus.Instance.OnPlayerDied += OnGameOver;
            EventBus.Instance.OnGameStarted += () => { state = GameState.Playing; };
        }

        // 绑定升级卡片按钮
        if (cm != null)
        {
            for (int i = 0; i < 3; i++)
            {
                int idx = i;
                cm.upgradeCards[i].selectBtn.onClick.AddListener(() => SelectUpgrade(idx));
            }

            // 暂停面板按钮
            if (cm.pauseResumeBtn) cm.pauseResumeBtn.onClick.AddListener(ResumeGame);
            if (cm.pauseMenuBtn) cm.pauseMenuBtn.onClick.AddListener(() => {
                Time.timeScale = 1f;
                UnityEngine.SceneManagement.SceneManager.LoadScene(
                    UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
            });

            // 音量滑块
            if (cm.pauseMasterSlider) cm.pauseMasterSlider.onValueChanged.AddListener(v => {
                if (SoundManager.Instance) SoundManager.Instance.SetMasterVolume(v);
                if (cm.pauseMasterText) cm.pauseMasterText.text = Mathf.RoundToInt(v * 100) + "%";
            });
            if (cm.pauseSFXSlider) cm.pauseSFXSlider.onValueChanged.AddListener(v => {
                if (SoundManager.Instance) SoundManager.Instance.SetSFXVolume(v);
                if (cm.pauseSFXText) cm.pauseSFXText.text = Mathf.RoundToInt(v * 100) + "%";
            });
            if (cm.pauseBGMVSlder) cm.pauseBGMVSlder.onValueChanged.AddListener(v => {
                if (SoundManager.Instance) SoundManager.Instance.SetBGMVolume(v);
                if (cm.pauseBGMText) cm.pauseBGMText.text = Mathf.RoundToInt(v * 100) + "%";
            });
        }
    }

    void Update()
    {
        if (player == null || cm == null) return;

        // ESC 暂停
        if (Input.GetKeyDown(KeyCode.Escape) && !showingUpgrade)
        {
            if (cm.pausePanel && cm.pausePanel.activeSelf)
                ResumeGame();
            else if (state == GameState.Playing)
                PauseGame();
        }

        if (state == GameState.Playing)
        {
            RefreshHUD();

            if (gameManager != null && gameManager.victory)
                OnVictory();
        }
    }

    void PauseGame()
    {
        Time.timeScale = 0f;
        cm.ShowOnly(cm.pausePanel);
        if (SoundManager.Instance)
        {
            if (cm.pauseMasterSlider) cm.pauseMasterSlider.value = SoundManager.Instance.masterVolume;
            if (cm.pauseSFXSlider) cm.pauseSFXSlider.value = SoundManager.Instance.sfxVolume;
            if (cm.pauseBGMVSlder) cm.pauseBGMVSlder.value = SoundManager.Instance.bgmVolume;
        }
        if (SoundManager.Instance) SoundManager.Instance.PauseBGM();
    }

    void ResumeGame()
    {
        Time.timeScale = 1f;
        cm.ShowOnly(cm.hudPanel);
        if (SoundManager.Instance) SoundManager.Instance.ResumeBGM();
    }

    // ========== HUD ==========

    void RefreshHUD()
    {
        // 护盾+生命
        if (cm.hudShieldText) cm.hudShieldText.text = player.currentShield > 0 ? "■ 护盾" : "□ 破碎";
        string hp = "";
        for (int i = 0; i < player.currentHealth; i++) hp += "♥ ";
        for (int i = player.currentHealth; i < player.maxHealth; i++) hp += "♡ ";
        if (cm.hudHPText) cm.hudHPText.text = hp;

        // 波次
        if (gameManager != null && cm.hudWaveText)
            cm.hudWaveText.text = $"Wave {gameManager.currentWave}/{gameManager.totalWaves}";

        // Boss 血条
        if (gameManager != null && gameManager.CurrentBoss != null)
        {
            if (cm.hudBossBar) cm.hudBossBar.SetActive(true);
            BossGolem boss = gameManager.CurrentBoss;
            float pct = (float)boss.currentHealth / boss.bossMaxHealth;
            if (cm.hudBossBarFill) cm.hudBossBarFill.GetComponent<RectTransform>().anchorMax = new Vector2(pct, 1f);
            if (cm.hudBossHPText) cm.hudBossHPText.text = $"{boss.currentHealth}/{boss.bossMaxHealth}";
        }
        else { if (cm.hudBossBar) cm.hudBossBar.SetActive(false); }

        // 经验条
        float xpPct = (float)player.currentXP / player.xpToNextLevel;
        if (cm.hudXPBar) cm.hudXPBar.GetComponent<RectTransform>().anchorMax = new Vector2(xpPct, 1f);
        if (cm.hudXPText) cm.hudXPText.text = $"Lv.{player.level}  XP:{player.currentXP}/{player.xpToNextLevel}  金币:{player.gold}";

        // 能力列表
        if (playerAim != null && cm.hudAbilitiesText)
        {
            string abs = "";
            if (playerAim.doubleShot) abs += "双重发射 ";
            if (playerAim.bounceHealThreshold > 0) abs += "弹射回血 ";
            if (playerAim.explosionChance > 0) abs += $"爆炸{Mathf.RoundToInt(playerAim.explosionChance*100)}% ";
            if (playerAim.canRecall) abs += "召回 ";
            if (playerAim.hasSlowMotion) abs += "缓滞 ";
            if (playerAim.lightningMarble) abs += "闪电⚡ ";
            if (playerAim.fireMarble) abs += "火焰🔥 ";
            cm.hudAbilitiesText.text = abs.Length > 0 ? "能力: " + abs : "";
        }
    }

    // ========== 升级面板 ==========

    void ShowUpgradePanel(int newLevel)
    {
        showingUpgrade = true;
        Time.timeScale = 0f;
        currentOptions = GenerateUpgradeOptions();

        cm.upgradeTitle.text = $"Lv.{player.level} — 选择强化";
        for (int i = 0; i < 3; i++)
        {
            cm.upgradeCards[i].nameText.text = currentOptions[i].name;
            cm.upgradeCards[i].descText.text = currentOptions[i].description;
        }
        cm.ShowOnly(cm.upgradePanel);

        if (SoundManager.Instance) SoundManager.Instance.Play("level_up");
        if (ParticleManager.Instance) ParticleManager.Instance.PlayLevelUp(player.transform.position);
    }

    List<UpgradeOption> GenerateUpgradeOptions()
    {
        PlayerAim aim = playerAim;
        List<UpgradeOption> all = new List<UpgradeOption>
        {
            new UpgradeOption { name = "弹射次数 +2", description = "弹珠最大弹射次数增加 2 次", onSelect = () => { if (aim) aim.bonusBounces += 2; } },
            new UpgradeOption { name = "弹珠尺寸 +30%", description = "弹珠碰撞体积增大，更容易命中", onSelect = () => { if (aim) aim.marbleSizeMult *= 1.3f; } },
            new UpgradeOption { name = "弹珠速度 +20%", description = "弹珠飞行速度提升", onSelect = () => { if (aim) aim.marbleSpeedMult *= 1.2f; } },
            new UpgradeOption { name = "生命上限 +1", description = "最大生命值增加 1 点（最高 5）", onSelect = () => { player.maxHealth = Mathf.Min(player.maxHealth + 1, 5); player.Heal(1); } },
            new UpgradeOption { name = "护盾加速恢复", description = "护盾恢复时间减少 10 秒", onSelect = () => { player.shieldRecoveryTime = Mathf.Max(5f, player.shieldRecoveryTime - 10f); } },
            new UpgradeOption { name = "冷却缩短 20%", description = "弹珠发射冷却减少 20%", onSelect = () => { if (aim) aim.cooldown *= 0.8f; } },
            new UpgradeOption { name = "拾取范围 +50%", description = "金币和经验球的吸附距离增加", onSelect = () => { if (aim) aim.pickupRangeMult *= 1.5f; } },
            new UpgradeOption { name = "击杀爆炸 20%", description = "敌人死亡时概率引发爆炸", onSelect = () => { if (aim) aim.explosionChance += 0.2f; } },
            new UpgradeOption { name = "弹射回血", description = "弹珠每弹射 10 次恢复 1 点生命", onSelect = () => { if (aim) aim.bounceHealThreshold = (aim.bounceHealThreshold <= 0) ? 10 : aim.bounceHealThreshold - 2; } },
            new UpgradeOption { name = "双重发射", description = "每次发射 2 颗弹珠（冷却 +50%）", onSelect = () => { if (aim) { aim.doubleShot = true; aim.cooldown *= 1.5f; } } },
            new UpgradeOption { name = "时间缓滞", description = "每 30 秒触发 3 秒子弹时间", onSelect = () => { if (aim) aim.hasSlowMotion = true; } },
            new UpgradeOption { name = "弹珠召回", description = "按 R 键回收所有弹珠", onSelect = () => { if (aim) aim.canRecall = true; } },
            new UpgradeOption { name = "闪电弹珠", description = "弹珠击中敌人时连锁电击附近 2 个敌人，伤害 50%", onSelect = () => { if (aim) aim.lightningMarble = true; } },
            new UpgradeOption { name = "火焰弹珠", description = "弹珠碰撞后留下燃烧地面，持续 3 秒", onSelect = () => { if (aim) aim.fireMarble = true; } },
        };

        List<UpgradeOption> pool = new List<UpgradeOption>(all);
        List<UpgradeOption> opts = new List<UpgradeOption>();
        for (int i = 0; i < 3; i++)
        {
            int idx = Random.Range(0, pool.Count);
            opts.Add(pool[idx]);
            pool.RemoveAt(idx);
        }
        return opts;
    }

    void SelectUpgrade(int index)
    {
        if (currentOptions == null || index >= currentOptions.Count) return;
        currentOptions[index].onSelect?.Invoke();
        showingUpgrade = false;
        Time.timeScale = 1f;
        cm.ShowOnly(cm.hudPanel);
        // 检查是否还有待升级
        if (player != null) player.CheckPendingLevelUp();
    }

    // ========== 结算 ==========

    void OnGameOver()
    {
        state = GameState.GameOver;
        Time.timeScale = 0f;
        if (SoundManager.Instance) SoundManager.Instance.StopBGM();
        CalculateScore();
        ShowResultPanel("游 戏 结 束", Color.red);
    }

    void OnVictory()
    {
        state = GameState.Victory;
        Time.timeScale = 0f;
        if (SoundManager.Instance) SoundManager.Instance.StopBGM();
        CalculateScore();
        ShowResultPanel("胜 利 ！", Color.yellow);
    }

    void CalculateScore()
    {
        finalLevel = player.level;
        finalGold = player.gold;
        finalWave = gameManager != null ? gameManager.currentWave : 0;
        finalKills = gameManager != null ? gameManager.totalKills : 0;

        finalScore = ScoreCalculator.Calculate(finalKills, finalLevel, finalGold, finalWave, state == GameState.Victory);

        if (state == GameState.Victory)
        {
            if (SoundManager.Instance) SoundManager.Instance.Play("game_win");
            if (ParticleManager.Instance) ParticleManager.Instance.PlayLevelUp(player.transform.position);
        }
        else
        {
            if (SoundManager.Instance) SoundManager.Instance.Play("game_over");
        }

        SaveData.AddGold(finalGold);
        SaveData.AddKills(finalKills);
        SaveData.AddGame();
        if (gameManager != null)
            SaveData.SetLevelHighScore(gameManager.currentLevelIndex, finalScore);
        SaveData.Save();
    }

    void ShowResultPanel(string title, Color titleColor)
    {
        cm.resultTitle.text = title;
        cm.resultTitle.color = titleColor;
        cm.resultWave.text = $"到达波次: {finalWave}";
        cm.resultLevel.text = $"玩家等级: Lv.{finalLevel}";
        cm.resultKills.text = $"击杀敌人: {finalKills}";
        cm.resultGold.text = $"获得金币: {finalGold}";
        cm.resultScore.text = $"总分: {finalScore}";
        cm.ShowOnly(cm.resultPanel);
    }
}
