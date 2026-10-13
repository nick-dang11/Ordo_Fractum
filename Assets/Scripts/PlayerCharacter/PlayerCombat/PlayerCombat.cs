using UnityEngine;

public class PlayerCombat : MonoBehaviour, IAttackDamageSource, IAttackLifecycle
{
    [SerializeField] public PlayerInputManager input;
    [SerializeField] public WeaponHitbox weaponHitbox;
    [SerializeField] private Animator animate;
    [SerializeField] private float lightAttackDamage = 10f;
    [SerializeField] private float heavyAttackDamage = 40f;
    [SerializeField] private BlockZone blockZone;
    [SerializeField] private CombatClashFeedback clashFeedback;
    [SerializeField] private PlayerPosture playerPosture;

    public bool wasAttacking = false;
    public bool isAttacking = false;
    private bool isBlocking = false;
    private bool canCombo = false;
    public bool IsAttacking => isAttacking;

    private int comboStep = 0;

    public float lightAttackCooldown = 0.4f;
    private float nextLightAttackTime = 0f;

    public float heavyAttackCooldown = 1.0f;
    private float nextHeavyAttackTime = 0f;

    private bool wasLightAttacking = false;
    private bool wasHeavyAttacking = false;

    public float lastBlockTime = 0f;
    public float deflectWindow = 0.3f;

    public float pendingDamage;
    private bool wasBlockHeld = false;

    [Header("Counter Window")]
    [SerializeField, Min(0f)]
    private float counterWindowDuration = 0.75f;

    [SerializeField, Min(0f)]
    private float counterRange = 3f;

    [SerializeField]
    private GameObject counterDebugIndicator;

    private EnemyCombat counterTarget;
    private float counterWindowEndTime;
    private bool counterWindowOpen;

    public bool CanCounter => counterWindowOpen && counterTarget != null;

    public EnemyCombat CounterTarget => counterTarget;

    public float CounterTimeRemaining => CanCounter ? Mathf.Max(0f, counterWindowEndTime - Time.time): 0f;

    [Header("Deflect Posture")]
    [SerializeField, Min(0f)]
    private float deflectPostureDamage = 25f;
    public float DeflectPostureDamage => deflectPostureDamage;
    public bool BlockPressedThisFrame => input != null && input.block && !wasBlockHeld;
    public bool BlockReleasedThisFrame => input != null && !input.block && wasBlockHeld;

    private bool isCountering = false;
    public bool IsCountering => isCountering;
    [Header("Counter Attack")]
    [SerializeField, Min(0f)]
    private float counterDamage = 25f;

    void Start() { }

    void Update() { UpdateCounterWindow(); }

    private void LateUpdate()
    {
        if (input != null)
        {
            wasBlockHeld = input.block;
        }
    }

    public void HandleBlock()
    {
        if (input == null)
            return;

        if (isAttacking)
        {
            ForceStopBlocking();
            return;
        }

        if (!input.block && isBlocking)
        {
            EndBlock();
        }
    }

    public bool IsBlocking()
    {
        return isBlocking;
    }

    public void TriggerBlockFeedback(CombatHitContext context)
    {
        if (context.AttackerRoot != null)
        {
            Debug.Log($"[Block] Blocked attack from " + $"{context.AttackerRoot.name}");
        }
        animate.SetTrigger("BlockHit");
        if (clashFeedback != null)
        {
            clashFeedback.PlayBlock();
        }
        playerPosture.posture += playerPosture.postureFillRate * 3f;
        playerPosture.posture = Mathf.Clamp(playerPosture.posture, 0f, playerPosture.maxPosture);
        playerPosture.postureSlider.value = playerPosture.posture;
    }

    public void LightAndHeavy()
    {
        if (input == null) { return; }

        bool lightPressed = input.attack && !wasLightAttacking;
        bool heavyPressed = input.heavyAttack && !wasHeavyAttacking;

        if (isCountering)
        {
            wasLightAttacking = input.attack;
            wasHeavyAttacking = input.heavyAttack;
            return;
        }
        if (lightPressed && CanCounter)
        {
            BeginCounter();
            wasLightAttacking = input.attack;
            wasHeavyAttacking = input.heavyAttack;
            return;
        }
        if (isBlocking)
        {
            wasLightAttacking = input.attack;
            wasHeavyAttacking = input.heavyAttack;
            return;
        }
        if (heavyPressed)
        {
            ProcessAttackInput(isHeavy: true);
        }
        else if (lightPressed)
        {
            ProcessAttackInput(isHeavy: false);
        }
        wasLightAttacking = input.attack;
        wasHeavyAttacking = input.heavyAttack;
    }

    private void ProcessAttackInput(bool isHeavy)
    {
        Debug.Log($"[Combat] Input received. canCombo={canCombo}, comboStep={comboStep}, isHeavy={isHeavy}");
        if (canCombo && comboStep < 3)
        {
            // Continuing an existing chain — Animator resolves the exact
            // branch (LA_LA, HA_LA, etc.) based on the state it's already in.
            animate.SetTrigger(isHeavy ? "ComboHeavy" : "ComboLight");
            comboStep++;
            SetPendingDamage(isHeavy ? heavyAttackDamage : lightAttackDamage);
        }
        else if (Time.time >= (isHeavy ? nextHeavyAttackTime : nextLightAttackTime))
        {
            if (isHeavy)
            {
                animate.SetTrigger("HeavyAttack");
                nextHeavyAttackTime = Time.time + heavyAttackCooldown;
            }
            else
            {
                animate.SetTrigger("LightAttack");
                nextLightAttackTime = Time.time + lightAttackCooldown;
            }
            comboStep = 1;
            SetPendingDamage(isHeavy ? heavyAttackDamage : lightAttackDamage);
        }
    }

    public void StartAttack()
    {
        ForceStopBlocking();
        isAttacking = true;
    }

    public void EndAttack()
    {
        isAttacking = false;
        canCombo = false;
        comboStep = 0;
        animate.ResetTrigger("LightAttack");
        animate.ResetTrigger("HeavyAttack");
        animate.ResetTrigger("ComboLight");
        animate.ResetTrigger("ComboHeavy");
        Debug.Log($"[Combat] EndAttack fired at {Time.time}");
    }

    public void OnAttackEnded()
    {
        EndAttack();
    }

    public void EnableCombo()
    {
        canCombo = true;
    }

    public void DisableCombo()
    {
        canCombo = false;
    }

    public void EnableWeaponHitbox()
    {
        Debug.Log("[PlayerCombat] EnableWeaponHitbox called");
        weaponHitbox.EnableHitbox(pendingDamage);
    }

    public void DisableWeaponHitbox()
    {
        Debug.Log("[PlayerCombat] DisableWeaponHitbox called");
        weaponHitbox.DisableHitbox();
    }

    public void ForceStopBlocking()
    {
        EndBlock();
    }

    public void TriggerDeflectFeedback(
    CombatHitContext context)
    {
        if (context.AttackerRoot != null)
        {
            Debug.Log($"[Deflect] Deflected attack from " + $"{context.AttackerRoot.name}");
        } else
        {
            Debug.LogWarning("[Deflect] Deflect occurred, but attacker was missing.");
        }

        animate.SetTrigger("Deflect");

        if (clashFeedback != null) { clashFeedback.PlayDeflect(); }
    }

    public void SetPendingDamage(float damage)
    {
        pendingDamage = damage;
    }
    public void BeginBlock()
    {
        Debug.Log($"[Block DEBUG] BeginBlock called. " + $"input.block={input?.block}, " + $"isAttacking={isAttacking}, " + $"blockZoneAssigned={blockZone != null}");
        if (input == null)
            return;

        if (isAttacking)
            return;

        lastBlockTime = Time.time;

        isBlocking = true;

        animate.SetBool("Block", true);

        if (blockZone != null)
        {
            blockZone.SetActive(true);
        }
        else
        {
            Debug.LogError("[PlayerCombat] BlockZone reference is missing.", this);
        }

        Debug.Log($"[Block] Block started at {lastBlockTime}");
    }
    private void EndBlock()
    {
        isBlocking = false;

        animate.SetBool("Block", false);

        if (blockZone != null)
        {
            blockZone.SetActive(false);
        }
    }

    public float GetAttackDamage(AttackData attackData)
    {
        return pendingDamage;
    }

    public void ResetForRespawn()
    {
        ForceStopBlocking();
        isAttacking = false;
        canCombo = false;
        if (weaponHitbox != null)
        {
            weaponHitbox.DisableHitbox();
        }
        Debug.Log("[PlayerCombat] Reset for respawn.");
    }

    public void OpenCounterWindow(EnemyCombat target)
    {
        if (target == null || target.IsDead) { return; }

        counterTarget = target;
        counterWindowEndTime = Time.time + counterWindowDuration;

        counterWindowOpen = true;

        if (counterDebugIndicator != null) { counterDebugIndicator.SetActive(true); }

        Debug.Log($"[Counter] Window OPEN for {target.name}.");
    }

    public void CloseCounterWindow(string reason = "")
    {
        if (!counterWindowOpen) { return; }

        string targetName = counterTarget != null ? counterTarget.name : "NULL";
        Debug.Log($"[Counter] Window CLOSED for {targetName}. " + $"Reason={reason}");

        counterWindowOpen = false;
        counterWindowEndTime = 0f;
        counterTarget = null;

        if (counterDebugIndicator != null) { counterDebugIndicator.SetActive(false); }
    }

    private void UpdateCounterWindow()
    {
        if (isCountering) { return; }
        if (!counterWindowOpen) { return; }

        if (Time.time >= counterWindowEndTime) {
            CloseCounterWindow("expired");
            return;
        }

        if (counterTarget == null) {
            CloseCounterWindow("target missing");
            return;
        }

        if (counterTarget.IsDead) {
            CloseCounterWindow("target died");
            return;
        }

        float distance = Vector3.Distance(transform.position, counterTarget.transform.position);

        if (distance > counterRange) { CloseCounterWindow("target out of range"); }
    }
    private void BeginCounter()
    {
        if (!CanCounter) { return; }

        if (counterTarget == null)
        {
            CloseCounterWindow("counter target missing");
            return;
        }

        if (counterTarget.IsDead)
        {
            CloseCounterWindow("counter target died");
            return;
        }

        float distance = Vector3.Distance(transform.position, counterTarget.transform.position);

        if (distance > counterRange)
        {
            CloseCounterWindow("target out of range before counter");
            return;
        }

        Debug.Log($"[Counter] Counter STARTED against " + $"{counterTarget.name}.");

        isCountering = true;

        ForceStopBlocking();

        FaceCounterTarget();

        // IMPORTANT:
        // Give the existing attack/hitbox system
        // the counter's damage value.
        SetPendingDamage(counterDamage);

        if (animate != null) { animate.SetTrigger("CounterAttack"); }
    }

    private void FaceCounterTarget()
    {
        if (counterTarget == null) { return; }

        Vector3 direction = counterTarget.transform.position - transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f) { return; }

        transform.rotation = Quaternion.LookRotation(direction);
    }
    public void EnableCounterHitbox()
    {
        if (!isCountering) { return; }

        if (weaponHitbox == null) { return; }

        weaponHitbox.EnableHitbox(counterDamage);

        Debug.Log($"[Counter] Counter hitbox ENABLED. Damage={counterDamage}");
    }
    public void DisableCounterHitbox()
    {
        if (weaponHitbox == null) { return; }

        weaponHitbox.DisableHitbox();

        Debug.Log("[Counter] Counter hitbox DISABLED.");
    }
    public void EndCounter()
    {
        if (!isCountering) { return; }

        if (weaponHitbox != null) { weaponHitbox.DisableHitbox(); }

        isCountering = false;

        CloseCounterWindow("counter completed");

        Debug.Log("[Counter] Counter COMPLETE.");
    }
}
