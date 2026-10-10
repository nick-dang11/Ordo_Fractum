using UnityEngine;
using UnityEngine.AI;

public class EnemyMovement : MonoBehaviour
{
    private NavMeshAgent agent;
    private EnemyAIController controller; // Connects to the EnemyController script to access the player transform
    // Access the public property from EnemyController
    [SerializeField] 
    private Transform[] patrolPoints; // Array of patrol points for the enemy to move between

    private int currentPatrolIndex = 0; // Index to keep track of the current patrol point

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        controller = GetComponent<EnemyAIController>();
    }

    public void MoveTo(Vector3 location){

        if(agent == null || !agent.isOnNavMesh)
        {
            Debug.LogWarning("NavMeshAgent is not available or not on a valid NavMesh in EnemyMovement");
            return;
        }

        agent.isStopped = false;
        bool accepted =  agent.SetDestination(location);// navigates toward this position

       /* Debug.Log("Desitination accepted" + accepted + 
        "| Requested Location: " + location +
        "| Agent Distance: " + agent.remainingDistance +
        "| Enemy Position: " + transform.position
        ); */

    }

    public void Chase()
    {
        if(controller == null || controller.Player == null)
        {
            Debug.LogWarning("EnemyAIController or Player is null in EnemyMovement");
            return;
        }

        //Debug.Log("CHASE DESTINATION:" + controller.Player.position);
        MoveTo(controller.Player.position);
    }

    public void Stop()
    {
        agent.isStopped = true;
    }

    public void Patrol()
    {
        if(patrolPoints.Length == 0)
        {
            return;
        }

        Debug.Log("PATROL is SETTING DESTINATION");
        MoveTo(patrolPoints[currentPatrolIndex].position);

        if (ReachedDestination())
        {
            NextPatrolPoint();
        }
    }

    public bool ReachedDestination()
    {
        if(agent == null || !agent.isOnNavMesh)
        {
            return false; // agent is not on a valid NavMesh
        }

        if (agent.pathPending)
        {
            return false; // path is being traveled and we have not arrived yet
        }
        //remainingDistance is how much farther does the agent have to travel from max distance to 0(reach your detination)

        if(float.IsInfinity(agent.remainingDistance))
        {
            return false; // remaining distance is not valid
        }
        
        return agent.remainingDistance <= agent.stoppingDistance; 
    }

    public void NextPatrolPoint()
    {
        currentPatrolIndex++;

        if(currentPatrolIndex >= patrolPoints.Length)
        {
            currentPatrolIndex = 0;
        }
    }

    public void ReturnToPatrol()
    {
        if(patrolPoints.Length == 0)
        {
            return;
        }

        MoveTo(patrolPoints[currentPatrolIndex].position);
    }

    
}

//add patrol, reached destination, next patrol points and returnm methods
//add array to hold patrol points in the scene.