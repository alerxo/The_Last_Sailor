using UnityEngine;

public class NavigationObstacle : MonoBehaviour
{
    public Vector2Int Position {  get; private set; }

    private void OnEnable()
    {
        NavigationManager.Instance.AddObstacle(this);
    }

    private void OnDisable()
    {
        NavigationManager.Instance.RemoveObstacle(this);
    }

    public void SetPosition()
    {

    }
}
