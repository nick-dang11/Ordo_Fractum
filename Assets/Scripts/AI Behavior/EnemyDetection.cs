using UnityEngine;

public class EnemyDetection : MonoBehaviour
{
    private EnemyAIController controller;

    [SerializeField]
    private float detectionDistance = 8f;

    [SerializeField]
    private float fieldOfViewAngle = 90f;

    [SerializeField]
    private LayerMask lineOfSightMask;

    private void Awake()
    {
        controller = GetComponent<EnemyAIController>();
    }

    public float DistanceToPlayer()
    {
        if (controller == null || controller.Player == null)
        {
            return Mathf.Infinity;
        }

        return Vector3.Distance(
            transform.position,
            controller.Player.position
        );
    }

    public Vector3 DirectionToPlayer()
    {
        if (controller == null || controller.Player == null)
        {
            return Vector3.zero;
        }

        return (
            controller.Player.position - transform.position
        ).normalized;
    }

    public bool IsPlayerDetected()
    {
        return DistanceToPlayer() <= detectionDistance;
    }

    public bool IsPlayerInFieldOfView()
    {
        if (controller == null || controller.Player == null)
        {
            return false;
        }

        float angleToPlayer = Vector3.Angle(
            transform.forward,
            DirectionToPlayer()
        );

        return angleToPlayer <= fieldOfViewAngle / 2f;
    }

    public bool HasLineOfSight()
    {
        if (controller == null || controller.Player == null)
        {
            return false;
        }

        Vector3 direction = DirectionToPlayer();

        RaycastHit hit;

        if (Physics.Raycast(
            transform.position,
            direction,
            out hit,
            detectionDistance,
            lineOfSightMask))
        {
            return hit.transform == controller.Player;
        }

        return false;
    }

    public bool CanSeePlayer()
    {
        return IsPlayerDetected()
            && IsPlayerInFieldOfView()
            && HasLineOfSight();
    }
}