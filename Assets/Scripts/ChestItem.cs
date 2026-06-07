using UnityEngine;

/// <summary>
/// 宝箱 — 弹珠击中后打开，随机奖励
/// </summary>
public class ChestItem : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        Marble marble = other.GetComponent<Marble>();
        if (marble == null) return;

        float r = Random.value;
        if (r < 0.4f)
        {
            // 金币
            Debug.Log("宝箱奖励：额外金币 +50");
        }
        else if (r < 0.7f)
        {
            // 直接升级
            Debug.Log("宝箱奖励：直接升级 1 次！");
            PlayerHealth ph = FindObjectOfType<PlayerHealth>();
            if (ph != null) ph.GainExperience(999);
        }
        else if (r < 0.9f)
        {
            // 临时攻击力提升
            Debug.Log("宝箱奖励：攻击力 +50% 持续 30 秒");
        }
        else
        {
            // 护盾恢复
            Debug.Log("宝箱奖励：护盾立即恢复！");
            PlayerHealth ph = FindObjectOfType<PlayerHealth>();
            if (ph != null) ph.RestoreShield();
        }

        Destroy(gameObject);
    }
}
