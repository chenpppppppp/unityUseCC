using UnityEngine;

/// <summary>
/// 对象池接口 — 实现此接口的 MonoBehaviour 可被对象池管理
/// </summary>
public interface IPoolable
{
    void OnSpawn();    // 从池取出时调用，重置状态
    void OnDespawn();  // 归还池时调用，清理现场
}
