using UnityEngine;

/// <summary>
/// 对象池管理器 — 集中管理所有对象池，替代 CreatePrimitive + Destroy
/// </summary>
public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance { get; private set; }

    private ObjectPool<Marble> _marblePool;
    private ObjectPool<Enemy> _slimePool;
    private ObjectPool<Enemy> _batPool;
    private ObjectPool<Enemy> _skeletonPool;
    private ObjectPool<Enemy> _magePool;
    private ObjectPool<Enemy> _bomberPool;
    private ObjectPool<Fireball> _fireballPool;
    private ObjectPool<DropItem> _dropPool;

    // 材质由 MaterialCache 统一管理

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        InitPools();
    }

    void InitPools()
    {
        _marblePool = new ObjectPool<Marble>(CreateMarble, 10, "Pool_Marble");
        _slimePool = new ObjectPool<Enemy>(() => CreateEnemy(EnemyType.Slime), 6, "Pool_Slime");
        _batPool = new ObjectPool<Enemy>(() => CreateEnemy(EnemyType.Bat), 4, "Pool_Bat");
        _skeletonPool = new ObjectPool<Enemy>(() => CreateEnemy(EnemyType.Skeleton), 3, "Pool_Skeleton");
        _magePool = new ObjectPool<Enemy>(() => CreateEnemy(EnemyType.Mage), 2, "Pool_Mage");
        _bomberPool = new ObjectPool<Enemy>(() => CreateEnemy(EnemyType.Bomber), 2, "Pool_Bomber");
        _fireballPool = new ObjectPool<Fireball>(CreateFireball, 5, "Pool_Fireball");
        _dropPool = new ObjectPool<DropItem>(CreateDropItem, 15, "Pool_Drop");

        Debug.Log($"[PoolManager] 所有对象池初始化完成");
    }

    // ========== 弹珠 ==========

    Marble CreateMarble()
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = "Marble";
        go.transform.localScale = Vector3.one * 0.25f;

        SphereCollider col = go.GetComponent<SphereCollider>();
        col.material = MaterialCache.GetBouncyPhysicMat();

        Rigidbody rb = go.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.drag = 0;
        rb.angularDrag = 0;
        rb.mass = 0.5f;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        TrailRenderer trail = go.AddComponent<TrailRenderer>();
        trail.startWidth = 0.12f;
        trail.endWidth = 0.02f;
        trail.time = 0.25f;
        trail.material = MaterialCache.GetTrailMaterial();

        MeshRenderer mr = go.GetComponent<MeshRenderer>();
        mr.material = MaterialCache.GetOpaque(new Color(1f, 0.85f, 0.1f));

        Marble m = go.AddComponent<Marble>();
        m.trail = trail;
        return m;
    }

    public Marble SpawnMarble(Vector3 pos, Vector3 dir, float force, int bounces, int healThresh, float size)
    {
        Marble m = _marblePool.Get();
        m.transform.position = pos;
        m.transform.localScale = Vector3.one * size;
        m.totalBounces = bounces;
        m.bounceHealThreshold = healThresh;

        // 忽略与玩家的碰撞
        PlayerAim aim = FindObjectOfType<PlayerAim>();
        if (aim != null)
        {
            Collider mc = m.GetComponent<Collider>();
            Collider pc = aim.GetComponent<Collider>();
            if (mc && pc) Physics.IgnoreCollision(mc, pc);
        }

        m.Launch(dir, force);
        return m;
    }

    public void DespawnMarble(Marble m)
    {
        _marblePool.Release(m);
    }

    public void DespawnAllMarbles()
    {
        _marblePool.ReleaseAll();
    }

    // ========== 敌人 ==========

    public enum EnemyType { Slime, Bat, Skeleton, Mage, Bomber }

    Enemy CreateEnemy(EnemyType type)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        MeshRenderer mr = go.GetComponent<MeshRenderer>();

        Enemy e = null;
        switch (type)
        {
            case EnemyType.Slime:
                go.name = "Slime"; mr.material = MaterialCache.GetOpaque(new Color(0.3f, 0.8f, 0.3f));
                go.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                e = go.AddComponent<SlimeEnemy>(); break;
            case EnemyType.Bat:
                go.name = "Bat"; mr.material = MaterialCache.GetOpaque(new Color(0.6f, 0.2f, 0.6f));
                go.transform.localScale = new Vector3(0.4f, 0.3f, 0.4f);
                e = go.AddComponent<BatEnemy>(); break;
            case EnemyType.Skeleton:
                go.name = "Skeleton"; mr.material = MaterialCache.GetOpaque(new Color(0.85f, 0.85f, 0.85f));
                go.transform.localScale = new Vector3(0.5f, 0.7f, 0.5f);
                e = go.AddComponent<SkeletonEnemy>(); break;
            case EnemyType.Mage:
                go.name = "Mage"; mr.material = MaterialCache.GetOpaque(new Color(0.2f, 0.4f, 0.9f));
                go.transform.localScale = new Vector3(0.5f, 0.6f, 0.5f);
                e = go.AddComponent<MageEnemy>(); break;
            case EnemyType.Bomber:
                go.name = "Bomber"; mr.material = MaterialCache.GetOpaque(new Color(1f, 0.3f, 0.1f));
                go.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                e = go.AddComponent<BomberEnemy>(); break;
        }
        return e;
    }

    ObjectPool<Enemy> GetEnemyPool(EnemyType type)
    {
        switch (type)
        {
            case EnemyType.Slime: return _slimePool;
            case EnemyType.Bat: return _batPool;
            case EnemyType.Skeleton: return _skeletonPool;
            case EnemyType.Mage: return _magePool;
            case EnemyType.Bomber: return _bomberPool;
            default: return _slimePool;
        }
    }

    public Enemy SpawnEnemy(EnemyType type, Vector3 pos)
    {
        ObjectPool<Enemy> pool = GetEnemyPool(type);
        Enemy e = pool.Get();
        e.transform.position = pos;
        return e;
    }

    public void DespawnEnemy(Enemy e)
    {
        // 通过组件类型反查池
        if (e is SlimeEnemy) _slimePool.Release(e);
        else if (e is BatEnemy) _batPool.Release(e);
        else if (e is SkeletonEnemy) _skeletonPool.Release(e);
        else if (e is MageEnemy) _magePool.Release(e);
        else if (e is BomberEnemy) _bomberPool.Release(e);
    }

    // ========== 火球 ==========

    Fireball CreateFireball()
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = "Fireball";
        go.transform.localScale = Vector3.one * 0.3f;

        MeshRenderer mr = go.GetComponent<MeshRenderer>();
        mr.material = MaterialCache.GetEmissive(new Color(1f, 0.3f, 0f), 0.5f);

        SphereCollider col = go.GetComponent<SphereCollider>();
        col.isTrigger = true;

        Rigidbody rb = go.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.drag = 0;
        rb.isKinematic = true;

        TrailRenderer trail = go.AddComponent<TrailRenderer>();
        trail.startWidth = 0.15f;
        trail.endWidth = 0.01f;
        trail.time = 0.2f;
        trail.material = MaterialCache.GetTrailMaterial();
        trail.startColor = new Color(1f, 0.4f, 0f);
        trail.endColor = new Color(1f, 0.1f, 0f, 0f);

        return go.AddComponent<Fireball>();
    }

    public Fireball SpawnFireball(Vector3 pos, Vector3 target)
    {
        Fireball f = _fireballPool.Get();
        f.transform.position = pos;
        f.Launch(target);
        // 定时回收
        f.StartCoroutine(AutoDespawnFireball(f, 5f));
        return f;
    }

    System.Collections.IEnumerator AutoDespawnFireball(Fireball f, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (f != null && f.gameObject.activeSelf)
            DespawnFireball(f);
    }

    public void DespawnFireball(Fireball f)
    {
        _fireballPool.Release(f);
    }

    // ========== 掉落物 ==========

    DropItem CreateDropItem()
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = "Drop";
        go.transform.localScale = Vector3.one * 0.2f;
        go.GetComponent<MeshRenderer>().material = MaterialCache.GetOpaque(Color.white);

        SphereCollider col = go.GetComponent<SphereCollider>();
        col.isTrigger = true;

        go.AddComponent<Rigidbody>();
        return go.AddComponent<DropItem>();
    }

    public DropItem SpawnDrop(Vector3 pos, DropItem.DropType type, int value)
    {
        DropItem d = _dropPool.Get();
        d.transform.position = pos;
        d.Init(type, value);
        // 定时回收
        d.StartCoroutine(AutoDespawnDrop(d, 15f));
        return d;
    }

    System.Collections.IEnumerator AutoDespawnDrop(DropItem d, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (d != null && d.gameObject.activeSelf)
            DespawnDrop(d);
    }

    public void DespawnDrop(DropItem d)
    {
        _dropPool.Release(d);
    }

    // ========== 宝箱（低频，直接 Destroy 就行） ==========
    public GameObject SpawnChest(Vector3 pos)
    {
        GameObject chest = GameObject.CreatePrimitive(PrimitiveType.Cube);
        chest.name = "Chest";
        chest.transform.position = pos;
        chest.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        chest.GetComponent<MeshRenderer>().material.color = new Color(1f, 0.84f, 0f);
        chest.GetComponent<BoxCollider>().isTrigger = true;
        chest.AddComponent<ChestItem>();
        return chest;
    }
}
