using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
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
        if (Input.GetMouseButtonDown(0))
        {
            isHolding = true;
            holdTimer = 0f;
        }
        if (isHolding && Input.GetMouseButton(0))
        {
            holdTimer += Time.deltaTime;
        }
        if (Input.GetMouseButtonUp(0))
        {
            if (holdTimer >= heavyHoldTime && Time.time >= nextHeavyAttackTime)
            {
                animate.SetTrigger("HeavyAttack");
                nextHeavyAttackTime = Time.time + heavyAttackCooldown;
            }
            else if (Time.time >= nextLightAttackTime)
            {
                animate.SetTrigger("LightAttack");
                nextLightAttackTime = Time.time + lightAttackCooldown;
            }
            isHolding = false;
            holdTimer = 0f;
        }
    }
}