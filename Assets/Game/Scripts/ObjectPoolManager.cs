using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class ObjectPoolManager : MonoBehaviour
{
    public static ObjectPoolManager Instance { get; private set; }

    public static readonly Vector3 InactiveObjectPosition = new(0f, 1000f, 0f);
    private readonly Dictionary<Type, ObjectPool<MonoBehaviour>> pools = new();

    [SerializeField] private List<MonoBehaviour> prefabs;
    private Transform parent;

    private void Awake()
    {
        Assert.IsNull(Instance);
        Instance = this;

        parent = GameObject.FindWithTag("ObjectPool").transform;
    }

    public T Spawn<T>(Vector3 _position, Quaternion _rotation, Transform _parent = null) where T : MonoBehaviour
    {
        if (!pools.ContainsKey(typeof(T)))
        {
            pools[typeof(T)] = new(() => Instantiate(prefabs.Find((m) => m is T), InactiveObjectPosition, Quaternion.identity, parent) as T);
        }

        T t = pools[typeof(T)].Get() as T;

        if (_parent != null)
        {
            t.transform.SetParent(_parent);
        }

        if (t.TryGetComponent(out Rigidbody rigidbody))
        {
            rigidbody.position = _position;
            rigidbody.rotation = _rotation;
        }

        else
        {
            t.transform.SetPositionAndRotation(_position, _rotation);
        }

        return t;
    }

    public void Release<T>(T _t) where T : MonoBehaviour
    {
        if (_t.transform.parent != parent)
        {
            _t.transform.SetParent(parent);
        }

        pools[typeof(T)].Release(_t);
    }
}

public class ObjectPool<T> where T : MonoBehaviour
{
    private readonly Func<T> create;
    private readonly Queue pool = new();

    public ObjectPool(Func<T> _create)
    {
        create = _create;
    }

    public T Get()
    {
        T t = pool.Count == 0 ? create() : (T)pool.Dequeue();
        t.gameObject.SetActive(true);

        return t;
    }

    public void Release(T _t)
    {
        _t.gameObject.SetActive(false);
        _t.transform.SetPositionAndRotation(ObjectPoolManager.InactiveObjectPosition, Quaternion.identity);

        pool.Enqueue(_t);
    }
}