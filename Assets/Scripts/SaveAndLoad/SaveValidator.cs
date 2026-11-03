using UnityEngine;
using UnityEngine.SceneManagement;
public static class SaveValidator
{
    public static bool IsValid(PlayerData data, int maxHealth){

        if(data == null){
            Debug.LogWarning("No save data found. Starting with default values.");
            return false;
        }

        if(data.health < 0 || data.health > maxHealth)
        {
            Debug.LogWarning("Invalid health value in save data. Starting with default values.");
            return false;
        }

         if (float.IsNaN(data.positionX) ||
            float.IsNaN(data.positionY) ||
            float.IsNaN(data.positionZ) ||
            float.IsInfinity(data.positionX) ||
            float.IsInfinity(data.positionY) ||
            float.IsInfinity(data.positionZ))
        {
            Debug.LogWarning("Invalid saved position.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(data.SceneName))
        {
            Debug.LogWarning("Invalid scene name in save data. Starting with default values.");
            return false;
        }

        return true;

    }
}
