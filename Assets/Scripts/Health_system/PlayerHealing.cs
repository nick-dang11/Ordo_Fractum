using UnityEngine;

public class PlayerHealing : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInputManager playerInputManager;
    [SerializeField] private HealthSystem healthSystem;
    [SerializeField] private Animator playerAnimator;

    private bool isHealing;
    private bool healingAlreadyApplied;

    public bool IsHealing => isHealing;

    private static readonly int HealTrigger = Animator.StringToHash("Heal");

    private void Update()
    {
        if(!playerInputManager.self_heal)
        {
           
            return;
        }

        playerInputManager.self_heal = false;
        TryStartHealing();
    }

    public void ApplyHealingEffect()
    {
        if (!isHealing)
        {
            Debug.Log("Cannot apply healing effect: Not currently healing.");
            return;
        }

        if(healingAlreadyApplied)
        {
            Debug.Log("Healing effect has already been applied.");
            return;
        }

        healingAlreadyApplied = true;
        healthSystem.Heal(healthSystem.heal_Amount);
    }

    private void TryStartHealing()
    {
        if (isHealing || healthSystem.health >= healthSystem.maxHealth)
        {
            Debug.Log("Cannot heal: Already healing or at max health.");
            return;
        }

        if(healthSystem.health == healthSystem.maxHealth)
        {
           Debug.Log("Cannot heal: Player is at max health.");
            return;
        }

        isHealing = true;
        healingAlreadyApplied = false;
        playerAnimator.SetTrigger(HealTrigger);
        Debug.Log("Healing started.");
    }

    public void FinishHealing()
    {
    
        isHealing = false;
        healingAlreadyApplied = false;
        Debug.Log("Healing finished.");
    }
  
}
