using UnityEngine;

public class PlayerWeaponHitbox : WeaponHitbox
{
    protected override bool IsValidTarget(Collider other)
    {
        return other.GetComponentInParent<EnemyHealth>() != null;
    }

    protected override void HandleHit(Collider other)
    {
        EnemyHealth enemyHealth = other.GetComponentInParent<EnemyHealth>();

        if (enemyHealth == null) return;

        enemyHealth.TakeDamage(CurrentDamage);

        SpawnBloodVFX(other);
    }

    [ContextMenu("DEBUG Enable Hitbox")]
    private void DebugEnableHitbox()
    {
        EnableHitbox(10f);
    }

    [ContextMenu("DEBUG Disable Hitbox")]
    private void DebugDisableHitbox()
    {
        DisableHitbox();
    }
}