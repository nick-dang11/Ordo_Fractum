using UnityEngine;
using System.IO;

public class SaveSystem
{
    private static string SavePath = Path.Combine(Application.persistentDataPath, "savefile.json");

    public static void SaveGame(PlayerData Data)
    {
        string json = JsonUtility.ToJson(Data,true);
        File.WriteAllText(SavePath, json);

        Debug.Log($"Game saved to {SavePath}");
    }

    public static PlayerData LoadGame()
    {
        if(File.Exists(SavePath))
        {
            string json = File.ReadAllText(SavePath);

            PlayerData Data = JsonUtility.FromJson<PlayerData>(json);

            Debug.Log($"Game loaded from {SavePath}");
            return Data;
        }
        else
        {
            Debug.LogWarning($"No save file found at {SavePath}");
            return null;
        }   
    }
}
