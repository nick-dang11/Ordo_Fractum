using UnityEngine;
using System.Collections;

public class PlayerCombat : MonoBehaviour
{
    [SerializeField] public PlayerInputManager input;
    [SerializeField] private Hitbox weaponHitbox;
    [SerializeField] private Animator animate;

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

    void Start()
    {
        //animate = GetComponent<Animator>();
        // removed this since manual assignment with serialized fields above - N
    }

    void Update()
    {
        //HandleBlock();
        LightAndHeavy();
    }

    //void HandleBlock()
    //{
    //    if (input == null) return;
    //    if (isAttacking)
    //    {
    //        isBlocking = false;
    //        animate.SetBool("Blocking", false);
    //        return;
    //    }
    //    if (input.block)
    //    {
    //        isBlocking = true;
    //        animate.SetBool("Blocking", true);
    //    }
    //    else
    //    {
    //        isBlocking = false;
    //        animate.SetBool("Blocking", false);
    //    }
    //}

    void LightAndHeavy()
    {
        if (input == null) return;
        if (isBlocking) return;

        if (input.attack && !wasAttacking)
        {
            //Debug.Log("NEW Attack input detected.");
            isHolding = true;
            holdTimer = 0f;

            StartCoroutine(AttackRoutine(0.3f));
            // currently used to launch attacks, enabling hitbox for 0.3s then disabling
            // we will eventually pivot to animations once we get them but no news yet - N, 6/13

            wasAttacking = true;
        }

        else if (!input.attack)
        {
            wasAttacking = false;
        }

        else if (isHolding && input.attack)
        {
            holdTimer += Time.deltaTime;
        }

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

    private IEnumerator AttackRoutine(float duration)
    {
        weaponHitbox.EnableHitbox();
        yield return new WaitForSeconds(duration);
        weaponHitbox.DisableHitbox();
        isAttacking = false;
    }
}