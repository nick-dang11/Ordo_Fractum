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

    private bool isHitboxActive = false;
    private float currentDamage;

    private readonly HashSet<GameObject> hitTargets = new HashSet<GameObject>();
    // now tracking GameObjects rather than colliders as a single enemy may have multiple colliders

    protected float CurrentDamage => currentDamage;
    protected bool IsHitboxActive => isHitboxActive;

    protected virtual void Awake()
    {
        weaponCollider = GetComponent<Collider>();

        if (!weaponCollider.isTrigger)
        {
            Debug.LogWarning($"{name} WeaponHitbox collider does not have is Trigger toggled on. Setting isTrigger to true", this);
            
            weaponCollider.isTrigger = true;
        }

        weaponCollider.enabled = false;

        weaponRenderer = GetComponent<Renderer>();

        if(weaponRenderer == null)
        {
            weaponRenderer = GetComponentInChildren<Renderer>();
        }

        SetWeaponColor(inactiveColor);
    }

    public virtual void EnableHitbox(float damage)
    {
        currentDamage = damage;
        hitTargets.Clear();

        isHitboxActive = true;
        weaponCollider.enabled = true;
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

        GameObject targetRoot = other.transform.root.gameObject;

        if (targetRoot == transform.root.gameObject) return; // prevent self damage for weapon owner

        if(hitTargets.Contains(targetRoot)) return; // prevent multiple instances of damage during one hitbox window

        if (!IsValidTarget(other)) return;

        hitTargets.Add(targetRoot);
        HandleHit(other);
    }



    protected abstract bool IsValidTarget(Collider other);

    protected abstract void HandleHit(Collider other);
}