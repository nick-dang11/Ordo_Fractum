using UnityEngine;

public class PlayerRespawn : MonoBehaviour
{
    public Transform respawnPoint;
    public void Respawn()
    {
        transform.position = respawnPoint.position;
    }
}
