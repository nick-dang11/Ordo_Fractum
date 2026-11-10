using UnityEngine;
using UnityEngine.AI;

public class EnemyAIAnimation : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
   private Animator EnemyAnimator;
   private NavMeshAgent EnemyNavMeshAgent;

   private void Awake()
    {
        EnemyAnimator = GetComponent<Animator>();
        EnemyNavMeshAgent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        if (EnemyNavMeshAgent == null || EnemyAnimator == null || !EnemyNavMeshAgent.isOnNavMesh) return;

        float desiredSpeed = EnemyNavMeshAgent.isStopped
            ? 0f
            : EnemyNavMeshAgent.desiredVelocity.magnitude;

        EnemyAnimator.SetFloat(
            "Speed",
            desiredSpeed,
            0.1f,
            Time.deltaTime
        );
    }

    private float nextDebugTime;

    private void LateUpdate()
    {
        if (EnemyAnimator == null ||
            EnemyNavMeshAgent == null ||
            !EnemyNavMeshAgent.isOnNavMesh ||
            Time.time < nextDebugTime)
        {
            return;
        }


    }
}
    