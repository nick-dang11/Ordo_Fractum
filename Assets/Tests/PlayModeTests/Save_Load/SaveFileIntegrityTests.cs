
using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class SaveFileIntegrityTests
{
    private Type playerDataType;
    private Type saveSystemType;

    private MethodInfo saveMethod;
    private MethodInfo loadMethod;
    private FieldInfo savePathField;

    private string originalPath;
    private string testPath;

    [SetUp]
    public void Setup()
    {
        // Find the existing classes at runtime.
        playerDataType = FindType("PlayerData");
        saveSystemType = FindType("SaveSystem");

        Assert.That(playerDataType, Is.Not.Null,
            "PlayerData class was not found.");

        Assert.That(saveSystemType, Is.Not.Null,
            "SaveSystem class was not found.");

        saveMethod = saveSystemType.GetMethod("SaveGame");
        loadMethod = saveSystemType.GetMethod("LoadGame");

        Assert.That(saveMethod, Is.Not.Null);
        Assert.That(loadMethod, Is.Not.Null);

        // Redirect SaveSystem to a temporary test file.
        savePathField = saveSystemType.GetField(
            "SavePath",
            BindingFlags.NonPublic | BindingFlags.Static
        );

        Assert.That(savePathField, Is.Not.Null,
            "SavePath field was not found.");

        originalPath = (string)savePathField.GetValue(null);

        testPath = Path.Combine(
            Application.temporaryCachePath,
            "save_test_" + Guid.NewGuid() + ".json"
        );

        savePathField.SetValue(null, testPath);
    }

    [TearDown]
    public void Cleanup()
    {
        // Restore the real save path.
        if (savePathField != null && originalPath != null)
        {
            savePathField.SetValue(null, originalPath);
        }

        if (!string.IsNullOrEmpty(testPath) &&
            File.Exists(testPath))
        {
            File.Delete(testPath);
        }
    }

    private Type FindType(string className)
    {
        foreach (Assembly assembly in
                 AppDomain.CurrentDomain.GetAssemblies())
        {
            Type found = assembly.GetType(className);

            if (found != null)
                return found;
        }

        return null;
    }

    private object Load()
    {
        return loadMethod.Invoke(null, null);
    }

    private void WriteCorruptedFile(string json)
    {
        File.WriteAllText(testPath, json);
    }

    [Test]
    public void ST01_ValidSaveFile_LoadsSuccessfully()
    {
        object data = Activator.CreateInstance(playerDataType);

        playerDataType.GetField("health").SetValue(data, 80);
        playerDataType.GetField("positionX").SetValue(data, 10f);
        playerDataType.GetField("positionY").SetValue(data, 0f);
        playerDataType.GetField("positionZ").SetValue(data, 5f);
        playerDataType.GetField("SceneName").SetValue(data, "Level1");

        saveMethod.Invoke(null, new object[] { data });

        object loaded = Load();

        Assert.That(loaded, Is.Not.Null);

        int health = (int)playerDataType
            .GetField("health").GetValue(loaded);

        Assert.That(health, Is.EqualTo(80));

        Debug.Log("ST-01 PASS: Valid save loaded.");
    }

    [Test]
    public void ST02_MalformedJSON_IsRejected()
    {
        WriteCorruptedFile(
            "{\"health\":80,\"SceneName\":\"Level1}"
        );

        object loaded = Load();

        Assert.That(loaded, Is.Null,
            "Malformed JSON should be rejected.");
    }

    [Test]
    public void ST03_EmptyJSON_IsRejected()
    {
        WriteCorruptedFile("");

        object loaded = Load();

        Assert.That(loaded, Is.Null,
            "Empty JSON should be rejected.");
    }

    [Test]
    public void ST04_TruncatedJSON_IsRejected()
    {
        WriteCorruptedFile(
            "{\"health\":80,\"positionX\":10," +
            "\"positionY\":0,\"positionZ\":"
        );

        object loaded = Load();

        Assert.That(loaded, Is.Null,
            "Truncated JSON should be rejected.");
    }

    [Test]
    public void ST05_IncompleteSaveData_IsRejected()
    {
        WriteCorruptedFile(
            "{\"health\":80,\"SceneName\":\"Level1\"}"
        );

        object loaded = Load();

        Assert.That(loaded, Is.Null,
            "Missing position fields should be rejected.");
    }
}
