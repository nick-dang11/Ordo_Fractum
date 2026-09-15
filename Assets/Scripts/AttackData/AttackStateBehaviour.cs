using UnityEngine;

public class AttackStateBehaviour : StateMachineBehaviour
{
    [SerializeField] private AttackData attackData;
    private AttackTimelineDriver timelineDriver;
    private int attackToken;
    private int lightAttackDamage = 10;

    public override void OnStateEnter(
        Animator animator,
        AnimatorStateInfo stateInfo,
        int layerIndex)
    {
        timelineDriver = animator.GetComponent<AttackTimelineDriver>();

        if (timelineDriver == null)
        {
            Debug.LogWarning($"AttackStateBehavior on {animator.name} could not find AttackTimelineDriver.");
            return;
        }

        if (attackData == null)
        {
            Debug.LogWarning($"AttackStateBehavior on {animator.name} has no AttackData assigned." +
                $"Check Player Animator's behavior for this attack.");
            return;
        }

        attackToken = timelineDriver.BeginAttack(attackData);
        Debug.Log($"Entered attack state with AttackData: {attackData.name}");
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (timelineDriver == null || attackData == null) return;

        timelineDriver.EvaluateAttack(attackToken, stateInfo.normalizedTime, lightAttackDamage); // temp 10 damage
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (timelineDriver == null) return;
        timelineDriver.EndAttack(attackToken);
    }
}
