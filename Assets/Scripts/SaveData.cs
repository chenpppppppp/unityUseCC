using UnityEngine;

/// <summary>
/// 本地存档：总金币、解锁进度
/// </summary>
public static class SaveData
{
    private const string KEY_GOLD = "TotalGold";
    private const string KEY_TOTAL_KILLS = "TotalKills";
    private const string KEY_TOTAL_BOUNCES = "TotalBounces";
    private const string KEY_GAMES_PLAYED = "GamesPlayed";

    // ===== 金币 =====

    public static int TotalGold
    {
        get => PlayerPrefs.GetInt(KEY_GOLD, 0);
        set => PlayerPrefs.SetInt(KEY_GOLD, value);
    }

    public static void AddGold(int amount)
    {
        TotalGold += amount;
        PlayerPrefs.Save();
    }

    // ===== 统计 =====

    public static int TotalKills
    {
        get => PlayerPrefs.GetInt(KEY_TOTAL_KILLS, 0);
        set => PlayerPrefs.SetInt(KEY_TOTAL_KILLS, value);
    }

    public static int TotalBounces
    {
        get => PlayerPrefs.GetInt(KEY_TOTAL_BOUNCES, 0);
        set => PlayerPrefs.SetInt(KEY_TOTAL_BOUNCES, value);
    }

    public static int GamesPlayed
    {
        get => PlayerPrefs.GetInt(KEY_GAMES_PLAYED, 0);
        set => PlayerPrefs.SetInt(KEY_GAMES_PLAYED, value);
    }

    public static void AddKills(int count) => TotalKills += count;
    public static void AddBounces(int count) => TotalBounces += count;
    public static void AddGame() => GamesPlayed++;

    // ===== 关卡最高分 =====

    public static int GetLevelHighScore(int levelIndex)
    {
        return PlayerPrefs.GetInt($"Level_{levelIndex}_HighScore", 0);
    }

    public static void SetLevelHighScore(int levelIndex, int score)
    {
        int old = GetLevelHighScore(levelIndex);
        if (score > old)
            PlayerPrefs.SetInt($"Level_{levelIndex}_HighScore", score);
    }

    // ===== 弹珠解锁 =====

    public static bool IsMarbleUnlocked(string marbleId)
    {
        return PlayerPrefs.GetInt($"Marble_{marbleId}", 0) == 1;
    }

    public static void UnlockMarble(string marbleId)
    {
        PlayerPrefs.SetInt($"Marble_{marbleId}", 1);
    }

    // ===== 强制写入 =====

    public static void Save()
    {
        PlayerPrefs.Save();
    }
}
