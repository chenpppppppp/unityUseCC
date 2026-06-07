using UnityEngine;

/// <summary>
/// 法师火球 — 飞向玩家，碰到玩家扣血，碰墙回收
/// </summary>
public class Fireball : MonoBehaviour, IPoolable
{
    public int damage = 1;
    public float speed = 3f;
    public float lifetime = 5f;

    private Vector3 direction;
    private Rigidbody rb;

    // 由 PoolManager 调用，替代旧的 Spawn
    public void Launch(Vector3 target)
    {
        direction = (target - transform.position).normalized;
        direction.y = 0f;
        if (rb) rb.velocity = Vector3.zero;
    }

    public void OnSpawn()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
    }

    public void OnDespawn()
    {
        if (rb) rb.velocity = Vector3.zero;
    }

    void Update()
    {
        if (rb)
            rb.MovePosition(transform.position + direction * speed * Time.deltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "Player")
        {
            PlayerHealth ph = other.GetComponent<PlayerHealth>();
            if (ph) ph.TakeDamage(damage);
            Despawn();
            return;
        }

        if (other.GetComponent<WallMarker>())
            Despawn();
    }

    void Despawn()
    {
        if (PoolManager.Instance) PoolManager.Instance.DespawnFireball(this);
        else Destroy(gameObject);
    }

    // 兜底：没有 PoolManager 时手动创建
    public static void Spawn(Vector3 position, Vector3 target)
    {
        if (PoolManager.Instance)
        {
            PoolManager.Instance.SpawnFireball(position, target);
        }
        else
        {
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            obj.name = "Fireball";
            obj.transform.position = position;
            obj.transform.localScale = Vector3.one * 0.3f;
            obj.GetComponent<MeshRenderer>().material.color = new Color(1f, 0.3f, 0f);
            obj.GetComponent<SphereCollider>().isTrigger = true;
            Rigidbody r = obj.AddComponent<Rigidbody>();
            r.useGravity = false; r.drag = 0; r.isKinematic = true;
            Fireball f = obj.AddComponent<Fireball>();
            f.Launch(target);
            Destroy(obj, 5f);
        }
    }
}
