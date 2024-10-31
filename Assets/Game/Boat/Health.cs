using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] private float startHealth;
    private float health;

    private void Awake()
    {
        health = startHealth;
    }

    public void Damage(float amount)
    {
        health -= amount;

        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }
}
