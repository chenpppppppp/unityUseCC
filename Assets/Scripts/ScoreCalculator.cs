/// <summary>
/// 分数计算 — 从 GameUI 提取，便于单独测试
/// </summary>
public static class ScoreCalculator
{
    /// <summary>计算单局总分</summary>
    public static int Calculate(int kills, int level, int gold, int wave, bool victory)
    {
        int score = kills * 10 + level * 50 + gold * 2 + wave * 100;
        if (victory) score += 500;
        return score;
    }
}
