using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;

public class ObjectPoolManager : MonoBehaviour
{
    public static ObjectPoolManager Instance { get; private set; }

    [SerializeField] private AIBoat aiBoatPrefab;
    [SerializeField] private Cannonball cannonballPrefab;
    [SerializeField] private Transform aiBoatParent;
    [SerializeField] private Transform cannonballParent;
    private ObjectPool<AIBoat> aiBoatPool;
    private ObjectPool<Cannonball> cannonballBool;

    private static readonly Vector3 InactiveObjectPosition = new(0f, -100f, 0f);

    private void Awake()
    {
        Assert.IsNull(Instance);
        Instance = this;
    }

    private void Start()
    {
        aiBoatPool = new(CreateAIBoat, OnGetAIBoat, OnReleaseAIBoat);
        cannonballBool = new(CreateCannonball, OnGetCannonball, OnReleaseCannonball);
    }

    #region AIBoat

    public AIBoat SpawnAIBoat(Vector3 _position, Quaternion _rotation)
    {
        AIBoat aiBoat = aiBoatPool.Get();
        aiBoat.GetComponent<Rigidbody>().position = _position;
        aiBoat.GetComponent<Rigidbody>().rotation = _rotation;

        return aiBoat;
    }

    private AIBoat CreateAIBoat()
    {
        return Instantiate(aiBoatPrefab, InactiveObjectPosition, Quaternion.identity, aiBoatParent);
    }

    private void OnGetAIBoat(AIBoat _aiBoat)
    {
        _aiBoat.gameObject.SetActive(true);
    }

    private void OnReleaseAIBoat(AIBoat _aiBoat)
    {
        _aiBoat.gameObject.SetActive(false);
    }

    public void ReleaseAIBoat(AIBoat _aiBoat)
    {
        aiBoatPool.Release(_aiBoat);
    }

    #endregion

    #region CannonBall

    public Cannonball SpawnCannonball(Vector3 _position, Quaternion _rotation)
    {
        Cannonball cannonball = cannonballBool.Get();
        cannonball.GetComponent<Rigidbody>().position = _position;
        cannonball.GetComponent<Rigidbody>().rotation = _rotation;

        return cannonball;
    }

    private Cannonball CreateCannonball()
    {
        return Instantiate(cannonballPrefab, InactiveObjectPosition, Quaternion.identity, cannonballParent);
    }

    private void OnGetCannonball(Cannonball _cannonball)
    {
        _cannonball.gameObject.SetActive(true);
    }

    private void OnReleaseCannonball(Cannonball _cannonball)
    {
        _cannonball.gameObject.SetActive(false);
    }

    public void ReleaseCannonball(Cannonball _cannonball)
    {
        cannonballBool.Release(_cannonball);
    }

    #endregion
}


public class ObjectPool<T>
{
    private readonly Func<T> create;
    private readonly Action<T> get;
    private readonly Action<T> release;
    private readonly Queue pool = new();

    public ObjectPool(Func<T> _create, Action<T> _get, Action<T> _release)
    {
        create = _create;
        get = _get;
        release = _release;
    }

    public T Get()
    {
        T t = pool.Count == 0 ? create() : (T)pool.Dequeue();
        get(t);

        return t;
    }

    public void Release(T _t)
    {
        release(_t);
        pool.Enqueue(_t);
    }
}