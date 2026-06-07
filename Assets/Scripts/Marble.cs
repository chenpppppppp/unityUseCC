using UnityEngine;

/// <summary>
/// 弹珠行为：弹射、伤害、回收、回血
/// </summary>
public class Marble : MonoBehaviour, IPoolable
{
    [HideInInspector] public int totalBounces = 8;
    [HideInInspector] public int bounceHealThreshold;

    public int damage = 10;
    public float stopThreshold = 0.5f;
    public TrailRenderer trail;

    private int bouncesLeft;
    private int bounceCount;
    private Rigidbody rb;
    private bool launched;
    private float stopTimer;
    private Coroutine _recoverRoutine;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    // ===== IPoolable =====
    public void OnSpawn()
    {
        launched = false;
        bouncesLeft = totalBounces;
        bounceCount = 0;
        stopTimer = 0f;
        if (trail) { trail.enabled = false; trail.Clear(); }
        if (rb) rb.velocity = Vector3.zero;
    }

    public void OnDespawn()
    {
        launched = false;
        if (trail) trail.enabled = false;
        if (rb) rb.velocity = Vector3.zero;
        if (_recoverRoutine != null) { StopCoroutine(_recoverRoutine); _recoverRoutine = null; }
    }

    public void Launch(Vector3 dir, float force)
    {
        rb.velocity = dir * force;
        bouncesLeft = totalBounces;
        bounceCount = 0;
        launched = true;
        stopTimer = 0f;
        if (trail) { trail.enabled = true; trail.Clear(); }
    }

    void Update()
    {
        if (!launched) return;
        if (rb.velocity.magnitude < stopThreshold)
        {
            stopTimer += Time.deltaTime;
            if (stopTimer > 0.5f) Recover();
        }
        else stopTimer = 0f;
    }

    void OnCollisionEnter(Collision col)
    {
        if (!launched) return;
        GameObject other = col.gameObject;

        if (other.GetComponent<PitMarker>()) { Recover(); return; }

        if (other.GetComponent<SpringMarker>())
        {
            rb.velocity = rb.velocity * 1.5f;
            return;
        }

        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null)
        {
            bouncesLeft--;
            bounceCount++;
            EventBus.Instance?.OnMarbleBounce?.Invoke();
            EventBus.Instance?.OnMarbleHitEnemy?.Invoke();
            enemy.TakeDamage(damage);
            CheckBounceHeal();
            if (SoundManager.Instance) SoundManager.Instance.Play("marble_hit");
            if (ParticleManager.Instance) ParticleManager.Instance.PlayMarbleBounce(transform.position);
        }
        else if (other.GetComponent<WallMarker>())
        {
            bouncesLeft--;
            bounceCount++;
            EventBus.Instance?.OnMarbleBounce?.Invoke();
            CheckBounceHeal();
            if (SoundManager.Instance) SoundManager.Instance.Play("marble_bounce");
            if (ParticleManager.Instance) ParticleManager.Instance.PlayMarbleBounce(transform.position);
        }

        if (bouncesLeft <= 0) { Recover(); return; }

        Portal portal = other.GetComponent<Portal>();
        if (portal && portal.linkedPortal)
            transform.position = portal.linkedPortal.transform.position;
    }

    void CheckBounceHeal()
    {
        if (bounceHealThreshold <= 0) return;
        if (bounceCount % bounceHealThreshold == 0)
        {
            PlayerHealth ph = FindObjectOfType<PlayerHealth>();
            if (ph) ph.Heal(1);
        }
    }

    void Recover()
    {
        launched = false;
        if (PoolManager.Instance)
            PoolManager.Instance.DespawnMarble(this);
        else
            Destroy(gameObject, 0.1f);
    }
}
