using UnityEngine;

public class BomberEnemy : Enemy
{
    public float explosionRadius = 3f;
    public int explosionDamage = 15;
    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.drag = 2f;
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        maxHealth = 20; moveSpeed = 3f; contactDamage = 1; expValue = 1; goldValue = 1;
    }
    public override void OnSpawn() { base.OnSpawn(); LoadFromLua("bomber"); }

    protected override void Die()
    {
        // 爆炸范围伤害
        Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider hit in hits)
        {
            Enemy e = hit.GetComponent<Enemy>();
            if (e != null && e != this)
                e.TakeDamage(explosionDamage);
        }
        Debug.Log($"{name} 爆炸！范围伤害 {explosionDamage}");

        // 正常掉落
        base.Die();
    }

    void FixedUpdate()
    {
        if (player == null) return;
        Vector3 dir = (player.position - transform.position).normalized;
        rb.velocity = dir * moveSpeed;
    }
}
