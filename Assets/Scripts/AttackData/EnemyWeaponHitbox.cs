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
        return other.GetComponentInParent<PlayerHealth>() != null;
    }

    protected override void HandleHit(Collider other, CombatHitContext context)
    {
        PlayerCombat playerCombat = ResolvePlayerCombat(other);
        if (playerCombat == null)
        {
            Debug.LogWarning($"[EnemyWeaponHitbox] Could not resolve " + $"PlayerCombat from '{other.name}'.", other);
            return;
        }
        ResolvePlayerImpact(playerCombat, other, context);
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
    private void ResolvePlayerImpact(PlayerCombat playerCombat, Collider contactedCollider, CombatHitContext context)
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
                playerCombat.TriggerDeflectFeedback(context);
                if (context.AttackerRoot != null)
                {
                    EnemyCombat enemyCombat = context.AttackerRoot.GetComponent<EnemyCombat>();
                    if (enemyCombat != null)
                    {
                        enemyCombat.HandleDeflected();
                        playerCombat.OpenCounterWindow(enemyCombat);
                    }
                    else { Debug.LogWarning($"[EnemyWeaponHitbox] " + $"{context.AttackerRoot.name} has no EnemyCombat."); }
                }
                else { Debug.LogWarning("[EnemyWeaponHitbox] Deflect occurred, " + "but AttackerRoot was null.");}
                ApplyDeflectPostureDamage(playerCombat, context);
                return;
            }
            Debug.Log("[Defense] Result = Block");
            playerCombat.TriggerBlockFeedback(context);
            return;
        }
        PlayerHealth healthSystem = playerCombat.GetComponentInParent<PlayerHealth>();
        if (healthSystem == null)
        {
            Debug.LogWarning("[EnemyWeaponHitbox] Player has no PlayerHealth.", playerCombat);
            return;
        }
        Debug.Log("[Defense] Result = HIT");
        healthSystem.TakeDamage(Mathf.RoundToInt(CurrentDamage)); // rounding to prevent unforseen errors
        SpawnBloodVFX(contactedCollider);
    }
    private void ApplyDeflectPostureDamage(PlayerCombat playerCombat, CombatHitContext context)
    {
        if (playerCombat == null) { return; }

        if (context.AttackerRoot == null)
        {
            Debug.LogWarning("[EnemyWeaponHitbox] Cannot apply deflect posture: " + "AttackerRoot is missing.");
            return;
        }

        EnemyPosture postureMeter = context.AttackerRoot.GetComponentInChildren<EnemyPosture>(true);

        if (postureMeter == null)
        {
            Debug.LogWarning($"[EnemyWeaponHitbox] {context.AttackerRoot.name} " + $"has no EnemyPostureMeter.");
            return;
        }

        postureMeter.ApplyPostureDamage(playerCombat.DeflectPostureDamage);
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
