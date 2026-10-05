using UnityEngine;

public class EnemyWeaponHitbox : WeaponHitbox
{
    protected override bool IsValidTarget(Collider other)
    {
        BlockZone blockZone = other.GetComponent<BlockZone>();
        if (blockZone != null)
        {
            return blockZone.Owner != null;
        }
        return other.GetComponentInParent<HealthSystem>() != null;
    }

    protected override void HandleHit(Collider other)
    {
        PlayerCombat playerCombat = ResolvePlayerCombat(other);
        if (playerCombat == null)
        {
            Debug.LogWarning($"[EnemyWeaponHitbox] Could not resolve " + $"PlayerCombat from '{other.name}'.", other);
            return;
        }
        ResolvePlayerImpact(playerCombat, other);
    }
    private PlayerCombat ResolvePlayerCombat(Collider other)
    {
        BlockZone blockZone = other.GetComponent<BlockZone>();
        if (blockZone != null)
        {
            Debug.Log($"[EnemyWeaponHitbox] BlockZone contact: " + $"{other.name}");
            return blockZone.Owner;
        }
        PlayerCombat playerCombat = other.GetComponent<PlayerCombat>();
        if (playerCombat != null)
        {
            Debug.Log($"[EnemyWeaponHitbox] Player body contact: " + $"{other.name}");
        }
        return playerCombat;
    }
    private void ResolvePlayerImpact(PlayerCombat playerCombat, Collider contactedCollider)
    {
        if (playerCombat == null)
            return;
        if (playerCombat.IsBlocking())
        {
            float timeSinceBlock = Time.time - playerCombat.lastBlockTime;
            Debug.Log($"[Defense] Contact={contactedCollider.name}, " + $"TimeSinceBlock={timeSinceBlock}");
            if (timeSinceBlock <= playerCombat.deflectWindow)
            {
                Debug.Log("[Defense] Result = DEFLECT");
                playerCombat.TriggerDeflectFeedback();
                return;
            }
            Debug.Log("[Defense] Result = Block");
            playerCombat.TriggerBlockFeedback();
            return;
        }
        HealthSystem healthSystem = playerCombat.GetComponentInParent<HealthSystem>();
        if (healthSystem == null)
        {
            Debug.LogWarning("[EnemyWeaponHitbox] Player has no HealthSystem.", playerCombat);
            return;
        }
        Debug.Log("[Defense] Result = HIT");
        healthSystem.TakeDamage(Mathf.RoundToInt(CurrentDamage)); // rounding to prevent unforseen errors
        SpawnBloodVFX(contactedCollider);
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
