using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Air Pressure 공기관의 방향을 키보드로 조절한다.
///
/// 1 / 2 : Mesh 로컬 Y축 회전
/// 3 / 4 : Mesh 로컬 Z축 회전
/// </summary>
public class AirPressureRotationController : MonoBehaviour
{
    [Header("References")]

    [SerializeField]
    private Transform airTubeMesh;

    [SerializeField]
    private GameObject airPressurePreview;


    [Header("Rotation Settings")]

    [Tooltip("키를 누르고 있을 때 초당 회전 각도")]
    [SerializeField]
    private float rotationSpeed = 60f;


    private void Update()
    {
        if (!CanControl())
            return;

        HandleRotation();
    }


    /// <summary>
    /// Air Pressure Preview가 활성화된 동안에만
    /// 회전 조작을 허용한다.
    /// </summary>
    private bool CanControl()
    {
        return
            airTubeMesh != null &&
            airPressurePreview != null &&
            airPressurePreview.activeInHierarchy;
    }


    /// <summary>
    /// 숫자키 입력에 따라
    /// Mesh 자신의 로컬축을 기준으로 회전한다.
    /// </summary>
    private void HandleRotation()
    {
        if (Keyboard.current == null)
            return;


        float rotationAmount =
            rotationSpeed *
            Time.deltaTime;


        // 1 → 로컬 Y축 +
        if (Keyboard.current.digit1Key.isPressed)
        {
            airTubeMesh.Rotate(
                Vector3.up,
                rotationAmount,
                Space.Self
            );
        }


        // 2 → 로컬 Y축 -
        if (Keyboard.current.digit2Key.isPressed)
        {
            airTubeMesh.Rotate(
                Vector3.up,
                -rotationAmount,
                Space.Self
            );
        }


        // 3 → 로컬 Z축 +
        if (Keyboard.current.digit3Key.isPressed)
        {
            airTubeMesh.Rotate(
                Vector3.forward,
                rotationAmount,
                Space.Self
            );
        }


        // 4 → 로컬 Z축 -
        if (Keyboard.current.digit4Key.isPressed)
        {
            airTubeMesh.Rotate(
                Vector3.forward,
                -rotationAmount,
                Space.Self
            );
        }
    }
}