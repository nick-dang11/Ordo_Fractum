using UnityEngine;

public class Hitbox : MonoBehaviour
{
    private Collider weaponCollider;

    [SerializeField] public float playerDamage = 10f;
    [SerializeField] public int  enemyDamage = 1;

    void Awake()
    {
        weaponCollider = GetComponent<Collider>();
        weaponCollider.enabled = false;
    }

    public void EnableHitbox()
    {
        weaponCollider.enabled = true;
        Debug.Log("Hitbox enabled.");
    }

    public void DisableHitbox()
    {
        weaponCollider.enabled = false;
        Debug.Log("Hitbox disabled.");
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            HealthSystem playerHealth = other.GetComponent<HealthSystem>();

            if (playerHealth != null)
            {
                Debug.Log("Player Hit: " + other.name);
                playerHealth.TakeDamage((int)enemyDamage);
            }
            return;
        }
        else if (other.CompareTag("Enemy"))
        {
            EnemyHealth enemyHealth = other.GetComponent<EnemyHealth>();

            if (enemyHealth != null)
            {
                Debug.Log("Enemy Hit: " + other.name);
                enemyHealth.TakeDamage(playerDamage);
            }
            return;
        }
    }
}
