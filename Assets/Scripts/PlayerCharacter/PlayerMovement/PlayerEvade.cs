using Unity.VisualScripting;
using UnityEngine;

public class PlayerEvade : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private float safetyTimeout = 1.5f;

    public float SafetyTimeout
    {
        get => safetyTimeout;
        set => safetyTimeout = value;
    }
    [SerializeField] private PlayerEvadeVFX evadeVFX;

    private int hashEvade = Animator.StringToHash("Evade");
    private int hashEvadeX = Animator.StringToHash("EvadeX");
    private int hashEvadeY = Animator.StringToHash("EvadeY");
    private int hashIsEvading = Animator.StringToHash("IsEvading");

    public bool IsEvadeComplete { get; private set;}
    private float evadeTimer;


    private void Awake()
    {
        if (animator == null) animator = GetComponentInChildren<Animator>();
    }

    public void StartEvade(Vector2 localDirection)
    {
        IsEvadeComplete = false;
        evadeTimer = 0f;

        animator.SetFloat(hashEvadeX, localDirection.x);
        animator.SetFloat(hashEvadeY, localDirection.y);
        animator.SetBool(hashIsEvading, true);
        animator.SetTrigger(hashEvade);
        
        if(evadeVFX != null)
        {
            evadeVFX.PlayEvadeStart();
        }
    }

    public void UpdateEvade()
    {
        if (IsEvadeComplete)
            return;

        evadeTimer += Time.deltaTime;

        AnimatorStateInfo stateInfo =
            animator.GetCurrentAnimatorStateInfo(0);

        bool isCurrentlyEvading =
            stateInfo.IsTag("Evading") ||
            stateInfo.IsName("Directional Evade");

        if (isCurrentlyEvading &&
            stateInfo.normalizedTime >= 1.0f &&
            !animator.IsInTransition(0))
        {
            IsEvadeComplete = true;
        }
        else if (evadeTimer >= safetyTimeout)
        {
            Debug.LogWarning(
                "PlayerEvade: Safety timeout reached before evade completed."
            );

            IsEvadeComplete = true;
        }
    }

    public void CleanupEvade()
    {
        animator.ResetTrigger(hashEvade);
        animator.SetBool(hashIsEvading, false);

        IsEvadeComplete = false;
    }
}
