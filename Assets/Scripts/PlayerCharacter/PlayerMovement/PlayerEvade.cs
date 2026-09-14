using Unity.VisualScripting;
using UnityEngine;

public class PlayerEvade : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private float safetyTimeout = 1.5f;

    private int evadeLayerIndex;

    private int hashEvade = Animator.StringToHash("Evade");
    private int hashEvadeX = Animator.StringToHash("EvadeX");
    private int hashEvadeY = Animator.StringToHash("EvadeY");
    private int hashIsEvading = Animator.StringToHash("IsEvading");

    public bool IsEvadeComplete { get; private set;}
    private float evadeTimer;


    private void Awake()
    {
        if (animator == null) animator = GetComponentInChildren<Animator>();

        evadeLayerIndex = animator.GetLayerIndex("Evade Layer");
    }

    public void StartEvade(Vector2 localDirection)
    {
        IsEvadeComplete = false;
        evadeTimer = 0f;

        animator.SetFloat(hashEvadeX, localDirection.x);
        animator.SetFloat(hashEvadeY, localDirection.y);
        animator.SetBool(hashIsEvading, true);
        animator.SetTrigger(hashEvade);
    }

    public void UpdateEvade()
    {
        if (IsEvadeComplete) return;

        evadeTimer += Time.deltaTime;

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(evadeLayerIndex);

        bool isCurrentlyEvading = stateInfo.IsTag("Evading") || stateInfo.IsName("Directional Evade");

        if(isCurrentlyEvading && stateInfo.normalizedTime >= 1.0f && !animator.IsInTransition(0))
        {
            IsEvadeComplete = true;
        }
        else if(evadeTimer >= safetyTimeout)
        {
            IsEvadeComplete = true;
        }
    }

    public void CleanupEvade()
    {
        animator.SetBool(hashIsEvading, false);
        IsEvadeComplete = false;
    }
}
