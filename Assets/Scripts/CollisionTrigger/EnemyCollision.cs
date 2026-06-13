using UnityEngine;

public class EnemyCollision : MonoBehaviour
{
    private EnemyHealth _EnemyHealth;

    void Start()
    {
        _EnemyHealth = GetComponent<EnemyHealth>();
        Debug.Log(gameObject.name + " has the script");
    }
    
   void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {   
            Debug.Log("Enemy Collision with Player");

            if(_EnemyHealth != null)
            {
                _EnemyHealth.TakeDamage(10f);
            }
            else
            {
                Debug.LogWarning("HealthBar component not found on " + gameObject.name);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Enemy exited Player Collision");
            
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Enemy staying in Player Collision");
        }
    } 
}
