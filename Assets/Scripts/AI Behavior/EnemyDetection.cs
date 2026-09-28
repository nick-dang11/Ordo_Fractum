
using UnityEngine;

public class EnemyDetection : MonoBehaviour
{
    private EnemyController controller;

    [SerializeField]
    private float detectionDistance = 8f;
    
    [SerializeField]
    private float fieldOfViewAngle = 90f; // can see 90 degrees cone

     //there a clear path between me and the player
    //layermask to know what layers it is allowed to hit
    [SerializeField]
    private LayerMask lineOfSightMask;

    private void Awake()
    {
       controller = GetComponent<EnemyController>();
    }

    public float DistanceToPlayer()
    {
        //calculates the distance between two postions
        return Vector3.Distance(
            transform.position,
            controller.Player.position
        );
        
    }

    public Vector3 DirectionToPlayer()
    {
        return (controller.Player.position - transform.position).normalized; //player position minus enemy position
        // gives us a direction vector pointing from the enemy toward the player 
        //.normalized keepts it direction but changes its legnth to 1
    }

    public bool IsPlayerDetected()
    {
        return DistanceToPlayer() <= detectionDistance;
    }

    public bool IsPlayerInFieldOfView()
    {
        //DirectionToPlayer: the direction the enemy is currently facing 
        float angleToPlayer = Vector3.Angle(
            transform.forward,
            DirectionToPlayer()

        );

        return angleToPlayer <= fieldOfViewAngle / 2f;
    }

    public bool HasLineOfSight()
    {
        Vector3 direction = DirectionToPlayer();

        RaycastHit hit;

        if(Physics.Raycast(
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
        return IsPlayerDetected() && IsPlayerInFieldOfView() && HasLineOfSight();
    }
}
