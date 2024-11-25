using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class NavigationManager : MonoBehaviour
{
    private const int NODE_SIZE = 10;
    private const int GRID_SIZE = 2000;

    public static NavigationManager Instance { get; private set; }

    public bool[,] Grid = new bool[10, 10];

    public readonly List<NavigationObstacle> obstacles = new();

    private void Awake()
    {
        Assert.IsNull(Instance);
        Instance = this;
    }

    public void UpdateGrid()
    {


        foreach(NavigationObstacle obstacle in obstacles)
        {

        }
    }

    public void AddObstacle(NavigationObstacle _navigationObstacle)
    {
        obstacles.Add(_navigationObstacle);
    }

    public void RemoveObstacle(NavigationObstacle _navigationObstacle)
    {
        obstacles.Remove(_navigationObstacle);
    }
}
