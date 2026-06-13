using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    public PlayerInputManager input;
    private bool wasAttacking = false;
    private bool isAttacking = false;
    private bool isBlocking = false;

    public float lightAttackCooldown = 0.4f;
    private float nextLightAttackTime = 0f;

    public float heavyHoldTime = 0.7f;
    public float heavyAttackCooldown = 1.0f;
    private float nextHeavyAttackTime = 0f;

    private float holdTimer = 0f;
    private bool isHolding = false;

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
            Debug.Log($"Attack released after {holdTimer:F2} seconds.");
            if (holdTimer >= heavyHoldTime && Time.time >= nextHeavyAttackTime)
            {
                Debug.Log("Heavy");
                animate.SetTrigger("HeavyAttack");
                nextHeavyAttackTime = Time.time + heavyAttackCooldown;
            }
            else if (Time.time >= nextLightAttackTime)
            {
                Debug.Log("Light");
                animate.SetTrigger("LightAttack");
                nextLightAttackTime = Time.time + lightAttackCooldown;
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
    }
}