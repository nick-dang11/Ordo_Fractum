
using UnityEngine;

public class EquipSwordOnUnsheathExit : StateMachineBehaviour
{
    public override void OnStateExit(
        Animator animator,
        AnimatorStateInfo stateInfo,
        int layerIndex)
    {
        // Only equip the sword if the player
        // is still in combat.
        if (!animator.GetBool("InCombat"))
            return;

        SheathSwordToggle swordToggle =
            animator.GetComponent<SheathSwordToggle>();

        if (swordToggle != null)
        {
            swordToggle.EquipSword();

            Debug.Log(
                "[Weapon] Unsheath exited. Sword equipped."
            );
        }
    }
}
