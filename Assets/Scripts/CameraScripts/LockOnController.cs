using System.Collections.Generic;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.InputSystem;

public class LockOnController : MonoBehaviour
{
    [Header("Refrences")]
    [SerializeField] private Transform PlayerRoot;
    [SerializeField] private Transform playerCameraPivot;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Transform characterModel;

    [Header("Cameras")]
    [SerializeField] private GameObject freeLookCamera;
    [SerializeField] private GameObject lockOnCamera;
    [SerializeField] private Transform  lockOnCameraTarget;

    [Header("TargetSearch")]
    [SerializeField] private LayerMask enemyLayers;
    [SerializeField] private LayerMask obstacleLayers;
    [SerializeField] private float lockRange = 18f;
    [SerializeField] private float breakLockRange = 24f;
    [SerializeField] private float screenWindow = 0.45f;
    [SerializeField] private bool  requireLineOfSight = true;

    [Header("Camera Target")]
    //[SerializeField] private float cameraTargetWeight = 0.45f;
    //[SerializeField] private float maxCameraTargetOffset = 5f;
    //[SerializeField] private float cameraTargetSmoothSpeed = 12f;

    [Header("Player Rotation")]
    [SerializeField] private bool rotatePlayerTowardTarget = true;
    [SerializeField] private float rotationSpeed = 14f;

    public Transform CurrentTarget { get; private set;  }
    public bool IsLockedOn => CurrentTarget != null;
    
    private void Awake()
    {
        if (PlayerRoot == null)
            PlayerRoot = transform;

        if (mainCamera == null)
            mainCamera = Camera.main;

        SetCameraMode(false);
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.tabKey.wasPressedThisFrame)
        {
            ToggleLockOn();
        }

        if (!IsLockedOn) 
            return;

        HandleTargetSwitchInput();

        if(!TargetStillValid())
        {
            ClearLockOn();
            return;
        }

        UpdateLockOnCameraTarget();

        if (rotatePlayerTowardTarget)
            RotatePlayerToTarget();

    }

    public void ToggleLockOn()
    {
        Debug.Log("Tab pressed. ToggleLockOn called.");
        if (IsLockedOn)
        {
            ClearLockOn();
            return;
        }

        Transform bestTarget = FindBestTarget();

        if(bestTarget != null)
        {
            LockOn(bestTarget);
        }
        else
        {
            Debug.Log("No Target Found.");
        }
            
            
    }

    public void ClearLockOn()
    {
        CurrentTarget = null;
        SetCameraMode(false);
    }

    private void LockOn(Transform target)
    {
        CurrentTarget = target;
        UpdateLockOnCameraTarget(true);
        SetCameraMode(true);
    }

    private Transform FindBestTarget()
    {
        Collider[] hits = Physics.OverlapSphere(PlayerRoot.position, lockRange, enemyLayers, QueryTriggerInteraction.Collide);

        Transform bestTarget = null;
        float bestScore = float.MaxValue;

        HashSet<LockOnTarget> checkedTargets = new HashSet<LockOnTarget>();

        foreach (Collider hit in hits)
        {
            LockOnTarget lockTarget = hit.GetComponentInParent<LockOnTarget>();

            if (lockTarget == null)
                continue;

            if (!lockTarget.IsLockable)
                continue;

            if (checkedTargets.Contains(lockTarget))
                continue;

            checkedTargets.Add(lockTarget);

            Transform aimPoint = lockTarget.AimPoint;

            Vector3 viewportPoint = mainCamera.WorldToViewportPoint(aimPoint.position);

            if (viewportPoint.z <= 0f)
                continue;

            Vector2 screenOffset = new Vector2(
                viewportPoint.x - 0.5f,
                viewportPoint.y - 0.5f
                );

            if (Mathf.Abs(screenOffset.x) > screenWindow || Mathf.Abs(screenOffset.y) > screenWindow)
                continue;

            float distance = Vector3.Distance(PlayerRoot.position, aimPoint.position);

            if (distance > lockRange)
                continue;

            if (requireLineOfSight)
            {
                Vector3 origin = playerCameraPivot != null
                    ? playerCameraPivot.position
                    : PlayerRoot.position + Vector3.up * 1.5f;

                if (Physics.Linecast(origin, aimPoint.position, obstacleLayers, QueryTriggerInteraction.Ignore))
                    continue;
            }

            float score = screenOffset.sqrMagnitude * 3f + distance / lockRange;

            if (score < bestScore)
            {
                bestScore = score;
                bestTarget = aimPoint;
            }
        }

        return bestTarget;
    }

    private bool TargetStillValid()
    {
        if(CurrentTarget == null) 
            return false;

        float distance = Vector3.Distance(PlayerRoot.position, CurrentTarget.position);

        if(distance > breakLockRange)
            return false;

        Vector3 viewportPoint = mainCamera.WorldToViewportPoint(CurrentTarget.position);

        if(viewportPoint.z <= 0f)
            return false;

        return true;

    }

    private void UpdateLockOnCameraTarget(bool instant = false)
    {
        if (lockOnCameraTarget == null || CurrentTarget == null)
            return;

        Vector3 playerPoint = playerCameraPivot != null
            ? playerCameraPivot.position
            : PlayerRoot.position + Vector3.up * 1.5f;

        
        lockOnCameraTarget.position = playerPoint;

       
        Vector3 lookDirection = CurrentTarget.position - playerPoint;
        lookDirection.y = 0f;

        if (lookDirection.sqrMagnitude > 0.001f)
        {
            lockOnCameraTarget.rotation = Quaternion.LookRotation(lookDirection);
        }
    }

    private void RotatePlayerToTarget()
    {
        Transform objectToRotate = characterModel != null ? characterModel : PlayerRoot;

        Vector3 direction = CurrentTarget.position - objectToRotate.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);

        objectToRotate.rotation = Quaternion.Slerp(
            objectToRotate.rotation,
            targetRotation,
            Time.deltaTime * rotationSpeed
        );
    }

    private void SetCameraMode(bool lockedOn)
    {
        if(freeLookCamera != null)
            freeLookCamera.SetActive(!lockedOn);

        if (lockOnCamera != null)
            lockOnCamera.SetActive(lockedOn);

        if(!lockedOn && lockOnCameraTarget != null)
        {
            Vector3 playerPoint = playerCameraPivot != null ? playerCameraPivot.position : PlayerRoot.position + Vector3.up * 1.5f;

            lockOnCameraTarget.position = playerPoint;
        }
    }

    private void HandleTargetSwitchInput()
    {
        if (Mouse.current == null)
            return;

        float scrollValue = Mouse.current.scroll.ReadValue().y;

        if(scrollValue > 0f)
        {
            SwitchTarget(1);
        }
        else if(scrollValue < 0f)
        {
            SwitchTarget(-1);
        }
    }

    private void SwitchTarget(int direction)
    {
        List<Transform> targets = FindAllValidTargets();

        if (targets.Count <= 1)
            return;

        targets.Sort((a, b) =>
        {
            float ax = mainCamera.WorldToViewportPoint(a.position).x;
            float bx = mainCamera.WorldToViewportPoint(b.position).x;
            return ax.CompareTo(bx);
        });

        int currentIndex = targets.IndexOf(CurrentTarget);

        if (currentIndex == -1)
            return;

        int nextIndex = currentIndex + direction;

        if (nextIndex >= targets.Count)
            nextIndex = 0;

        if(nextIndex < 0)
            nextIndex = targets.Count - 1;

        LockOn(targets[nextIndex]);
    }
    private List<Transform> FindAllValidTargets()
    {
        List<Transform> targets = new List<Transform>();

        Collider[] hits = Physics.OverlapSphere(
            PlayerRoot.position,
            lockRange,
            enemyLayers,
            QueryTriggerInteraction.Collide
        );

        HashSet<LockOnTarget> checkedTargets = new HashSet<LockOnTarget>();

        foreach (Collider hit in hits)
        {
            LockOnTarget lockTarget = hit.GetComponentInParent<LockOnTarget>();

            if (lockTarget == null)
                continue;

            if (!lockTarget.IsLockable)
                continue;

            if (checkedTargets.Contains(lockTarget))
                continue;

            checkedTargets.Add(lockTarget);

            Transform aimPoint = lockTarget.AimPoint;

            Vector3 viewportPoint = mainCamera.WorldToViewportPoint(aimPoint.position);

            if (viewportPoint.z <= 0f)
                continue;

            float distance = Vector3.Distance(PlayerRoot.position, aimPoint.position);

            if (distance > lockRange)
                continue;

            if (requireLineOfSight)
            {
                Vector3 origin = playerCameraPivot != null
                    ? playerCameraPivot.position
                    : PlayerRoot.position + Vector3.up * 1.5f;

                if (Physics.Linecast(origin, aimPoint.position, obstacleLayers, QueryTriggerInteraction.Ignore))
                    continue;
            }

            targets.Add(aimPoint);
        }

        return targets;
    }

}
