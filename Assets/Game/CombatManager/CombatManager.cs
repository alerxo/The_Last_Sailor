using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance { get; private set; }
    public static event UnityAction<BattleResult> OnBattleConcluded;
    public static event UnityAction<Admiral> OnAdmiralInCombatChanged;

    public CombatManagerState State { get; private set; } = CombatManagerState.None;
    private float stateTimer = 0;
    private const float CALM_DURATION = 10f;

    public const float RING_OF_FIRE_SIZE = 750f;
    private const float RING_OF_FIRE_BUFFER_SIZE_PER_SHIP = 100f;
    private const float DE_SPAWN_SIZE = RING_OF_FIRE_BUFFER_SIZE_PER_SHIP * 20;

    public static readonly int[] ENEMY_FLEET_SIZES = { 0, 0, 1, 2, 3, 5, 7, 9, 12, 16 };
    public int Round { get; private set; } = 0;

    private PlayerBoatController player;
    public EnemyAdmiralController Enemy { get; private set; }

    private void Awake()
    {
        Assert.IsNull(Instance);
        Instance = this;

        player = FindFirstObjectByType<PlayerBoatController>();
    }

    private void Update()
    {
        switch (State)
        {
            case CombatManagerState.Spawning:
                SpawningState();
                break;

            case CombatManagerState.Calm:
                CalmState();
                break;

            case CombatManagerState.PreCombat:
                PreCombatState();
                break;

            case CombatManagerState.Combat:
                CombatState();
                break;

            case CombatManagerState.PostCombat:
                break;
        }
    }

    public void EnableSpawning()
    {
        State = CombatManagerState.Spawning;
    }

    private void SpawningState()
    {
        StartCoroutine(SpawnTimer());

        Round++;
        stateTimer = CALM_DURATION;
        State = CombatManagerState.Calm;
    }

    private int GetEnemyFleetSize()
    {
        if (Round < ENEMY_FLEET_SIZES.Length)
        {
            return ENEMY_FLEET_SIZES[Round];
        }

        return player.AdmiralController.Fleet.Count;
    }

    private IEnumerator SpawnTimer()
    {
        int size = GetEnemyFleetSize();
        Vector3 origin = GetSpawnPosition(size);
        Quaternion rotation = Quaternion.Euler(0, Random.Range(-180f, 180f), 0);

        AIBoatController admiralBoat = ObjectPoolManager.Instance.Spawn<AIBoatController>(origin, rotation);
        Enemy = admiralBoat.PromoteToAdmiral();
        Enemy.GetRandomFormation();

        yield return null;

        Vector3[] positions = Enemy.GetFormationPositions(size);

        for (int i = 0; i < size; i++)
        {
            Enemy.SpawnSubordinate(positions[i], rotation);

            yield return null;
        }
    }

    private void CalmState()
    {
        if (UIManager.Instance.GetState() == UIState.Fleet || UIManager.Instance.GetState() == UIState.Formation)
        {
            stateTimer = CALM_DURATION;
        }

        if ((stateTimer -= Time.deltaTime) <= 0)
        {
            State = CombatManagerState.PreCombat;
            Enemy.AIBoatController.ClearTrail();
        }
    }

    private void PreCombatState()
    {
        if (!CanEnterCombat())
        {
            stateTimer = CALM_DURATION;
            State = CombatManagerState.Calm;
        }

        if (Vector3.Distance(player.transform.position, Enemy.transform.position) <= GetRingOfFireSize())
        {
            player.AdmiralController.SetEnemy(Enemy);
            Enemy.SetEnemy(player.AdmiralController);
            OnAdmiralInCombatChanged?.Invoke(Enemy);

            Enemy.AIBoatController.ClearTrail();
            State = CombatManagerState.Combat;
        }
    }

    public bool CanEnterCombat()
    {
        return UIManager.Instance.GetState() != UIState.Fleet && UIManager.Instance.GetState() != UIState.Formation && State == CombatManagerState.PreCombat;
    }

    private void CombatState()
    {
        if (player.AdmiralController.Fleet.All((b) => b.IsSunk) || Enemy.Fleet.All((b) => b.IsSunk))
        {
            UIManager.Instance.SetState(UIState.PostCombat);
            FirstPersonController.Instance.SetState(PlayerState.Inactive);
            CameraManager.Instance.SetState(CameraState.Formation);

            OnBattleConcluded?.Invoke(GetBattleResult());

            State = CombatManagerState.PostCombat;
        }
    }

    public BattleResult GetBattleResult()
    {
        if (PlayerBoatController.Instance.AdmiralController.Fleet.All((b) => b.IsSunk))
        {
            return BattleResult.Defeat;
        }

        if (Round == ENEMY_FLEET_SIZES.Length)
        {
            return BattleResult.BossDefeated;
        }

        return BattleResult.Victory;
    }

    public void BattleResultsCompleted()
    {
        foreach (AIBoatController boatController in player.AdmiralController.Subordinates.ToArray())
        {
            if (boatController.Boat.IsSunk && boatController.State == AIBoatControllerState.Active)
            {
                boatController.SinkToBottom();
            }
        }

        foreach (Boat boat in Enemy.Fleet.ToArray())
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

        Enemy.SetEnemy(null);
        player.AdmiralController.SetEnemy(null);
        OnAdmiralInCombatChanged?.Invoke(null);
        Enemy = null;

        State = CombatManagerState.Spawning;

        switch (Round)
        {
            case 1:
                HUDScreen.Instance.AddObjective(ObjectiveType.RepairShip);
                break;

            case 2:
                HUDScreen.Instance.AddObjective(ObjectiveType.UpgradeShip);
                break;

            case 3:
                HUDScreen.Instance.AddObjective(ObjectiveType.BuildShip);
                break;
        }
    }

    public void ForceSinkEnemy()
    {
        foreach (Boat boat in Enemy.Fleet.ToArray())
        {
            boat.GetComponent<AIBoatController>().SinkToBottom();
        }

        Enemy.SetEnemy(null);
        player.AdmiralController.SetEnemy(null);
        OnAdmiralInCombatChanged?.Invoke(null);
        Enemy = null;
        Round--;

        State = CombatManagerState.Spawning;
    }

    public static Vector3 GetPositionInSideRingOfFire()
    {
        return PlayerBoatController.Instance.transform.position + new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized * Random.Range(0, GetRingOfFireSize());
    }

    public static Vector3 GetPositionOutSideRingOfFire()
    {
        return PlayerBoatController.Instance.transform.position + new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized * Random.Range(GetRingOfFireSize(), GetRingOfFireWithBuffer(1));
    }

    public static Vector3 GetSpawnPosition(int _fleetSize)
    {
        return PlayerBoatController.Instance.transform.position + new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized * GetRingOfFireWithBuffer(_fleetSize);
    }

    public static Vector3 GetClosestPositionOutSideRingOfFire(Vector3 position)
    {
        return PlayerBoatController.Instance.transform.position + ((position - PlayerBoatController.Instance.transform.position).normalized * GetRingOfFireWithBuffer(1));
    }

    public static float GetRingOfFireSize()
    {
        return RING_OF_FIRE_SIZE;
    }

    public static float GetRingOfFireWithBuffer(int _fleetSize)
    {
        return RING_OF_FIRE_SIZE + (RING_OF_FIRE_BUFFER_SIZE_PER_SHIP * _fleetSize);
    }

    public static float GetMapSize()
    {
        return RING_OF_FIRE_SIZE + DE_SPAWN_SIZE;
    }

    public int GetDifficulty()
    {
        return Mathf.RoundToInt(Mathf.Lerp(0, 5, (float)Round / ENEMY_FLEET_SIZES.Length));
    }
}

public enum CombatManagerState
{
    None,
    Spawning,
    Calm,
    PreCombat,
    Combat,
    PostCombat
}

public enum BattleResult
{
    Defeat,
    Victory,
    BossDefeated
}