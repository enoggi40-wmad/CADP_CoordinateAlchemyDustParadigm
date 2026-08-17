using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class AirPressureRotationController : MonoBehaviour
{
    [Header("References")]

    [Tooltip("실제로 회전시킬 Air Tube Mesh")]
    [SerializeField]
    private Transform airTubeMesh;

    [Tooltip("Air Pressure Preview 루트")]
    [SerializeField]
    private GameObject airPressurePreview;


    [Header("Rotation Settings")]

    [Tooltip("LMB Drag 회전 감도")]
    [SerializeField]
    private float mouseRotationSpeed = 1.5f;

    [Tooltip("Q/E 회전 속도")]
    [SerializeField]
    private float zRotationSpeed = 60f;


    private void Update()
    {
        if (!CanControl())
            return;

        HandleMouseRotation();
        HandleZRotation();
    }


    /// <summary>
    /// 현재 Air Pressure 방향을
    /// 조절할 수 있는 상태인지 확인한다.
    /// </summary>
    private bool CanControl()
    {
        if (airTubeMesh == null ||
            airPressurePreview == null)
        {
            return false;
        }

        return airPressurePreview.activeInHierarchy;
    }


    /// <summary>
    /// LMB Drag로
    /// Air Tube Mesh 자신의 로컬 X축을 회전시킨다.
    /// </summary>
    private void HandleMouseRotation()
    {
        if (Mouse.current == null)
            return;

        // UI 조작 중에는 회전시키지 않는다.
        if (EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (!Mouse.current.leftButton.isPressed)
            return;


        Vector2 mouseDelta =
            Mouse.current.delta.ReadValue();


        float rotationAmount =
            -mouseDelta.y *
            mouseRotationSpeed;


        /*
         * Space.Self가 핵심.
         *
         * Vector3.right = X축이지만
         * Air Tube Mesh 자신의 로컬 X축을 사용한다.
         */
        airTubeMesh.Rotate(
            Vector3.up,
            rotationAmount,
            Space.Self
        );
    }


    /// <summary>
    /// Q/E로
    /// Air Tube Mesh 자신의 로컬 Z축을 회전시킨다.
    /// </summary>
    private void HandleZRotation()
    {
        if (Keyboard.current == null)
            return;


        float rotationAmount = 0f;


        if (Keyboard.current.qKey.isPressed)
        {
            rotationAmount +=
                zRotationSpeed *
                Time.deltaTime;
        }

        if (Keyboard.current.eKey.isPressed)
        {
            rotationAmount -=
                zRotationSpeed *
                Time.deltaTime;
        }


        if (Mathf.Approximately(
                rotationAmount,
                0f))
        {
            return;
        }


        /*
         * Mesh 자신의 로컬 Z축 기준 회전.
         */
        airTubeMesh.Rotate(
            Vector3.forward,
            rotationAmount,
            Space.Self
        );
    }
}