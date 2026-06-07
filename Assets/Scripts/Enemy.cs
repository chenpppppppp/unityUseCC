using UnityEngine;

/// <summary>
/// 敌人基类 — 生命、受伤、死亡掉落
/// </summary>
public class Enemy : MonoBehaviour, IPoolable
{
    [Header("Stats")]
    public int maxHealth = 10;
    public int contactDamage = 1;
    public float moveSpeed = 2f;
    public int expValue = 1;
    public int goldValue = 1;

    [HideInInspector] public int currentHealth;
    protected Transform player;
    private static PlayerAim _cachedAim;

    protected virtual void Start()
    {
        // 缓存 player 引用（非池化对象也需要）
        if (player == null) player = GameObject.Find("Player")?.transform;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }

    public virtual void TakeDamage(int amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0)
            Die();
    }

    protected virtual void Die()
    {
        // 事件总线通知（替代直接调用 GameManager.Instance.AddKill()）
        EventBus.Instance?.OnEnemyKilled?.Invoke(this);

        // 死亡音效+粒子
        if (SoundManager.Instance) SoundManager.Instance.Play("enemy_death");
        if (ParticleManager.Instance)
        {
            Color deathColor = GetComponent<MeshRenderer>() != null
                ? GetComponent<MeshRenderer>().material.color : Color.white;
            ParticleManager.Instance.PlayEnemyDeath(transform.position, deathColor);
        }

        // 掉落经验球（必定）
        DropItem.Spawn(transform.position, DropItem.DropType.Experience, expValue);

        // 掉落金币（概率）
        if (Random.value < 0.6f)
            DropItem.Spawn(transform.position + Vector3.right * 0.3f, DropItem.DropType.Gold, goldValue);

        // 掉落血瓶（低概率）
        if (Random.value < 0.08f)
            DropItem.Spawn(transform.position + Vector3.left * 0.3f, DropItem.DropType.Health, 1);

        // 击杀爆炸（升级触发，缓存引用）
        if (_cachedAim == null) _cachedAim = FindObjectOfType<PlayerAim>();
        if (_cachedAim && Random.value < _cachedAim.explosionChance)
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, 3f);
            foreach (Collider h in hits)
            {
                Enemy e = h.GetComponent<Enemy>();
                if (e && e != this) e.TakeDamage(15);
            }
        }

        // 使用对象池回收
        if (PoolManager.Instance) PoolManager.Instance.DespawnEnemy(this);
        else Destroy(gameObject);
    }

    // ===== IPoolable =====
    /// <summary>从 Lua 热更配置中读取数值（有 XLuaManager 时生效）</summary>
    protected void LoadFromLua(string enemyType)
    {
        if (XLuaManager.Instance == null) return;

        var table = XLuaManager.Instance.Get<XLua.LuaTable>("Balance");
        if (table == null) return;

        var enemies = table.Get<XLua.LuaTable>("enemies");
        if (enemies == null) { table.Dispose(); return; }

        var cfg = enemies.Get<XLua.LuaTable>(enemyType);
        if (cfg == null) { enemies.Dispose(); table.Dispose(); return; }

        maxHealth = cfg.Get<int>("health");
        moveSpeed = cfg.Get<float>("speed");
        contactDamage = cfg.Get<int>("damage");
        expValue = cfg.Get<int>("exp");
        goldValue = cfg.Get<int>("gold");

        cfg.Dispose();
        enemies.Dispose();
        table.Dispose();

        Debug.Log($"[Lua] {enemyType}: HP={maxHealth} SPD={moveSpeed}");
    }

    public virtual void OnSpawn()
    {
        currentHealth = maxHealth;
        player = GameObject.Find("Player")?.transform;
        Rigidbody r = GetComponent<Rigidbody>();
        if (r) { r.velocity = Vector3.zero; r.angularVelocity = Vector3.zero; }
    }

    public virtual void OnDespawn()
    {
        Rigidbody r = GetComponent<Rigidbody>();
        if (r) { r.velocity = Vector3.zero; r.angularVelocity = Vector3.zero; }
    }

    protected virtual void OnCollisionEnter(Collision col)
    {
        // 碰到玩家造成伤害并自毁
        if (col.gameObject.name == "Player")
        {
            PlayerHealth ph = col.gameObject.GetComponent<PlayerHealth>();
            if (ph != null)
                ph.TakeDamage(contactDamage);
            Die();
        }
    }
}
