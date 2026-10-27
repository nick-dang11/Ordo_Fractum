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

    [Header("Early Block Cancel Window")]

    [Tooltip("Allow blocking during the early attack frames")]
    public bool allowEarlyBlockCancel = false;

    [Tooltip("First frame where early blocking is allowed")]
    private int earlyBlockCancelStartFrame = 1;

    [Tooltip("Last frame where early blocking is allowed")]
    private int earlyBlockCancelEndFrame => startHitFrame - 1;

    [Header("Late Block Cancel Window")]

    [Tooltip("Allow blocking during the later attack frames")]
    public bool allowLateBlockCancel = false;

    [Tooltip("First frame where late blocking is allowed")]
    private int lateBlockCancelStartFrame => ((totalFrames - endHitFrame) / 2) + endHitFrame;

    [Tooltip("Last frame where late blocking is allowed")]
    private int lateBlockCancelEndFrame => totalFrames;

    //[Header("Combat Stats")]
    //public float damage;

    // normalizes timing conversions (0.0f to 1.0f)
    public float StartNormalized => totalFrames > 0 ? (float)startHitFrame / totalFrames : 0f;
    public float EndNormalized => totalFrames > 0 ? (float)endHitFrame / totalFrames : 1f;

    public float EarlyBlockCancelStartNormalized => totalFrames > 0 ? (float)earlyBlockCancelStartFrame / totalFrames : 0f;

    public float EarlyBlockCancelEndNormalized => totalFrames > 0 ? (float)earlyBlockCancelEndFrame / totalFrames : 0f;

    public float LateBlockCancelStartNormalized => totalFrames > 0 ? (float)lateBlockCancelStartFrame / totalFrames : 0f;

    public float LateBlockCancelEndNormalized => totalFrames > 0 ? (float)lateBlockCancelEndFrame / totalFrames : 0f;

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

    private bool IsValidCancelWindow(int start, int end)
    {
        if (totalFrames <= 0 ||
            start < 1 ||
            end < start ||
            end > totalFrames)
        {
            return false;
        }
        bool overlapsHitFrames =
            start <= endHitFrame &&
            end >= startHitFrame;

        return !overlapsHitFrames;
    }
    public bool AreBlockCancelWindowsValid()
    {
        if (allowEarlyBlockCancel &&
            !IsValidCancelWindow(
                earlyBlockCancelStartFrame,
                earlyBlockCancelEndFrame))
        {
            return false;
        }
        if (allowLateBlockCancel &&
            !IsValidCancelWindow(
                lateBlockCancelStartFrame,
                lateBlockCancelEndFrame))
        {
            return false;
        }
        if (allowEarlyBlockCancel &&
            allowLateBlockCancel &&
            earlyBlockCancelEndFrame >=
                lateBlockCancelStartFrame)
        {
            return false;
        }

        return true;
    }
    public bool IsWithinBlockCancelWindow(
        float normalizedTime)
    {
        if (totalFrames <= 0 ||
            float.IsNaN(normalizedTime) ||
            normalizedTime < 0f ||
            normalizedTime > 1f ||
            !AreBlockCancelWindowsValid())
        {
            return false;
        }
        int currentFrame = Mathf.Clamp(
            Mathf.FloorToInt(normalizedTime * totalFrames),
            1,
            totalFrames
        );
        if (allowEarlyBlockCancel &&
            currentFrame >= earlyBlockCancelStartFrame &&
            currentFrame <= earlyBlockCancelEndFrame)
        {
            return true;
        }
        if (allowLateBlockCancel &&
            currentFrame >= lateBlockCancelStartFrame &&
            currentFrame <= lateBlockCancelEndFrame)
        {
            return true;
        }
        return false;
    }
}
