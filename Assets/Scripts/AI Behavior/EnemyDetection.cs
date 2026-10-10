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

    [SerializeField]
    private Transform visionDirection;

    [SerializeField]
    private float lostSightDelay = 5.0f;// how long the enemy will continue to "see" the player after losing line of sight

    private float lastTimePlayerSeen = Mathf.NegativeInfinity; // time when the player was last seen

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

       Vector3 origin = visionDirection != null ? visionDirection.position : transform.position;

       Vector3 target = controller.Player.position + Vector3.up; // Aim for the player's chest height

       Vector3 directionToPlayer = (target - origin).normalized;

       Vector3 forwardDirection = visionDirection != null ? visionDirection.forward : transform.forward;

       float angleToPlayer = Vector3.Angle(
            forwardDirection,
            directionToPlayer
        );

        Debug.Log(
           "EnemyForward: " + forwardDirection +
            " | AngleToPlayer: " + angleToPlayer +
            "| FOV limit: " + fieldOfViewAngle / 2f
        );

        return angleToPlayer <= fieldOfViewAngle / 2f;
    }

    public bool HasLineOfSight()
    {
        if (controller == null || controller.Player == null)
        {
            return false;
        }

       Vector3 origin = visionDirection != null ? visionDirection.position : transform.position;

       Vector3 target  = controller.Player.position + Vector3.up; // Aim for the player's chest height

       Vector3 direction = target - origin;

       float distance = direction.magnitude;

       if(distance  <= 0f)
        {
            return false;
        }

        RaycastHit hit;

        if (Physics.Raycast(
            origin,
            direction.normalized,
            out hit,
            distance,
            lineOfSightMask))
        {
           // Debug.Log("Raycast hit: " + hit.transform.name);
            return hit.transform == controller.Player 
                || hit.transform.IsChildOf(controller.Player);
        }

        return false;
    }

    public bool CanSeePlayer()
    {
        return IsPlayerDetected()
            && IsPlayerInFieldOfView()
            && HasLineOfSight();

        /*
        1) is the player within the detection distance?
        2) is the player within the field of view angle?
        3) is there a clear line of sight to the player?


        */
    }

    public bool ShouldChasePlayer()
    {
        if(CanSeePlayer())
        {
            lastTimePlayerSeen = Time.time;
            Debug.Log("Player Visible:Timer reset to: " + lastTimePlayerSeen);
            return true;
        }

        if(float.IsNegativeInfinity(lastTimePlayerSeen))
        {
            return false;
        }

        float timeSinceLastSeen = Time.time - lastTimePlayerSeen;

        float timeRemaining = Mathf.Max(0f, lostSightDelay - timeSinceLastSeen);

        Debug.Log("Lost sight|Time elapsed: " + 
        timeSinceLastSeen.ToString("F2") + 
        " | Time remaining: " + 
        timeRemaining.ToString("F2") + 'S');
        return timeSinceLastSeen <= lostSightDelay;

       
    }

    private void OnDrawGizmosSelected()
    {
        Transform origin = visionDirection != null ? visionDirection : transform;

        Gizmos.color = Color.green;
        Gizmos.DrawLine(
            origin.position, origin.position + origin.forward * 5f
        );
    }

   /* private void Update()
    {
        Debug.Log(
            "CanSeePlayer: " + CanSeePlayer() +
            " | DistanceToPlayer: " + DistanceToPlayer() +
            " | FOV " + IsPlayerInFieldOfView() +
            " | LOS: " + HasLineOfSight()
        );
    }*/

}