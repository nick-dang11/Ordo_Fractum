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
    [Header("Weapon Damage system")]

    public bool wasAttacking = false;
    public bool isAttacking = false;
    private bool isBlocking = false;
    //private bool canCombo = false;
    private bool comboQueued = false;
    public bool IsAttacking => isAttacking;

    public int AttackSequence { get; private set; }
    public int ConfirmedAttackSequence { get; private set; }

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

    [Header("Attack Cancel Data")]
    [SerializeField]
    private AttackData[] attackDefinitions;

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

    [Header("Enemy Targeting")]
    [SerializeField] private PlayerActionController targeting;

    [SerializeField] private WeaponDamageSystem weaponDamageSystem;

    private void Awake()
    {
        if (targeting == null)
            targeting = GetComponent<PlayerActionController>();

        if(weaponDamageSystem == null)
        {
            weaponDamageSystem = GetComponent<WeaponDamageSystem>();
        }

        if(weaponDamageSystem != null)
        {
            weaponDamageSystem.OnWeaponLevelChanged += UpdateAttackDamage;

            UpdateAttackDamage();
        }
        else
        {
            Debug.LogWarning("[PlayerCombat] WeaponDamageSystem reference is missing.");
        }
    }

    void Start() { }

    void Update()
    {
        UpdateCounterWindow();
        UpdateAttackRecovery();
    }

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

        // Release blocking normally.
        if (!input.block)
        {
            if (isBlocking)
                EndBlock();

            return;
        }

        // Only attempt to begin blocking on a fresh press.
        if (BlockPressedThisFrame && !isBlocking)
        {
            BeginBlock();
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
            Debug.Log($"[Block] Blocked attack from " +
                      $"{context.AttackerRoot.name}");
    
            
            if (targeting != null)
            {
                targeting.FocusOnEnemy(
                    context.AttackerRoot.transform
                );
            }
            EnemyCombat enemyCombat = context.AttackerRoot.GetComponentInParent<EnemyCombat>();

            if (enemyCombat != null)
            {
                enemyCombat.HandleBlocked();
            } else {
                Debug.LogWarning("[Block] Could not find EnemyCombat on attacker.");
            }
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
        if (animate == null || isCountering)
            return;

        bool attackActive = isAttacking || IsAttackAnimationPlaying();

        Debug.Log($"[Combat] Input received. " + $"attackActive={attackActive}, " + $"comboQueued={comboQueued}, " + $"comboStep={comboStep}, " + $"isHeavy={isHeavy}");

        // CASE 1: Player is currently attacking.
        if (attackActive)
        {
            // Only one follow-up request per combo attack.
            if (comboQueued)
            {
                Debug.LogWarning($"[Combo] REJECTED: Previous attack still queued. " + $"Step={comboStep}");
                return;
            }

            // Three attacks maximum.
            if (comboStep >= 3)
            {
                Debug.Log("[Combo] Maximum three-hit chain reached.");
                return;
            }

            // Check the current AttackData combo window.
            if (!CanComboCurrentAttack())
            {
                Debug.Log("[Combo] Input outside combo window.");
                return;
            }

            comboQueued = true;
            comboStep++;
            AttackSequence++;

            if (targeting != null)
            {
                targeting.SnapToAttackTarget();
            }

            SetPendingDamage(
                isHeavy ? heavyAttackDamage : lightAttackDamage
            );

            animate.SetTrigger(
                isHeavy ? "ComboHeavy" : "ComboLight"
            );

            Debug.Log(
                $"[Combo] Follow-up accepted. Step={comboStep}"
            );
            return;
        }

        // CASE 2: Player is not attacking.
        // Avoid starting an attack during an Animator transition.
        if (animate.IsInTransition(0))
            return;

        float nextAllowedTime = isHeavy ? nextHeavyAttackTime : nextLightAttackTime;

        if (Time.time < nextAllowedTime)
            return;

        comboStep = 1;
        comboQueued = false;
        AttackSequence++;

        if (targeting != null)
        {
            targeting.SnapToAttackTarget();
        }

        SetPendingDamage(isHeavy ? heavyAttackDamage : lightAttackDamage);

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

        Debug.Log($"[Combo] Initial attack started. Step={comboStep}");
    }


    public void StartAttack()
    {
        ForceStopBlocking();
        isAttacking = true;
        // The queued follow-up has entered its attack animation.
        if (comboQueued)
        {
            comboQueued = false;
            Debug.Log($"[Combo] Attack {comboStep} animation started.");
        }
    }

    public void ConfirmAttackStarted()
    {
        if (ConfirmedAttackSequence >= AttackSequence) return;

        ConfirmedAttackSequence = AttackSequence;

        Debug.Log(
            $"[PlayerCombat] Confirmed attack started. " +
            $"Sequence={ConfirmedAttackSequence}",
            this
        );
    }




    public void EndAttack()
    {
        // Ensure the previous attack cannot keep dealing damage.
        if (weaponHitbox != null)
        {
            weaponHitbox.DisableHitbox();
        }

        // A previous animation's end event should not
        // reset the chain while an attack state is active.
        if (animate != null && IsAttackAnimationPlaying())
        {
            Debug.Log("[Combo] EndAttack received, but Animator " + "is still in an attack state. Deferring reset.");

            return;
        }

        ResetAttackChain();
    }


    private void ResetAttackChain()
    {
        isAttacking = false;
        comboQueued = false;
        comboStep = 0;
        if (weaponHitbox != null)
        {
            weaponHitbox.DisableHitbox();
        }

        if (animate != null)
        {
            animate.ResetTrigger("LightAttack");
            animate.ResetTrigger("HeavyAttack");
            animate.ResetTrigger("ComboLight");
            animate.ResetTrigger("ComboHeavy");
        }
        Debug.Log("[Combo] Attack chain reset.");
    }


    public void OnAttackEnded()
    {
        EndAttack();
    }

    public void EnableCombo()
    {
    }

    public void DisableCombo()
    {
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

    
    public void TriggerDeflectFeedback(CombatHitContext context)
    {
        if (context.AttackerRoot != null)
        {
            Debug.Log($"[Deflect] Deflected attack from " +
                      $"{context.AttackerRoot.name}");
    
            
            if (targeting != null)
            {
                targeting.FocusOnEnemy(
                    context.AttackerRoot.transform
                );
            }
        }
        else
        {
            Debug.LogWarning(
                "[Deflect] Deflect occurred, but attacker was missing."
            );
        }
    
        animate.SetTrigger("Deflect");
    
        if (clashFeedback != null)
        {
            clashFeedback.PlayDeflect();
        }
    }

    public void SetPendingDamage(float damage)
    {
        pendingDamage = damage;
    }
    public void BeginBlock()
    {
        if (input == null || animate == null || isCountering)
            return;

        if (!input.block || isBlocking)
            return;

        bool attackActive = isAttacking || IsAttackAnimationPlaying();

        if (attackActive)
        {
            // Allow attack cancellation only during valid
            // early or late block-cancel windows.
            if (!CanBlockCancelCurrentAttack())
            {
                Debug.Log("[BlockCancel] Block rejected. " + "Outside valid cancel window.");
                return;
            }

            Debug.Log("[BlockCancel] Valid cancel window. " + "Canceling current attack.");

            // Stop the attack timeline so an outgoing attack
            // cannot reactivate its weapon hitbox.
            AttackTimelineDriver timelineDriver = GetComponent<AttackTimelineDriver>();

            if (timelineDriver != null)
            {
                timelineDriver.CancelCurrentAttack();
            }

            // Clear pending combo inputs and attack state.
            ResetAttackChain();
        }

        // Begin normal blocking.
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
        ResetAttackChain();
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

    private AttackData FindAttackDefinition(
        AnimatorStateInfo stateInfo)
    {
        if (attackDefinitions == null)
            return null;

        foreach (AttackData data in attackDefinitions)
        {
            if (data == null ||
                string.IsNullOrEmpty(data.animationStateName))
                continue;

            if (stateInfo.IsName(data.animationStateName) ||
                stateInfo.shortNameHash ==
                    Animator.StringToHash(data.animationStateName))
            {
                return data;
            }
        }

        return null;
    }

    public bool IsAttackAnimationPlaying()
    {
        if (animate == null)
            return false;

        AnimatorStateInfo current =
            animate.GetCurrentAnimatorStateInfo(0);

        if (current.IsTag("Attack"))
            return true;

        if (animate.IsInTransition(0))
        {
            AnimatorStateInfo next =
                animate.GetNextAnimatorStateInfo(0);

            return next.IsTag("Attack");
        }

        return false;
    }

    public bool CanBlockCancelCurrentAttack()
    {
        if (animate == null || isCountering)
            return false;

        AnimatorStateInfo current =
            animate.GetCurrentAnimatorStateInfo(0);

        // The attack must be the current Animator state.
        if (!current.IsTag("Attack"))
        {
            Debug.Log("[BlockCancel] Attack not active yet.");
            return false;
        }

        // Conservatively prevent canceling while blending
        // between two separate attack states.
        if (animate.IsInTransition(0))
        {
            AnimatorStateInfo next =
                animate.GetNextAnimatorStateInfo(0);

            if (next.IsTag("Attack"))
            {
                Debug.Log(
                    "[BlockCancel] Denied during combo blend."
                );
                return false;
            }
        }

        AttackData data = FindAttackDefinition(current);

        if (data == null)
        {
            Debug.LogWarning(
                "[BlockCancel] No matching AttackData. " +
                "Check animationStateName and Inspector array."
            );
            return false;
        }

        if (!data.IsAttackValid() ||
            !data.AreBlockCancelWindowsValid())
        {
            return false;
        }

        bool allowed = data.IsWithinBlockCancelWindow(
            current.normalizedTime
        );

        int frame = Mathf.Clamp(
            Mathf.FloorToInt(
                current.normalizedTime * data.totalFrames
            ),
            1,
            data.totalFrames
        );

        Debug.Log(
            $"[BlockCancel] {data.name}, " +
            $"Frame={frame}/{data.totalFrames}, " +
            $"Allowed={allowed}"
        );

        return allowed;
    }

    private void UpdateAttackDamage()
    {
        if(weaponDamageSystem == null)
        {
            Debug.LogWarning("[PlayerCombat] WeaponDamageSystem reference is missing.");
            return;
        }

        lightAttackDamage = weaponDamageSystem.GetLightDamage();
        heavyAttackDamage = weaponDamageSystem.GetHeavyDamage();
        counterDamage = weaponDamageSystem.GetCounterDamage();

        Debug.Log($"[PlayerCombat] Attack damage updated: " +
                  $"Light={lightAttackDamage}, " +
                  $"Heavy={heavyAttackDamage}, " +
                  $"Counter={counterDamage}");
    }

    private void OnDestroy()
    {
        if(weaponDamageSystem != null)
        {
            weaponDamageSystem.OnWeaponLevelChanged -= UpdateAttackDamage;
        }
    }

    private bool CanComboCurrentAttack()
    {
        if (animate == null || isCountering)
            return false;

        AnimatorStateInfo current = animate.GetCurrentAnimatorStateInfo(0);

        if (animate.IsInTransition(0))
        {
            AnimatorStateInfo next = animate.GetNextAnimatorStateInfo(0);
            Debug.LogWarning($"[Combo] REJECTED: Animator transitioning. " + $"CurrentHash={current.shortNameHash}, " + $"NextHash={next.shortNameHash}");
            return false;
        }

        if (!current.IsTag("Attack"))
        {
            Debug.LogWarning("[Combo] REJECTED: Current state is not an attack.");
            return false;
        }

        AttackData data = FindAttackDefinition(current);

        if (data == null)
        {
            Debug.LogWarning($"[Combo] REJECTED: No matching AttackData. " + $"StateHash={current.shortNameHash}. " + $"Check PlayerCombat's Attack Definitions array.");
            return false;
        }

        if (!data.IsAttackValid() || !data.IsComboWindowValid())
        {
            Debug.LogWarning($"[Combo] REJECTED: Invalid or disabled " + $"combo window on {data.name}. " + $"Enabled={data.allowComboWindow}");
            return false;
        }

        bool allowed = data.IsWithinComboWindow(current.normalizedTime);

        Debug.Log($"[Combo] State={data.animationStateName}, " + $"Time={current.normalizedTime:F3}, " + $"Window={data.ComboWindowStartNormalized:F3}-" + $"{data.ComboWindowEndNormalized:F3}, " + $"Step={comboStep}, " + $"Allowed={allowed}");

        return allowed;
    }

    private void UpdateAttackRecovery()
    {
        if (!isAttacking || isCountering)
            return;

        // If the Animator has returned to a non-attack state
        // but the script still thinks we're attacking,
        // clean up the attack chain.
        if (!IsAttackAnimationPlaying())
        {
            Debug.LogWarning("[Combo] Attack state mismatch detected. Resetting.");
            ResetAttackChain();
        }
    }
}
