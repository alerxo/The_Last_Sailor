using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance { get; private set; }

    public const float RING_OF_FIRE = 1000f;
    public const float SPAWN_DISTANCE = 1000f;
    public const float DESPAWN_DISTANCE = 1000f;

    private const int MAX_BOAT_COUNT = 5;
    private const float SPAWN_COOLDOWN = 30f;

    private readonly List<AIBoatController> boats = new();

    private EnemyAdmiralController admiralInCombat;

    private CombatManagerSpawnState spawnState;

    private PlayerBoatController playerBoatController;

    private void Awake()
    {
        Assert.IsNull(Instance);
        Instance = this;

        playerBoatController = FindFirstObjectByType<PlayerBoatController>();
    }

    private void Update()
    {
        SpawnEnemyAdmiral();
    }

    private void SpawnEnemyAdmiral()
    {
        if (UIManager.Instance.State == UIState.HUD && spawnState == CombatManagerSpawnState.None && boats.Count < MAX_BOAT_COUNT)
        {
            spawnState = CombatManagerSpawnState.Spawning;
            StartCoroutine(SpawnTimer());
        }
    }

    private IEnumerator SpawnTimer()
    {
        yield return new WaitForSeconds(3f);

        Vector3 position = playerBoatController.transform.position + GetPositionOutSideRingOfFire();
        Quaternion rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
        ObjectPoolManager.Instance.Spawn<EnemyAdmiralController>(position, rotation);

        yield return new WaitForSeconds(SPAWN_COOLDOWN);

        spawnState = CombatManagerSpawnState.None;
    }


    public static Vector3 GetPositionOutSideRingOfFire()
    {
        return new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized * (RING_OF_FIRE + Random.Range(0, SPAWN_DISTANCE));
    }

    public static Vector3 GetClosestPositionOutSideRingOfFire(Vector3 position)
    {
        return position + ((position - PlayerBoatController.Instance.transform.position).normalized * RING_OF_FIRE);
    }

    public static Vector3 GetPositionInSideRingOfFire()
    {
        return new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized * Random.Range(0, RING_OF_FIRE);
    }


    public bool CanEnterCombat()
    {
        return admiralInCombat == null;
    }

    public void EnterCombat(EnemyAdmiralController _admiral)
    {
        admiralInCombat = _admiral;
    }

    public void ExitCombat(EnemyAdmiralController _admiral)
    {
        admiralInCombat = null;
    }


    public void AddBoat(AIBoatController _boat)
    {
        if (!boats.Contains(_boat))
        {
            boats.Add(_boat);
        }
    }

    public void RemoveBoat(AIBoatController _boat)
    {
        if (boats.Contains(_boat))
        {
            boats.Remove(_boat);
        }
    }
}

public enum CombatManagerSpawnState
{
    None,
    Spawning
}
