
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class EnemyDetectionGizmos : MonoBehaviour
{
    [Header("Enemy Detection Radius")]
    [SerializeField]
    private float detectionRadius = 10f;

    [SerializeField]
    private Color radiusColor = new Color(0f, 1f, 0f, 0.2f);

    [SerializeField]
    private float floorOffset = 0.05f;

    private void OnDrawGizmos()
    {
#if UNITY_EDITOR
        Vector3 center = transform.position +
            Vector3.up * floorOffset;

        Handles.color = radiusColor;

        Handles.DrawSolidDisc(
            center,
            Vector3.up,
            detectionRadius
        );

        Handles.color = Color.green;

        Handles.DrawWireDisc(
            center,
            Vector3.up,
            detectionRadius
        );
#endif
    }
}
