
using UnityEngine;
//using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    [SerializeField]
    private Transform player;

    private EnemyMovement movement;
    public Transform Player => player; // postions,rotaion,scale

    private void Awake()
    {
        movement = GetComponent<EnemyMovement>();
    }
}
