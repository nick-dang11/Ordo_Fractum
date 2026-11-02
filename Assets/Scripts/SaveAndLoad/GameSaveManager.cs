using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSaveManager : MonoBehaviour
{
   
   [Header("Plyer References")]
   [SerializeField] private PlayerHealth playerHealth;
   [SerializeField] private Transform playerTransform;

   public void SaveGame()
    {
        PlayerData data = new PlayerData();

        data.health = playerHealth.health;

        //save position
        data.positionX = playerTransform.position.x;
        data.positionY = playerTransform.position.y;
        data.positionZ = playerTransform.position.z;

        data.SceneName= SceneManager.GetActiveScene().name; // Save the current scene name

        SaveSystem.SaveGame(data);


        Debug.Log("Player data saved: Health = " + data.health);
        Debug.Log("Player position saved: X = " + data.positionX + ", Y = " + data.positionY + ", Z = " + data.positionZ);
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

        //Restore Position 
        Vector3 savedPosition = new Vector3(data.positionX, data.positionY, data.positionZ);

        CharacterController controller = playerTransform.GetComponent<CharacterController>();

        if(controller != null)
        {
            controller.enabled = false;
        }

        playerTransform.position = savedPosition;

        if(controller != null)
        {
            controller.enabled = true;
        }
        Debug.Log("Player position loaded: X = " + savedPosition.x + ", Y = " + savedPosition.y + ", Z = " + savedPosition.z);
        Debug.Log("Player data loaded: Health = " + playerHealth.health);
    }



}