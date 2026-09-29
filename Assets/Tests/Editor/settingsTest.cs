using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsTests
{
    private GameObject menuObject;
    private GameObject textObject;
    private GameObject sliderObject;
    private GameObject toggleObject;
    private GameObject confirmationObject;

    private MenuController menuController;
    private TMP_Text sensitivityText;
    private Slider sensitivitySlider;
    private Toggle invertYToggle;

    private FieldInfo sensitivityTextField;
    private FieldInfo sensitivitySliderField;
    private FieldInfo invertYToggleField;
    private FieldInfo confirmationPromptField;

    [SetUp]
    public void SetUp()
    {
        // ---------------------------------------------------------
        // Create a temporary MenuController.
        // ---------------------------------------------------------

        menuObject = new GameObject("Settings_TestMenuController");
        menuController = menuObject.AddComponent<MenuController>();

        // ---------------------------------------------------------
        // Create temporary UI components required by the
        // gameplay settings methods.
        // ---------------------------------------------------------

        textObject = new GameObject("Settings_TestSensitivityText");
        sensitivityText = textObject.AddComponent<TextMeshProUGUI>();

        sliderObject = new GameObject("Settings_TestSensitivitySlider");
        sensitivitySlider = sliderObject.AddComponent<Slider>();

        // Match the intended gameplay sensitivity range.
        sensitivitySlider.minValue = 1f;
        sensitivitySlider.maxValue = 10f;
        sensitivitySlider.value = 4f;

        toggleObject = new GameObject("Settings_TestInvertYToggle");
        invertYToggle = toggleObject.AddComponent<Toggle>();
        invertYToggle.isOn = false;

        confirmationObject =
            new GameObject("Settings_TestConfirmationPrompt");

        confirmationObject.SetActive(false);

        // ---------------------------------------------------------
        // Get the private serialized fields from MenuController.
        // ---------------------------------------------------------

        sensitivityTextField = typeof(MenuController).GetField(
            "controllerSenTextValue",
            BindingFlags.Instance | BindingFlags.NonPublic
        );

        sensitivitySliderField = typeof(MenuController).GetField(
            "controllerSenSlider",
            BindingFlags.Instance | BindingFlags.NonPublic
        );

        invertYToggleField = typeof(MenuController).GetField(
            "invertYToggle",
            BindingFlags.Instance | BindingFlags.NonPublic
        );

        confirmationPromptField = typeof(MenuController).GetField(
            "confirmationPrompt",
            BindingFlags.Instance | BindingFlags.NonPublic
        );

        Assert.IsNotNull(sensitivityTextField);
        Assert.IsNotNull(sensitivitySliderField);
        Assert.IsNotNull(invertYToggleField);
        Assert.IsNotNull(confirmationPromptField);

        // ---------------------------------------------------------
        // Connect the temporary UI to MenuController.
        // ---------------------------------------------------------

        sensitivityTextField.SetValue(
            menuController,
            sensitivityText
        );

        sensitivitySliderField.SetValue(
            menuController,
            sensitivitySlider
        );

        invertYToggleField.SetValue(
            menuController,
            invertYToggle
        );

        confirmationPromptField.SetValue(
            menuController,
            confirmationObject
        );

        // ---------------------------------------------------------
        // Clear saved settings so each test starts clean.
        // ---------------------------------------------------------

        PlayerPrefs.DeleteKey("masterSen");
        PlayerPrefs.DeleteKey("masterInvertY");
    }

    [TearDown]
    public void TearDown()
    {
        // Remove test PlayerPrefs.
        PlayerPrefs.DeleteKey("masterSen");
        PlayerPrefs.DeleteKey("masterInvertY");

        // Destroy temporary Unity objects.
        if (menuObject != null)
            Object.DestroyImmediate(menuObject);

        if (textObject != null)
            Object.DestroyImmediate(textObject);

        if (sliderObject != null)
            Object.DestroyImmediate(sliderObject);

        if (toggleObject != null)
            Object.DestroyImmediate(toggleObject);

        if (confirmationObject != null)
            Object.DestroyImmediate(confirmationObject);
    }

    // -------------------------------------------------------------
    // TEST 1: DEFAULT SETTINGS
    // -------------------------------------------------------------

    [Test]
    public void GameplaySettings_DefaultSensitivity_IsFour()
    {
        Assert.AreEqual(
            4,
            menuController.mainControllerSen,
            "Default controller sensitivity should be 4."
        );
    }

    // -------------------------------------------------------------
    // TEST 2: VALID RANGE
    // -------------------------------------------------------------

    [Test]
    public void ControllerSensitivity_StaysWithinValidRange()
    {
        // Test the minimum allowed slider value.
        sensitivitySlider.value = sensitivitySlider.minValue;
        menuController.SetControllerSen(sensitivitySlider.value);

        Assert.That(
            menuController.mainControllerSen,
            Is.InRange(1, 10),
            "Minimum sensitivity should remain inside the valid range."
        );

        // Test the maximum allowed slider value.
        sensitivitySlider.value = sensitivitySlider.maxValue;
        menuController.SetControllerSen(sensitivitySlider.value);

        Assert.That(
            menuController.mainControllerSen,
            Is.InRange(1, 10),
            "Maximum sensitivity should remain inside the valid range."
        );
    }

    // -------------------------------------------------------------
    // TEST 3: APPLY SETTINGS
    // -------------------------------------------------------------

    [Test]
    public void GameplayApply_SavesSettingsToPlayerPrefs()
    {
        // Arrange
        menuController.SetControllerSen(7f);
        invertYToggle.isOn = true;

        // Act
        menuController.GameplayApply();

        // Assert
        Assert.IsTrue(
            PlayerPrefs.HasKey("masterSen"),
            "GameplayApply should save controller sensitivity."
        );

        Assert.AreEqual(
            7f,
            PlayerPrefs.GetFloat("masterSen"),
            "Saved sensitivity should match the selected sensitivity."
        );

        Assert.IsTrue(
            PlayerPrefs.HasKey("masterInvertY"),
            "GameplayApply should save the Invert Y setting."
        );

        Assert.AreEqual(
            1,
            PlayerPrefs.GetInt("masterInvertY"),
            "Invert Y should be saved as 1 when enabled."
        );
    }

    // -------------------------------------------------------------
    // TEST 4: RESET SETTINGS
    // -------------------------------------------------------------

    [Test]
    public void GameplayReset_RestoresDefaultSettings()
    {
        // Arrange: simulate settings changed by the player.
        menuController.SetControllerSen(8f);
        sensitivitySlider.value = 8f;
        invertYToggle.isOn = true;

        // Act
        menuController.ResetButton("Gameplay");

        // Assert
        Assert.AreEqual(
            4,
            menuController.mainControllerSen,
            "Reset should restore sensitivity to the default value of 4."
        );

        Assert.AreEqual(
            4f,
            sensitivitySlider.value,
            "Reset should return the sensitivity slider to 4."
        );

        Assert.IsFalse(
            invertYToggle.isOn,
            "Reset should disable Invert Y."
        );

        Assert.AreEqual(
            4f,
            PlayerPrefs.GetFloat("masterSen"),
            "Reset should save the default sensitivity."
        );

        Assert.AreEqual(
            0,
            PlayerPrefs.GetInt("masterInvertY"),
            "Reset should save Invert Y as disabled."
        );
    }
}
