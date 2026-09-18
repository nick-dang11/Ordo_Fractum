using UnityEngine;

public class HealingAnimation : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInputManager playerInputManager;
    [SerializeField] private HealthSystem healthSystem;
    [SerializeField] private Animator playerAnimator;

    [Header("Timing for Heal Animation")]
    [SerializeField] private float applyHealAfter = 0.8f;
    [SerializeField] private float healingDuration = 1.8f;
    
    //Represnets whether the player is currently healing or not
    private bool isHealing; // Represents whether the player is currently healing or not
    private bool healingAlreadyApplied; // Represents whether the healing effect has already been applied during the current healing process
    private float healingTimer; // Timer for the healing process

    private static readonly int HealTrigger = Animator.StringToHash("Heal");
    // Healtrigger is hash value of the "Heal" trigger parameter in the animator to trigger the healing animation.
    public bool IsHealing => isHealing; // Public property to access the isHealing state from other scripts

    private void Update()
    {
        if(playerInputManager.self_heal)// when press "R" key,self_heal is set to true in PlayerInputManager
        {
            playerInputManager.self_heal = false; // Reset the self_heal flag to false after processing the healing input
            TryStartHealing();
        }

        if(!isHealing)
        {
            return;
        }

        healingTimer += Time.deltaTime;
        if(!healingAlreadyApplied && healingTimer >= applyHealAfter)
        {
            ApplyHealingEffect();
        }

        if(healingTimer >= healingDuration)
        {
            FinishHealing();
        }
       // isHealing = true;
       // healingAlreadyApplied = false;
       // ApplyHealingEffect();
    }

     private void TryStartHealing()
    {
        if (isHealing)
        {
            Debug.Log("Cannot heal: Already healing.");
            return;
        }

        if(healthSystem.health >= healthSystem.maxHealth)
        {
            Debug.Log("Cannot heal: Health is already full.");
            return;
        }

        //Reseting the healing state and timer when starting a new healing process
        isHealing = true;
        healingAlreadyApplied = false;
        healingTimer = 0f;
        playerAnimator.SetTrigger(HealTrigger); //Trigger the healing animation in the animator
        Debug.Log("Healing started.");
    }

    private void ApplyHealingEffect()
    {
        if (!isHealing || healingAlreadyApplied)
        {
            Debug.Log("Cannot apply healing effect: Not currently healing.");
            return;
        }

        healingAlreadyApplied = true;
        healthSystem.Heal(healthSystem.heal_Amount);
        Debug.Log("Healing applied");
    }
    public void FinishHealing()
    {
    
        isHealing = false;
        healingAlreadyApplied = false;
        healingTimer = 0f;
        Debug.Log("Healing finished.");
    }
  
}
