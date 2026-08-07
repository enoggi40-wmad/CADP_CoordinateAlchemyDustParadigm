using System;
using UnityEngine;

/// <summary>
/// Smart Dust가 전개될 중심 위치와 반경,
/// 선택된 현상을 관리하는 컨트롤러다.
///
/// 실제 Preview를 그리는 역할은 하지 않고,
/// 전개에 필요한 데이터만 계산하고 보관한다.
///
/// 현상 선택이 변경되면
/// OnSelectedPhenomenonChanged 이벤트를 발생시킨다.
/// </summary>
public class SmartDustDeploymentController : MonoBehaviour
{
    [Header("References")]

    [SerializeField]
    private Transform playerTransform;

    [SerializeField]
    private ScanLockOnSystem scanLockOnSystem;


    [Header("Origin Mode")]

    [SerializeField]
    private CoordinateOriginMode originMode =
        CoordinateOriginMode.Player;


    [Header("Player Coordinate")]

    [SerializeField]
    private Vector3 playerCoordinateOffset =
        new Vector3(0f, 1f, 2f);


    [Header("Locked Object Coordinate")]

    [SerializeField]
    private Vector3 lockedObjectCoordinateOffset =
        Vector3.zero;


    [Header("Deployment Settings")]

    [SerializeField]
    private float radius = 2f;

    [SerializeField]
    private float minimumRadius = 0.1f;

    [SerializeField]
    private float maximumRadius = 20f;


    [Header("Phenomenon")]

    [SerializeField]
    private PhenomenonType selectedPhenomenon;


    /// <summary>
    /// 선택된 현상이 변경되었을 때 호출된다.
    /// </summary>
    public event Action<PhenomenonType>
        OnSelectedPhenomenonChanged;


    public CoordinateOriginMode OriginMode
    {
        get
        {
            return originMode;
        }
    }


    public Vector3 CoordinateOffset
    {
        get
        {
            switch (originMode)
            {
                case CoordinateOriginMode.Player:
                    return playerCoordinateOffset;

                case CoordinateOriginMode.LockedObject:
                    return lockedObjectCoordinateOffset;

                default:
                    return Vector3.zero;
            }
        }
    }


    public float Radius
    {
        get
        {
            return radius;
        }
    }


    public PhenomenonType SelectedPhenomenon
    {
        get
        {
            return selectedPhenomenon;
        }
    }


    public Vector3 WorldCenter
    {
        get
        {
            switch (originMode)
            {
                case CoordinateOriginMode.Player:
                    return CalculatePlayerWorldCenter();

                case CoordinateOriginMode.LockedObject:
                    return CalculateLockedObjectWorldCenter();

                default:
                    return transform.position;
            }
        }
    }


    private void Awake()
    {
        radius =
            Mathf.Clamp(
                radius,
                minimumRadius,
                maximumRadius
            );
    }


    public void SetOriginMode(
        CoordinateOriginMode newOriginMode)
    {
        originMode =
            newOriginMode;
    }


    public void SetCoordinateOffset(
        Vector3 newOffset)
    {
        if (originMode ==
            CoordinateOriginMode.Player)
        {
            playerCoordinateOffset =
                newOffset;
        }
        else if (
            originMode ==
            CoordinateOriginMode.LockedObject)
        {
            lockedObjectCoordinateOffset =
                newOffset;
        }
    }


    public void SetPlayerCoordinateOffset(
        Vector3 newOffset)
    {
        playerCoordinateOffset =
            newOffset;
    }


    public void SetLockedObjectCoordinateOffset(
        Vector3 newOffset)
    {
        lockedObjectCoordinateOffset =
            newOffset;
    }


    public void ResetCurrentCoordinateOffset()
    {
        SetCoordinateOffset(
            Vector3.zero
        );
    }


    public void SetRadius(
        float newRadius)
    {
        radius =
            Mathf.Clamp(
                newRadius,
                minimumRadius,
                maximumRadius
            );
    }


    /// <summary>
    /// 사용할 현상을 변경한다.
    ///
    /// 실제 값이 바뀐 경우에만
    /// OnSelectedPhenomenonChanged 이벤트를 발생시킨다.
    /// </summary>
    public void SetSelectedPhenomenon(
        PhenomenonType newPhenomenon)
    {
        if (selectedPhenomenon ==
            newPhenomenon)
        {
            return;
        }

        selectedPhenomenon =
            newPhenomenon;

        OnSelectedPhenomenonChanged?.Invoke(
            selectedPhenomenon
        );

        Debug.Log(
            $"[Smart Dust] Phenomenon Changed: " +
            $"{selectedPhenomenon}",
            this
        );
    }


    private Vector3 CalculatePlayerWorldCenter()
    {
        if (playerTransform == null)
        {
            Debug.LogWarning(
                "[Smart Dust] Player Transform이 연결되지 않았습니다.",
                this
            );

            return transform.position +
                   playerCoordinateOffset;
        }

        return playerTransform.TransformPoint(
            playerCoordinateOffset
        );
    }


    private Vector3 CalculateLockedObjectWorldCenter()
    {
        if (scanLockOnSystem == null)
        {
            Debug.LogWarning(
                "[Smart Dust] ScanLockOnSystem이 연결되지 않았습니다.",
                this
            );

            return CalculatePlayerWorldCenter();
        }

        Transform lockedTarget =
            scanLockOnSystem.LockedTarget;

        if (lockedTarget == null)
        {
            return CalculatePlayerWorldCenter();
        }

        Renderer targetRenderer =
            lockedTarget.GetComponentInChildren<Renderer>();

        Vector3 originPosition =
            targetRenderer != null
                ? targetRenderer.bounds.center
                : lockedTarget.position;

        return originPosition +
               lockedObjectCoordinateOffset;
    }


    public bool TryToggleOriginMode()
    {
        if (originMode ==
            CoordinateOriginMode.Player)
        {
            if (scanLockOnSystem == null ||
                scanLockOnSystem.LockedTarget == null)
            {
                return false;
            }

            originMode =
                CoordinateOriginMode.LockedObject;

            return true;
        }

        originMode =
            CoordinateOriginMode.Player;

        return true;
    }
}