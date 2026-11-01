using UnityEngine;

public class GameSaveManager : MonoBehaviour
{
   
   [Header("Plyer References")]
   [SerializeField] private PlayerHealth playerHealth;


   public void SaveGame()
    {
        PlayerData data = new PlayerData();

        data.health = playerHealth.health;

        SaveSystem.SaveGame(data);


        Debug.Log("Player data saved: Health = " + data.health);
    }

    public void LoadGame()
    {
        PlayerData data = SaveSystem.LoadGame();

        if (data == null)
        {
            Debug.LogWarning("No save data found. Starting with default values.");
            return;
        }

        playerHealth.health = Mathf.Clamp(data.health, 0, playerHealth.maxHealth);

        Debug.Log("Player data loaded: Health = " + playerHealth.health);
    }



}