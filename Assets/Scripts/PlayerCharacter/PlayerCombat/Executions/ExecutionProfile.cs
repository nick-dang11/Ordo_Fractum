using UnityEngine;

[CreateAssetMenu(
    fileName = "ExecutionProfile",
    menuName = "Combat/Execution Profile"
)]
public class ExecutionProfile : ScriptableObject
{
    [Header("Paired Animations")]
    [SerializeField] private AnimationClip playerClip;
    [SerializeField] private AnimationClip enemyClip;

    [Header("Timing Adjustments")]
    [SerializeField, Min(0f)]
    private float playerRecoveryOffset = 0f;

    [SerializeField, Min(0f)]
    private float enemyDeathOffset = 0f;

    public float PlayerRecoveryTime =>
        playerClip != null
            ? playerClip.length + playerRecoveryOffset
            : 0f;

    public float EnemyDeathTime =>
        enemyClip != null
            ? enemyClip.length + enemyDeathOffset
            : 0f;
}