using System.Runtime.CompilerServices;
using UnityEngine;

public class CombatDetection : MonoBehaviour
{
    public float detectionRadius = 10f;
    [SerializeField] public LayerMask enemyLayerMask;
    [SerializeField] private Animator _Animator;
    public bool isEnemyNearby;
    private const int nearbyEnemyLimit = 5;

    [SerializeField] private float combatExitDelay = 1f;
    private float noEnemyTimer;
    
    public Collider[] detectedEnemy = new Collider[nearbyEnemyLimit];


    private void Update()
    {
        bool enemyDetected = CheckForEnemies() >= 1;

        if (enemyDetected)
        {
            noEnemyTimer = 0f;
            isEnemyNearby = true;
        }
        else
        {
            noEnemyTimer += Time.deltaTime;

            if (noEnemyTimer >= combatExitDelay)
                isEnemyNearby = false;
        }

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
