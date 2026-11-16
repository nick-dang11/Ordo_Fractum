
using UnityEngine;
using UnityEditor;

public class PlayerPrefsViewer : EditorWindow
{
    [MenuItem("Tools/PlayerPrefs Viewer")]
    public static void ShowWindow()
    {
        GetWindow<PlayerPrefsViewer>("PlayerPrefs Viewer");
    }

    private void OnGUI()
    {
        GUILayout.Label("Saved Game Settings", EditorStyles.boldLabel);

        if (GUILayout.Button("Refresh"))
        {
            Repaint();
        }

        EditorGUILayout.Space();

        ShowFloat("masterVolume");
        ShowFloat("masterSen");
        ShowInt("masterInvertY");
        ShowFloat("masterBrightness");
        ShowInt("masterQuality");
        ShowInt("masterFullscreen");
    }

    private void ShowFloat(string key)
    {
        if (PlayerPrefs.HasKey(key))
        {
            EditorGUILayout.LabelField(
                key,
                PlayerPrefs.GetFloat(key).ToString("0.00")
            );
        }
        else
        {
            EditorGUILayout.LabelField(key, "Not Saved");
        }
    }

    private void ShowInt(string key)
    {
        if (PlayerPrefs.HasKey(key))
        {
            EditorGUILayout.LabelField(
                key,
                PlayerPrefs.GetInt(key).ToString()
            );
        }
        else
        {
            EditorGUILayout.LabelField(key, "Not Saved");
        }
    }
}
