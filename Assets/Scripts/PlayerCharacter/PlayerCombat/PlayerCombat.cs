using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    public PlayerInputManager input;
    private bool wasAttacking = false;

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
        LightAndHeavy();
    }

    void LightAndHeavy()
    {
        if (input == null) return;
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
}