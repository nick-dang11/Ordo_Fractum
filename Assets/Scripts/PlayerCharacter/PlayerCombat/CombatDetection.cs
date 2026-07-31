using UnityEngine;

public class CombatDetection : MonoBehaviour
{
    public float detectionRadius = 10f;
    [SerializeField] public LayerMask enemyLayerMask;
    [SerializeField] private Animator _Animator;
    public bool isEnemyNearby = false;
    private const int nearbyEnemyLimit = 5;

    public Collider[] detectedEnemy = new Collider[nearbyEnemyLimit];

    private void Update()
    {
        isEnemyNearby = CheckForEnemies() >= 1;
        //Debug.Log(isEnemyNearby);
        _Animator.SetBool("InCombat", isEnemyNearby);
    }

    private int CheckForEnemies()
    {
        int hitCount = Physics.OverlapSphereNonAlloc(
            transform.position,
            detectionRadius,
            detectedEnemy,
            enemyLayerMask
        );
        return hitCount;
    }
}
