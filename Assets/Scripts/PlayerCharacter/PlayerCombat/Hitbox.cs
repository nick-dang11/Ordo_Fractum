using UnityEngine;

public class Hitbox : MonoBehaviour
{
    private Collider weaponCollider;

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
        EnemyHealth enemy = other.GetComponent<EnemyHealth>();

        if (enemy != null)
        {
            Debug.Log("Hit: " + other.name);
            enemy.TakeDamage(10f);
        }
    }
}
