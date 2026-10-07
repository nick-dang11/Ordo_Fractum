using UnityEngine;
using UnityEngine.AI;

public class EnemyStateManager : MonoBehaviour
{
    public EnemyBaseState currentState;

    [Header("References")]
    public Transform player;
    public NavMeshAgent agent;
    public EnemyCombat combat;
    public EnemyHealth health;
    public Animator animator;

    [Header("AI Settings")]
    public float detectionRange = 10f;
    public float attackRange = 3f;
    public float attackCooldown = 2f;
    public float stunDuration = 2f;
    public float maxAttackDuration = 2f;

    [HideInInspector] public float nextAttackTime;
    [HideInInspector] public float stunEndTime;
    [HideInInspector] public float attackStartTime;

    public EnemyIdleState idleState = new EnemyIdleState();
    public EnemyChaseState chaseState = new EnemyChaseState();
    public EnemyAttackState attackState = new EnemyAttackState();
    public EnemyStunnedState stunnedState = new EnemyStunnedState();

    private void Awake()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        if (combat == null)
            combat = GetComponent<EnemyCombat>();

        if (health == null)
            health = GetComponent<EnemyHealth>();

        if (animator == null)
            animator = GetComponent<Animator>();
    }

    private void Start()
    {
        currentState = idleState;
        currentState.EnterState(this);
    }

    private void Update()
    {
        currentState?.UpdateState(this);
    }

    public void SwitchState(
        EnemyBaseState newState,
        bool bypassPriority = false)
    {
        if (newState == null || newState == currentState)
            return;

        if (currentState == null ||
            bypassPriority ||
            newState.priorityLevel >= currentState.priorityLevel)
        {
            string previousState =
                currentState != null
                    ? currentState.GetType().Name
                    : "None";

            currentState?.ExitState(this);

            currentState = newState;

            Debug.Log(
                $"[Enemy FSM] {previousState} -> " +
                $"{currentState.GetType().Name}");

            currentState.EnterState(this);
        }
    }

    public bool CanDetectPlayer()
    {
        if (player == null)
            return false;

        return Vector3.Distance(
            transform.position,
            player.position) <= detectionRange;
    }

    public bool IsPlayerInAttackRange()
    {
        if (player == null)
            return false;

        return Vector3.Distance(
            transform.position,
            player.position) <= attackRange;
    }

    public bool CanAttack()
    {
        return Time.time >= nextAttackTime;
    }

    public void StartAttackCooldown()
    {
        nextAttackTime = Time.time + attackCooldown;
    }

    public void MoveToPlayer()
    {
        if (player == null ||
            agent == null ||
            !agent.enabled ||
            !agent.isOnNavMesh)
        {
            return;
        }

        agent.isStopped = false;
        agent.SetDestination(player.position);
    }

    public void StopMoving()
    {
        if (agent != null &&
            agent.enabled &&
            agent.isOnNavMesh)
        {
            agent.isStopped = true;
        }
    }

    public void FacePlayer()
    {
        if (player == null)
            return;

        Vector3 direction =
            player.position - transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f)
            return;

        Quaternion targetRotation =
            Quaternion.LookRotation(direction);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            10f * Time.deltaTime);
    }

    public void RequestStun()
    {
        stunEndTime = Time.time + stunDuration;

        SwitchState(stunnedState, true);
    }
}