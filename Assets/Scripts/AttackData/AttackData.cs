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

    public bool IsAttackValid()
    {
        if(startHitFrame < 0 || endHitFrame < 0 || totalFrames <= 0)
        {
            Debug.LogError($"Invalid frame data in {name}: startHitFrame={startHitFrame}, endHitFrame={endHitFrame}, totalFrames={totalFrames}");
            return false;
        }

        else if (startHitFrame >= endHitFrame)
        {
            Debug.LogError($"Invalid frame data in {name}: startHitFrame ({startHitFrame}) is greater than OR EQUAL TO endHitFrame ({endHitFrame})");
            return false;
        }
        else if (endHitFrame > totalFrames)
        {
            Debug.LogError($"Invalid frame data in {name}: endHitFrame ({endHitFrame}) exceeds totalFrames ({totalFrames})");
            return false;
        }
        return true;
    }
}
