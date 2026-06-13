using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    public PlayerInputManager input;
    private bool wasAttacking = false;
    private bool isAttacking = false;
    private bool isBlocking = false;
    private bool isHolding = false;
    private bool canCombo = false;

    private int comboStep = 0;

    public float lightAttackCooldown = 0.4f;
    private float nextLightAttackTime = 0f;

    public float heavyHoldTime = 0.7f;
    public float heavyAttackCooldown = 1.0f;
    private float nextHeavyAttackTime = 0f;

    private float holdTimer = 0f;

    private Animator animate;

    void Start()
    {
        animate = GetComponent<Animator>();
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
        if (input.block)
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

    void LightAndHeavy()
    {
        if (input == null) return;
        if (isBlocking) return;

        if (input.attack && !wasAttacking)
        {
            isHolding = true;
            holdTimer = 0f;
            Debug.Log("Attack input pressed.");
        }

        if (isHolding && input.attack)
        {
            holdTimer += Time.deltaTime;
        }

        if (!input.attack && wasAttacking)
        {
            if (canCombo)
            {
                animate.SetInteger("ComboStep", comboStep);
                animate.SetTrigger("ComboAttack");
                comboStep++;
            }
            else
            {
                Debug.Log($"Attack released after {holdTimer:F2} seconds.");
                if (holdTimer >= heavyHoldTime && Time.time >= nextHeavyAttackTime)
                {
                    animate.SetInteger("ComboStep", comboStep);
                    Debug.Log("Heavy");
                    animate.SetTrigger("HeavyAttack");
                    nextHeavyAttackTime = Time.time + heavyAttackCooldown;
                    comboStep++;
                }
                else if (Time.time >= nextLightAttackTime)
                {
                    animate.SetInteger("ComboStep", comboStep);
                    Debug.Log("Light");
                    animate.SetTrigger("LightAttack");
                    nextLightAttackTime = Time.time + lightAttackCooldown;
                    comboStep++;
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
}