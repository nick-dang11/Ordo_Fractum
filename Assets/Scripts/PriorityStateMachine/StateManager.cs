using UnityEngine;

public class StateManager : MonoBehaviour
{
    public BaseState currentState;

    public PlayerCombat combat;

    public IdleState idleState = new IdleState();
    public BlockingState blockState = new BlockingState();
    public AttackingState attackingState = new AttackingState();

    private void Start()
    {
        if (combat == null) combat = GetComponent<PlayerCombat>();
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