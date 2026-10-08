// using UnityEngine;

// public class EnemyAttackState : EnemyBaseState
// {
//     public EnemyAttackState()
//     {
//         priorityLevel = 1;
//     }

//     public override void EnterState(EnemyStateManager context)
//     {
//         Debug.Log("[Enemy FSM] Entered Attack");

//         context.StopMoving();
//         context.FacePlayer();

//         context.attackStartTime = Time.time;
//         context.StartAttackCooldown();

//         if (context.combat != null)
//         {
//             //context.combat.StartAttack();
//         }

//         if (context.animator != null)
//         {
//             context.animator.SetTrigger("Attack");
//         }
//     }

//     public override void UpdateState(EnemyStateManager context)
//     {
//         context.FacePlayer();

//         if (context.combat == null)
//         {
//             LeaveAttack(context);
//             return;
//         }

//         // Animation event should eventually call EndAttack().
//         if (!context.combat.IsAttacking &&
//             Time.time > context.attackStartTime + 0.05f)
//         {
//             LeaveAttack(context);
//             return;
//         }

//         // Safety fallback in case an animation event is missing.
//         if (Time.time >=
//             context.attackStartTime +
//             context.maxAttackDuration)
//         {
//             context.combat.EndAttack();

//             LeaveAttack(context);
//         }
//     }

//     private void LeaveAttack(EnemyStateManager context)
//     {
//         if (context.CanDetectPlayer())
//         {
//             context.SwitchState(
//                 context.chaseState,
//                 true);
//         }
//         else
//         {
//             context.SwitchState(
//                 context.idleState,
//                 true);
//         }
//     }

//     public override void ExitState(EnemyStateManager context)
//     {
//         if (context.combat != null)
//         {
//             //context.combat.DisableWeaponHitbox();
//         }
//     }
// }