using UnityEngine;
using UnityEngine.UI;
public class EnemyHealth : MonoBehaviour
{

    public Slider healthbarSlider;
    public Slider easeHealthbarSlider;
    public float maxHealth = 100f;
    public float health;
    private float lerpSpeed = 2f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    void Start()
    {
        health = maxHealth;
        healthbarSlider.maxValue = maxHealth;
        easeHealthbarSlider.maxValue = maxHealth;
        healthbarSlider.value = maxHealth;
        easeHealthbarSlider.value = maxHealth;
        Debug.Log(gameObject.name + " has the script");

    }

    // Update is called once per frame
    void Update()
    {

        if(healthbarSlider.value != health)
        {
            healthbarSlider.value = health;
        }
        //TakeDamage(10);
        if(healthbarSlider.value != easeHealthbarSlider.value)
        {
            easeHealthbarSlider.value = Mathf.Lerp(easeHealthbarSlider.value, health, Time.deltaTime * lerpSpeed);

        }

        else
        {
            //Debug.Log("Player is Alive");
        }
    }

    public void TakeDamage(float damage)
    {
        health -= damage; 
        health = Mathf.Clamp(health, 0, maxHealth);// (value,min,max)
        Debug.Log("Enemy took damage: " + damage);

        if(health <= 0)
        {
            Debug.Log("Enemy is Dead");
            Destroy(gameObject);
            return;
        }
    
    }
}
