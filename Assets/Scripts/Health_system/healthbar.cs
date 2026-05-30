using UnityEngine;
using UnityEngine.UI;
public class healthbar : MonoBehaviour
{

    public Slider healthbarSlider;
    public Slider easeHealthbarSlider;
    public float maxHealth = 100f;
    public float health;
    private float lerpSpeed = 5f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        health = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        if(healthbarSlider.value != health)
        {
            healthbarSlider.value = health;
        }

        if(healthbarSlider.value == easeHealthbarSlider.value)
        {
            easeHealthbarSlider.value = Mathf.Lerp(easeHealthbarSlider.value, health, Time.deltaTime * lerpSpeed);

        }

        if(health <= 0)
        {
            Debug.Log("Player is Dead");
        }
    }

    void TakeDamage(float damage)
    {
        health -= damage;

    
    }
}
