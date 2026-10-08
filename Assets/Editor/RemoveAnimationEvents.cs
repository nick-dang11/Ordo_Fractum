using UnityEditor;
using UnityEngine;

public static class RemoveAnimationEvents
{
    [MenuItem("Tools/Animation/Remove Events From Selected Clips")]
    private static void RemoveEventsFromSelectedClips()
    {
        Object[] selectedObjects = Selection.objects;

        int modifiedCount = 0;

        foreach (Object obj in selectedObjects)
        {
            AnimationClip clip = obj as AnimationClip;

            if (clip == null)
                continue;

            AnimationUtility.SetAnimationEvents(
                clip,
                System.Array.Empty<AnimationEvent>()
            );

            EditorUtility.SetDirty(clip);
            modifiedCount++;
        }

        AssetDatabase.SaveAssets();

        Debug.Log(
            $"Removed animation events from {modifiedCount} selected AnimationClip(s)."
        );
    }
}