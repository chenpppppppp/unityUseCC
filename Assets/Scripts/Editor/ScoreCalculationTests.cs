using NUnit.Framework;

/// <summary>
/// ScoreCalculator 单元测试
/// </summary>
public class ScoreCalculationTests
{
    [Test]
    public void ZeroStats_ReturnsZero()
    {
        int score = ScoreCalculator.Calculate(0, 0, 0, 0, false);
        Assert.AreEqual(0, score);
    }

    [Test]
    public void TypicalGame_CalculatesCorrectly()
    {
        // 击杀50 + Lv8 + 金币120 + 第10波 + 未通关
        // 50×10 + 8×50 + 120×2 + 10×100 = 500 + 400 + 240 + 1000 = 2140
        int score = ScoreCalculator.Calculate(50, 8, 120, 10, false);
        Assert.AreEqual(2140, score);
    }

    [Test]
    public void Victory_Adds500Bonus()
    {
        int score = ScoreCalculator.Calculate(50, 8, 120, 10, true);
        Assert.AreEqual(2640, score);
    }

    [Test]
    public void OnlyKills_CalculatesCorrectly()
    {
        int score = ScoreCalculator.Calculate(5, 0, 0, 0, false);
        Assert.AreEqual(50, score);
    }

    [Test]
    public void OnlyLevel_CalculatesCorrectly()
    {
        int score = ScoreCalculator.Calculate(0, 3, 0, 0, false);
        Assert.AreEqual(150, score);
    }

    [Test]
    public void OnlyGold_CalculatesCorrectly()
    {
        int score = ScoreCalculator.Calculate(0, 0, 25, 0, false);
        Assert.AreEqual(50, score);
    }

    [Test]
    public void OnlyWaves_CalculatesCorrectly()
    {
        int score = ScoreCalculator.Calculate(0, 0, 0, 7, false);
        Assert.AreEqual(700, score);
    }

    [Test]
    public void EdgeCase_LargeValues_DoesNotOverflow()
    {
        int score = ScoreCalculator.Calculate(9999, 99, 99999, 99, true);
        Assert.IsTrue(score > 0);
    }
}
