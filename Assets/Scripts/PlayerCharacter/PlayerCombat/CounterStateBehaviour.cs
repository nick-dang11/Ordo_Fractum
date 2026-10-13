using UnityEngine;

public class CounterStateBehaviour : StateMachineBehaviour
{
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        PlayerCombat playerCombat = animator.GetComponentInParent<PlayerCombat>();

        if (playerCombat == null) { return; }
        playerCombat.EndCounter();
    }
}