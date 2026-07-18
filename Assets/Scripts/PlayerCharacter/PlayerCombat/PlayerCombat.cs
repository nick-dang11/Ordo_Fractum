using UnityEngine;
using System.Collections;

public class PlayerCombat : MonoBehaviour
{
    [SerializeField] public PlayerInputManager input;
    [SerializeField] private Hitbox weaponHitbox;
    [SerializeField] private Animator animate;
    [SerializeField] private float lightAttackDamage = 10f;
    [SerializeField] private float heavyAttackDamage = 40f;
    [SerializeField] private PlayerStamina playerStamina;
    [SerializeField] private PlayerPosture playerPosture;

    private bool wasAttacking = false;
    private bool isAttacking = false;
    private bool isBlocking = false;
    private bool isHolding = false;
    private bool canCombo = false;
    public bool IsAttacking => isAttacking;

    private int comboStep = 0;

    public float lightAttackCooldown = 0.4f;
    private float nextLightAttackTime = 0f;

    public float heavyHoldTime = 0.7f;
    public float heavyAttackCooldown = 1.0f;
    private float nextHeavyAttackTime = 0f;

    private float holdTimer = 0f;

    void Start()
    {
        //animate = GetComponent<Animator>();
        // removed this since manual assignment with serialized fields above - N
    }

    void Update()
    {
        HandleBlock();
        LightAndHeavy();
    }

    void HandleBlock()
    {
        if (input == null) return;
        if (isAttacking)
        {
            isBlocking = false;
            animate.SetBool("Blocking", false);
            return;
        }
        if (input.block && playerStamina.HasStamina(playerStamina.blockStaminaCost))
        {
            isBlocking = true;
            animate.SetBool("Blocking", true);
        }
        else
        {
            isBlocking = false;
            animate.SetBool("Blocking", false);
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
    }

    void LightAndHeavy()
    {
        if (input == null) return;
        if (isBlocking) return;

        if (input.attack && !wasAttacking)
        {
            isHolding = true;
            holdTimer = 0f;

            //StartCoroutine(AttackRoutine(0.3f));

            wasAttacking = true;
        }

        // Continue holding
        else if (isHolding && input.attack)
        {
            holdTimer += Time.deltaTime;
        }

        // Released attack
        else if (!input.attack && wasAttacking)
        {
            if (canCombo)
            {
                animate.SetInteger("ComboStep", comboStep);
                animate.SetTrigger("ComboAttack");
                comboStep++;
            }
            else
            {
                if (holdTimer >= heavyHoldTime && Time.time >= nextHeavyAttackTime)
                {
                    if (playerStamina.HasStamina(playerStamina.heavyAttackStaminaCost)) {
                        playerStamina.UseStamina(playerStamina.heavyAttackStaminaCost);
                        animate.SetInteger("ComboStep", comboStep);
                        animate.SetTrigger("HeavyAttack");
                        nextHeavyAttackTime = Time.time + heavyAttackCooldown;
                        comboStep++;
                        StartCoroutine(AttackRoutine(0.6f, heavyAttackDamage));
                    }
                    else
                    {
                        Debug.Log("Not enough stamina for heavy attack!");
                    }
                }
                else if (Time.time >= nextLightAttackTime)
                {
                    if (playerStamina.HasStamina(playerStamina.lightAttackStaminaCost)) {
                        playerStamina.UseStamina(playerStamina.lightAttackStaminaCost);
                        animate.SetInteger("ComboStep", comboStep);
                        animate.SetTrigger("LightAttack");
                        nextLightAttackTime = Time.time + lightAttackCooldown;
                        comboStep++;
                        StartCoroutine(AttackRoutine(0.3f, lightAttackDamage));
                    }
                    else
                    {
                        Debug.Log("Not enough stamina for light attack!");
                    }
                }
            }
            isHolding = false;
            holdTimer = 0f;
        }
        wasAttacking = input.attack;
    }

    public void StartAttack()
    {
        isAttacking = true;
        isBlocking = false;
        animate.SetBool("Blocking", false);
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
    public void ForceStopBlocking()
    {
        isBlocking = false;
        animate.SetBool("Blocking", false);
    }

    private IEnumerator AttackRoutine(float duration, float damage)
    {
        StartAttack(); // simulate animation event
        weaponHitbox.EnableHitbox(damage);
        yield return new WaitForSeconds(duration * 0.5f);
        EnableCombo(); // simulate combo window
        yield return new WaitForSeconds(duration * 0.5f);
        DisableCombo();
        weaponHitbox.DisableHitbox();
        EndAttack(); // simulate animation event
    }
}