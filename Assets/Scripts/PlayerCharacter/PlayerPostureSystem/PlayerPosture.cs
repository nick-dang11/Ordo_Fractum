using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayerPosture : MonoBehaviour
{
    [Header("References")]
    private StateManager stateManager;

    [Header("Posture Settings")]
    public float maxPosture = 100f;
    public float posture = 0f;
    public float postureFillRate = 10f;
    public float posturedecreaseRate = 10f;
    public float decreaseDelay = 1.5f;
    private float decayTimer = 0f;
    public float recentHitTimer = 0f;
    public float recentHitDuration = 3f;

    [Header("UI")]
    public Slider postureSlider;
    private PlayerCombat playerCombat;

    private bool postureBroken;

    void Start()
    {
        stateManager = GetComponent<StateManager>();
        playerCombat = GetComponent<PlayerCombat>();

        maxPosture = Mathf.Max(1f, maxPosture);
        posture = Mathf.Clamp(posture, 0f, maxPosture);

        if (postureSlider != null)
        {
            postureSlider.maxValue = maxPosture;
            postureSlider.value = posture;
        }
    }

    void Update()
    {
        maxPosture = Mathf.Max(1f, maxPosture);
        posture = Mathf.Clamp(posture, 0f, maxPosture);

        if (stateManager != null &&
            stateManager.currentState == stateManager.playerStunnedState)
        {
            if (postureSlider != null)
                postureSlider.value = posture;

            return;
        }

        if (recentHitTimer > 0f)
            recentHitTimer = Mathf.Max(0f, recentHitTimer - Time.deltaTime);

        if (posture >= maxPosture && !postureBroken)
            TriggerGuardBreak();

        if (!postureBroken && playerCombat != null && !playerCombat.IsBlocking())
        {
            decayTimer += Time.deltaTime;

            if (decayTimer >= decreaseDelay)
            {
                posture -= posturedecreaseRate * Time.deltaTime;
                posture = Mathf.Clamp(posture, 0f, maxPosture);
            }
        }

        if (postureSlider != null)
        {
            postureSlider.maxValue = maxPosture;
            postureSlider.value = posture;
        }
    }

    private void TriggerGuardBreak()
    {
        if (postureBroken) return;
        if (stateManager == null) return;

        if (stateManager.currentState == stateManager.playerStunnedState)
            return;

        playerCombat?.ForceStopBlocking();

        stateManager.SwitchState(
            stateManager.playerStunnedState,
            true
        );

        postureBroken =
            stateManager.currentState == stateManager.playerStunnedState;
    }

    public void RecoverFromPostureBreak(float recoveryFraction)
    {
        posture = Mathf.Clamp01(recoveryFraction) * maxPosture;

        decayTimer = 0f;
        recentHitTimer = 0f;

        postureBroken = false;

        if (postureSlider != null)
            postureSlider.value = posture;
    }

    public void NotifyHit()
    {
        recentHitTimer = recentHitDuration;
    }
}
