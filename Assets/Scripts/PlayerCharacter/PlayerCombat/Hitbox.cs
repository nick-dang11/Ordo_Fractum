using UnityEngine;

public class Hitbox : MonoBehaviour
{
    private Collider weaponCollider;

    [SerializeField] private GameObject parentObject;
    private float currentDamage;
    void Awake()
    {
        weaponCollider = GetComponent<Collider>();
        weaponCollider.enabled = false;
    }

    public void EnableHitbox(float damage)
    {
        weaponCollider.enabled = true;
        currentDamage = damage;
        Debug.Log("Hitbox enabled with damage: " + currentDamage);
        //Debug.Log("Hitbox enabled.");
    }

    public void DisableHitbox()
    {
        weaponCollider.enabled = false;
        //Debug.Log("Hitbox disabled.");
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == parentObject) return;

        if (other.CompareTag("Player"))
        {
            PlayerCombat playerCombat = other.GetComponent<PlayerCombat>();
            HealthSystem playerHealth = other.GetComponent<HealthSystem>();

            if (playerCombat != null && playerCombat.IsBlocking())
            {
                Debug.Log("Player BLOCKED the attack!");
                playerCombat.TriggerBlockFeedback();
                return;
            }

            if (playerHealth != null)
            {
                Debug.Log("Player Hit: " + other.name);
                playerHealth.TakeDamage((int)currentDamage);
            }
            return;
        }
        else if (other.CompareTag("Enemy"))
        {
            EnemyHealth enemyHealth = other.GetComponent<EnemyHealth>();

            if (enemyHealth != null)
            {
                Debug.Log("Enemy Hit: " + other.name);
                enemyHealth.TakeDamage(currentDamage);
            }
            return;
        }
    }

}
