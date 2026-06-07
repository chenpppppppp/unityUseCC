using UnityEngine;
using System.Collections;

/// <summary>
/// Boss 状态基类 — 状态模式
/// </summary>
public abstract class BossState
{
    protected BossGolem Boss;

    public BossState(BossGolem boss) { Boss = boss; }

    public virtual void Enter() { }
    public virtual void Update() { }
    public virtual void Exit() { }
}

// ===== 阶段1：召唤 =====
public class Phase1Summon : BossState
{
    private float summonTimer;

    public Phase1Summon(BossGolem boss) : base(boss) { }

    public override void Enter()
    {
        summonTimer = 2f;
        Boss.SetColor(new Color(0.5f, 0.3f, 0.1f));
        Boss.MoveSpeed = 1f;
    }

    public override void Update()
    {
        Boss.MoveTowardPlayer(1f);

        summonTimer -= Time.deltaTime;
        if (summonTimer <= 0f)
        {
            Boss.SummonSlimes();
            summonTimer = 5f;
        }
    }
}

// ===== 阶段2：追踪（地震前）=====
public class Phase2Chase : BossState
{
    private float quakeTimer;

    public Phase2Chase(BossGolem boss) : base(boss) { }

    public override void Enter()
    {
        quakeTimer = 4f;
        Boss.MoveSpeed = 0.5f;
    }

    public override void Update()
    {
        Boss.MoveTowardPlayer(0.5f);

        quakeTimer -= Time.deltaTime;
        if (quakeTimer <= 0f)
        {
            Boss.TransitionTo(new Phase2Quake(Boss));
        }
    }
}

// ===== 阶段2：地震中 =====
public class Phase2Quake : BossState
{
    private GameObject[] weakPoints;
    private int destroyedCount;
    private float elapsed;
    private const float QuakeDuration = 5f;

    public Phase2Quake(BossGolem boss) : base(boss) { }

    public override void Enter()
    {
        destroyedCount = 0;
        elapsed = 0f;
        weakPoints = new GameObject[3];

        for (int i = 0; i < 3; i++)
        {
            Vector3 pos = new Vector3(Random.Range(-7f, 7f), 0.3f, Random.Range(-7f, 7f));
            weakPoints[i] = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            weakPoints[i].name = $"WeakPoint_{i}";
            weakPoints[i].transform.position = pos;
            weakPoints[i].transform.localScale = Vector3.one * 0.4f;
            weakPoints[i].GetComponent<MeshRenderer>().material.color = Color.red;
            weakPoints[i].GetComponent<SphereCollider>().isTrigger = true;

            WeakPoint wp = weakPoints[i].AddComponent<WeakPoint>();
            wp.boss = Boss;
        }
    }

    public override void Update()
    {
        elapsed += Time.deltaTime;

        // Boss 震动
        float shake = Mathf.Sin(elapsed * 20f) * 0.3f;
        Boss.transform.position += Vector3.up * shake;

        if (destroyedCount >= 3 || elapsed >= QuakeDuration)
        {
            Cleanup();
            if (destroyedCount >= 3)
            {
                Boss.TakeBossDamage(30);
                Boss.TransitionTo(new PhaseStunned(Boss));
            }
            else
            {
                Boss.TransitionTo(new Phase2Chase(Boss));
            }
        }
    }

    public void OnWeakPointHit()
    {
        destroyedCount++;
    }

    private void Cleanup()
    {
        if (weakPoints != null)
            foreach (var wp in weakPoints)
                if (wp != null) Object.Destroy(wp);
    }

    public override void Exit()
    {
        Cleanup();
    }
}

// ===== 硬直 =====
public class PhaseStunned : BossState
{
    private float stunTimer;

    public PhaseStunned(BossGolem boss) : base(boss) { }

    public override void Enter()
    {
        stunTimer = 2f;
        Boss.SetColor(Color.yellow);
        Boss.MoveSpeed = 0f;
    }

    public override void Update()
    {
        stunTimer -= Time.deltaTime;
        if (stunTimer <= 0f)
            Boss.TransitionTo(new Phase3Chase(Boss));
    }
}

// ===== 阶段3：追踪（冲撞前）=====
public class Phase3Chase : BossState
{
    private float chargeTimer;

    public Phase3Chase(BossGolem boss) : base(boss) { }

    public override void Enter()
    {
        Boss.SetColor(new Color(1f, 0.2f, 0.1f));
        Boss.MoveSpeed = 6f;
        chargeTimer = 2f;
    }

    public override void Update()
    {
        Boss.MoveTowardPlayer(2f);

        chargeTimer -= Time.deltaTime;
        if (chargeTimer <= 0f)
        {
            Boss.TransitionTo(new Phase3Charge(Boss, Boss.Player.position));
        }
    }
}

// ===== 阶段3：冲撞中 =====
public class Phase3Charge : BossState
{
    private Vector3 chargeDir;
    private float chargeTime;
    private float trailTimer;

    public Phase3Charge(BossGolem boss, Vector3 playerPos) : base(boss)
    {
        chargeDir = (playerPos - boss.transform.position).normalized;
    }

    public override void Enter()
    {
        chargeTime = 0f;
        trailTimer = 0f;
    }

    public override void Update()
    {
        chargeTime += Time.deltaTime;
        trailTimer += Time.deltaTime;

        Boss.Rigidbody.velocity = chargeDir * 8f;

        // 留伤害路径
        if (trailTimer > 0.2f)
        {
            trailTimer = 0f;
            GameObject trail = GameObject.CreatePrimitive(PrimitiveType.Cube);
            trail.transform.position = Boss.transform.position;
            trail.transform.localScale = new Vector3(0.5f, 0.05f, 0.5f);
            trail.GetComponent<MeshRenderer>().material.color = new Color(1f, 0f, 0f, 0.5f);
            Object.Destroy(trail, 3f);
        }

        if (chargeTime >= 1f)
        {
            Boss.Rigidbody.velocity = Vector3.zero;
            Boss.TransitionTo(new Phase3Chase(Boss));
        }
    }
}
