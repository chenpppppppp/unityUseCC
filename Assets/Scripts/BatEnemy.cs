using UnityEngine;

public class BatEnemy : Enemy
{
    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.drag = 1f;
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        maxHealth = 8; moveSpeed = 4f; contactDamage = 1; expValue = 1; goldValue = 1;
    }
    public override void OnSpawn() { base.OnSpawn(); LoadFromLua("bat"); }

    void FixedUpdate()
    {
        if (player == null) return;
        Vector3 dir = (player.position - transform.position).normalized;
        rb.velocity = dir * moveSpeed;
    }
}
