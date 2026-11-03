
using System.IO;
using NUnit.Framework;
using UnityEngine;

public class SaveFileIntegrityTests
{
    private string savePath;
    private string originalContents;
    private bool hadOriginalSave;

    [SetUp]
    public void Setup()
    {
        savePath = Path.Combine(
            Application.persistentDataPath,
            "savefile.json"
        );

        hadOriginalSave = File.Exists(savePath);

        if (hadOriginalSave)
        {
            originalContents = File.ReadAllText(savePath);
        }
    }

    [TearDown]
    public void Cleanup()
    {
        if (hadOriginalSave)
        {
            File.WriteAllText(savePath, originalContents);
        }
        else if (File.Exists(savePath))
        {
            File.Delete(savePath);
        }
    }

    [Test]
    public void ST01_ValidSaveFile_LoadsSuccessfully()
    {
        PlayerData data = new PlayerData
        {
            health = 80,
            positionX = 10,
            positionY = 0,
            positionZ = 5,
            SceneName = "Level1"
        };

        SaveSystem.SaveGame(data);

        PlayerData loaded = SaveSystem.LoadGame();

        Assert.IsNotNull(loaded);
        Assert.AreEqual(80, loaded.health);
        Assert.AreEqual(10, loaded.positionX);
        Assert.AreEqual("Level1", loaded.SceneName);
    }

    [Test]
    public void ST02_MalformedJSON_IsRejected()
    {
        string corruptedJson =
            "{\"health\":80,\"SceneName\":\"Level1}";

        File.WriteAllText(savePath, corruptedJson);

        PlayerData loaded = SaveSystem.LoadGame();

        Assert.IsNull(loaded,
            "Malformed JSON should be rejected.");
    }

    [Test]
    public void ST03_EmptyJSON_IsRejected()
    {
        File.WriteAllText(savePath, "");

        PlayerData loaded = SaveSystem.LoadGame();

        Assert.IsNull(loaded,
            "Empty JSON should be rejected.");
    }

    [Test]
    public void ST04_TruncatedJSON_IsRejected()
    {
        string truncatedJson =
            "{\"health\":80,\"positionX\":10," +
            "\"positionY\":0,\"positionZ\":";

        File.WriteAllText(savePath, truncatedJson);

        PlayerData loaded = SaveSystem.LoadGame();

        Assert.IsNull(loaded,
            "Truncated JSON should be rejected.");
    }

    [Test]
    public void ST05_IncompleteSaveData_IsRejected()
    {
        string incompleteJson =
            "{\"health\":80,\"SceneName\":\"Level1\"}";

        File.WriteAllText(savePath, incompleteJson);

        PlayerData loaded = SaveSystem.LoadGame();

        Assert.IsNull(loaded,
            "Missing position fields should be rejected.");
    }
}
