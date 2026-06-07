using UnityEngine;

public class MageEnemy : Enemy
{
    public float preferredDistance = 5f;
    public float fireballInterval = 2f;

    private Rigidbody rb;
    private float fireballTimer;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.drag = 4f;
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        maxHealth = 15; moveSpeed = 1f; contactDamage = 1; expValue = 2; goldValue = 2;
        fireballTimer = fireballInterval;
    }
    public override void OnSpawn() { base.OnSpawn(); LoadFromLua("mage"); }

    void FixedUpdate()
    {
        if (player == null) return;
        Vector3 toPlayer = player.position - transform.position;
        float dist = toPlayer.magnitude;

        if (dist < preferredDistance - 1f)
            rb.velocity = -toPlayer.normalized * moveSpeed;
        else if (dist > preferredDistance + 1f)
            rb.velocity = toPlayer.normalized * moveSpeed;
        else
            rb.velocity = Vector3.zero;

        transform.forward = toPlayer.normalized;
    }

    void Update()
    {
        if (player == null) return;
        fireballTimer -= Time.deltaTime;
        if (fireballTimer <= 0f)
        {
            // 发射火球：从法师位置飞向玩家当前位置
            Vector3 spawnPos = transform.position + transform.forward * 0.5f;
            spawnPos.y = 0f;
            Fireball.Spawn(spawnPos, player.position);
            fireballTimer = fireballInterval;
        }
    }
}
