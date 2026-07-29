using UnityEngine;

/// <summary>
/// Smart Dust가 전개될 중심 위치와 반경,
/// 선택된 현상을 관리하는 컨트롤러다.
///
/// 실제 Preview를 그리는 역할은 하지 않고,
/// 전개에 필요한 데이터만 계산하고 보관한다.
/// </summary>
public class SmartDustDeploymentController : MonoBehaviour
{
    [Header("References")]

    // Player 기준 좌표를 계산할 때 사용할 플레이어 Transform
    [SerializeField]
    private Transform playerTransform;

    // 현재 Lock-On된 대상을 가져오기 위한 시스템
    [SerializeField]
    private ScanLockOnSystem scanLockOnSystem;


    [Header("Origin Mode")]

    // 현재 좌표의 원점이 Player인지
    // Locked Object인지 결정한다.
    [SerializeField]
    private CoordinateOriginMode originMode =
        CoordinateOriginMode.Player;


    [Header("Player Coordinate")]

    // Player를 원점으로 사용할 때의 좌표 오프셋
    //
    // X: 플레이어 기준 오른쪽
    // Y: 플레이어 기준 위쪽
    // Z: 플레이어 기준 앞쪽
    //
    // Preview가 플레이어 발밑이 아니라
    // 플레이어 앞쪽 공중에서 시작하도록 기본값을 설정한다.
    [SerializeField]
    private Vector3 playerCoordinateOffset =
        new Vector3(0f, 1f, 2f);


    [Header("Locked Object Coordinate")]

    // Locked Object를 원점으로 사용할 때의 좌표 오프셋
    //
    // 기본값이 0, 0, 0이므로 Locked Object 모드로
    // 처음 변경하면 잠긴 오브젝트 중심에 Preview가 나타난다.
    [SerializeField]
    private Vector3 lockedObjectCoordinateOffset =
        Vector3.zero;


    [Header("Deployment Settings")]

    // Smart Dust 전개 반경
    [SerializeField]
    private float radius = 2f;

    // 허용할 최소 반경
    [SerializeField]
    private float minimumRadius = 0.1f;

    // 허용할 최대 반경
    [SerializeField]
    private float maximumRadius = 20f;


    [Header("Phenomenon")]

    // 현재 선택된 현상
    [SerializeField]
    private PhenomenonType selectedPhenomenon;


    /// <summary>
    /// 현재 선택된 좌표 원점 모드
    /// </summary>
    public CoordinateOriginMode OriginMode
    {
        get
        {
            return originMode;
        }
    }


    /// <summary>
    /// 현재 원점 모드에 해당하는 좌표 오프셋
    ///
    /// Player 모드라면 Player용 좌표를 반환하고,
    /// Locked Object 모드라면 Locked Object용 좌표를 반환한다.
    /// </summary>
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


    /// <summary>
    /// Smart Dust의 현재 전개 반경
    /// </summary>
    public float Radius
    {
        get
        {
            return radius;
        }
    }


    /// <summary>
    /// 현재 선택된 현상
    /// </summary>
    public PhenomenonType SelectedPhenomenon
    {
        get
        {
            return selectedPhenomenon;
        }
    }


    /// <summary>
    /// 현재 설정을 기준으로 계산된
    /// Smart Dust의 실제 월드 중심 좌표
    /// </summary>
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
        // Inspector에서 잘못된 반경이 입력되어 있더라도
        // 게임 시작 시 허용 범위 안으로 제한한다.
        radius = Mathf.Clamp(
            radius,
            minimumRadius,
            maximumRadius
        );
    }


    /// <summary>
    /// 좌표 원점 모드를 변경한다.
    ///
    /// 각 모드의 Coordinate Offset은 따로 저장되므로
    /// Player와 Locked Object를 오가더라도
    /// 서로의 좌표값을 덮어쓰지 않는다.
    /// </summary>
    public void SetOriginMode(
        CoordinateOriginMode newOriginMode)
    {
        originMode = newOriginMode;
    }


    /// <summary>
    /// 현재 활성화된 원점 모드의
    /// Coordinate Offset을 변경한다.
    /// </summary>
    public void SetCoordinateOffset(Vector3 newOffset)
    {
        if (originMode == CoordinateOriginMode.Player)
        {
            // Player 모드에서는 Player 좌표만 변경
            playerCoordinateOffset = newOffset;
        }
        else if (
            originMode == CoordinateOriginMode.LockedObject)
        {
            // Locked Object 모드에서는 Object 좌표만 변경
            lockedObjectCoordinateOffset = newOffset;
        }
    }


    /// <summary>
    /// Player 모드의 좌표 오프셋을 직접 변경한다.
    ///
    /// 현재 원점 모드와 관계없이 Player용 좌표만 변경한다.
    /// </summary>
    public void SetPlayerCoordinateOffset(
        Vector3 newOffset)
    {
        playerCoordinateOffset = newOffset;
    }


    /// <summary>
    /// Locked Object 모드의 좌표 오프셋을 직접 변경한다.
    ///
    /// 현재 원점 모드와 관계없이
    /// Locked Object용 좌표만 변경한다.
    /// </summary>
    public void SetLockedObjectCoordinateOffset(
        Vector3 newOffset)
    {
        lockedObjectCoordinateOffset = newOffset;
    }


    /// <summary>
    /// 현재 활성화된 원점 모드의
    /// Coordinate Offset을 0, 0, 0으로 초기화한다.
    /// </summary>
    public void ResetCurrentCoordinateOffset()
    {
        SetCoordinateOffset(Vector3.zero);
    }


    /// <summary>
    /// Smart Dust 전개 반경을 변경한다.
    /// </summary>
    public void SetRadius(float newRadius)
    {
        // 반경이 음수가 되거나
        // 지나치게 커지지 않도록 제한한다.
        radius = Mathf.Clamp(
            newRadius,
            minimumRadius,
            maximumRadius
        );
    }


    /// <summary>
    /// 사용할 현상을 변경한다.
    /// </summary>
    public void SetSelectedPhenomenon(
        PhenomenonType newPhenomenon)
    {
        selectedPhenomenon = newPhenomenon;
    }


    /// <summary>
    /// Player를 원점으로 한 월드 좌표를 계산한다.
    /// </summary>
    private Vector3 CalculatePlayerWorldCenter()
    {
        // Player Transform이 연결되지 않았다면
        // 현재 컨트롤러 위치를 기준으로 임시 계산한다.
        if (playerTransform == null)
        {
            Debug.LogWarning(
                "[Smart Dust] Player Transform이 연결되지 않았습니다.",
                this
            );

            return transform.position +
                   playerCoordinateOffset;
        }

        // TransformPoint를 사용하면 플레이어의 회전 방향을
        // 반영한 로컬 좌표를 월드 좌표로 변환할 수 있다.
        //
        // 예:
        // Z = 2라면 플레이어가 바라보는 방향으로 2m 앞이다.
        return playerTransform.TransformPoint(
            playerCoordinateOffset
        );
    }


    /// <summary>
    /// Locked Object를 원점으로 한 월드 좌표를 계산한다.
    /// </summary>
    private Vector3 CalculateLockedObjectWorldCenter()
    {
        // Lock-On 시스템이 연결되어 있지 않다면
        // 잠긴 대상을 가져올 수 없다.
        if (scanLockOnSystem == null)
        {
            Debug.LogWarning(
                "[Smart Dust] ScanLockOnSystem이 연결되지 않았습니다.",
                this
            );

            return CalculatePlayerWorldCenter();
        }

        // 현재 Lock-On된 대상 Transform을 가져온다.
        Transform lockedTarget =
            scanLockOnSystem.LockedTarget;

        // 잠긴 대상이 없다면 Player 위치를 임시로 사용한다.
        if (lockedTarget == null)
        {
            return CalculatePlayerWorldCenter();
        }

        // Locked Object의 로컬 좌표를
        // 실제 월드 좌표로 변환한다.
        //
        // lockedObjectCoordinateOffset이 0, 0, 0이면
        // lockedTarget의 위치가 그대로 WorldCenter가 된다.
        return lockedTarget.position +
       lockedObjectCoordinateOffset;
    }

    /// <summary>
    /// Player 원점과 Locked Object 원점을 서로 전환한다.
    ///
    /// Locked Object로 전환할 때 잠긴 대상이 없다면
    /// 전환하지 않고 false를 반환한다.
    /// </summary>
    /// <summary>
    /// Player와 Locked Object 원점을 전환한다.
    /// 각 원점의 좌표값은 서로 복사하지 않는다.
    /// </summary>
    public bool TryToggleOriginMode()
    {
        // Player에서 Locked Object로 전환
        if (originMode == CoordinateOriginMode.Player)
        {
            // 잠긴 대상이 없으면 전환할 수 없다.
            if (scanLockOnSystem == null ||
                scanLockOnSystem.LockedTarget == null)
            {
                return false;
            }

            // 원점 모드만 변경한다.
            // Player 좌표를 Locked Object 좌표에 복사하면 안 된다.
            originMode = CoordinateOriginMode.LockedObject;

            return true;
        }

        // Locked Object에서 Player로 복귀
        originMode = CoordinateOriginMode.Player;

        return true;
    }
}