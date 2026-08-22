using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [SerializeField] public PlayerInputManager input;
    [SerializeField] public WeaponHitbox weaponHitbox;
    [SerializeField] private Animator animate;
    [SerializeField] private float lightAttackDamage = 10f;
    [SerializeField] private float heavyAttackDamage = 40f;
    // [SerializeField] private PlayerStamina playerStamina;
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

    void Start() { }

    void Update() { }

    public void HandleBlock()
    {
        if (input == null) return;
        if (isAttacking)
        {
            isBlocking = false;
            animate.SetBool("Block", false);
            return;
        }
        if (input.block)
        {
            lastBlockTime = Time.time;
            isBlocking = true;
            animate.SetBool("Block", true);
        }
        else
        {
            isBlocking = false;
            animate.SetBool("Block", false);
        }
    }

    public bool IsBlocking()
    {
        return isBlocking;
    }

    public void TriggerBlockFeedback()
    {
        Debug.Log("Player BLOCKED the attack!");
        animate.SetTrigger("BlockHit");
        playerPosture.posture += playerPosture.postureFillRate * 3f;
        playerPosture.posture = Mathf.Clamp(playerPosture.posture, 0f, playerPosture.maxPosture);
        playerPosture.postureSlider.value = playerPosture.posture;
    }

    public void LightAndHeavy()
    {
        if (input == null) return;
        if (isBlocking) return;

        bool lightPressed = input.attack && !wasLightAttacking;
        bool heavyPressed = input.heavyAttack && !wasHeavyAttacking;

        // If both are pressed on the same frame, heavy takes priority.
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
        wasAttacking = input.attack || input.heavyAttack;
    }

    private void ProcessAttackInput(bool isHeavy)
    {
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
        isAttacking = true;
        isBlocking = false;
        animate.SetBool("Block", false);
    }

    public void EndAttack()
    {
        isAttacking = false;
        canCombo = false;
        comboStep = 0;
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
        weaponHitbox.EnableHitbox(pendingDamage);
    }

    public void DisableWeaponHitbox()
    {
        weaponHitbox.DisableHitbox();
    }

    public void ForceStopBlocking()
    {
        isBlocking = false;
        animate.SetBool("Block", false);
    }

    public void TriggerDeflectFeedback()
    {
        Debug.Log("DEFLECT!");
        animate.SetTrigger("Deflect");
    }

    public void SetPendingDamage(float damage)
    {
        pendingDamage = damage;
    }
}