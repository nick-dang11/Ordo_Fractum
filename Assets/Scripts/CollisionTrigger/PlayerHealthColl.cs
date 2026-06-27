using UnityEngine;

public class PlayerHealthColl : MonoBehaviour
{
    public HealthSystem playerHealth; // Reference to the HealthSystem script that enables takedamage() function
    public int damageAmount = 1; // Amount of damage to apply to the player from enemy collision. will need to change to be accessed by other scripts of enemy damage amount

    void Start()
    {
    
        Debug.Log("PlayerHealthColl script is attached to " + gameObject.name);
    }
   void OnTriggerEnter(Collider other)
    {
       if (other.gameObject.CompareTag("Enemy"))
        {
            playerHealth.TakeDamage(damageAmount);
            Debug.Log("Player took damage from Enemy");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            Debug.Log("Exit:Player exited Enemy Collision");
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            Debug.Log("Stay: Player is taking damage from Enemy");
        }
    }
}
