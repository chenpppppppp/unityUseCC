using UnityEngine;

public class SkeletonEnemy : Enemy
{
    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.drag = 3f;
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        maxHealth = 25; moveSpeed = 1.8f; contactDamage = 1; expValue = 2; goldValue = 2;
    }
    public override void OnSpawn() { base.OnSpawn(); LoadFromLua("skeleton"); }

    void FixedUpdate()
    {
        if (player == null) return;
        Vector3 dir = (player.position - transform.position).normalized;
        rb.velocity = dir * moveSpeed;

        if (rb.velocity.magnitude > 0.1f)
            transform.forward = rb.velocity.normalized;
    }
}
