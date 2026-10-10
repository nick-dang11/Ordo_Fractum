using UnityEngine;
using UnityEngine.UI;

public class EnemyPosture : MonoBehaviour
{
    [Header("Posture")]
    [SerializeField, Min(1f)]
    private float maxPosture = 100f;

    [SerializeField, Min(0f)]
    private float postureDamageMultiplier = 1f;

    [Header("Posture Recovery")]
    [SerializeField, Min(0f)]
    private float postureDecreaseRate = 10f;

    [SerializeField, Min(0f)]
    private float decreaseDelay = 2f;

    [Header("UI")]
    [SerializeField]
    private Slider postureSlider;
    private float currentPosture;
    private float decayTimer;
    private bool isPostureBroken;

    public float CurrentPosture => currentPosture;
    public float MaxPosture => maxPosture;
    public bool IsPostureBroken => isPostureBroken;

    private void Start()
    {
        currentPosture = 0f;
        decayTimer = 0f;

        if (postureSlider != null)
        {
            postureSlider.minValue = 0f;
            postureSlider.maxValue = maxPosture;
            postureSlider.value = currentPosture;
        }
    }

    private void Update()
    {
        if (isPostureBroken)
            return;

        decayTimer += Time.deltaTime;

        if (decayTimer >= decreaseDelay &&
            currentPosture > 0f)
        {
            currentPosture -=
                postureDecreaseRate * Time.deltaTime;

            currentPosture = Mathf.Clamp(
                currentPosture,
                0f,
                maxPosture
            );

            UpdatePostureUI();
        }
    }

    public void ApplyPostureDamage(float damage)
    {
        if (damage <= 0f || isPostureBroken)
            return;

        currentPosture +=
            damage * postureDamageMultiplier;

        currentPosture = Mathf.Clamp(
            currentPosture,
            0f,
            maxPosture
        );

        decayTimer = 0f;

        UpdatePostureUI();

        Debug.Log(
            $"[EnemyPosture] {name}: " +
            $"{currentPosture}/{maxPosture}"
        );

        if (currentPosture >= maxPosture)
        {
            TriggerPostureBreak();
        }
    }

    private void TriggerPostureBreak()
    {
        if (isPostureBroken)
            return;

        isPostureBroken = true;

        Debug.Log(
            $"[EnemyPosture] Posture broken on {name}."
        );

        // EnemyStateManager -> StunnedState
        // will be connected next.
    }

    private void UpdatePostureUI()
    {
        if (postureSlider != null)
        {
            postureSlider.value = currentPosture;
        }
    }

    [ContextMenu("Debug Deal Posture Damage")]
    private void DebugAddPosture()
    {
        ApplyPostureDamage(25f);
    }
}