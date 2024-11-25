using UnityEngine;
using UnityEngine.Assertions;

public class NavigationObstacle : MonoBehaviour
{
    public Vector2Int Position { get; private set; }

    private bool isOccupying = false;

    private void OnEnable()
    {
        TryStartOccupying();
    }

    private void OnDisable()
    {
        TryStopOccupying();
    }

    private void Update()
    {
        if (isOccupying)
        {
            SetPosition();
        }
    }

    public void TryStartOccupying()
    {
        if (NavigationManager.Instance != null)
        {
            Position = NavigationManager.Instance.WorldToGrid(transform.position);
            NavigationManager.Instance.AddObstacle(this, Position);
            isOccupying = true;
        }
    }

    public void TryStopOccupying()
    {
        if (NavigationManager.Instance != null)
        {
            NavigationManager.Instance.RemoveObstacle(this, Position);
            isOccupying = false;
        }
    }

    public void SetPosition()
    {
        Vector2Int newPosition = NavigationManager.Instance.WorldToGrid(transform.position);

        if (newPosition != Position)
        {
            NavigationManager.Instance.UpdateGrid(Position, newPosition);
            Position = newPosition;
        }
    }
}
