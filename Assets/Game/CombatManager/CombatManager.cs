using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance { get; private set; }
    public static event UnityAction<Admiral> OnAdmiralInCombatChanged;

    public const float RING_OF_FIRE_SIZE = 500f;
    private const float RING_OF_FIRE_BUFFER = 500f;
    private const float GET_POSITION_INSIDE_BUFFER_OFFSET = 50f;
    private const float SPAWN_SIZE = 1000f;
    private const float SPAWN_BUFFER = 300f;

    private const int MAX_ADMIRAL_COUNT = 1;
    private const float SPAWN_COOLDOWN = 30f;
    private const float SPAWN_PAUSE_DURATION = 0.2f;
    private const int MIN_FLEET_SIZE = 10;
    private const int MAX_FLEET_SIZE = 10;
    private CombatManagerSpawnState spawnState = CombatManagerSpawnState.SpawningFirstAdmiral;

    private readonly List<EnemyAdmiralController> admirals = new();

    private PlayerBoatController player;
    private EnemyAdmiralController admiralInCombat;
    private EnemyAdmiralController admiralInRingOfFireBuffer;

    private void Awake()
    {
        Assert.IsNull(Instance);
        Instance = this;

        player = FindFirstObjectByType<PlayerBoatController>();
    }

    private void Update()
    {
        SpawnEnemyAdmiral();
        CheckRingOfFire();
    }

    private void CheckRingOfFire()
    {
        if (admiralInRingOfFireBuffer != null)
        {
            if (admiralInCombat == null && Vector3.Distance(player.transform.position, admiralInRingOfFireBuffer.transform.position) <= RING_OF_FIRE_SIZE)
            {
                admiralInCombat = admiralInRingOfFireBuffer;
                OnAdmiralInCombatChanged?.Invoke(admiralInCombat);
            }

            else if (admiralInCombat != null && Vector3.Distance(player.transform.position, admiralInCombat.transform.position) > RING_OF_FIRE_SIZE)
            {
                admiralInCombat = null;
                OnAdmiralInCombatChanged?.Invoke(admiralInCombat);
            }
        }
    }

    private void SpawnEnemyAdmiral()
    {
        if (UIManager.Instance.State == UIState.HUD && spawnState != CombatManagerSpawnState.Spawning && admirals.Count < MAX_ADMIRAL_COUNT)
        {
            Vector3 position = player.transform.position + (spawnState == CombatManagerSpawnState.SpawningFirstAdmiral ?
                GetPositionInSideRingOfFireBuffer() :
                GetPositionOutSideRingOfFire());

            spawnState = CombatManagerSpawnState.Spawning;
            StartCoroutine(SpawnTimer(position));
        }
    }

    private IEnumerator SpawnTimer(Vector3 position)
    {
        yield return new WaitForSeconds(SPAWN_PAUSE_DURATION);

        Quaternion rotation = Quaternion.LookRotation((player.transform.position - position).normalized);
        EnemyAdmiralController admiral = ObjectPoolManager.Instance.Spawn<EnemyAdmiralController>(position, rotation);

        yield return new WaitForSeconds(SPAWN_PAUSE_DURATION);

        int size = Random.Range(MIN_FLEET_SIZE, MAX_FLEET_SIZE + 1);
        Vector3[] positions = Formations.GetLine(admiral.transform.position, admiral.transform.forward, size);

        for (int i = 0; i < size; i++)
        {
            admiral.SpawnSubordinate(positions[i]);
            yield return new WaitForSeconds(SPAWN_PAUSE_DURATION);
        }

        yield return new WaitForSeconds(SPAWN_COOLDOWN);

        spawnState = CombatManagerSpawnState.None;
    }

    public bool CanEnterRingOfFire()
    {
        return admiralInRingOfFireBuffer == null;
    }

    public void EnterRingOfFire(EnemyAdmiralController _admiral)
    {
        admiralInRingOfFireBuffer = _admiral;
    }

    public void ExitRingOfFire()
    {
        admiralInRingOfFireBuffer = null;
    }

    public void AddAdmiral(EnemyAdmiralController _admiral)
    {
        if (!admirals.Contains(_admiral))
        {
            admirals.Add(_admiral);
        }
    }

    public void RemoveAdmiral(EnemyAdmiralController _admiral)
    {
        if (admirals.Contains(_admiral))
        {
            admirals.Remove(_admiral);
        }
    }

    public static Vector3 GetPositionInSideRingOfFire()
    {
        return new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized * Random.Range(0, GetRingOfFireSize());
    }

    public static Vector3 GetPositionInSideRingOfFireBuffer()
    {
        return new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized * (RING_OF_FIRE_SIZE + GET_POSITION_INSIDE_BUFFER_OFFSET + Random.Range(0, RING_OF_FIRE_BUFFER));
    }

    public static Vector3 GetPositionOutSideRingOfFire()
    {
        return new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized * (GetRingOfFireSize() + Random.Range(0, SPAWN_SIZE));
    }

    public static Vector3 GetClosestPositionOutSideRingOfFire(Vector3 position)
    {
        return position + ((position - PlayerBoatController.Instance.transform.position).normalized * GetRingOfFireSize());
    }

    public static float GetRingOfFireSize()
    {
        return RING_OF_FIRE_SIZE + RING_OF_FIRE_BUFFER;
    }

    public static float GetMapSize()
    {
        return RING_OF_FIRE_SIZE + RING_OF_FIRE_BUFFER + SPAWN_SIZE + SPAWN_BUFFER;
    }
}

public enum CombatManagerSpawnState
{
    None,
    Spawning,
    SpawningFirstAdmiral
}
