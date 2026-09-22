using UnityEngine;
using UnityEngine.AI;

public class EnemyMovements : MonoBehaviour
{
    private NavMeshAgent agent;
    private EnemyController enemyController; // Connects to the EnemyController script to access the player transform

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        enemyController= GetComponent<EnemyController>();
    }

    public void MoveTo(Vector3 location){
        agent.isStopped = false;
        agent.SetDestination(location);

    }

    public void Chase()
    {
        MoveTo(enemyController.Player.position);
    }

    public void Stop()
    {
        agent.isStopped = true;
    }
    
}
