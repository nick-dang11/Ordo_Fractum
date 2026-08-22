using UnityEngine;
using UnityEngine.UI;
public class EnemyPostureMeter : MonoBehaviour
{
    [Header("Posture Settings")]
    public float maxPosture = 100f;
    public float posture = 0f;
    public float postureFillRate = 15f;
    public float posturedecreaseRate = 10f;
    public float decreaseDelay = 1.5f;
    private float decayTimer = 0f;
    public float postureDamageMultiplier = 1.0f; // Multiplier for posture damage from attacks

    [Header("UI")]
    public Slider postureSlider;
    private PillEnemyCombat enemyCombat;
    void Start()
    {
        enemyCombat = GetComponent<PillEnemyCombat>();
        if (postureSlider != null)
        {
            postureSlider.maxValue = maxPosture;
            postureSlider.value = posture;
        }
    }

    void Update()
    {
        if (enemyCombat.IsBlocking())
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
        if (postureSlider != null)
        {
            postureSlider.value = posture;
        }
        if (posture >= maxPosture)
        {
            TriggerPostureBreak();
        }
    }

    public void ApplyPostureDamage(float damage)
    {
        posture += damage * postureDamageMultiplier;
        posture = Mathf.Clamp(posture, 0f, maxPosture);
        decayTimer = 0f;
        if (posture >= maxPosture)
        {
            TriggerPostureBreak();
        }
    }

    private void TriggerPostureBreak()
    {
        Debug.Log("Enemy's posture broken!");
        enemyCombat.ForceSturn();
        if (postureSlider != null)
        {
            postureSlider.value = posture;
        }
    }
}
