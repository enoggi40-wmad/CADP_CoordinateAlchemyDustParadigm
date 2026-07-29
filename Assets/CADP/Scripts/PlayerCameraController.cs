using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCameraController : MonoBehaviour
{
    [Header("References")]

    // 좌우 회전의 기준이 되는 Player Transform
    [SerializeField]
    private Transform playerTransform;

    // 상하 회전을 담당하는 Camera Pivot
    [SerializeField]
    private Transform cameraPivot;

    [Header("Mouse Look")]

    // 마우스 회전 감도
    [SerializeField]
    private float mouseSensitivity = 120f;

    // 카메라가 아래쪽으로 회전할 수 있는 최대 각도
    [SerializeField]
    private float minPitch = -30f;

    // 카메라가 위쪽으로 회전할 수 있는 최대 각도
    [SerializeField]
    private float maxPitch = 45f;

    [Header("State")]

    // 외부 시스템에서 카메라 회전을 허용하거나 차단할 때 사용한다.
    [SerializeField]
    private bool rotationEnabled = true;

    // 현재 카메라의 상하 회전값
    private float pitch;

    private void Update()
    {
        // 카메라 회전이 차단된 상태라면 입력을 처리하지 않는다.
        if (!rotationEnabled)
            return;

        RotateByMouse();
    }

    private void RotateByMouse()
    {
        // 마우스 장치가 존재하지 않으면 실행하지 않는다.
        if (Mouse.current == null)
            return;

        // 현재 프레임의 마우스 이동량을 읽는다.
        Vector2 mouseDelta =
            Mouse.current.delta.ReadValue();

        // 마우스 이동량에 감도와 프레임 시간을 적용한다.
        float mouseX =
            mouseDelta.x *
            mouseSensitivity *
            Time.deltaTime;

        float mouseY =
            mouseDelta.y *
            mouseSensitivity *
            Time.deltaTime;

        // 좌우 회전은 Player 전체를 회전시킨다.
        if (playerTransform != null)
        {
            playerTransform.Rotate(
                Vector3.up * mouseX
            );
        }

        // 마우스를 위로 움직이면 카메라는 위를 바라보도록
        // pitch 값을 반대 방향으로 변경한다.
        pitch -= mouseY;

        // 카메라가 너무 위나 아래로 뒤집히지 않도록 제한한다.
        pitch = Mathf.Clamp(
            pitch,
            minPitch,
            maxPitch
        );

        // 상하 회전은 Camera Pivot에만 적용한다.
        if (cameraPivot != null)
        {
            cameraPivot.localRotation =
                Quaternion.Euler(
                    pitch,
                    0f,
                    0f
                );
        }
    }

    /// <summary>
    /// 외부 시스템에서 카메라 회전 가능 여부를 설정한다.
    /// </summary>
    public void SetRotationEnabled(bool enabled)
    {
        rotationEnabled = enabled;
    }
}