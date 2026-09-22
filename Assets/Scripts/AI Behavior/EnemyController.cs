using UnityEngine;

public class EnemyController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    //Enemy controller script is responsible for controlling the behavior of enemy characters in the game. It can include movement, attack patterns, and interactions with the player or environment. its gonna be hybrid witht the tree visual behavior tree and the code behavior tree. The hybrid approach allows for a combination of visual representation and code-based logic, providing flexibility in designing and implementing enemy behaviors.

    [SerializeField]
    private Transform player;

    private EnemyMovements enemyMovements;

    public Transform Player => player;// public property to access the player transform

    private void Awake()
    {
        enemyMovements = GetComponent<EnemyMovements>();
    }

}
