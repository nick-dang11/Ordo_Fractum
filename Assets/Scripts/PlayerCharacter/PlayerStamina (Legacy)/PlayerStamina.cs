using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStamina : MonoBehaviour {
    [Header("Stamina Settings")]
    public float maxStamina = 100f;
    public float stamina;
    public float staminaRegenRate = 5f;
    public float blockStaminaCost = 20f;
    public float blockStaminaDrainRate = 5f;
    public float lightAttackStaminaCost = 10f;
    public float heavyAttackStaminaCost = 25f;

    [UnitHeaderInspectable("UI")]
    public Slider staminaSlider;
    private PlayerCombat playerCombat;
    void Start() {
        stamina = maxStamina;
        staminaSlider.maxValue = maxStamina;
        staminaSlider.value = maxStamina;
        playerCombat = GetComponent<PlayerCombat>();
    }
    void Update() {
        // Regenerate stamina over time if not blocking or attacking
        if (!playerCombat.IsBlocking() && !playerCombat.IsAttacking) {
            stamina += staminaRegenRate * Time.deltaTime;
            stamina = Mathf.Clamp(stamina, 0f, maxStamina);
        }
        // Drain stamina while blocking
        if (playerCombat.IsBlocking()) {
            stamina -= blockStaminaDrainRate * Time.deltaTime;
            stamina = Mathf.Clamp(stamina, 0f, maxStamina);
            // If stamina runs out while blocking, force stop blocking
            if (stamina <= 0f)
            {
                playerCombat.ForceStopBlocking();
            }
        }
        staminaSlider.value = stamina;
    }
    // Check if the player has enough stamina for an action
    public bool HasStamina(float cost)
    {
        return stamina >= cost;
    }
    // Use stamina for an action
    public void UseStamina(float cost)
    {
        stamina -= cost;
        stamina = Mathf.Clamp(stamina, 0f, maxStamina);
        staminaSlider.value = stamina;
    }
}
