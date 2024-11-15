using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance { get; private set; }

    private const float SPAWN_DISTANCE = 1000f;
    private const float DESPAWN_DISTANCE = 1000f;
    private const float RING_OF_FIRE = 1000f;
    private const int MAX_BOAT_COUNT = 100;
    private const float SPAWN_COOLDOWN = 60f;

    private readonly List<AIBoatController> boats = new();

    private CombatManagerSpawnState spawnState;
    private CombatState combatState;

    private PlayerBoatController playerBoatController;

    private void Awake()
    {
        Assert.IsNull(Instance);
        Instance = this;

        playerBoatController = FindFirstObjectByType<PlayerBoatController>();
    }

    private void Update()
    {
        SpawnBoat();
        ManageBoats();
    }

    public void ManageBoats()
    {
        switch (combatState)
        {
            case CombatState.OutOfCombat:
                OutOfCombatState();
                break;

            case CombatState.InCombat:
                InCombatState();
                break;
        }
    }

    private void OutOfCombatState()
    {
        foreach (AIBoatController boat in boats)
        {
            if (Vector3.Distance(playerBoatController.transform.position, boat.transform.position) < RING_OF_FIRE)
            {
                boat.SetTarget(playerBoatController.GetComponent<Boat>());
                combatState = CombatState.InCombat;
            }

            else if (Vector3.Distance(playerBoatController.transform.position, boat.transform.position) > RING_OF_FIRE + SPAWN_DISTANCE + DESPAWN_DISTANCE)
            {
                boat.GetComponent<Boat>().Damage(999999);
            }

            else if (boat.Destination == null)
            {
                boat.SetDestination(GetPositionInSideRingOfFire());
            }
        }
    }

    private void InCombatState()
    {
        bool isInCombat = false;

        foreach (AIBoatController boat in boats)
        {
            if (boat.Target != null)
            {
                if (Vector3.Distance(boat.transform.position, boat.Target.transform.position) > RING_OF_FIRE)
                {
                    boat.SetTarget(null);
                }

                else
                {
                    isInCombat = true;
                }

                continue;
            }

            if (Vector3.Distance(playerBoatController.transform.position, boat.transform.position) < RING_OF_FIRE)
            {
                boat.SetDestination(GetClosestPositionOutSideRingOfFire(boat.transform.position));
            }

            else if (Vector3.Distance(playerBoatController.transform.position, boat.transform.position) > RING_OF_FIRE + SPAWN_DISTANCE + DESPAWN_DISTANCE)
            {
                boat.GetComponent<Boat>().Damage(999);
            }

            else if (boat.Destination == null)
            {
                boat.SetDestination(GetPositionOutSideRingOfFire());
            }
        }

        if (!isInCombat)
        {
            combatState = CombatState.OutOfCombat;
        }
    }

    private void SpawnBoat()
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
        ObjectPoolManager.Instance.Spawn<AIBoatController>(position, rotation);

        yield return new WaitForSeconds(SPAWN_COOLDOWN);

        spawnState = CombatManagerSpawnState.None;
    }

    private Vector3 GetPositionOutSideRingOfFire()
    {
        return new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized * (RING_OF_FIRE + Random.Range(0, SPAWN_DISTANCE));
    }

    private Vector3 GetClosestPositionOutSideRingOfFire(Vector3 position)
    {
        return position + ((position - playerBoatController.transform.position).normalized * RING_OF_FIRE);
    }

    private Vector3 GetPositionInSideRingOfFire()
    {
        return new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized * Random.Range(0, RING_OF_FIRE);
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

public enum CombatState
{
    OutOfCombat,
    InCombat
}
