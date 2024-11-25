using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class NavigationManager : MonoBehaviour
{
    public const int NODE_SIZE = 25;
    private const int GRID_SIZE_IN_NODES = 200;
    public static readonly Vector2Int WORLD_POSITION_OFFSET = new(GRID_SIZE_IN_NODES / 2, GRID_SIZE_IN_NODES / 2);

    public static NavigationManager Instance { get; private set; }

    public byte[,] Grid = new byte[GRID_SIZE_IN_NODES, GRID_SIZE_IN_NODES];

    public readonly List<NavigationObstacle> obstacles = new();

    private Transform player;

#if UNITY_EDITOR
    [SerializeField] private bool isDebugMode;
#endif

    private void Awake()
    {
        Assert.IsNull(Instance);
        Instance = this;

        player = FindFirstObjectByType<PlayerBoatController>().transform;
        player.GetComponent<NavigationObstacle>().TryStartOccupying();
    }

    private void Update()
    {
#if UNITY_EDITOR
        if (isDebugMode)
        {
            DebugDraw();
        }
#endif
    }

    private void DebugDraw()
    {
        for (int x = 0; x < GRID_SIZE_IN_NODES; x++)
        {
            for (int y = 0; y < GRID_SIZE_IN_NODES; y++)
            {
                if (IsOccupied(x, y))
                {
                    DebugUtil.DrawBox(GridToWorld(x, y), Quaternion.identity, Vector3.one * NODE_SIZE, Color.red, Time.deltaTime);
                }
            }
        }
    }

    public void AddObstacle(NavigationObstacle _navigationObstacle, Vector2Int _occupied)
    {
        if (!obstacles.Contains(_navigationObstacle))
        {
            obstacles.Add(_navigationObstacle);
            OccupyNode(_occupied);
        }
    }

    public void RemoveObstacle(NavigationObstacle _navigationObstacle, Vector2Int _clear)
    {
        if (obstacles.Contains(_navigationObstacle))
        {
            obstacles.Remove(_navigationObstacle);
            ClearNode(_clear);
        }
    }

    public void UpdateGrid(Vector2Int _clear, Vector2Int _occupied)
    {
        ClearNode(_clear);
        OccupyNode(_occupied);
    }

    private void OccupyNode(Vector2Int _position)
    {
        if (IsValidNodePosition(_position))
        {
            Grid[_position.x, _position.y]++;
        }
    }

    private void ClearNode(Vector2Int _position)
    {
        if (IsValidNodePosition(_position) && IsOccupied(_position))
        {
            Grid[_position.x, _position.y]--;
        }
    }

    public Vector2Int WorldToGrid(Vector3 _position)
    {
        _position -= player.position;
        Vector2Int gridPosition = new((int)(_position.x / NODE_SIZE), (int)(_position.z / NODE_SIZE));

        return gridPosition + WORLD_POSITION_OFFSET;
    }

    private Vector3 GridToWorld(int _x, int _y)
    {
        return GridToWorld(new Vector2Int(_x, _y));
    }

    private Vector3 GridToWorld(Vector2Int _position)
    {
        _position -= WORLD_POSITION_OFFSET;
        Vector3 worldPosition = new(_position.x * NODE_SIZE, 0, _position.y * NODE_SIZE);

        return worldPosition + player.transform.position;
    }

    public bool IsOccupied(Vector2Int _position)
    {
        return IsOccupied(_position.x, _position.y);
    }

    public bool IsOccupied(int _x, int _y)
    {
        return IsValidNodePosition(_x, _y) && Grid[_x, _y] > 0;
    }

    public bool IsValidNodePosition(Vector2Int _position)
    {
        return IsValidNodePosition(_position.x, _position.y);
    }

    public bool IsValidNodePosition(int _x, int _y)
    {
        return _x >= 0 && _x < GRID_SIZE_IN_NODES && _y >= 0 && _y < GRID_SIZE_IN_NODES;
    }
}