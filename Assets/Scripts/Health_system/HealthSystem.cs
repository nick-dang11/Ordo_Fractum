using UnityEngine;

public class HealthSystem : MonoBehaviour
{
   public int health;
   public int maxHealth = 10;

   void Start()
   {
       health = maxHealth;
   }

   public void TakeDamage(int damage_amount)
    {
        health -= damage_amount;
        if(health <= 0)
        {
            health = 0;
            Debug.Log("player is dead. Destroy player");
            Destroy(gameObject);
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
            return;
        }
    }
}
