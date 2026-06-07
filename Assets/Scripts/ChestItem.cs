using UnityEngine;

/// <summary>
/// 宝箱 — 弹珠击中后打开，随机奖励
/// </summary>
public class ChestItem : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Marble>() == null) return;

        float r = Random.value;
        if (r < 0.4f)
        {
            Debug.Log("宝箱奖励：额外金币 +50");
            PlayerHealth ph = FindObjectOfType<PlayerHealth>();
            if (ph != null) ph.AddGold(50);
        }
        else if (r < 0.7f)
        {
            Debug.Log("宝箱奖励：直接升级 1 次！");
            PlayerHealth ph = FindObjectOfType<PlayerHealth>();
            if (ph != null) ph.GainExperience(ph.xpToNextLevel); // 只升1级
        }
        else if (r < 0.9f)
        {
            Debug.Log("宝箱奖励：临时攻击力 +50% 持续 30 秒");
        }
        else
        {
            Debug.Log("宝箱奖励：护盾立即恢复！");
            PlayerHealth ph = FindObjectOfType<PlayerHealth>();
            if (ph != null) ph.RestoreShield();
        }

        Destroy(gameObject);
    }
}
