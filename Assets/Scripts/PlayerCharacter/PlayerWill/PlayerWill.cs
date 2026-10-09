using UnityEngine;
using UnityEngine.UI;

public class PlayerWill : MonoBehaviour
{
    [Header("Will Settings")]
    [SerializeField] private float maxWill = 99f;
    [SerializeField] private float currentWill = 99f;

    [Header("UI")]
    [SerializeField] private RectTransform willFill;
    public float CurrentWill => currentWill;
    public float MaxWill => maxWill;

    private void Start()
    {
        currentWill = Mathf.Clamp(currentWill, 0f, maxWill);
        UpdateWillUI();
    }

    public bool HasWill(float amount)
    {
        return currentWill >= amount;
    }

    public bool SpendWill(float amount)
    {
        if (!HasWill(amount))
        {
            Debug.Log("Not enough Will.");
            return false;
        }

        currentWill -= amount;
        currentWill = Mathf.Clamp(currentWill, 0f, maxWill);

        UpdateWillUI();

        Debug.Log(
            $"[Will] Current Will: {currentWill}/{maxWill}"
        );

        return true;
    }

    public void RestoreWill(float amount)
    {
        currentWill += amount;
        currentWill = Mathf.Clamp(currentWill, 0f, maxWill);

        UpdateWillUI();
    }

    private void UpdateWillUI()
    {
        if (willFill == null)
            return;

        float normalizedWill =
            maxWill > 0f
                ? currentWill / maxWill
                : 0f;

        Vector2 anchorMax = willFill.anchorMax;
        anchorMax.x = normalizedWill;
        willFill.anchorMax = anchorMax;

        Debug.Log(
            $"[Will UI] {currentWill}/{maxWill} = " +
            $"{normalizedWill * 100f}%"
        );
    }
}