using UnityEngine;

public class EnemyAIController : MonoBehaviour
{
    [SerializeField]
    private Transform player;

    private EnemyMovement movement;

    public Transform Player => player;

    private void Awake()
    {
        movement = GetComponent<EnemyMovement>();
    }
}