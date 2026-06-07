using UnityEngine;
using System;

/// <summary>
/// 事件总线 — 全局发布订阅，解耦模块间通信
/// 使用 Action 委托（非 event），允许任意模块触发
/// </summary>
public class EventBus : MonoBehaviour
{
    public static EventBus Instance { get; private set; }

    // 战斗
    public Action<Enemy> OnEnemyKilled;
    public Action OnMarbleBounce;
    public Action OnMarbleHitEnemy;

    // 掉落
    public Action<DropItem.DropType, int> OnDropCollected;

    // 玩家
    public Action OnPlayerDied;
    public Action<int> OnPlayerLevelUp;
    public Action<int> OnExperienceGained;
    public Action<int> OnGoldGained;
    public Action<int> OnPlayerHealed;
    public Action<int> OnPlayerDamaged;

    // 波次
    public Action<int, int> OnWaveStarted;
    public Action<int> OnWaveEnded;
    public Action<int> OnBossPhaseChanged;

    // 游戏状态
    public Action OnGameStarted;
    public Action OnGameWon;
    public Action OnGameLost;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }
}
