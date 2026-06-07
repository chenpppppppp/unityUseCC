using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 泛型对象池
/// T 必须继承 MonoBehaviour 并实现 IPoolable
/// </summary>
public class ObjectPool<T> where T : MonoBehaviour, IPoolable
{
    private readonly Queue<T> _available = new Queue<T>();
    private readonly System.Func<T> _createFunc;
    private readonly Transform _container;
    private readonly int _prewarm;

    public int ActiveCount { get; private set; }
    public int AvailableCount => _available.Count;
    public int TotalCount => ActiveCount + AvailableCount;

    public ObjectPool(System.Func<T> createFunc, int prewarm = 5, string containerName = null)
    {
        _createFunc = createFunc;
        _prewarm = prewarm;

        _container = new GameObject(containerName ?? $"Pool_{typeof(T).Name}").transform;
        _container.gameObject.SetActive(false);

        for (int i = 0; i < prewarm; i++)
        {
            T obj = CreateNew();
            _available.Enqueue(obj);
        }
    }

    private T CreateNew()
    {
        T obj = _createFunc();
        obj.transform.SetParent(_container);
        obj.gameObject.SetActive(false);
        return obj;
    }

    public T Get()
    {
        T obj;
        if (_available.Count > 0)
        {
            obj = _available.Dequeue();
        }
        else
        {
            obj = CreateNew();
            Debug.Log($"[Pool] {typeof(T).Name} 池扩容, 当前总数: {TotalCount + 1}");
        }

        obj.gameObject.SetActive(true);
        obj.transform.SetParent(null);
        obj.OnSpawn();
        ActiveCount++;
        return obj;
    }

    public void Release(T obj)
    {
        if (obj == null) return;

        obj.OnDespawn();
        obj.gameObject.SetActive(false);
        obj.transform.SetParent(_container);
        _available.Enqueue(obj);
        ActiveCount--;
    }

    /// <summary>回收所有活跃对象</summary>
    public void ReleaseAll()
    {
        // 安全：先收集再释放（避免迭代中修改）
        T[] all = Object.FindObjectsOfType<T>();
        foreach (T obj in all)
        {
            if (obj.gameObject.activeSelf && obj.transform.parent != _container)
                Release(obj);
        }
    }
}
