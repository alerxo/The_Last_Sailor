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

    private const float CALM_DURATION_BASE = 30f;
    private const float CALM_DURATION_PER_ROUND = 3f;
    private float GetCalmStateTimer => CALM_DURATION_BASE + (CALM_DURATION_PER_ROUND * Mathf.Clamp(Round, 0, 10));

    public const float RING_OF_FIRE_SIZE = 750f;
    private const float RING_OF_FIRE_BUFFER_BASE = 500f;
    private const float RING_OF_FIRE_BUFFER_PER_ROUND = 125f;
    private const float DE_SPAWN_BUFFER_BASE = 100f;

    public static readonly int[] ENEMY_FLEET_SIZES = { 0, 0, 1, 2, 3, 5, 7, 9, 12, 16 };
    public static readonly int[] ROUND_RESOURCE_WORTH = { 15, 15, 25, 35, 50, 55, 75, 80, 95, 125 };

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
        stateTimer = GetCalmStateTimer;
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
        Vector3 origin = BuoyancyManager.Instance.GetPointOnWater(GetSpawnPosition());
        Quaternion rotation = Quaternion.LookRotation((player.transform.position - origin).normalized);

        AIBoatController admiralBoat = ObjectPoolManager.Instance.Spawn<AIBoatController>(origin, rotation);

        admiralBoat.LerpSize();
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
            if (stateTimer < CALM_DURATION_BASE)
            {
                stateTimer = CALM_DURATION_BASE;
            }
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
            if (stateTimer < CALM_DURATION_BASE)
            {
                stateTimer = CALM_DURATION_BASE;
            }

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

    public Vector3 GetPositionOutSideRingOfFire()
    {
        return PlayerBoatController.Instance.transform.position + new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized * Random.Range(GetRingOfFireSize(), GetRingOfFireWithBuffer());
    }

    public Vector3 GetSpawnPosition()
    {
        return Round == 0 ? PlayerBoatController.Instance.transform.position + new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized * GetRingOfFireSize()
            : PlayerBoatController.Instance.transform.position + new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized * GetRingOfFireWithBuffer();
    }

    public Vector3 GetClosestPositionOutSideRingOfFire(Vector3 position)
    {
        return PlayerBoatController.Instance.transform.position + ((position - PlayerBoatController.Instance.transform.position).normalized * GetRingOfFireSize());
    }

    public static float GetRingOfFireSize()
    {
        return RING_OF_FIRE_SIZE;
    }

    public float GetRingOfFireWithBuffer()
    {
        return RING_OF_FIRE_SIZE + RING_OF_FIRE_BUFFER_BASE + (RING_OF_FIRE_BUFFER_PER_ROUND * Mathf.Clamp(Round, 0, 10));
    }

    public static float GetMapSize()
    {
        return RING_OF_FIRE_SIZE + RING_OF_FIRE_BUFFER_BASE + (RING_OF_FIRE_BUFFER_PER_ROUND * 10) + DE_SPAWN_BUFFER_BASE;
    }

    public int GetRoundResourceWorth()
    {
        return ROUND_RESOURCE_WORTH[Mathf.Clamp(Round, 0, 10)];
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