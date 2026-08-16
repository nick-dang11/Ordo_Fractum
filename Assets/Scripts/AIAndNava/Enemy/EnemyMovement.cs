using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyMovement : MonoBehaviour
{
    public Transform Target;
    public float updateSpeed = 0.1f;

    private NavMeshAgent Agent;

    private void Awake()
    {
        Agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        StartCoroutine(FollowTarget());
    }

    private IEnumerator FollowTarget()
    {
       WaitForSeconds wait = new WaitForSeconds(updateSpeed);

        while (enabled)
        {
            Agent.SetDestination(Target.transform.position);
            yield return wait;
        }
    }
}
