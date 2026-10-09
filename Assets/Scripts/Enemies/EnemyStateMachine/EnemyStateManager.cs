using UnityEngine;

public class EnemyStateManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyCombat combat;

    public EnemyCombat Combat => combat;

    public EnemyBaseState CurrentState { get; private set; }
    public EnemyBlockState BlockState { get; private set; }

    public void Awake()
    {
        if(combat == null)
        {
            combat = GetComponent<EnemyCombat>();
        }

        BlockState = new EnemyBlockState();
    }

    private void Start()
    {
        SwitchState(BlockState);
    }

    private void Update()
    {
        CurrentState?.UpdateState(this);
    }

    public bool SwitchState(EnemyBaseState newState)
    {
        if (newState == null)
        {
            Debug.LogWarning($"EnemyStateManager cannot switch {name} to a null state.", this);

            return false;
        }

        if (CurrentState == newState) return false;

        string previousState = CurrentState != null ? CurrentState.GetType().Name : "None";

        CurrentState?.ExitState(this);
        
        CurrentState = newState;
        Debug.Log($"{name}: {previousState} -> {CurrentState.GetType().Name}");

        CurrentState.EnterState(this);

        return true;
    }
}