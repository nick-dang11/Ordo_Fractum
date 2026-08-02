using UnityEngine;

public class WeaponHitbox : MonoBehaviour
{
    private Collider weaponCollider;
    private Renderer weaponRenderer;

    [SerializeField] private GameObject PlayerObject;
    private Color activeColor = Color.red;
    private Color inactiveColor = Color.grey;

    private float currentDamage;

    void Awake()
    {
        weaponCollider = GetComponent<Collider>();
        weaponCollider.enabled = false;

        weaponRenderer = GetComponent<Renderer>();
        if (weaponRenderer == null)
        {
            weaponRenderer = GetComponentInChildren<Renderer>();
        }

        SetWeaponColor(inactiveColor);
    }

    public void EnableHitbox(float damage)
    {
        weaponCollider.enabled = true;
        currentDamage = damage;
        Debug.Log("Hitbox enabled with damage: " + currentDamage);
        
        if(weaponRenderer != null)
        {
            SetWeaponColor(inactiveColor);
        }
        SetWeaponColor(activeColor);
    }

    public void DisableHitbox()
    {
        weaponCollider.enabled = false;
        //Debug.Log("Hitbox disabled.");
        SetWeaponColor(inactiveColor);
    }

    private void SetWeaponColor(Color color)
    {
        if (weaponRenderer != null)
        {
            weaponRenderer.material.color = color; // may need material.SetColor("_BaseColor", color) with URP/HDRP
        }
    }

    void OnTriggerEnter(Collider other)
    {
        PlayerCombat ownerCombat = GetComponentInParent<PlayerCombat>();
        if (ownerCombat != null && other.transform.root == ownerCombat.transform.root)
        {
            return;
        }

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
                    // correction, only avoid/evade restores a bit of posture! - N, 7/22/26
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
