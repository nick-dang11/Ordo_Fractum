using UnityEngine;

public class HealthSystem : MonoBehaviour
{
   public int health;
   public int maxHealth = 4;

   PlayerRespawn playerRespawn;
   void Start()
   {
       health = maxHealth;
       playerRespawn = GetComponent<PlayerRespawn>();

   }

   public void TakeDamage(int damage_amount)
    {
        health -= damage_amount;

        if(health <= 0)
        {
            health = 0;
            Debug.Log("Player is dead. Destroy player");
            playerRespawn.Respawn();
        }
    }

    public void Heal(int heal_amount)
    {
        if(health <= maxHealth)
        {
            health += heal_amount;
        }
        else
        {
            Debug.Log("Player is at max health");
            return;
        }
    }
}
