using UnityEngine;

public class CannonBallEasterEgg : MonoBehaviour
{
    [SerializeField] private Transform egg;
    public void SpawnCannonBallEasterEgg()
    {
        egg.gameObject.SetActive(true);
    }
}
