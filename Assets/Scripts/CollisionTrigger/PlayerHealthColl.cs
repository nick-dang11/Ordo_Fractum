using UnityEngine;

public class PlayerHealthColl : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemay"))
        {
            Debug.Log("Player took damage from Enemy");
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Debug.Log("Player exited Enemy Collision");
        }
    }

    private void OnCollisionStay(Collision Collision)
    {
        if (Collision.gameObject.CompareTag("Enemy"))
        {
            Debug.Log("Player is taking damage from Enemy");
        }
    }
    
   void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            Debug.Log("Enter:Player took damage from Enemy ");
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
