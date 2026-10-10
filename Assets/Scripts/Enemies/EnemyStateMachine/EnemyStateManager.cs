using UnityEngine;

public class EnemyStateManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyCombat combat;
    [SerializeField] private EnemyPosture posture;

    public EnemyCombat Combat => combat;
    public EnemyPosture Posture => posture;

    public EnemyBaseState CurrentState { get; private set; }
    public EnemyBlockState BlockState { get; private set; }
    public EnemyStunnedState StunnedState { get; private set; }

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

        BlockState = new EnemyBlockState();
        StunnedState = new EnemyStunnedState();
    }

    private void Start()
    {
        SwitchState(BlockState);
    }

    private void Update()
    {
        CurrentState?.UpdateState(this);
    }

    public bool SwitchState(EnemyBaseState newState, bool force = false)
    {
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
}