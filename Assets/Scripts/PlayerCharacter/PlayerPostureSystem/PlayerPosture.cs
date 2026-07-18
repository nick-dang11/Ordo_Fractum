using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayerPosture : MonoBehaviour
{
    [Header("Posture Settings")]
    public float maxPosture = 100f;
    public float posture = 0f;
    public float postureFillRate = 15f;
    public float posturedecreaseRate = 10f;
    public float decreaseDelay = 1.5f;
    private float decayTimer = 0f;

    [Header("UI")]
    public Slider postureSlider;
    private PlayerCombat playerCombat;
    void Start()
    {
        playerCombat = GetComponent<PlayerCombat>();
        postureSlider.maxValue = maxPosture;
        postureSlider.value = posture;
    }

    void Update()
    {
        if (playerCombat.IsBlocking())
        {
            posture += postureFillRate * Time.deltaTime;
            posture = Mathf.Clamp(posture, 0f, maxPosture);
            decayTimer = 0f; // Reset the decay timer when blocking
        }
        else
        {
            decayTimer += Time.deltaTime;
            if (decayTimer >= decreaseDelay)
            {
                posture -= posturedecreaseRate * Time.deltaTime;
                posture = Mathf.Clamp(posture, 0f, maxPosture);
            }
        }
        postureSlider.value = posture;
        if (posture >= maxPosture)
        {
            TriggerGuardBreak();
        }
    }

    void TriggerGuardBreak()
    {
        playerCombat.ForceStopBlocking();
    }
}
