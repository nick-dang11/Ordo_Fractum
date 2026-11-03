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


    public void SwitchState(
        BaseState newState,
        bool bypassPriority = false)
    {
        if (newState == null || currentState == null)
            return;

        if (newState == currentState)
            return;

        bool legalAttackCancel = false;

        if (newState == blockState && combat != null)
        {
            // Never interrupt counterattacks with block.
            if (combat.IsCountering)
            {
                Debug.Log("[BlockCancel] Counter is active.");
                return;
            }

            bool attackActive =
                currentState == attackingState ||
                combat.IsAttacking ||
                combat.IsAttackAnimationPlaying();

            if (attackActive)
            {
                // Only newly pressed block input may cancel.
                if (!combat.BlockPressedThisFrame ||
                    !combat.CanBlockCancelCurrentAttack())
                {
                    Debug.Log(
                        "[BlockCancel] Block denied."
                    );
                    return;
                }

                legalAttackCancel = true;
            }
        }

        // Existing priority rule.
        if (!bypassPriority &&
            newState.prioritylevel < currentState.prioritylevel)
        {
            Debug.Log("State switch blocked.");
            return;
        }

        bool leavingAttacking =
            currentState == attackingState;

        // Exit old state first.
        currentState.ExitState(this);

        // Clean up if Animator was attacking while
        // the gameplay state had already returned to Idle.
        if (legalAttackCancel && !leavingAttacking)
        {
            combat.EndAttack();
        }

        currentState = newState;
        currentState.EnterState(this);
    }

}