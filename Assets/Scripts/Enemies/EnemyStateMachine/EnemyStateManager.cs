using UnityEngine;
using Unity.Behavior;
using UnityEngine.AI;

public class EnemyStateManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyCombat combat;
    [SerializeField] private EnemyPosture posture;
    [SerializeField] private BehaviorGraphAgent behaviorAgent;
    [SerializeField] private NavMeshAgent navAgent;

    public bool AllowsAI =>
        CurrentState != StunnedState &&
        CurrentState != ExecutedState &&
        CurrentState != DeathState;

    public void RefreshAI()
    {
        SetAIEnabled(AllowsAI);
    }

    public EnemyCombat Combat => combat;
    public EnemyPosture Posture => posture;

    public EnemyBaseState CurrentState { get; private set; }
    public EnemyNeutralState NeutralState { get; private set; }
    public EnemyBlockState BlockState { get; private set; }
    public EnemyStunnedState StunnedState { get; private set; }
    public EnemyDeathState DeathState { get; private set; }
    public EnemyExecutedState ExecutedState { get; private set; }


    public void Awake()
    {
        if (combat == null)
        {
            combat = GetComponent<EnemyCombat>();
        }

        if (posture == null)
        {
            posture = GetComponent<EnemyPosture>();
        }

        if(behaviorAgent == null)
        {
            behaviorAgent = GetComponent<BehaviorGraphAgent>();
        }

        if(navAgent == null)
        {
            navAgent = GetComponent<NavMeshAgent>();
        }

        NeutralState = new EnemyNeutralState();
        BlockState = new EnemyBlockState();
        StunnedState = new EnemyStunnedState();
        DeathState = new EnemyDeathState();
        ExecutedState = new EnemyExecutedState();
    }

    private void Start()
    {
        SwitchState(NeutralState);
    }

    private void Update()
    {
        CurrentState?.UpdateState(this);
    }

    public bool SwitchState(EnemyBaseState newState, bool force = false)
    {
        if (CurrentState == DeathState) return false; // death has utmost prio!

        if (newState == null || newState == CurrentState)
        {
            Debug.LogWarning($"EnemyStateManager cannot switch {name} to a null or own state.", this);

            return false;
        }

        if (!force && CurrentState != null && newState.PriorityLevel < CurrentState.PriorityLevel) return false;

        string previousState = CurrentState != null ? CurrentState.GetType().Name : "None";

        CurrentState?.ExitState(this);
        CurrentState = newState;
        Debug.Log($"{name}: {previousState} -> {CurrentState.GetType().Name}");
        CurrentState.EnterState(this);
        RefreshAI();
        return true;
    }

    public void RequestStun()
    {
        SwitchState(StunnedState);
    }

    public void RecoverFromStun()
    {
        Posture?.RecoverFromPostureBreak(0.8f);
    }

    public void RequestDeath()
    {
        SwitchState(DeathState, true);
    }

    public bool RequestExecution()
    {
        if(CurrentState != StunnedState) return false;

        return SwitchState(ExecutedState);
    }

    public void SetAIEnabled(bool enabled)
    {
        if (!enabled)
        {
            if (behaviorAgent != null) behaviorAgent.enabled = false;

            if (navAgent != null && navAgent.isOnNavMesh)
            {
                navAgent.isStopped = true;
                navAgent.ResetPath();
            }
        }

        else
        {
            if (navAgent != null && navAgent.isOnNavMesh)
            {
                navAgent.isStopped = false;
            }

            if (behaviorAgent != null)
            {
                behaviorAgent.enabled = true;
            }
        }
    }
}