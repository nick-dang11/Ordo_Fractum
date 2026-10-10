using UnityEngine;
using UnityEngine.UI;
public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private EnemyCombat enemyCombat;
    [SerializeField] private EnemyPosture enemyPosture;

    public Slider healthbarSlider;
    public Slider easeHealthbarSlider;
    public float maxHealth = 100f;
    public float health;
    private float lerpSpeed = 2f;

    private void Awake()
    {
        if (enemyCombat == null) enemyCombat = GetComponent <EnemyCombat>();
        if (enemyPosture == null) enemyPosture = GetComponent<EnemyPosture>();
    }

    void Start()
    {
        health = maxHealth;
        healthbarSlider.maxValue = maxHealth;
        easeHealthbarSlider.maxValue = maxHealth;
        healthbarSlider.value = maxHealth;
        easeHealthbarSlider.value = maxHealth;
        Debug.Log(gameObject.name + " has the script");

    }

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
        if (damage <= 0f) return;

        if(enemyCombat != null && enemyCombat.IsBlocking)
        {
            Debug.Log($"Enemy {name} blocked {damage} damage.");

            if(enemyPosture != null)
            {
                enemyPosture.ApplyPostureDamage(damage);
            }

            if (!enemyPosture.IsPostureBroken)
            {
                enemyCombat.PlayBlockFeedback();
            }
            return;
        }

        health -= damage; 
        health = Mathf.Clamp(health, 0, maxHealth);// (value,min,max)
        Debug.Log("Enemy took damage: " + damage);

        if (health <= 0f)
        {
            Debug.Log("Enemy is Dead");
            Destroy(gameObject);
        }
    }
}
