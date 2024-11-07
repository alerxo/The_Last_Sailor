using System.Collections;
using UnityEngine;

public class AIBoatManager : MonoBehaviour
{
    private AIBoatManagerState state;
    private AIBoat current;

    private void Update()
    {
        if (UIManager.Instance.State == UIState.Game && state == AIBoatManagerState.None && (current == null || current.gameObject.activeSelf == false))
        {
            state = AIBoatManagerState.Spawning;
            StartCoroutine(SpawnTimer());
        }
    }

    private IEnumerator SpawnTimer()
    {
        yield return new WaitForSeconds(3f);

        Vector3 position = FindFirstObjectByType<PlayerBoat>().transform.position + new Vector3(GetRangomCoordinate(), -10, GetRangomCoordinate());
        Quaternion rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
        current = ObjectPoolManager.Instance.Spawn<AIBoat>(position, rotation);
        state = AIBoatManagerState.None;
    }

    private float GetRangomCoordinate()
    {
        return Random.Range(50, 75) * (Random.Range(0, 2) == 0 ? 1 : -1);
    }
}

public enum AIBoatManagerState
{
    None,
    Spawning
}