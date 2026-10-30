using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.ExceptionServices;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/*
 * SCRUM-196 - Multi-collider exactly-once damage regression tests.
 *
 * Play Mode integration testing. This fixture follows the production-assembly
 * reflection pattern used in PostureBreakAttackInterruptionTests, avoiding
 * compile-time dependencies on Assembly-CSharp from the PlayModeTests asmdef.
 *
 * Production path under test:
 * EnemyCombat (IAttackDamageSource)
 *   -> AttackTimelineDriver (attack tokens and windows)
 *   -> EnemyWeaponHitbox / WeaponHitbox (logical target deduplication)
 *   -> PlayerCombat / PlayerHealth (real target damage)
 *
 * Test methodology mapping:
 * - Play Mode integration: all six tests use production components.
 * - Equivalence partitioning: one collider, multiple colliders on one logical
 *   target, and colliders on separate logical targets.
 * - Negative path: repeated contact and stale attack token after cancellation.
 * - Regression: attack-window resets, multi-target damage, and interruption.
 *
 * Trigger callbacks are invoked directly and deterministically. This tests
 * hit registration, not the physics engine's trigger-pair delivery.
 * Defensive resolution (block/deflect/posture) is intentionally out of scope.
 * The fixture never calls PlayerCombat.StartBlock or enters a blocking state.
 *
 * IMPORTANT: The current WeaponHitbox clears its target set whenever a hitbox
 * window is enabled. These tests check exactly-once within one active window,
 * and new attack tokens across separate windows. A future attack with multiple
 * independently enabled windows may require token-scoped deduplication.
 */
public class MultiColliderExactlyOnceDamageTests
{
    private const int StartingHealth = 100;
    private const float DamagePerHit = 2f;
    private const float InsideWindow = 0.50f;
    private const float AfterWindow = 0.90f;

    private const BindingFlags InstanceMembers =
        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

    private GameObject attacker;
    private GameObject weapon;
    private readonly List<GameObject> targetObjects = new List<GameObject>();

    private Component enemyCombat;
    private Component attackDriver;
    private Component weaponHitbox;
    private Collider weaponCollider;
    private ScriptableObject attackData;

    private Type enemyCombatType;
    private Type driverType;
    private Type attackDataType;
    private Type weaponHitboxType;
    private Type enemyWeaponHitboxType;
    private Type playerCombatType;
    private Type playerHealthType;

    private MethodInfo beginAttackMethod;
    private MethodInfo evaluateAttackMethod;
    private MethodInfo endAttackMethod;
    private MethodInfo cancelAttackMethod;
    private MethodInfo onTriggerEnterMethod;

    private FieldInfo enemyWeaponHitboxField;
    private FieldInfo enemyAttackDriverField;
    private FieldInfo enemyDamageField;
    private FieldInfo enemyAttackingField;
    private FieldInfo driverHitboxField;
    private FieldInfo driverDamageField;
    private FieldInfo driverTokenField;
    private FieldInfo driverAttackDataField;
    private FieldInfo driverHitboxActiveField;
    private FieldInfo hitboxOwnerField;
    private FieldInfo hitboxActiveField;
    private FieldInfo hitTargetsField;
    private FieldInfo attackStartField;
    private FieldInfo attackEndField;
    private FieldInfo attackFramesField;
    private FieldInfo healthField;
    private FieldInfo maxHealthField;

    private int activeToken;

    // ---------------------------------------------------------
    // SETUP / TEARDOWN
    // ---------------------------------------------------------

    [SetUp]
    public void SetUp()
    {
        ResolveTypes();
        ResolveMembers();
        CreateAttackerFixture();
        CreateAttackData();
        VerifyFixtureWiring();
    }

    [TearDown]
    public void TearDown()
    {
        foreach (GameObject target in targetObjects)
        {
            if (target != null)
                UnityEngine.Object.DestroyImmediate(target);
        }
        targetObjects.Clear();

        if (attackData != null)
            UnityEngine.Object.DestroyImmediate(attackData);

        if (attacker != null)
            UnityEngine.Object.DestroyImmediate(attacker);

        attackData = null;
        attacker = null;
        weapon = null;
        enemyCombat = null;
        attackDriver = null;
        weaponHitbox = null;
        weaponCollider = null;
    }

    // ---------------------------------------------------------
    // TEST 1 - SINGLE COLLIDER / POSITIVE CONTROL
    // ---------------------------------------------------------

    [UnityTest]
    public IEnumerator SingleCollider_OneAttack_DealsDamageExactlyOnce()
    {
        LogicalTarget target = CreateTarget("Single", 1);
        yield return null;
        ResetHealth(target);

        int token = BeginActiveAttack();
        Assert.Greater(token, 0);
        AssertWindowActive();

        Contact(target.Colliders[0]);
        AssertHealth(target, StartingHealth - (int)DamagePerHit,
            "First valid collider contact must apply one damage outcome.");

        AssertRegisteredTargets(1);
        EndActiveAttack(token);
        AssertWindowClosed();
    }

    // ---------------------------------------------------------
    // TEST 2 - MULTIPLE COLLIDERS, ONE LOGICAL TARGET
    // ---------------------------------------------------------

    [UnityTest]
    public IEnumerator MultipleColliders_SameLogicalTarget_OnlyOneDamage()
    {
        LogicalTarget target = CreateTarget("MultiCollider", 3);
        yield return null;
        ResetHealth(target);

        Assert.AreEqual(3, target.Colliders.Length);
        Assert.AreSame(target.Root, target.Colliders[0].transform.root.gameObject);
        Assert.AreSame(target.Root, target.Colliders[1].transform.root.gameObject);
        Assert.AreSame(target.Root, target.Colliders[2].transform.root.gameObject);

        int token = BeginActiveAttack();
        AssertWindowActive();

        Contact(target.Colliders[0]);
        AssertHealth(target, StartingHealth - (int)DamagePerHit,
            "First child collider must resolve the parent combat target.");

        Contact(target.Colliders[1]);
        Contact(target.Colliders[2]);

        AssertHealth(target, StartingHealth - (int)DamagePerHit,
            "Three child colliders of one logical target must not triple damage.");
        AssertRegisteredTargets(1);

        EndActiveAttack(token);
        AssertWindowClosed();
    }

    // ---------------------------------------------------------
    // TEST 3 - RE-ENTRY IN ONE ATTACK TOKEN
    // ---------------------------------------------------------

    [UnityTest]
    public IEnumerator ColliderReentry_SameAttackToken_DoesNotDamageTwice()
    {
        LogicalTarget target = CreateTarget("Reentry", 2);
        yield return null;
        ResetHealth(target);

        int token = BeginActiveAttack();
        AssertWindowActive();

        Contact(target.Colliders[0]);
        AssertHealth(target, StartingHealth - (int)DamagePerHit);

        // Re-entry is simulated by another OnTriggerEnter callback for the
        // same physical collider, then for another collider on that target.
        Contact(target.Colliders[0]);
        Contact(target.Colliders[1]);
        Contact(target.Colliders[0]);

        Assert.AreEqual(token, CurrentAttackToken());
        AssertHealth(target, StartingHealth - (int)DamagePerHit,
            "Repeated callbacks within one token must be ignored.");
        AssertRegisteredTargets(1);

        EndActiveAttack(token);
        AssertWindowClosed();
    }

    // ---------------------------------------------------------
    // TEST 4 - NEW TOKEN, SAME TARGET
    // ---------------------------------------------------------

    [UnityTest]
    public IEnumerator NewAttackToken_SameTarget_CanReceiveNewDamage()
    {
        LogicalTarget target = CreateTarget("RepeatTarget", 2);
        yield return null;
        ResetHealth(target);

        int firstToken = BeginActiveAttack();
        Contact(target.Colliders[0]);
        AssertHealth(target, StartingHealth - (int)DamagePerHit);
        EndActiveAttack(firstToken);
        AssertWindowClosed();

        int secondToken = BeginActiveAttack();
        Assert.Greater(secondToken, firstToken,
            "BeginAttack must allocate a fresh attack token.");
        AssertWindowActive();

        Contact(target.Colliders[1]);
        Contact(target.Colliders[0]);
        AssertHealth(target, StartingHealth - 2 * (int)DamagePerHit,
            "A new attack token must permit exactly one new damage event.");
        AssertRegisteredTargets(1);

        EndActiveAttack(secondToken);
        AssertWindowClosed();
    }

    // ---------------------------------------------------------
    // TEST 5 - MULTIPLE INDEPENDENT TARGETS
    // ---------------------------------------------------------

    [UnityTest]
    public IEnumerator MultipleTargets_EachReceivesOneDamagePerToken()
    {
        LogicalTarget first = CreateTarget("First", 2);
        LogicalTarget second = CreateTarget("Second", 3);
        yield return null;
        ResetHealth(first);
        ResetHealth(second);

        Assert.AreNotSame(first.Root, second.Root);
        int token = BeginActiveAttack();

        Contact(first.Colliders[0]);
        Contact(second.Colliders[0]);
        Contact(first.Colliders[1]);
        Contact(second.Colliders[1]);
        Contact(second.Colliders[2]);

        AssertHealth(first, StartingHealth - (int)DamagePerHit,
            "First logical target must receive one hit.");
        AssertHealth(second, StartingHealth - (int)DamagePerHit,
            "Second logical target must receive one hit.");
        AssertRegisteredTargets(2);

        EndActiveAttack(token);
        AssertWindowClosed();
    }

    // ---------------------------------------------------------
    // TEST 6 - INTERRUPTION, STALE TOKEN, NEW ATTACK
    // ---------------------------------------------------------

    [UnityTest]
    public IEnumerator InterruptedAttack_RejectsStaleToken_ThenAllowsFreshHits()
    {
        LogicalTarget first = CreateTarget("BeforeInterrupt", 2);
        LogicalTarget second = CreateTarget("AfterInterrupt", 2);
        yield return null;
        ResetHealth(first);
        ResetHealth(second);

        int interruptedToken = BeginActiveAttack();
        Contact(first.Colliders[0]);
        AssertHealth(first, StartingHealth - (int)DamagePerHit);
        AssertRegisteredTargets(1);

        Invoke(cancelAttackMethod, attackDriver);
        AssertWindowClosed();
        Assert.IsNull(driverAttackDataField.GetValue(attackDriver),
            "CancelCurrentAttack must clear AttackData.");
        Assert.IsFalse((bool)driverHitboxActiveField.GetValue(attackDriver),
            "CancelCurrentAttack must clear the active window flag.");
        Assert.AreEqual(0f, (float)driverDamageField.GetValue(attackDriver), 0.001f,
            "CancelCurrentAttack must clear pending damage.");
        Assert.Greater(CurrentAttackToken(), interruptedToken,
            "Cancellation must invalidate the previous token.");
        AssertRegisteredTargets(0);

        // A stale animator timeline update must not reactivate the hitbox.
        Invoke(evaluateAttackMethod, attackDriver, interruptedToken, InsideWindow);
        AssertWindowClosed();

        // Even a manually delivered callback cannot damage a new target
        // while the weapon is disabled. A fresh target avoids a false pass
        // caused by prior deduplication state.
        Contact(second.Colliders[0]);
        AssertHealth(second, StartingHealth,
            "Cancelled attack must not damage a previously untouched target.");

        int freshToken = BeginActiveAttack();
        Assert.Greater(freshToken, interruptedToken);
        AssertWindowActive();

        Contact(first.Colliders[1]);
        Contact(second.Colliders[1]);
        Contact(second.Colliders[0]);

        AssertHealth(first, StartingHealth - 2 * (int)DamagePerHit,
            "Interrupted attack must not leave stale deduplication state.");
        AssertHealth(second, StartingHealth - (int)DamagePerHit,
            "Fresh attack must damage a new target exactly once.");
        AssertRegisteredTargets(2);

        EndActiveAttack(freshToken);
        AssertWindowClosed();
    }

    // ---------------------------------------------------------
    // TYPE AND MEMBER RESOLUTION
    // ---------------------------------------------------------

    private void ResolveTypes()
    {
        enemyCombatType = ResolveType("EnemyCombat");
        driverType = ResolveType("AttackTimelineDriver");
        attackDataType = ResolveType("AttackData");
        weaponHitboxType = ResolveType("WeaponHitbox");
        enemyWeaponHitboxType = ResolveType("EnemyWeaponHitbox");
        playerCombatType = ResolveType("PlayerCombat");
        playerHealthType = ResolveType("PlayerHealth");

        Assert.IsTrue(weaponHitboxType.IsAssignableFrom(enemyWeaponHitboxType),
            "EnemyWeaponHitbox must inherit WeaponHitbox.");
        Assert.IsTrue(typeof(MonoBehaviour).IsAssignableFrom(enemyCombatType));
        Assert.IsTrue(typeof(MonoBehaviour).IsAssignableFrom(driverType));
        Assert.IsTrue(typeof(MonoBehaviour).IsAssignableFrom(enemyWeaponHitboxType));
        Assert.IsTrue(typeof(MonoBehaviour).IsAssignableFrom(playerHealthType));
        Assert.IsTrue(typeof(ScriptableObject).IsAssignableFrom(attackDataType));
    }

    private static Type ResolveType(string name)
    {
        Type type = Type.GetType(name + ", Assembly-CSharp");
        if (type == null)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = assembly.GetType(name);
                if (type != null) break;
            }
        }
        Assert.IsNotNull(type, "Missing production type: " + name);
        return type;
    }

    private void ResolveMembers()
    {
        beginAttackMethod = RequiredMethod(driverType, "BeginAttack", attackDataType);
        evaluateAttackMethod = RequiredMethod(driverType, "EvaluateAttack", typeof(int), typeof(float));
        endAttackMethod = RequiredMethod(driverType, "EndAttack", typeof(int));
        cancelAttackMethod = RequiredMethod(driverType, "CancelCurrentAttack");
        onTriggerEnterMethod = RequiredMethod(weaponHitboxType, "OnTriggerEnter", typeof(Collider));

        enemyWeaponHitboxField = RequiredField(enemyCombatType, "weaponHitbox");
        enemyAttackDriverField = RequiredField(enemyCombatType, "attackTimelineDriver");
        enemyDamageField = RequiredField(enemyCombatType, "enemyDamage");
        enemyAttackingField = RequiredField(enemyCombatType, "isAttacking");
        driverHitboxField = RequiredField(driverType, "weaponHitbox");
        driverDamageField = RequiredField(driverType, "currentDamage");
        driverTokenField = RequiredField(driverType, "currentAttackToken");
        driverAttackDataField = RequiredField(driverType, "currentAttackData");
        driverHitboxActiveField = RequiredField(driverType, "hitboxActive");
        hitboxOwnerField = RequiredField(weaponHitboxType, "ownerRoot");
        hitboxActiveField = RequiredField(weaponHitboxType, "isHitboxActive");
        hitTargetsField = RequiredField(weaponHitboxType, "hitTargets");
        attackStartField = RequiredField(attackDataType, "startHitFrame");
        attackEndField = RequiredField(attackDataType, "endHitFrame");
        attackFramesField = RequiredField(attackDataType, "totalFrames");
        healthField = RequiredField(playerHealthType, "health");
        maxHealthField = RequiredField(playerHealthType, "maxHealth");
    }

    private static FieldInfo RequiredField(Type type, string name)
    {
        FieldInfo field = type.GetField(name, InstanceMembers);
        Assert.IsNotNull(field, "Missing production field: " + type.Name + "." + name);
        return field;
    }

    private static MethodInfo RequiredMethod(Type type, string name, params Type[] parameters)
    {
        MethodInfo method = type.GetMethod(name, InstanceMembers, null, parameters, null);
        Assert.IsNotNull(method, "Missing production method: " + type.Name + "." + name);
        return method;
    }

    // ---------------------------------------------------------
    // FIXTURE CONSTRUCTION
    // ---------------------------------------------------------

    private void CreateAttackerFixture()
    {
        // Match the working SCRUM-85 fixture: EnemyCombat is added before
        // AttackTimelineDriver so its Awake can resolve IAttackDamageSource.
        attacker = new GameObject("SCRUM196_Attacker");
        enemyCombat = (Component)attacker.AddComponent(enemyCombatType);
        attackDriver = (Component)attacker.AddComponent(driverType);

        weapon = new GameObject("SCRUM196_Weapon");
        weapon.transform.SetParent(attacker.transform, false);
        weaponCollider = weapon.AddComponent<BoxCollider>();
        weaponCollider.isTrigger = true;
        weaponHitbox = (Component)weapon.AddComponent(enemyWeaponHitboxType);

        enemyWeaponHitboxField.SetValue(enemyCombat, weaponHitbox);
        enemyAttackDriverField.SetValue(enemyCombat, attackDriver);
        driverHitboxField.SetValue(attackDriver, weaponHitbox);
        hitboxOwnerField.SetValue(weaponHitbox, attacker);
        enemyDamageField.SetValue(enemyCombat, DamagePerHit);

        // This test begins after attack eligibility is granted; it does
        // not invoke TryAttack or require a scene/Animator/BehaviorGraph.
        enemyAttackingField.SetValue(enemyCombat, true);
    }

    private void CreateAttackData()
    {
        attackData = ScriptableObject.CreateInstance(attackDataType);
        attackData.name = "SCRUM196_AttackData";
        attackStartField.SetValue(attackData, 25);
        attackEndField.SetValue(attackData, 75);
        attackFramesField.SetValue(attackData, 100);
    }

    private void VerifyFixtureWiring()
    {
        Assert.AreSame(weaponHitbox, enemyWeaponHitboxField.GetValue(enemyCombat));
        Assert.AreSame(weaponHitbox, driverHitboxField.GetValue(attackDriver));
        Assert.AreSame(attackDriver, enemyAttackDriverField.GetValue(enemyCombat));
        Assert.AreSame(attacker, hitboxOwnerField.GetValue(weaponHitbox));
        Assert.AreEqual(DamagePerHit, (float)enemyDamageField.GetValue(enemyCombat), 0.001f);
        Assert.IsFalse(weaponCollider.enabled,
            "WeaponHitbox.Awake must start with the weapon collider disabled.");
        Assert.IsFalse((bool)hitboxActiveField.GetValue(weaponHitbox));
    }

    private LogicalTarget CreateTarget(string label, int colliderCount)
    {
        Assert.Greater(colliderCount, 0);
        GameObject root = new GameObject("SCRUM196_Target_" + label);
        targetObjects.Add(root);

        // EnemyWeaponHitbox resolves the player by its PlayerCombat component.
        // The real PlayerHealth receives damage; no test-only damage mock.
        root.AddComponent(playerCombatType);
        Component health = (Component)root.AddComponent(playerHealthType);

        Collider[] colliders = new Collider[colliderCount];
        for (int i = 0; i < colliderCount; i++)
        {
            GameObject child = new GameObject(label + "_Collider_" + i);
            child.transform.SetParent(root.transform, false);
            BoxCollider collider = child.AddComponent<BoxCollider>();
            collider.isTrigger = false;
            colliders[i] = collider;
        }

        return new LogicalTarget(root, health, colliders);
    }

    private static void ResetHealth(LogicalTarget target)
    {
        // PlayerHealth.Start may have initialized health on the first frame.
        // Seed it afterward to avoid a Start() race and keep hits nonlethal.
        FieldInfo max = RequiredField(target.Health.GetType(), "maxHealth");
        FieldInfo current = RequiredField(target.Health.GetType(), "health");
        max.SetValue(target.Health, StartingHealth);
        current.SetValue(target.Health, StartingHealth);
        Assert.AreEqual(StartingHealth, (int)current.GetValue(target.Health));
    }

    // ---------------------------------------------------------
    // ATTACK AND CONTACT HELPERS
    // ---------------------------------------------------------

    private int BeginActiveAttack()
    {
        int token = (int)Invoke(beginAttackMethod, attackDriver, attackData);
        Assert.Greater(token, 0);
        Assert.AreEqual(token, CurrentAttackToken());
        Assert.AreSame(attackData, driverAttackDataField.GetValue(attackDriver));

        // Unlike the previous version, this uses the production
        // IAttackDamageSource.GetAttackDamage -> currentDamage pathway.
        Assert.AreEqual(DamagePerHit, (float)driverDamageField.GetValue(attackDriver), 0.001f,
            "AttackTimelineDriver should resolve EnemyCombat's configured damage.");

        Invoke(evaluateAttackMethod, attackDriver, token, InsideWindow);
        AssertWindowActive();
        activeToken = token;
        return token;
    }

    private void EndActiveAttack(int token)
    {
        Invoke(evaluateAttackMethod, attackDriver, token, AfterWindow);
        AssertWindowClosed();
        Invoke(endAttackMethod, attackDriver, token);
        AssertWindowClosed();
    }

    private void Contact(Collider collider)
    {
        Assert.IsNotNull(collider);
        Invoke(onTriggerEnterMethod, weaponHitbox, collider);
    }

    private int CurrentAttackToken()
    {
        return (int)driverTokenField.GetValue(attackDriver);
    }

    private void AssertWindowActive()
    {
        Assert.IsTrue(weaponCollider.enabled, "Attack window should enable weapon collider.");
        Assert.IsTrue((bool)hitboxActiveField.GetValue(weaponHitbox),
            "Attack window should set WeaponHitbox active flag.");
        Assert.IsTrue((bool)driverHitboxActiveField.GetValue(attackDriver),
            "AttackTimelineDriver should report an active window.");
    }

    private void AssertWindowClosed()
    {
        Assert.IsFalse(weaponCollider.enabled, "Closed/cancelled hitbox collider must be disabled.");
        Assert.IsFalse((bool)hitboxActiveField.GetValue(weaponHitbox),
            "Closed/cancelled WeaponHitbox must not register contacts.");
        Assert.IsFalse((bool)driverHitboxActiveField.GetValue(attackDriver),
            "Driver must report a closed window.");
    }

    private void AssertRegisteredTargets(int expectedCount)
    {
        object set = hitTargetsField.GetValue(weaponHitbox);
        Assert.IsNotNull(set);
        PropertyInfo countProperty = set.GetType().GetProperty("Count");
        Assert.IsNotNull(countProperty,
            "WeaponHitbox.hitTargets must expose a Count property.");
        int count = (int)countProperty.GetValue(set, null);
        Assert.AreEqual(expectedCount, count,
            "Deduplication set should contain one entry per logical target.");
    }

    private static void AssertHealth(LogicalTarget target, int expected, string message = null)
    {
        int actual = (int)RequiredField(target.Health.GetType(), "health").GetValue(target.Health);
        Assert.AreEqual(expected, actual, message ?? "Unexpected logical-target damage count.");
    }

    private static object Invoke(MethodInfo method, object instance, params object[] args)
    {
        Assert.IsNotNull(method);
        Assert.IsNotNull(instance);
        try
        {
            return method.Invoke(instance, args);
        }
        catch (TargetInvocationException exception)
        {
            if (exception.InnerException != null)
                ExceptionDispatchInfo.Capture(exception.InnerException).Throw();
            throw;
        }
    }

    private sealed class LogicalTarget
    {
        public readonly GameObject Root;
        public readonly Component Health;
        public readonly Collider[] Colliders;

        public LogicalTarget(GameObject root, Component health, Collider[] colliders)
        {
            Root = root;
            Health = health;
            Colliders = colliders;
        }
    }
}
