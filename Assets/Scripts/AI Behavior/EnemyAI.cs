using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    //Notes for me and groupmates

    //-----Refrences-----

    [Header("Refrences")]
    public Transform player;
    public Transform[] patrolPoints;

    [Header("Distance")]
    public float detectionDistance = 5f;
    public float escapeDistance = 10f;

    private NavMeshAgent agent;

    private int currentPatrolIndex = 0;
    private float waitTimer;

     //-----States-----
    private enum EnemyState
    {
        Patrol,
        Follow,
        ReturnToPatrol  
    }

    private EnemyState currentState = EnemyState.Patrol;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        if(patrolPoints.Length > 0)
        {
            agent.SetDestination(patrolPoints[currentPatrolIndex].position);
        }
    }

    void Update()
    {
        if(player == null)
           return;
           // Calculate the distance between the enemy and the player
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        //switch between states bases on the distance to the player
        switch (currentState)
        {
            case EnemyState.Patrol:
                Patrol(distanceToPlayer);
                break;
            
            case EnemyState.Follow:
                FollowPlayer(distanceToPlayer);
                break;
            
            case EnemyState.ReturnToPatrol:
                //ReturnToPatrol(distanceToPlayer);
                break;
        }
    }

    void Patrol(float distanceToPlayer)
    {
        //if the player is within detection distance, switch to follow state
        if(distanceToPlayer <= detectionDistance){
            currentState = EnemyState.Follow;
            return;
        }

        if(patrolPoints.Length == 0)
        {
            return;
        }

        if(!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            waitTimer += Time.deltaTime;

            if(waitTimer >= 2f)
            {
                waitTimer = 0f;
                GoToNextPatrolPoint();
            }
        
        }
    }

    void GoToNextPatrolPoint()
    {
        currentPatrolIndex++;

        if(currentPatrolIndex >= patrolPoints.Length)
        {
            currentPatrolIndex = 0;
        }

        agent.SetDestination(patrolPoints[currentPatrolIndex].position);
    }

    void FindNearestPatrolPoint()
    {
        float shortestDistance = Mathf.Infinity;
        int nearestPoint = 0;

        for(int i = 0; i < patrolPoints.Length; i++)
        {
            float distance = Vector3.Distance(transform.position, patrolPoints[i].position);

            if(distance < shortestDistance)
            {
                shortestDistance = distance;
                nearestPoint = i;
            }
        }

        currentPatrolIndex = nearestPoint;
    }

    void FollowPlayer(float distanceToPlayer)
    {
        //if the player is outside of the escape distance, switch to return to patrol state
        if(distanceToPlayer > escapeDistance)
        {
            Debug.Log("Player Escaped");

            currentState = EnemyState.ReturnToPatrol;

            FindNearestPatrolPoint();

            agent.SetDestination(patrolPoints[currentPatrolIndex].position);
        }
    }

    //Draws the range of the Enemy's radius in the scene
    //almost like python turutle graphics

    void OnDrawGizmosSelected()
    {
        //setup color and radius
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionDistance);

    }


    
}
