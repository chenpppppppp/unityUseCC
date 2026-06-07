using UnityEngine;
using System.Collections;

/// <summary>
/// 巨石傀儡 Boss — 状态机驱动三阶段战斗
/// </summary>
public class BossGolem : Enemy
{
    [Header("Boss Stats")]
    public int bossMaxHealth = 500;

    // 状态机暴露给 State 类的属性
    public Rigidbody Rigidbody => _rb;
    public Transform Player => player;
    public float MoveSpeed { get => moveSpeed; set => moveSpeed = value; }

    private int bossHealth;
    private BossState _currentState;
    private Rigidbody _rb;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        if (_rb == null) _rb = gameObject.AddComponent<Rigidbody>();
        _rb.useGravity = false;
        _rb.drag = 3f;
        _rb.constraints = RigidbodyConstraints.FreezeRotation;

        bossHealth = bossMaxHealth;
        currentHealth = bossMaxHealth;
        maxHealth = bossMaxHealth;
        moveSpeed = 1f;
        contactDamage = 1;
        expValue = 50;
        goldValue = 50;
    }

    protected override void Start()
    {
        base.Start();

        transform.localScale = new Vector3(2f, 2f, 2f);
        SetColor(new Color(0.5f, 0.3f, 0.1f));

        // 进入初始状态
        TransitionTo(new Phase1Summon(this));

        Debug.Log("=== 巨石傀儡苏醒！===");
    }

    void Update()
    {
        if (player == null) return;

        // 根据 HP 自动切阶段
        CheckPhaseTransition();

        // 状态更新
        _currentState?.Update();
    }

    /// <summary>状态切换（供 State 类和内部使用）</summary>
    public void TransitionTo(BossState newState)
    {
        _currentState?.Exit();
        var oldPhase = GetPhaseNumber(_currentState);
        _currentState = newState;
        _currentState.Enter();

        int newPhase = GetPhaseNumber(newState);
        if (oldPhase != newPhase)
        {
            Debug.Log($"=== Boss 进入第 {newPhase} 阶段！===");

            // 阶段转换掉落
            for (int i = 0; i < 5; i++)
            {
                Vector3 dropPos = transform.position + Random.insideUnitSphere * 1f;
                dropPos.y = 0f;
                DropItem.Spawn(dropPos, DropItem.DropType.Experience, 5);
                DropItem.Spawn(dropPos + Vector3.right * 0.3f, DropItem.DropType.Gold, 3);
            }

            EventBus.Instance?.OnBossPhaseChanged?.Invoke(newPhase);
        }
    }

    void CheckPhaseTransition()
    {
        int targetPhase = bossHealth > 300 ? 1 : bossHealth > 100 ? 2 : 3;
        int currentPhase = GetPhaseNumber(_currentState);

        if (targetPhase != currentPhase)
        {
            BossState next = targetPhase switch
            {
                1 => new Phase1Summon(this),
                2 => new Phase2Chase(this),
                3 => new Phase3Chase(this),
                _ => new Phase1Summon(this)
            };
            TransitionTo(next);
        }
    }

    int GetPhaseNumber(BossState state)
    {
        if (state is Phase1Summon) return 1;
        if (state is Phase2Chase || state is Phase2Quake || state is PhaseStunned) return 2;
        if (state is Phase3Chase || state is Phase3Charge) return 3;
        return 0;
    }

    // ===== 公开方法（State 类调用）=====

    public void SummonSlimes()
    {
        Debug.Log("Boss 召唤史莱姆！");
        for (int i = 0; i < 3; i++)
        {
            Vector3 pos = transform.position + new Vector3(Random.Range(-2f, 2f), 0f, Random.Range(-2f, 2f));
            PoolManager.Instance?.SpawnEnemy(PoolManager.EnemyType.Slime, pos);
        }
    }

    public void MoveTowardPlayer(float speed)
    {
        if (player == null) return;
        Vector3 dir = (player.position - transform.position).normalized;
        _rb.velocity = dir * speed;
    }

    public void SetColor(Color c)
    {
        var mr = GetComponent<MeshRenderer>();
        if (mr) mr.material.color = c;
    }

    public void OnWeakPointHit()
    {
        if (_currentState is Phase2Quake quake)
            quake.OnWeakPointHit();
    }

    // ===== 伤害与死亡 =====

    public void TakeBossDamage(int amount)
    {
        bossHealth -= amount;
        currentHealth = bossHealth;
    }

    public override void TakeDamage(int amount)
    {
        bossHealth -= amount;
        currentHealth = bossHealth;
        StartCoroutine(FlashRed());

        if (bossHealth <= 0)
            Die();
    }

    IEnumerator FlashRed()
    {
        var mr = GetComponent<MeshRenderer>();
        if (!mr) yield break;
        Color original = mr.material.color;
        mr.material.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        if (mr) mr.material.color = original;
    }

    protected override void Die()
    {
        Debug.Log("=== 巨石傀儡被击败！===");

        EventBus.Instance?.OnEnemyKilled?.Invoke(this);

        if (SoundManager.Instance) SoundManager.Instance.Play("boss_death");
        if (ParticleManager.Instance) ParticleManager.Instance.PlayBossDeath(transform.position);

        for (int i = 0; i < 20; i++)
        {
            Vector3 pos = transform.position + Random.insideUnitSphere * 2f;
            pos.y = 0f;
            DropItem.Spawn(pos, DropItem.DropType.Experience, 10);
            if (i % 2 == 0) DropItem.Spawn(pos + Vector3.right, DropItem.DropType.Gold, 5);
        }

        Destroy(gameObject);
    }

    /// <summary>供 WaveManager 销毁时调用</summary>
    void OnDestroy()
    {
        _currentState?.Exit();
    }
}

/// <summary>Boss 弱点 — 被弹珠击中通知 Boss</summary>
public class WeakPoint : MonoBehaviour
{
    public BossGolem boss;

    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Marble>() != null && boss != null)
        {
            boss.OnWeakPointHit();
            Destroy(gameObject);
        }
    }
}
