using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
//FIle is to be used to display the players health on the UI. The files is to display the players heart health on the UI.
    [Header("Health")]
    public int health;
    public int maxHealth;
    
    [Header("UI")]
    public Sprite emptyHeart;
    public Sprite FullHeart;
    public Image[] hearts;

    public HealthSystem playerHealth;


    void Start()
    {
      
    }
    void Update()
    {
      health = playerHealth.health;
      maxHealth = playerHealth.maxHealth;

        for(int i = 0; i < hearts.Length; i++)
        {
            if(i < health)
            {
                //hearts[i].sprite = FullHeart; 1
                //will take its sprite
            }
            else
            {
                //hearts[i].sprite = emptyHeart; 2
            }

            if(i < maxHealth)
            {
                //hearts[i].enabled = true; 3
                // the purpose of this if/else is to check each heart in our UI
                //to see if it should be turned on 
            }

            else
            {
                //hearts[i].enabled = false; 4
                //Turn off any hearts that should not be active
            }
        } 
    }
}
