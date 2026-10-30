using UnityEngine;

public class StateManager : MonoBehaviour
{
    public BaseState currentState;

    public PlayerCombat combat;
    public PlayerInputManager inputManager;
    public PlayerMovement movement;
    public PlayerEvade playerEvade;
    public PlayerExecution playerExecution;
    public PlayerPosture posture;

    public IdleState idleState = new IdleState();
    public BlockingState blockState = new BlockingState();
    public AttackingState attackingState = new AttackingState();
    public EvadingState EvadingState = new EvadingState();
    public ExecutingState playerExecutingState = new ExecutingState();
    public PlayerStunnedState playerStunnedState = new PlayerStunnedState();

    private void Start()
    {
        if (combat == null) combat = GetComponent<PlayerCombat>();
        if (inputManager == null) inputManager = GetComponent<PlayerInputManager>();
        if (movement == null) movement = GetComponent<PlayerMovement>();
        if (playerEvade == null) playerEvade = GetComponent<PlayerEvade>();
        if (playerExecution == null) playerExecution = GetComponent<PlayerExecution>();
        if (posture == null) posture = GetComponent<PlayerPosture>();
        
        currentState = idleState;
        currentState.EnterState(this);
    }

    private void Update()
    {
        if(currentState != null)
        {
            currentState.UpdateState(this);
        }
    }

    public void SwitchState(BaseState newState, bool bypassPriority = false)
    {
        if(bypassPriority || newState.prioritylevel >= currentState.prioritylevel)
        {
            currentState.ExitState(this);
            currentState = newState;
            currentState.EnterState(this);
        }
        else
        {
            Debug.Log($"State switch blocked");
        }
    }
}