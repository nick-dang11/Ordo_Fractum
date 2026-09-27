using UnityEngine;

public class EnemyWeaponHitbox : WeaponHitbox
{
    protected override bool IsValidTarget(Collider other)
    {
        return other.GetComponentInParent<HealthSystem>() != null;
    }

    protected override void HandleHit(Collider other)
    {
        HealthSystem healthSystem = other.GetComponentInParent<HealthSystem>();
        if (healthSystem == null) return;

        healthSystem.TakeDamage(Mathf.RoundToInt(CurrentDamage)); // rounding to prevent unforseen errors

        SpawnBloodVFX(other);
    }

    [ContextMenu("DEBUG Enable Hitbox")]
    private void DebugEnableHitbox()
    {
        EnableHitbox(1f);
    }

    [ContextMenu("DEBUG Disable Hitbox")]
    private void DebugDisableHitbox()
    {
        DisableHitbox();
    }
}
