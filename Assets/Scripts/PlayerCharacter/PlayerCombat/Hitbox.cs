using UnityEngine;

public class Hitbox : MonoBehaviour
{
    private Collider weaponCollider;

    [SerializeField] private GameObject parentObject;
    [SerializeField] public float damageToEnemy = 10f;
    [SerializeField] public int  damageToPlayer = 1;

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
        if (other.gameObject == parentObject) return; // prevents weapon from damaging own parent

        if (other.CompareTag("Player"))
        {
            HealthSystem playerHealth = other.GetComponent<HealthSystem>();

            if (playerHealth != null)
            {
                Debug.Log("Player Hit: " + other.name);
                playerHealth.TakeDamage((int)damageToPlayer);
            }
            return;
        }
        else if (other.CompareTag("Enemy"))
        {
            EnemyHealth enemyHealth = other.GetComponent<EnemyHealth>();

            if (enemyHealth != null)
            {
                Debug.Log("Enemy Hit: " + other.name);
                enemyHealth.TakeDamage(damageToEnemy);
            }
            return;
        }
    }
}
