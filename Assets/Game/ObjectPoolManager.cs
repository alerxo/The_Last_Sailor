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
        aiBoatPool = new(CreateAIBoat);
        cannonballBool = new(CreateCannonball);
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

    public void ReleaseCannonball(Cannonball _cannonball)
    {
        cannonballBool.Release(_cannonball);
    }

    #endregion
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
        pool.Enqueue(_t);
    }
}