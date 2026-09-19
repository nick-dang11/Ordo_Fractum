using UnityEngine;

[CreateAssetMenu(fileName = "SO_Attack_New", menuName = "ScriptableObjects/Attack Animation Data")]

public class AttackData : ScriptableObject
{
    [Header("Animation Reference")]
    [Tooltip("Please ensure names are exact same in Animator Controller")]

    public string animationStateName;

    [Header("Frame Timing")]

    [Tooltip("Frame where the weapon becomes lethal")]
    public int startHitFrame;

    [Tooltip("Frame where the weapon stops being lethal")]
    public int endHitFrame;

    [Tooltip("Total frame length of the clip")]
    public int totalFrames;

    //[Header("Combat Stats")]
    //public float damage;

    // normalizes timing conversions (0.0f to 1.0f)
    public float StartNormalized => totalFrames > 0 ? (float)startHitFrame / totalFrames : 0f;
    public float EndNormalized => totalFrames > 0 ? (float)endHitFrame / totalFrames : 1f;
}
