using UnityEngine;
using System;

/// <summary>
/// 玩家系统：生命、护盾、经验、金币
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    public event Action<int> OnLevelUp;
    public event Action OnDeath;
    [Header("Health")]
    public int maxHealth = 3;
    [HideInInspector] public int currentHealth;

    [Header("Shield")]
    public int maxShield = 1;
    [HideInInspector] public int currentShield;
    public float shieldRecoveryTime = 30f;
    private float shieldRecoveryTimer;

    [Header("Experience")]
    public int currentXP = 0;
    public int xpToNextLevel = 10;
    public int level = 1;
    public int maxLevel = 15;

    [Header("Gold")]
    public int gold = 0;

    void Start()
    {
        currentHealth = maxHealth;
        currentShield = maxShield;
    }

    void Update()
    {
        // 护盾恢复
        if (currentShield < maxShield)
        {
            shieldRecoveryTimer += Time.deltaTime;
            if (shieldRecoveryTimer >= shieldRecoveryTime)
            {
                currentShield = maxShield;
                shieldRecoveryTimer = 0f;
                Debug.Log("护盾已恢复！");
            }
        }
    }

    public void TakeDamage(int amount)
    {
        if (currentShield > 0)
        {
            currentShield--;
            shieldRecoveryTimer = 0f;
            Debug.Log($"护盾破碎！({currentShield}/{maxShield})");
            return;
        }

        currentHealth -= amount;
        Debug.Log($"玩家受伤！HP: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
            Die();
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        Debug.Log($"恢复生命！HP: {currentHealth}/{maxHealth}");
    }

    public void RestoreShield()
    {
        currentShield = maxShield;
        shieldRecoveryTimer = 0f;
        Debug.Log("护盾已恢复！");
    }

    private int _pendingLevelUps;

    public void GainExperience(int amount)
    {
        currentXP += amount;
        Debug.Log($"经验 +{amount}  ({currentXP}/{xpToNextLevel})");

        // 一次只升一级，等玩家选完升级再触发下一级
        if (currentXP >= xpToNextLevel && level < maxLevel)
        {
            LevelUp();
        }
    }

    /// <summary>升级面板关闭后，检查是否还有待处理的升级</summary>
    public void CheckPendingLevelUp()
    {
        if (currentXP >= xpToNextLevel && level < maxLevel)
            LevelUp();
    }

    void LevelUp()
    {
        currentXP -= xpToNextLevel;
        level++;
        xpToNextLevel += 5;

        Debug.Log($"=== 升级！Lv.{level} | 下次需要 {xpToNextLevel} XP ===");
        OnLevelUp?.Invoke(level);
        EventBus.Instance?.OnPlayerLevelUp?.Invoke(level);
    }

    public void AddGold(int amount)
    {
        gold += amount;
        EventBus.Instance?.OnGoldGained?.Invoke(amount);
    }

    void Die()
    {
        Debug.Log($"游戏结束！等级: {level}  金币: {gold}");
        OnDeath?.Invoke();
        EventBus.Instance?.OnPlayerDied?.Invoke();
    }
}
