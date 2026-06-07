using UnityEngine;

/// <summary>
/// 波次管理器 — 敌人生成时机、波次切换、Boss 生成
/// 从 GameManager 拆分出来，单一职责：管理波次
/// </summary>
public class WaveManager : MonoBehaviour
{
    [Header("Wave Config")]
    public int currentWave = 1;
    public int totalWaves = 10;
    public int enemiesPerWave = 8;
    public float spawnInterval = 3f;
    public float arenaHalfSize = 9f;

    private float spawnTimer;
    private int spawnedCount;
    private int aliveEnemies;
    private bool waveActive;
    private bool bossWave;
    private BossGolem currentBoss;
    public BossGolem CurrentBoss => currentBoss;
    public bool WaveActive => waveActive;

    public System.Action OnWaveEnd;
    public System.Action OnBossDefeated;

    void Start()
    {
        waveActive = false;

        // 订阅事件：替代每帧 FindObjectsOfType<Enemy>()
        if (EventBus.Instance != null)
        {
            EventBus.Instance.OnEnemyKilled += (e) => aliveEnemies = Mathf.Max(0, aliveEnemies - 1);
        }
    }

    public void BeginWave(int wave)
    {
        currentWave = wave;
        waveActive = true;
        spawnedCount = 0;
        spawnTimer = 0.5f;

        if (currentWave == totalWaves)
        {
            bossWave = true;
            SpawnBoss();
            if (SoundManager.Instance) SoundManager.Instance.PlayBossBGM();
            Debug.Log("=== Boss 战！===");
            return;
        }

        bossWave = false;
        enemiesPerWave = 6 + currentWave * 2;
        spawnInterval = Mathf.Max(0.8f, 3f - (currentWave - 1) * 0.25f);
        Debug.Log($"[WaveManager] 第{currentWave}/{totalWaves}波 敌{enemiesPerWave} 间隔{spawnInterval:F1}s");
    }

    void Update()
    {
        if (!waveActive) return;

        if (bossWave)
        {
            if (currentBoss == null)
                EndWave();
            return;
        }

        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0f && spawnedCount < enemiesPerWave)
        {
            SpawnEnemy();
            spawnedCount++;
            spawnTimer = spawnInterval;
        }

        if (spawnedCount >= enemiesPerWave && aliveEnemies <= 0)
        {
            EndWave();
        }
    }

    void EndWave()
    {
        waveActive = false;

        if (bossWave)
        {
            OnBossDefeated?.Invoke();
            return;
        }

        SpawnChest();

        if (currentWave >= totalWaves)
        {
            OnBossDefeated?.Invoke();
        }
        else
        {
            currentWave++;
            OnWaveEnd?.Invoke();
            Invoke("BeginNextWave", 3f);
        }
    }

    void BeginNextWave()
    {
        BeginWave(currentWave);
    }

    // ========== 敌人生成 ==========

    void SpawnEnemy()
    {
        Vector3 pos = GetRandomEdgePosition();

        if (currentWave <= 3)
            PoolManager.Instance.SpawnEnemy(PoolManager.EnemyType.Slime, pos);
        else if (currentWave <= 6)
        {
            var t = Random.value < 0.6f ? PoolManager.EnemyType.Slime : PoolManager.EnemyType.Bat;
            PoolManager.Instance.SpawnEnemy(t, pos);
        aliveEnemies++;
        }
        else if (currentWave <= 9)
        {
            float r = Random.value;
            PoolManager.EnemyType t;
            if (r < 0.3f) t = PoolManager.EnemyType.Slime;
            else if (r < 0.55f) t = PoolManager.EnemyType.Bat;
            else if (r < 0.75f) t = PoolManager.EnemyType.Skeleton;
            else t = PoolManager.EnemyType.Mage;
            PoolManager.Instance.SpawnEnemy(t, pos);
        aliveEnemies++;
        }
    }

    void SpawnBoss()
    {
        Vector3 pos = new Vector3(0f, 0f, -arenaHalfSize + 3f);
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        obj.name = "Boss_Golem";
        obj.transform.position = pos;
        obj.transform.localScale = new Vector3(2f, 2f, 2f);
        obj.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Standard"));
        obj.GetComponent<MeshRenderer>().material.color = new Color(0.5f, 0.3f, 0.1f);
        currentBoss = obj.AddComponent<BossGolem>();
        Debug.Log("巨石傀儡登场！HP: 500");
    }

    void SpawnChest()
    {
        Vector3 pos = new Vector3(
            Random.Range(-arenaHalfSize + 2, arenaHalfSize - 2),
            0.2f,
            Random.Range(-arenaHalfSize + 2, arenaHalfSize - 2));
        if (PoolManager.Instance) PoolManager.Instance.SpawnChest(pos);
    }

    // ========== 工具 ==========

    Vector3 GetRandomEdgePosition()
    {
        float x, z;
        if (Random.value < 0.5f)
        {
            x = Random.Range(-arenaHalfSize, arenaHalfSize);
            z = Random.value < 0.5f ? -arenaHalfSize : arenaHalfSize;
        }
        else
        {
            x = Random.value < 0.5f ? -arenaHalfSize : arenaHalfSize;
            z = Random.Range(-arenaHalfSize, arenaHalfSize);
        }
        return new Vector3(x, 0f, z);
    }

    int EnemyCount()
    {
        return FindObjectsOfType<Enemy>().Length;
    }
}
