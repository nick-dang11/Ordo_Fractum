using UnityEngine;

public class LockOnTarget : MonoBehaviour
{
    [SerializeField] private Transform aimPoint;
    public bool IsLockable = true;

    public Transform AimPoint
    {
        get
        {
            return aimPoint != null ? aimPoint : transform;
        }
    }
}
