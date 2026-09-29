using System;
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

    private Component menuController;
    private Type menuControllerType;

    private TMP_Text sensitivityText;
    private Slider sensitivitySlider;
    private Toggle invertYToggle;

    private FieldInfo sensitivityTextField;
    private FieldInfo sensitivitySliderField;
    private FieldInfo invertYToggleField;
    private FieldInfo confirmationPromptField;
    private FieldInfo mainControllerSenField;

    private MethodInfo setControllerSenMethod;
    private MethodInfo gameplayApplyMethod;
    private MethodInfo resetButtonMethod;

    [SetUp]
    public void SetUp()
    {
        PlayerPrefs.DeleteKey("masterSen");
        PlayerPrefs.DeleteKey("masterInvertY");

        menuObject = new GameObject("Settings_TestMenuController");

        menuControllerType =
            Type.GetType("MenuController, Assembly-CSharp");

        Assert.IsNotNull(
            menuControllerType,
            "Could not find MenuController in Assembly-CSharp."
        );

        menuController =
            menuObject.AddComponent(menuControllerType);

        Assert.IsNotNull(
            menuController,
            "Could not add MenuController component."
        );

        textObject = new GameObject("Settings_TestSensitivityText");
        sensitivityText = textObject.AddComponent<TextMeshProUGUI>();

        sliderObject = new GameObject("Settings_TestSensitivitySlider");
        sensitivitySlider = sliderObject.AddComponent<Slider>();
        sensitivitySlider.minValue = 1f;
        sensitivitySlider.maxValue = 10f;
        sensitivitySlider.value = 4f;

        toggleObject = new GameObject("Settings_TestInvertYToggle");
        invertYToggle = toggleObject.AddComponent<Toggle>();
        invertYToggle.isOn = false;

        confirmationObject =
            new GameObject("Settings_TestConfirmationPrompt");

        confirmationObject.SetActive(false);

        sensitivityTextField = menuControllerType.GetField(
            "controllerSenTextValue",
            BindingFlags.Instance | BindingFlags.NonPublic
        );

        sensitivitySliderField = menuControllerType.GetField(
            "controllerSenSlider",
            BindingFlags.Instance | BindingFlags.NonPublic
        );

        invertYToggleField = menuControllerType.GetField(
            "invertYToggle",
            BindingFlags.Instance | BindingFlags.NonPublic
        );

        confirmationPromptField = menuControllerType.GetField(
            "confirmationPrompt",
            BindingFlags.Instance | BindingFlags.NonPublic
        );

        mainControllerSenField = menuControllerType.GetField(
            "mainControllerSen",
            BindingFlags.Instance | BindingFlags.Public
        );

        setControllerSenMethod = menuControllerType.GetMethod(
            "SetControllerSen",
            BindingFlags.Instance | BindingFlags.Public
        );

        gameplayApplyMethod = menuControllerType.GetMethod(
            "GameplayApply",
            BindingFlags.Instance | BindingFlags.Public
        );

        resetButtonMethod = menuControllerType.GetMethod(
            "ResetButton",
            BindingFlags.Instance | BindingFlags.Public
        );

        Assert.IsNotNull(sensitivityTextField);
        Assert.IsNotNull(sensitivitySliderField);
        Assert.IsNotNull(invertYToggleField);
        Assert.IsNotNull(confirmationPromptField);
        Assert.IsNotNull(mainControllerSenField);

        Assert.IsNotNull(setControllerSenMethod);
        Assert.IsNotNull(gameplayApplyMethod);
        Assert.IsNotNull(resetButtonMethod);

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
    }

    [TearDown]
    public void TearDown()
    {
        PlayerPrefs.DeleteKey("masterSen");
        PlayerPrefs.DeleteKey("masterInvertY");

        if (menuObject != null)
            UnityEngine.Object.DestroyImmediate(menuObject);

        if (textObject != null)
            UnityEngine.Object.DestroyImmediate(textObject);

        if (sliderObject != null)
            UnityEngine.Object.DestroyImmediate(sliderObject);

        if (toggleObject != null)
            UnityEngine.Object.DestroyImmediate(toggleObject);

        if (confirmationObject != null)
            UnityEngine.Object.DestroyImmediate(confirmationObject);
    }

    [Test]
    public void GameplaySettings_DefaultSensitivity_IsFour()
    {
        int sensitivity =
            (int)mainControllerSenField.GetValue(menuController);

        Assert.AreEqual(
            4,
            sensitivity,
            "Default controller sensitivity should be 4."
        );
    }

    [Test]
    public void SetControllerSen_RoundsAndStoresSensitivity()
    {
        setControllerSenMethod.Invoke(
            menuController,
            new object[] { 6.7f }
        );

        int sensitivity =
            (int)mainControllerSenField.GetValue(menuController);

        Assert.AreEqual(
            7,
            sensitivity,
            "SetControllerSen should round the sensitivity to the nearest integer."
        );

        Assert.AreEqual(
            "7",
            sensitivityText.text,
            "Sensitivity text should display the rounded sensitivity."
        );
    }

    [Test]
    public void ControllerSensitivity_StaysWithinConfiguredSliderRange()
    {
        sensitivitySlider.value = sensitivitySlider.minValue;

        setControllerSenMethod.Invoke(
            menuController,
            new object[] { sensitivitySlider.value }
        );

        int minimumSensitivity =
            (int)mainControllerSenField.GetValue(menuController);

        Assert.That(
            minimumSensitivity,
            Is.InRange(1, 10)
        );

        sensitivitySlider.value = sensitivitySlider.maxValue;

        setControllerSenMethod.Invoke(
            menuController,
            new object[] { sensitivitySlider.value }
        );

        int maximumSensitivity =
            (int)mainControllerSenField.GetValue(menuController);

        Assert.That(
            maximumSensitivity,
            Is.InRange(1, 10)
        );
    }

    [Test]
    public void GameplayApply_SavesSensitivityAndInvertY()
    {
        setControllerSenMethod.Invoke(
            menuController,
            new object[] { 7f }
        );

        invertYToggle.isOn = true;

        gameplayApplyMethod.Invoke(
            menuController,
            null
        );

        Assert.IsTrue(
            PlayerPrefs.HasKey("masterSen")
        );

        Assert.AreEqual(
            7f,
            PlayerPrefs.GetFloat("masterSen")
        );

        Assert.IsTrue(
            PlayerPrefs.HasKey("masterInvertY")
        );

        Assert.AreEqual(
            1,
            PlayerPrefs.GetInt("masterInvertY")
        );
    }

    [Test]
    public void GameplayReset_RestoresDefaultSettings()
    {
        setControllerSenMethod.Invoke(
            menuController,
            new object[] { 8f }
        );

        sensitivitySlider.value = 8f;
        invertYToggle.isOn = true;

        resetButtonMethod.Invoke(
            menuController,
            new object[] { "Gameplay" }
        );

        int sensitivity =
            (int)mainControllerSenField.GetValue(menuController);

        Assert.AreEqual(
            4,
            sensitivity
        );

        Assert.AreEqual(
            4f,
            sensitivitySlider.value
        );

        Assert.IsFalse(
            invertYToggle.isOn
        );

        Assert.AreEqual(
            4f,
            PlayerPrefs.GetFloat("masterSen")
        );

        Assert.AreEqual(
            0,
            PlayerPrefs.GetInt("masterInvertY")
        );
    }
}
