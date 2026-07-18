using UnityEngine;

public class HealthSystem : MonoBehaviour
{

    [Header("Health")]
   public int health;
   public int maxHealth = 4;

   public int heal_Amount = 1;

   [Header("Respawn")]
   PlayerRespawn playerRespawn;

   [Header("References")]
   public PlayerInputManager playerInputManager;

   void Start()
   {
       health = maxHealth;
       playerRespawn = GetComponent<PlayerRespawn>();

   }

    void Update()
    {
        if(playerInputManager.self_heal)
        {
            Heal(heal_Amount);
            Debug.Log("Player healed");

        }
    }

    public void TakeDamage(int damage_amount)
    {
        health -= damage_amount;

        if(health < 0)
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
            
            health = Mathf.Clamp(health, 0, maxHealth);

            Debug.Log("Player healed by " + heal_amount + ". Current health: " + health);
        }
        else
        {
            Debug.Log("Player is at max health");
            return;
        }
    }
}
