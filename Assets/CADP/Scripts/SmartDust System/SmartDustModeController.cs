using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Smart Dust 모드의 입력, UI, 좌표 원점 전환,
/// 로봇 목표 위치를 관리한다.
///
/// 모드 OFF:
/// - 로봇은 플레이어 기준 Standby Offset 위치를 계속 추적한다.
/// - Smart Dust 구체는 숨긴다.
///
/// 모드 ON:
/// - 로봇은 DeploymentController의 WorldCenter를 계속 추적한다.
/// - Smart Dust 구체를 표시한다.
/// </summary>
public class SmartDustModeController : MonoBehaviour
{
    [Header("Controllers")]

    [Tooltip("좌표 원점, 오프셋, 반경, WorldCenter를 관리하는 컨트롤러")]
    [SerializeField]
    private SmartDustDeploymentController deploymentController;

    [Tooltip("Smart Dust 구체의 표시와 크기를 관리하는 컨트롤러")]
    [SerializeField]
    private SmartDustController smartDustController;

    [Tooltip("Smart Dust 운반 로봇의 이동을 관리하는 컨트롤러")]
    [SerializeField]
    private SmartDustRobotController smartDustRobot;

    [Tooltip("좌표 입력 UI를 관리하는 컨트롤러")]
    [SerializeField]
    private SmartDustUIController smartDustUIController;


    [Header("Player")]

    [Tooltip("플레이어 루트 Transform")]
    [SerializeField]
    private Transform playerTransform;

    [Tooltip(
        "모드 OFF 상태에서 로봇이 머무를 플레이어 기준 로컬 좌표입니다.\n" +
        "예: X 1.3, Y 2, Z 1.5"
    )]
    [SerializeField]
    private Vector3 standbyOffset;


    [Header("UI")]

    [Tooltip("Smart Dust 모드 UI 전체 패널")]
    [SerializeField]
    private GameObject smartDustPanel;

    [Tooltip("현상 선택 Dropdown")]
    [SerializeField]
    private TMP_Dropdown phenomenonDropdown;

    [Tooltip("현재 좌표 원점을 표시하는 텍스트")]
    [SerializeField]
    private TMP_Text originText;


    [Header("Camera")]

    [Tooltip("플레이어 카메라 회전을 담당하는 컨트롤러")]
    [SerializeField]
    private PlayerCameraController cameraController;


    [Header("Input")]

    [Tooltip("Smart Dust 모드 ON/OFF 키")]
    [SerializeField]
    private Key toggleModeKey = Key.G;

    [Tooltip("Player / Locked Object Origin 전환 키")]
    [SerializeField]
    private Key toggleOriginKey = Key.Tab;


    [Header("Runtime State")]

    [SerializeField]
    private bool isSmartDustModeActive;


    public bool IsSmartDustModeActive =>
        isSmartDustModeActive;


    private void Start()
    {
        SetupDropdown();
        RefreshOriginText();

        isSmartDustModeActive = false;

        ApplyModeState();

        /*
         * 게임 시작 즉시 로봇 목표를 설정한다.
         * 따라서 G를 누르지 않아도 처음부터
         * 플레이어 기준 Standby Offset 위치를 따라간다.
         */
        UpdateRobotTarget();
    }


    private void Update()
    {
        HandleInput();

        /*
         * 모드 상태와 상관없이 매 프레임 목표 위치를 갱신한다.
         *
         * 모드 OFF:
         * 플레이어가 움직이면 Standby 위치도 함께 움직인다.
         *
         * 모드 ON:
         * Player 또는 Locked Object가 움직이면
         * WorldCenter도 함께 갱신된다.
         */
        UpdateCameraControl();
        UpdateRobotTarget();
    }


    /// <summary>
    /// Smart Dust 관련 입력을 처리한다.
    /// </summary>
    private void HandleInput()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current[toggleModeKey].wasPressedThisFrame)
        {
            SetSmartDustMode(
                !isSmartDustModeActive
            );
        }

        // Origin 전환은 Smart Dust 모드 중에만 허용한다.
        if (!isSmartDustModeActive)
            return;

        if (Keyboard.current[toggleOriginKey].wasPressedThisFrame)
        {
            ToggleOriginMode();
        }
    }


    /// <summary>
    /// Smart Dust 모드를 설정한다.
    /// </summary>
    private void SetSmartDustMode(bool active)
    {
        isSmartDustModeActive = active;

        ApplyModeState();

        // 모드 변경 직후 목표 위치를 즉시 갱신한다.
        UpdateRobotTarget();

        Debug.Log(
            active
                ? "[Smart Dust] Mode ON"
                : "[Smart Dust] Mode OFF",
            this
        );
    }


    /// <summary>
    /// 현재 모드 상태를 UI, 카메라, 커서, 구체에 적용한다.
    /// </summary>
    private void ApplyModeState()
    {
        SetCameraState(
            isSmartDustModeActive
        );

        SetUIState(
            isSmartDustModeActive
        );

        SetCursorState(
            isSmartDustModeActive
        );

        SetSmartDustSphereState(
            isSmartDustModeActive
        );
    }


    /// <summary>
    /// 현재 모드와 Origin에 맞는 목표 위치를
    /// 로봇에게 매 프레임 전달한다.
    /// </summary>
    private void UpdateRobotTarget()
    {
        if (smartDustRobot == null ||
            playerTransform == null ||
            deploymentController == null)
        {
            return;
        }

        // Smart Dust 모드 OFF
        // 로봇은 플레이어의 대기 위치로 복귀한다.
        if (!isSmartDustModeActive)
        {
            Vector3 standbyWorldPosition =
                playerTransform.TransformPoint(
                    standbyOffset
                );

            smartDustRobot.SetTarget(
                standbyWorldPosition,
                playerTransform.forward
            );

            return;
        }

        // Smart Dust 모드 ON
        // Player Origin과 Locked Object Origin 모두
        // DeploymentController의 WorldCenter를 사용한다.
        smartDustRobot.SetTargetPosition(
            deploymentController.WorldCenter
        );
    }

    /// <summary>
    /// Player Origin과 Locked Object Origin을 전환한다.
    /// </summary>
    private void ToggleOriginMode()
    {
        if (deploymentController == null)
            return;

        bool changed =
            deploymentController.TryToggleOriginMode();

        if (!changed)
            return;

        if (smartDustUIController != null)
        {
            smartDustUIController
                .RefreshCoordinateInputs();
        }

        RefreshOriginText();

        // Origin 변경 직후 새 WorldCenter 적용
        UpdateRobotTarget();
    }


    /// <summary>
    /// Smart Dust 모드에 따라 카메라 회전을 설정한다.
    /// </summary>
    private void SetCameraState(bool active)
    {
        if (cameraController == null)
            return;

        // Smart Dust 모드 진입 시 우선 회전 차단
        // 이후 UpdateCameraControl()이 우클릭 상태를 계속 확인한다.
        cameraController.SetRotationEnabled(
            !active
        );
    }

    /// <summary>
    /// 일반 모드에서는 항상 카메라 회전 가능.
    ///
    /// Smart Dust 모드에서는
    /// 마우스 오른쪽 버튼을 누르는 동안에만
    /// 카메라 회전을 허용한다.
    /// </summary>
    private void UpdateCameraControl()
    {
        if (cameraController == null)
            return;

        // 일반 모드
        if (!isSmartDustModeActive)
        {
            cameraController.SetRotationEnabled(true);
            return;
        }

        bool isRightMousePressed =
            Mouse.current != null &&
            Mouse.current.rightButton.isPressed;

        cameraController.SetRotationEnabled(
            isRightMousePressed
        );

        if (isRightMousePressed)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }


    /// <summary>
    /// Smart Dust UI를 표시하거나 숨긴다.
    /// </summary>
    private void SetUIState(bool active)
    {
        if (smartDustPanel != null)
        {
            smartDustPanel.SetActive(active);
        }

        if (phenomenonDropdown != null)
        {
            phenomenonDropdown.interactable =
                active;
        }
    }


    /// <summary>
    /// Smart Dust 모드에 따라 마우스 커서를 설정한다.
    /// </summary>
    private void SetCursorState(bool active)
    {
        if (active)
        {
            Cursor.lockState =
                CursorLockMode.None;

            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState =
                CursorLockMode.Locked;

            Cursor.visible = false;
        }
    }


    /// <summary>
    /// 로봇은 유지하고 Smart Dust 구체만 표시하거나 숨긴다.
    /// </summary>
    private void SetSmartDustSphereState(bool active)
    {
        if (smartDustController == null)
            return;

        smartDustController.SetVisible(
            active
        );
    }


    /// <summary>
    /// 현상 선택 Dropdown을 초기화한다.
    /// </summary>
    private void SetupDropdown()
    {
        if (phenomenonDropdown == null)
            return;

        phenomenonDropdown.onValueChanged
            .RemoveListener(OnPhenomenonChanged);

        phenomenonDropdown.ClearOptions();

        phenomenonDropdown.AddOptions(
            new List<string>
            {
                "None",
                "Moisture Condensation",
                "Air Pressure Gradient",
                "Thermal Ignition",
                "Electric Discharge",
                "Heat Extraction"
            }
        );

        phenomenonDropdown.value = 0;
        phenomenonDropdown.RefreshShownValue();

        phenomenonDropdown.onValueChanged
            .AddListener(OnPhenomenonChanged);

        OnPhenomenonChanged(
            phenomenonDropdown.value
        );
    }


    /// <summary>
    /// 선택된 현상을 DeploymentController에 전달한다.
    /// </summary>
    private void OnPhenomenonChanged(int index)
    {
        if (deploymentController == null)
            return;

        if (!System.Enum.IsDefined(
                typeof(PhenomenonType),
                index
            ))
        {
            Debug.LogWarning(
                $"[Smart Dust] 잘못된 현상 값: {index}",
                this
            );

            return;
        }

        PhenomenonType selectedPhenomenon =
            (PhenomenonType)index;

        deploymentController
            .SetSelectedPhenomenon(
                selectedPhenomenon
            );
    }


    /// <summary>
    /// 현재 Origin을 UI 텍스트에 표시한다.
    /// </summary>
    private void RefreshOriginText()
    {
        if (originText == null)
            return;

        if (deploymentController == null)
            return;

        originText.text =
            deploymentController.OriginMode ==
            CoordinateOriginMode.Player
                ? "Player"
                : "Locked Object";
    }


    private void OnDestroy()
    {
        if (phenomenonDropdown == null)
            return;

        phenomenonDropdown.onValueChanged
            .RemoveListener(OnPhenomenonChanged);
    }
}