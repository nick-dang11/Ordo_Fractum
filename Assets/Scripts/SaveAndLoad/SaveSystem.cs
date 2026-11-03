using UnityEngine;
using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class SaveSystem
{
    private static string SavePath =
        Path.Combine(
            Application.persistentDataPath,
            "savefile.json"
        );

    public static void SaveGame(PlayerData data)
    {
        try
        {
            string json = JsonUtility.ToJson(data, true);

            File.WriteAllText(SavePath, json);

            Debug.Log("Game saved to " + SavePath);
        }
        catch (Exception ex)
        {
            Debug.LogError("Failed to save game: " + ex.Message);
        }
    }

    public static PlayerData LoadGame()
    {
        if (!File.Exists(SavePath))
        {
            Debug.LogWarning("No save file found.");
            return null;
        }

        try
        {
            string json = File.ReadAllText(SavePath);

            // ST-03: Detect empty JSON
            if (string.IsNullOrWhiteSpace(json))
            {
                Debug.LogWarning(
                    "Corrupted save file: Empty JSON."
                );
                return null;
            }

            // ST-02 / ST-04:
            // Parse JSON and reject malformed syntax.
            JObject parsed = JObject.Parse(
                json,
                new JsonLoadSettings
                {
                    DuplicatePropertyNameHandling =
                        DuplicatePropertyNameHandling.Error
                }
            );

            // Reject missing or incorrectly typed fields.
            if (!HasField(parsed, "health", JTokenType.Integer) ||
                !HasNumber(parsed, "positionX") ||
                !HasNumber(parsed, "positionY") ||
                !HasNumber(parsed, "positionZ") ||
                !HasField(parsed, "SceneName", JTokenType.String))
            {
                Debug.LogWarning(
                    "Corrupted save file: Missing or invalid fields."
                );
                return null;
            }

            PlayerData data =
                JsonUtility.FromJson<PlayerData>(json);

            if (data == null)
            {
                Debug.LogWarning("Invalid save data.");
                return null;
            }

            Debug.Log("Game loaded from " + SavePath);

            return data;
        }
        catch (JsonReaderException ex)
        {
            Debug.LogWarning(
                "Corrupted JSON detected: " + ex.Message
            );
            return null;
        }
        catch (JsonException ex)
        {
            Debug.LogWarning(
                "Invalid JSON detected: " + ex.Message
            );
            return null;
        }
        catch (Exception ex)
        {
            Debug.LogWarning(
                "Failed to load save: " + ex.Message
            );
            return null;
        }
    }

    private static bool HasField(
        JObject json,
        string fieldName,
        JTokenType expectedType)
    {
        JToken token = json[fieldName];

        return token != null &&
               token.Type == expectedType;
    }

    private static bool HasNumber(
        JObject json,
        string fieldName)
    {
        JToken token = json[fieldName];

        return token != null &&
               (token.Type == JTokenType.Integer ||
                token.Type == JTokenType.Float);
    }
}

