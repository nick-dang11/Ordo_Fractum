using UnityEngine;

public readonly struct CombatHitContext
{
    public GameObject AttackerRoot { get; }
    public GameObject TargetRoot { get; }

    public Collider WeaponCollider { get; }

    public AttackData AttackData { get; }

    public float Damage { get; }
    public float PostureDamage { get; }

    public Vector3 ImpactPoint { get; }

    public CombatHitContext(
        GameObject attackerRoot,
        GameObject targetRoot,
        Collider weaponCollider,
        AttackData attackData,
        float damage,
        float postureDamage,
        Vector3 impactPoint)
    {
        AttackerRoot = attackerRoot;
        TargetRoot = targetRoot;
        WeaponCollider = weaponCollider;

        AttackData = attackData;

        Damage = damage;
        PostureDamage = postureDamage;

        ImpactPoint = impactPoint;
    }
}