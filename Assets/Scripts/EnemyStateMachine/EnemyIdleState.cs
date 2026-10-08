// using UnityEngine;

// public class EnemyIdleState : EnemyBaseState
// {
//     public EnemyIdleState()
//     {
//         priorityLevel = 0;
//     }

//     public override void EnterState(EnemyStateManager context)
//     {
//         Debug.Log("[Enemy FSM] Entered Idle");

//         context.StopMoving();
//     }

//     public override void UpdateState(EnemyStateManager context)
//     {
//         if (context.CanDetectPlayer())
//         {
//             context.SwitchState(context.chaseState);
//         }
//     }
// }