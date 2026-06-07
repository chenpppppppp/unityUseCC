using UnityEngine;

/// <summary>
/// 掉落物 — 经验球/金币/血瓶，玩家靠近自动吸附
/// </summary>
public class DropItem : MonoBehaviour, IPoolable
{
    private float _despawnTimer;
    private Coroutine _despawnRoutine;
    public enum DropType
    {
        Experience,
        Gold,
        Health
    }

    public DropType dropType;
    public int value = 1;
    public float attractRange = 3f;
    public float attractSpeed = 5f;
    public float lifetime = 15f;

    private Transform player;
    private static PlayerAim _cachedAim;

    // 由 PoolManager 调用，替代 Start
    public void Init(DropType type, int val)
    {
        dropType = type;
        value = val;
        player = GameObject.Find("Player")?.transform;
        if (_cachedAim == null) _cachedAim = FindObjectOfType<PlayerAim>();

        MeshRenderer mr = GetComponent<MeshRenderer>();
        if (mr)
        {
            switch (type)
            {
                case DropType.Experience: mr.material = MaterialCache.GetOpaque(Color.green); break;
                case DropType.Gold: mr.material = MaterialCache.GetOpaque(Color.yellow); break;
                case DropType.Health: mr.material = MaterialCache.GetOpaque(Color.red); break;
            }
        }

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb)
        {
            rb.velocity = Vector3.zero;
            Vector3 popDir = new Vector3(Random.Range(-1f, 1f), 1f, Random.Range(-1f, 1f)).normalized;
            rb.AddForce(popDir * 3f, ForceMode.Impulse);
            rb.useGravity = true;
        }
    }

    public void OnSpawn() { }
    public void OnDespawn()
    {
        if (_despawnRoutine != null) { StopCoroutine(_despawnRoutine); _despawnRoutine = null; }
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb) rb.velocity = Vector3.zero;
    }

    void Update()
    {
        if (player == null) return;

        // 拾取范围受升级影响（缓存引用，不再每帧 Find）
        float range = attractRange;
        if (_cachedAim) range *= _cachedAim.pickupRangeMult;

        float dist = Vector3.Distance(transform.position, player.position);
        if (dist < range)
        {
            Vector3 dir = (player.position - transform.position).normalized;
            transform.position += dir * attractSpeed * Time.deltaTime;

            if (dist < 0.3f)
            {
                Collect();
            }
        }
    }

    void Collect()
    {
        PlayerHealth ph = FindObjectOfType<PlayerHealth>();
        if (ph == null) { Destroy(gameObject); return; }

        switch (dropType)
        {
            case DropType.Experience:
                ph.GainExperience(value);
                if (SoundManager.Instance) SoundManager.Instance.Play("pickup_exp");
                if (ParticleManager.Instance) ParticleManager.Instance.PlayPickup(transform.position, Color.green);
                break;
            case DropType.Gold:
                ph.AddGold(value);
                if (SoundManager.Instance) SoundManager.Instance.Play("pickup_gold");
                if (ParticleManager.Instance) ParticleManager.Instance.PlayPickup(transform.position, Color.yellow);
                break;
            case DropType.Health:
                ph.Heal(value);
                if (SoundManager.Instance) SoundManager.Instance.Play("pickup_health");
                if (ParticleManager.Instance) ParticleManager.Instance.PlayPickup(transform.position, Color.red);
                break;
        }

        // 事件总线通知（Gold 的存档由 GameManager 订阅处理）
        EventBus.Instance?.OnDropCollected?.Invoke(dropType, value);

        if (PoolManager.Instance) PoolManager.Instance.DespawnDrop(this);
        else Destroy(gameObject);
    }

    /// <summary>在指定位置生成掉落物</summary>
    public static void Spawn(Vector3 position, DropType type, int value = 1)
    {
        if (PoolManager.Instance)
            PoolManager.Instance.SpawnDrop(position, type, value);
        else
        {
            // 兜底：没有 PoolManager 时用旧方式
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            obj.name = $"Drop_{type}";
            obj.transform.position = position;
            obj.transform.localScale = Vector3.one * 0.2f;
            MeshRenderer mr = obj.GetComponent<MeshRenderer>();
            mr.material = new Material(Shader.Find("Standard"));
            mr.material.color = type == DropType.Experience ? Color.green :
                               type == DropType.Gold ? Color.yellow : Color.red;
            obj.GetComponent<SphereCollider>().isTrigger = true;
            obj.AddComponent<Rigidbody>();
            DropItem d = obj.AddComponent<DropItem>();
            d.Init(type, value);
        }
    }
}
