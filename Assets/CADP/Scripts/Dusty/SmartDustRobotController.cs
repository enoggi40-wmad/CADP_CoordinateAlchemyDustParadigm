using UnityEngine;

public class SmartDustRobotController : MonoBehaviour
{
    [Header("Movement")]

    [SerializeField]
    private float moveSpeed = 5f;

    [SerializeField]
    private float rotationSpeed = 8f;

    [SerializeField]
    private float arrivalDistance = 0.05f;


    private Vector3 targetPosition;

    // 목적지에 도착한 뒤 바라볼 최종 방향
    private Vector3 arrivalFacingDirection;

    // 도착 후 최종 방향을 적용할지 여부
    private bool useArrivalFacingDirection;


    private void Awake()
    {
        targetPosition = transform.position;
        arrivalFacingDirection = transform.forward;
    }


    private void Update()
    {
        UpdateMovementAndRotation();
    }


    /// <summary>
    /// 이동 목적지만 지정한다.
    /// 도착하면 이동 중 바라보던 방향을 그대로 유지한다.
    /// </summary>
    public void SetTargetPosition(Vector3 position)
    {
        targetPosition = position;
        useArrivalFacingDirection = false;

        //Debug.Log(
        //    $"[SmartDustRobot] Target Position = {targetPosition}",
        //    this
        //);
    }


    /// <summary>
    /// 이동 목적지와 도착 후 바라볼 방향을 함께 지정한다.
    /// </summary>
    public void SetTarget(
        Vector3 position,
        Vector3 finalFacingDirection
    )
    {
        targetPosition = position;

        finalFacingDirection.y = 0f;

        if (finalFacingDirection.sqrMagnitude > 0.0001f)
        {
            arrivalFacingDirection =
                finalFacingDirection.normalized;

            useArrivalFacingDirection = true;
        }
    }


    /// <summary>
    /// 목표 위치로 자연스럽게 이동하고,
    /// 이동 중에는 수평 이동 방향을 바라본다.
    /// </summary>
    private void UpdateMovementAndRotation()
    {
        // 실제 3차원 이동 방향
        // X, Y, Z 거리를 모두 포함한다.
        Vector3 directionToTarget =
            targetPosition - transform.position;

        // 도착 여부는 Y축까지 포함한 실제 거리로 판단한다.
        bool hasArrived =
            directionToTarget.sqrMagnitude <=
            arrivalDistance * arrivalDistance;

        if (!hasArrived)
        {
            // X, Y, Z 모든 축으로 자연스럽게 이동한다.
            MoveToTarget();

            /*
             * 회전에는 Y축 높이 차이를 제외한다.
             * 그렇지 않으면 위로 이동할 때 로봇이
             * 고개를 하늘로 들거나 아래로 숙일 수 있다.
             */
            Vector3 horizontalDirection =
                new Vector3(
                    directionToTarget.x,
                    0f,
                    directionToTarget.z
                );

            RotateTowards(horizontalDirection);

            return;
        }

        // 매우 가까워졌을 때만 남은 미세한 위치 오차를 정리한다.
        transform.position = targetPosition;

        // 플레이어 대기 위치에 도착한 경우에는
        // 플레이어가 바라보는 방향으로 자연스럽게 정렬한다.
        if (useArrivalFacingDirection)
        {
            RotateTowards(
                arrivalFacingDirection
            );
        }
    }



    private void MoveToTarget()
    {
        //Debug.Log(
        //    $"[SmartDustRobot] Current = {transform.position} / Target = {targetPosition}",
        //    this
        //);

        transform.position =
            Vector3.MoveTowards(
                transform.position,
                targetPosition,
                moveSpeed * Time.deltaTime
            );
    }


    private void RotateTowards(Vector3 direction)
    {
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.0001f)
            return;

        Quaternion targetRotation =
            Quaternion.LookRotation(
                direction.normalized,
                Vector3.up
            );

        transform.rotation =
            Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
    }
}