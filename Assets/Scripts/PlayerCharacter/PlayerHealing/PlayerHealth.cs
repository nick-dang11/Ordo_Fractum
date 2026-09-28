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

    public void TakeDamage(int damage_amount)
    {
        health -= damage_amount;
        health = Mathf.Clamp(health, 0, maxHealth);
        Debug.Log("Current Health: " + health);

        if(health <= 0)
        {
            HandleDeath();
        }
    }

    public void Heal(int heal_amount)
    {
        if(health < maxHealth)
        {
            health = health + heal_amount;
            
            health = Mathf.Clamp(health, 0, maxHealth);

            Debug.Log("Player healed by " + heal_amount + ". Current health: " + health);

            playerInputManager.self_heal = false;
        }
        else
        {
            Debug.Log("Player is at max health");
            return;
        }
    }

    public void RestoreForRespawn()
    {
        health = maxHealth;
        Debug.Log("Player health restored for respawn. Current Health: " + health);
    }

    private void HandleDeath()
    {
        health = 0;
        Debug.Log("Player is dead.");
        if (playerRespawn != null)
        {
            playerRespawn.Respawn();
        } else
        {
            Debug.LogError("[HealthSystem] PlayerRespawn could not be found.", this);
        }
    }
    [ContextMenu("Test - Kill Player")]
    private void TestKillPlayer()
    {
        TakeDamage(maxHealth);
    }
}
