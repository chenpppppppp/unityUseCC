using UnityEngine;

public class SlimeEnemy : Enemy
{
    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.drag = 2f;
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        maxHealth = 10; moveSpeed = 1.5f; contactDamage = 1; expValue = 1; goldValue = 1;
    }

    public override void OnSpawn()
    {
        base.OnSpawn();
        LoadFromLua("slime"); // 每次出池重新读 Lua 配置
    }

    void FixedUpdate()
    {
        if (player == null) return;

        // 尝试 Lua 热更逻辑（传 transform 和参数，Lua 返回速度向量）
        if (XLuaManager.Instance != null)
        {
            var result = XLuaManager.Instance.Call("SlimeAI.GetVelocity",
                transform.position, moveSpeed, player.position, currentHealth, maxHealth);
            if (result != null && result.Length > 0 && result[0] is Vector3)
            {
                rb.velocity = (Vector3)result[0];
                return;
            }
        }

        // C# 兜底逻辑
        Vector3 dir = (player.position - transform.position).normalized;
        rb.velocity = dir * moveSpeed;
    }
}
