using System.Collections;
using UnityEngine;

public class AIBoatManager : MonoBehaviour
{
    private const float SPAWN_DISTANCE = 300f;
    private AIBoatManagerState state;
    private AIBoat current;

    private void Update()
    {
        if (UIManager.Instance.State == UIState.HUD && state == AIBoatManagerState.None && (current == null || current.gameObject.activeSelf == false))
        {
            state = AIBoatManagerState.Spawning;
            StartCoroutine(SpawnTimer());
        }
    }

    private IEnumerator SpawnTimer()
    {
        yield return new WaitForSeconds(3f);

        Vector3 position = FindFirstObjectByType<PlayerBoat>().transform.position;
        Vector3 random = (new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized * SPAWN_DISTANCE);
        Quaternion rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);

        current = ObjectPoolManager.Instance.Spawn<AIBoat>(position + random, rotation);
        current.SetTarget(FindFirstObjectByType<PlayerBoat>());
        state = AIBoatManagerState.None;
    }
}

public enum AIBoatManagerState
{
    None,
    Spawning
}