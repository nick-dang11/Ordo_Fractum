using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{

    [Header("Health")]
    public int health;
    public int maxHealth;
    
    [Header("UI")]
    public Sprite emptyHeart;
    public Sprite FullHeart;
    public Image[] hearts;

    //public PlayerHealth;
    
    // Update is called once per frame
    void Update()
    {
       //health = playerHealth.health;

        for( int i = 0; i < hearts.Length; i++)
        {
            if(i < health)
            {
                hearts[i].sprite = FullHeart;
            }
            else
            {
                hearts[i].sprite = emptyHeart;
            }
            if(i < maxHealth)
            {
                hearts[i].enabled = true;
            }

            else
            {
                hearts[i].enabled = false;
            }
        } 
    }
}
