using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance { get; private set; }
    public static event UnityAction<BattleResult> OnBattleConcluded;
    public static event UnityAction<Admiral> OnAdmiralInCombatChanged;

    public const float RING_OF_FIRE_SIZE = 700f;
    private const float RING_OF_FIRE_BUFFER = 300f;
    private const float SPAWN_SIZE = 500f;
    private const float SPAWN_BUFFER = 100f;

    private const int MAX_ADMIRAL_COUNT = 3;
    private const float SPAWN_COOLDOWN = 5f;
    private const float SPAWN_PAUSE_DURATION = 0.1f;
    private const int MIN_FLEET_SIZE = 1;
    private const int MAX_FLEET_SIZE = 1;
    private CombatManagerSpawnState spawnState = CombatManagerSpawnState.SpawningFirstAdmiral;

    private readonly List<EnemyAdmiralController> admirals = new();

    private PlayerBoatController player;
    public EnemyAdmiralController AdmiralInCombat { get; private set; }
    public EnemyAdmiralController AdmiralInRingOfFireBuffer { get; private set; }
    private CombatState combatState;

    private void Awake()
    {
        Assert.IsNull(Instance);
        Instance = this;

        player = FindFirstObjectByType<PlayerBoatController>();
    }

    private void Update()
    {
        TrySpawnEnemyAdmiral();

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
        if (AdmiralInRingOfFireBuffer != null && AdmiralInCombat == null && Vector3.Distance(player.transform.position, AdmiralInRingOfFireBuffer.transform.position) <= RING_OF_FIRE_SIZE)
        {
            EnterCombat();
        }
    }

    private void InCombatState()
    {
        if (AdmiralInRingOfFireBuffer != null && AdmiralInCombat != null &&
            (Vector3.Distance(player.transform.position, AdmiralInCombat.transform.position) > RING_OF_FIRE_SIZE ||
            player.AdmiralController.Fleet.All((b) => b.IsSunk) ||
            AdmiralInCombat.Fleet.All((b) => b.IsSunk)))
        {
            UIManager.Instance.SetState(UIState.PostCombat);
            FirstPersonController.Instance.SetState(PlayerState.Inactive);
            CameraManager.Instance.SetState(CameraState.Command);

            OnBattleConcluded?.Invoke(GetBattleResult(AdmiralInCombat));
            combatState = CombatState.PostCombat;

            Time.timeScale = 0.3f;
        }
    }

    public BattleResult GetBattleResult(EnemyAdmiralController _enemyAdmiral)
    {
        if (PlayerBoatController.Instance.AdmiralController.Fleet.All((b) => b.IsSunk))
        {
            return BattleResult.Defeat;
        }

        else if (!_enemyAdmiral.AIBoatController.Boat.IsSunk)
        {
            return BattleResult.Inconclusive;
        }

        return BattleResult.Victory;
    }

    public void BattleResultsCompleted()
    {
        EnemyAdmiralController admiral = AdmiralInCombat;

        foreach (AIBoatController boatController in player.AdmiralController.Subordinates.ToArray())
        {
            if (boatController.Boat.IsSunk && boatController.State == AIBoatControllerState.Active)
            {
                boatController.SinkToBottom();
            }
        }

        foreach (Boat boat in AdmiralInCombat.Fleet.ToArray())
        {
            AIBoatController boatController = boat.GetComponent<AIBoatController>();

            if (boatController.Boat.IsSunk && boatController.State == AIBoatControllerState.Active)
            {
                boatController.SinkToBottom();
            }
        }

        UIManager.Instance.SetState(UIState.HUD);
        FirstPersonController.Instance.SetState(PlayerState.FirstPerson);
        CameraManager.Instance.SetState(CameraState.Player);

        ExitCombat();

        if (admiral.AIBoatController == null)
        {
            ExitRingOfFire();
        }

        Time.timeScale = 1f;
    }

    public void EnterCombat()
    {
        AdmiralInCombat = AdmiralInRingOfFireBuffer;
        AdmiralInCombat.SetEnemy(player.AdmiralController);
        player.AdmiralController.SetEnemy(AdmiralInCombat);
        OnAdmiralInCombatChanged?.Invoke(AdmiralInCombat);
        combatState = CombatState.InCombat;
    }

    public void ExitCombat()
    {
        AdmiralInCombat.SetEnemy(null);
        player.AdmiralController.SetEnemy(null);
        AdmiralInCombat = null;
        OnAdmiralInCombatChanged?.Invoke(AdmiralInCombat);
        combatState = CombatState.OutOfCombat;
    }

    public bool CanEnterRingOfFire()
    {
        return AdmiralInRingOfFireBuffer == null;
    }

    public void EnterRingOfFire(EnemyAdmiralController _admiral)
    {
        Assert.IsNull(AdmiralInRingOfFireBuffer);
        AdmiralInRingOfFireBuffer = _admiral;
    }

    public void ExitRingOfFire()
    {
        Assert.IsNotNull(AdmiralInRingOfFireBuffer);
        AdmiralInRingOfFireBuffer = null;
    }

    private void TrySpawnEnemyAdmiral()
    {
        if (UIManager.Instance.State == UIState.HUD && spawnState != CombatManagerSpawnState.Spawning && admirals.Count < MAX_ADMIRAL_COUNT)
        {
            Vector3 position = spawnState == CombatManagerSpawnState.SpawningFirstAdmiral ?
                GetPositionInSideRingOfFireBuffer() :
                GetPositionOutSideRingOfFire();

            spawnState = CombatManagerSpawnState.Spawning;
            StartCoroutine(SpawnTimer(position));
        }
    }

    private IEnumerator SpawnTimer(Vector3 position)
    {
        yield return new WaitForSeconds(SPAWN_PAUSE_DURATION);

        Quaternion rotation = Quaternion.LookRotation((player.transform.position - position).normalized);
        AIBoatController admiralBoat = ObjectPoolManager.Instance.Spawn<AIBoatController>(position, rotation);
        EnemyAdmiralController admiralController = admiralBoat.PromoteToAdmiral();

        yield return new WaitForSeconds(SPAWN_PAUSE_DURATION);

        int size = Random.Range(MIN_FLEET_SIZE, MAX_FLEET_SIZE + 1);
        Vector3[] positions = Formations.GetLine(size);

        for (int i = 0; i < size; i++)
        {
            admiralController.SpawnSubordinate(admiralBoat.transform.position + admiralBoat.transform.TransformVector(positions[i]));
            yield return new WaitForSeconds(SPAWN_PAUSE_DURATION);
        }

        admiralController.GetRandomFormation();
        admiralController.SetFleetFormation();

        yield return new WaitForSeconds(SPAWN_COOLDOWN);

        spawnState = CombatManagerSpawnState.None;
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
        return PlayerBoatController.Instance.transform.position + new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized * Random.Range(0, GetRingOfFireSize());
    }

    public static Vector3 GetPositionInSideRingOfFireBuffer()
    {
        return PlayerBoatController.Instance.transform.position + new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized * Random.Range(RING_OF_FIRE_BUFFER, GetRingOfFireSize());
    }

    public static Vector3 GetPositionOutSideRingOfFire()
    {
        return PlayerBoatController.Instance.transform.position + new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized * Random.Range(GetRingOfFireSize(), GetSpawnSize());
    }

    public static Vector3 GetClosestPositionOutSideRingOfFire(Vector3 position)
    {
        return position + ((position - PlayerBoatController.Instance.transform.position).normalized * GetRingOfFireSize());
    }

    public static float GetRingOfFireSize()
    {
        return RING_OF_FIRE_SIZE + RING_OF_FIRE_BUFFER;
    }

    public static float GetSpawnSize()
    {
        return GetRingOfFireSize() + SPAWN_SIZE;
    }

    public static float GetMapSize()
    {
        return GetSpawnSize() + SPAWN_BUFFER;
    }
}

public enum CombatManagerSpawnState
{
    None,
    Spawning,
    SpawningFirstAdmiral
}

public enum CombatState
{
    OutOfCombat,
    InCombat,
    PostCombat
}

public enum BattleResult
{
    Defeat,
    Victory,
    Inconclusive
}