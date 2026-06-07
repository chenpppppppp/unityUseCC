using UnityEngine;

/// <summary>
/// 游戏管理器 — 核心协调器
/// 职责：游戏状态、击杀计数、协调 WaveManager
/// </summary>
public class GameManager : MonoBehaviour
{
    [HideInInspector] public bool victory;
    [HideInInspector] public int currentLevelIndex;
    [HideInInspector] public int totalKills;
    [HideInInspector] public int totalWaves = 10;
    [HideInInspector] public int currentWave = 1;

    private WaveManager _wave;
    private bool _gameStarted;
    public BossGolem CurrentBoss => _wave != null ? _wave.CurrentBoss : null;

    public static GameManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    void Start()
    {
        _wave = GetComponent<WaveManager>();
        if (_wave == null) _wave = gameObject.AddComponent<WaveManager>();

        // 确保基础设施存在
        if (PoolManager.Instance == null)
            new GameObject("PoolManager").AddComponent<PoolManager>();
        if (EventBus.Instance == null)
            new GameObject("EventBus").AddComponent<EventBus>();

        // 订阅事件（替代被直接调用）
        if (EventBus.Instance != null)
        {
            EventBus.Instance.OnEnemyKilled += (e) => totalKills++;
            EventBus.Instance.OnMarbleBounce += () => SaveData.AddBounces(1);
            EventBus.Instance.OnDropCollected += (type, val) => {
                if (type == DropItem.DropType.Gold) SaveData.AddGold(val);
            };
            EventBus.Instance.OnGameStarted += () => totalKills = 0;
        }

        // 波次结束回调
        _wave.OnWaveEnd = () => currentWave = _wave.currentWave;
        _wave.OnBossDefeated = () => victory = true;
    }

    void Update()
    {
        // 同步波次
        if (_wave != null) currentWave = _wave.currentWave;
    }

    public void StartGame()
    {
        victory = false;
        totalKills = 0;
        currentWave = 1;

        if (SoundManager.Instance) SoundManager.Instance.PlayBattleBGM();

        _wave.totalWaves = totalWaves;
        _wave.BeginWave(1);
        _gameStarted = true;
    }

    public void AddKill() => totalKills++;
}
