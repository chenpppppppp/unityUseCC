using NUnit.Framework;
using UnityEngine;

/// <summary>
/// SaveData 单元测试 — EditMode 运行，不需要 Play Mode
/// </summary>
public class SaveDataTests
{
    // 备份用户真实数据
    private int _backupGold;
    private int _backupKills;
    private int _backupScores_0, _backupScores_1;

    [SetUp]
    public void BackupAndClear()
    {
        _backupGold = PlayerPrefs.GetInt("TotalGold", 0);
        _backupKills = PlayerPrefs.GetInt("TotalKills", 0);
        _backupScores_0 = PlayerPrefs.GetInt("Level_0_HighScore", 0);
        _backupScores_1 = PlayerPrefs.GetInt("Level_1_HighScore", 0);

        PlayerPrefs.DeleteKey("TotalGold");
        PlayerPrefs.DeleteKey("TotalKills");
        PlayerPrefs.DeleteKey("Level_0_HighScore");
        PlayerPrefs.DeleteKey("Level_1_HighScore");
        PlayerPrefs.DeleteKey("Marble_fire");
        PlayerPrefs.Save();
    }

    [TearDown]
    public void Restore()
    {
        PlayerPrefs.SetInt("TotalGold", _backupGold);
        PlayerPrefs.SetInt("TotalKills", _backupKills);
        PlayerPrefs.SetInt("Level_0_HighScore", _backupScores_0);
        PlayerPrefs.SetInt("Level_1_HighScore", _backupScores_1);
        PlayerPrefs.DeleteKey("Marble_fire");
        PlayerPrefs.Save();
    }

    [Test]
    public void AddGold_IncreasesTotalGold()
    {
        SaveData.AddGold(100);
        Assert.AreEqual(100, SaveData.TotalGold);
    }

    [Test]
    public void AddGold_Accumulates()
    {
        SaveData.AddGold(100);
        SaveData.AddGold(50);
        Assert.AreEqual(150, SaveData.TotalGold);
    }

    [Test]
    public void SetLevelHighScore_KeepsHighest()
    {
        SaveData.SetLevelHighScore(0, 500);
        SaveData.SetLevelHighScore(0, 300);
        Assert.AreEqual(500, SaveData.GetLevelHighScore(0));
    }

    [Test]
    public void SetLevelHighScore_UpdatesWhenHigher()
    {
        SaveData.SetLevelHighScore(0, 300);
        SaveData.SetLevelHighScore(0, 800);
        Assert.AreEqual(800, SaveData.GetLevelHighScore(0));
    }

    [Test]
    public void IsMarbleUnlocked_DefaultsToFalse()
    {
        Assert.IsFalse(SaveData.IsMarbleUnlocked("fire"));
    }

    [Test]
    public void UnlockMarble_ThenReturnsTrue()
    {
        SaveData.UnlockMarble("fire");
        Assert.IsTrue(SaveData.IsMarbleUnlocked("fire"));
    }

    [Test]
    public void TotalGold_DefaultsToZero()
    {
        Assert.AreEqual(0, SaveData.TotalGold);
    }

    [Test]
    public void AddKills_Accumulates()
    {
        SaveData.AddKills(3);
        SaveData.AddKills(7);
        Assert.AreEqual(10, SaveData.TotalKills);
    }
}
