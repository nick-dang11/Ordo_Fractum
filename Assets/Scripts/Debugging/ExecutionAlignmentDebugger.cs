using UnityEngine;

public class ExecutionAlignmentDebugger : MonoBehaviour
{
    [SerializeField] private Transform playerRig;
    [SerializeField] private Transform enemyRig;

    [ContextMenu("Calculate Execution Offset")]
    private void CalculateExecutionOffset()
    {
        if (playerRig == null || enemyRig == null)
        {
            Debug.LogWarning("Assign both rig references.");
            return;
        }

        Vector3 localPosition =
            enemyRig.InverseTransformPoint(playerRig.position);

        Quaternion localRotation =
            Quaternion.Inverse(enemyRig.rotation) * playerRig.rotation;

        Debug.Log(
            $"Execution Anchor Local Position: {localPosition}\n" +
            $"Execution Anchor Local Rotation: {localRotation.eulerAngles}"
        );
    }
}