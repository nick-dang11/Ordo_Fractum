using UnityEngine;

public class PlayerAnimationRelay : MonoBehaviour
{
    public PlayerCombat playerCombat;

    public void EnableHitbox()
    {
        if (playerCombat != null)
            playerCombat.weaponHitbox.EnableHitbox(playerCombat.pendingDamage);
    }

    public void DisableHitbox()
    {
        if (playerCombat != null)
            playerCombat.weaponHitbox.DisableHitbox();
    }
}
