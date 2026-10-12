using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public abstract class WeaponHitbox : MonoBehaviour
{
    private Collider weaponCollider;
    private Renderer weaponRenderer;
    [SerializeField] private GameObject PlayerObject;

    [Header("Debug Visualization")]
    [SerializeField] private bool showHitboxState = true;
    [SerializeField] private Color activeColor = Color.red;
    [SerializeField] private Color inactiveColor = Color.grey;

    [Header("Hit VFX")]
    [SerializeField] private GameObject bloodVFXPrefab;
    [SerializeField] private float bloodVFXLifetime = 2f;
    [SerializeField] private bool logHitRegistration = false;

    [SerializeField] private GameObject ownerRoot;
    private AttackData currentAttackData;
    private float currentPostureDamage;
    public GameObject OwnerRoot => ownerRoot;

    private bool isHitboxActive = false;
    private float currentDamage;

    private readonly HashSet<GameObject> hitTargets = new HashSet<GameObject>();
    // Track root GameObjects instead of individual colliders.
    // One combat target may contain multiple colliders, but should only
    // resolve one hit per hitbox activation window.

    protected float CurrentDamage => currentDamage;
    protected bool IsHitboxActive => isHitboxActive;

    protected virtual void Awake()
    {
        weaponCollider = GetComponent<Collider>();
        weaponCollider.enabled = false;
        weaponRenderer = GetComponent<Renderer>();
        if (weaponRenderer == null)
        {
            weaponRenderer = GetComponentInChildren<Renderer>();
        }
        ResolveOwner();
        SetWeaponColor(inactiveColor);
    }

    public virtual void EnableHitbox(float damage, AttackData attackData = null, float postureDamage = 0f)
    {
        if (ownerRoot != null)
        {
            EnemyCombat enemyCombat = ownerRoot.GetComponent<EnemyCombat>();
            if (enemyCombat != null && enemyCombat.CurrentAttackInvalidated)
            {
                Debug.Log($"[WeaponHitbox] Hitbox activation ignored for " + $"{ownerRoot.name} because the current attack " + $"was invalidated.");
                return;
            }
        }
        hitTargets.Clear();

        isHitboxActive = true;
        weaponCollider.enabled = true;

        currentDamage = damage;
        currentAttackData = attackData;
        currentPostureDamage = postureDamage;

        Debug.Log($"Hitbox enabled with damage: {currentDamage}");

        SetWeaponColor(activeColor);
    }

    public virtual void DisableHitbox()
    {
        isHitboxActive = false;
        weaponCollider.enabled = false;
        hitTargets.Clear();

        SetWeaponColor(inactiveColor);
    }
    protected virtual void OnDisable()
    {
        if (weaponCollider != null)
        {
            weaponCollider.enabled = false;
        }

        isHitboxActive = false;
        hitTargets.Clear();

        SetWeaponColor(inactiveColor);
    }


    private void SetWeaponColor(Color color)
    {
        if (!showHitboxState || weaponRenderer == null)
        {
            return;
        }
        weaponRenderer.material.color = color; // may need material.SetColor("_BaseColor", color) with URP/HDRP
    }

    protected void SpawnBloodVFX(Collider target)
    {
        if (bloodVFXPrefab == null)
        {
            return;
        }

        Vector3 hitpoint = target.ClosestPoint(weaponCollider.bounds.center);

        GameObject bloodVFX = Instantiate(
            bloodVFXPrefab,
            hitpoint,
            Quaternion.identity
        );

        Destroy(bloodVFX, bloodVFXLifetime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isHitboxActive) return;
        if (other == null) return;

        GameObject targetRoot = ResolveTargetRoot(other);

        if (targetRoot == null) return; // prevent self damage for weapon owner

        if (hitTargets.Contains(targetRoot))
        {
            if (logHitRegistration)
            {
                Debug.Log($"[WeaponHitbox] Duplicate contact ignored for " + $"target '{targetRoot.name}' from collider '{other.name}'.", this);
            }
            return;
        }
        if (!IsValidTarget(other)) return;
        CombatHitContext context = BuildHitContext(other);
        hitTargets.Add(targetRoot);
        if (logHitRegistration)
        {
            Debug.Log($"[WeaponHitbox] Registered target " + $"'{targetRoot.name}' from collider '{other.name}'.", this);
        }
        HandleHit(other, context);
    }

    private void ResolveOwner()
    {
        MonoBehaviour[] behaviours = GetComponentsInParent<MonoBehaviour>(true);
        foreach (MonoBehaviour behaviour in behaviours)
        {
            if (behaviour is IAttackDamageSource)
            {
                if (ownerRoot == null) { ownerRoot = behaviour.gameObject; }
                return;
            }
        }
        Debug.LogWarning($"[WeaponHitbox] Could not resolve an attack owner for {name}.");
    }

    private GameObject ResolveTargetRoot(Collider other)
    {
        PlayerCombat playerCombat = other.GetComponentInParent<PlayerCombat>();

        if (playerCombat != null) { return playerCombat.gameObject; }

        EnemyCombat enemyCombat = other.GetComponentInParent<EnemyCombat>();

        if (enemyCombat != null) { return enemyCombat.gameObject; }

        return other.transform.root.gameObject;
    }
    private CombatHitContext BuildHitContext(Collider other)
    {
        GameObject targetRoot = ResolveTargetRoot(other);

        Vector3 impactPoint = other.ClosestPoint(weaponCollider.bounds.center);

        return new CombatHitContext(
            attackerRoot: ownerRoot,
            targetRoot: targetRoot,
            weaponCollider: weaponCollider,
            attackData: currentAttackData,
            damage: currentDamage,
            postureDamage: currentPostureDamage,
            impactPoint: impactPoint
        );
    }

    protected abstract bool IsValidTarget(Collider other);

    protected abstract void HandleHit(Collider other, CombatHitContext context);
}