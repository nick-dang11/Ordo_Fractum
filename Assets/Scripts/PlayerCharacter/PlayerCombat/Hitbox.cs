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
                float timeSinceBlock = Time.time - playerCombat.lastBlockTime;
                // DEFLECT
                if (timeSinceBlock <= playerCombat.deflectWindow)
                {
                    Debug.Log("DEFLECT!");
                    playerCombat.TriggerDeflectFeedback();
                    // Small posture gain on deflect
                    PlayerPosture posture = other.GetComponent<PlayerPosture>();
                    if (posture != null)
                    {
                        posture.posture += posture.postureFillRate * 1f;
                        posture.posture = Mathf.Clamp(posture.posture, 0f, posture.maxPosture);
                        posture.postureSlider.value = posture.posture;
                    }
                    return;
                }
                else
                {
                    // Normal Block
                    Debug.Log("Player BLOCKED the attack!");
                    playerCombat.TriggerBlockFeedback();
                    return;
                }
            }

            if (playerHealth != null)
            {
                Debug.Log("Player Hit: " + other.name);
                playerHealth.TakeDamage((int)currentDamage);
                PlayerPosture posture = other.GetComponent<PlayerPosture>();
                if (posture != null)
                {
                    posture.posture += posture.postureFillRate * 5f;
                    posture.posture = Mathf.Clamp(posture.posture, 0f, posture.maxPosture);
                    posture.postureSlider.value = posture.posture;
                    posture.NotifyHit();
                }

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
