using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Smart Dust 모드의 입력, UI, 좌표 원점 전환,
/// 로봇 목표 위치를 관리한다.
///
/// 모드 OFF:
/// - 로봇은 플레이어 기준 Standby Offset 위치를 추적한다.
/// - Smart Dust 구체를 숨긴다.
/// - Smart Dust 관련 UI를 숨긴다.
///
/// 모드 ON:
/// - 로봇은 DeploymentController의 WorldCenter를 추적한다.
/// - Smart Dust 구체를 표시한다.
///
/// UI 흐름:
/// - 현상 미선택(None): SmartDustPanel 표시
/// - 현상 선택: SmartDustPanel 숨김 / PhenomenonInfoUI 표시
/// </summary>
public class SmartDustModeController : MonoBehaviour
{
    [Header("Controllers")]

    [Tooltip("좌표 원점, 오프셋, 반경, 선택 현상을 관리하는 컨트롤러")]
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

    [Tooltip("좌표, 반경, 현상 선택 등을 표시하는 Smart Dust 설정 패널")]
    [SerializeField]
    private GameObject smartDustPanel;

    [Tooltip("현상 선택 후 현상명, 현재 에너지, 예상 소비량을 표시하는 UI")]
    [SerializeField]
    private GameObject phenomenonInfoUI;

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

    [SerializeField]
    private PhenomenonSettingsUIController
    phenomenonSettingsUIController;

    public event Action<bool>
        OnSmartDustModeChanged;

    public bool IsSmartDustModeActive =>
        isSmartDustModeActive;


    private void Start()
    {
        SetupDropdown();
        RefreshOriginText();

        isSmartDustModeActive = false;

        ApplyModeState();

        // 게임 시작 즉시 로봇 목표 위치 설정
        UpdateRobotTarget();
    }


    private void Update()
    {
        HandleInput();

        /*
         * 모드 상태와 관계없이 로봇의 목표 위치는
         * 계속 갱신한다.
         */
        UpdateCameraControl();
        UpdateRobotTarget();
    }


    /// <summary>
    /// Smart Dust 관련 입력 처리.
    /// </summary>
    private void HandleInput()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current[toggleModeKey].wasPressedThisFrame)
        {
            HandleSmartDustToggle();
        }

        // Origin 변경은 Smart Dust 모드에서만 가능
        if (!isSmartDustModeActive)
            return;

        if (Keyboard.current[toggleOriginKey].wasPressedThisFrame)
        {
            ToggleOriginMode();
        }
    }

    /// <summary>
    /// G키 입력 처리
    ///
    /// OFF
    /// → Smart Dust ON
    ///
    /// SmartDustPanel
    /// → Smart Dust OFF
    ///
    /// PhenomenonInfoUI
    /// → SmartDustPanel 복귀
    /// </summary>
    private void HandleSmartDustToggle()
    {
        // Smart Dust 모드 OFF → ON
        if (!isSmartDustModeActive)
        {
            SetSmartDustMode(true);
            return;
        }

        /*
         * 현재 PhenomenonInfoUI가 실제로 열려 있으면
         * SmartDustPanel로 돌아간다.
         */
        bool isPhenomenonInfoOpen =
            phenomenonInfoUI != null &&
            phenomenonInfoUI.activeSelf;

        if (isPhenomenonInfoOpen)
        {
            ReturnToPhenomenonSelection();
            return;
        }

        /*
         * 이미 SmartDustPanel 화면이라면
         * G를 다시 눌렀을 때 Smart Dust 모드 종료.
         */
        SetSmartDustMode(false);
    }

    /// <summary>
    /// 현상 실행이 끝난 뒤
    /// Smart Dust UI와 현상 선택 상태를 초기화한다.
    ///
    /// 다음에 G를 눌렀을 때
    /// SmartDustPanel부터 시작하도록 한다.
    /// </summary>
    public void ResetAfterPhenomenonExecution()
    {
        if (deploymentController != null)
        {
            deploymentController.SetSelectedPhenomenon(
                PhenomenonType.None
            );
        }

        if (phenomenonDropdown != null)
        {
            phenomenonDropdown.SetValueWithoutNotify(
                (int)PhenomenonType.None
            );

            phenomenonDropdown.RefreshShownValue();
        }

        // Smart Dust 모드 자체를 종료
        isSmartDustModeActive = false;

        ApplyModeState();

        UpdateRobotTarget();

        Debug.Log(
            "[Smart Dust] 현상 실행 완료 → 상태 초기화",
            this
        );
    }


    /// <summary>
    /// Smart Dust 모드 활성 상태 변경.
    /// </summary>
    private void SetSmartDustMode(
        bool active)
    {
        isSmartDustModeActive =
            active;

        /*
          Smart Dust 모드 상태가 변경되었음을
          외부 시스템에게 알린다.
        */
        OnSmartDustModeChanged?.Invoke(
            isSmartDustModeActive
        );

        ApplyModeState();

        UpdateRobotTarget();

        Debug.Log(
            active
                ? "[Smart Dust] Mode ON"
                : "[Smart Dust] Mode OFF",
            this
        );
    }


    /// <summary>
    /// 현재 Smart Dust 모드 상태를
    /// 카메라, UI, 커서, 구체에 적용한다.
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
    /// 현재 상태에 맞는 로봇 목표 위치를 갱신한다.
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
        smartDustRobot.SetTargetPosition(
            deploymentController.WorldCenter
        );
    }


    /// <summary>
    /// Player / Locked Object Origin 전환.
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

        UpdateRobotTarget();
    }


    /// <summary>
    /// 카메라 회전 상태 설정.
    /// </summary>
    private void SetCameraState(
        bool active)
    {
        if (cameraController == null)
            return;

        cameraController.SetRotationEnabled(
            !active
        );
    }


    /// <summary>
    /// Smart Dust 모드에서는
    /// RMB를 누르는 동안만 카메라 회전 가능.
    /// </summary>
    private void UpdateCameraControl()
    {
        if (cameraController == null)
            return;

        if (!isSmartDustModeActive)
        {
            cameraController.SetRotationEnabled(
                true
            );

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
            Cursor.lockState =
                CursorLockMode.Locked;

            Cursor.visible =
                false;
        }
        else
        {
            Cursor.lockState =
                CursorLockMode.None;

            Cursor.visible =
                true;
        }
    }


    /// <summary>
    /// 현재 Smart Dust 모드와
    /// 현상 선택 상태를 기준으로 UI를 설정한다.
    /// </summary>
    private void SetUIState(bool active)
    {
        if (!active)
        {
            if (smartDustPanel != null)
            {
                smartDustPanel.SetActive(false);
            }

            if (phenomenonInfoUI != null)
            {
                phenomenonInfoUI.SetActive(false);
            }

            return;
        }

        // Smart Dust 모드를 새로 켰을 때는
        // 항상 설정 화면부터 시작한다.
        if (smartDustPanel != null)
        {
            smartDustPanel.SetActive(true);
        }

        if (phenomenonInfoUI != null)
        {
            phenomenonInfoUI.SetActive(false);
        }

        if (phenomenonDropdown != null)
        {
            phenomenonDropdown.interactable = true;
        }
    }


    /// <summary>
    /// 마우스 커서 상태 설정.
    /// </summary>
    private void SetCursorState(
        bool active)
    {
        if (active)
        {
            Cursor.lockState =
                CursorLockMode.None;

            Cursor.visible =
                true;
        }
        else
        {
            Cursor.lockState =
                CursorLockMode.Locked;

            Cursor.visible =
                false;
        }
    }


    /// <summary>
    /// Smart Dust Sphere 표시 상태 설정.
    /// </summary>
    private void SetSmartDustSphereState(
        bool active)
    {
        if (smartDustController == null)
            return;

        smartDustController.SetVisible(
            active
        );
    }


    /// <summary>
    /// Phenomenon Dropdown 초기화.
    /// </summary>
    private void SetupDropdown()
    {
        if (phenomenonDropdown == null)
            return;

        phenomenonDropdown
            .onValueChanged
            .RemoveListener(
                OnPhenomenonChanged
            );


        phenomenonDropdown.ClearOptions();


        phenomenonDropdown.AddOptions(
            new List<string>
            {
                "None",
                "Moisture Condensation",
                "Air Pressure Gradient",
                "Thermal Ignition",
                "Electric Discharge",
                "Heat Extraction",
                "Kinetic Manipulation"
            }
        );


        phenomenonDropdown.value =
            0;

        phenomenonDropdown
            .RefreshShownValue();


        phenomenonDropdown
            .onValueChanged
            .AddListener(
                OnPhenomenonChanged
            );


        OnPhenomenonChanged(
            phenomenonDropdown.value
        );
    }


    /// <summary>
    /// Dropdown에서 선택된 현상을
    /// DeploymentController에 전달한다.
    ///
    /// 현상이 선택되면:
    /// SmartDustPanel OFF
    /// PhenomenonInfoUI ON
    /// </summary>
    private void OnPhenomenonChanged(
        int index)
    {
        if (deploymentController == null)
            return;


        if (!System.Enum.IsDefined(
                typeof(PhenomenonType),
                index))
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

        if (phenomenonSettingsUIController != null)
        {
            phenomenonSettingsUIController.Refresh();
        }


        /*
         * UI 상태도 선택된 현상에 맞게
         * 즉시 갱신한다.
         */
        RefreshPhenomenonUIState();


        Debug.Log(
            $"[Smart Dust] Selected Phenomenon: " +
            $"{selectedPhenomenon}",
            this
        );
    }


    /// <summary>
    /// 현재 선택된 현상에 따라
    /// SmartDustPanel / PhenomenonInfoUI를 전환한다.
    /// </summary>
    private void RefreshPhenomenonUIState()
    {
        if (!isSmartDustModeActive)
        {
            if (smartDustPanel != null)
            {
                smartDustPanel.SetActive(
                    false
                );
            }

            if (phenomenonInfoUI != null)
            {
                phenomenonInfoUI.SetActive(
                    false
                );
            }

            return;
        }


        bool hasSelectedPhenomenon =
            deploymentController != null &&
            deploymentController.SelectedPhenomenon !=
            PhenomenonType.None;


        if (smartDustPanel != null)
        {
            smartDustPanel.SetActive(
                !hasSelectedPhenomenon
            );
        }


        if (phenomenonInfoUI != null)
        {
            phenomenonInfoUI.SetActive(
                hasSelectedPhenomenon
            );
        }
    }


    public void ReturnToPhenomenonSelection()
    {
        if (!isSmartDustModeActive)
            return;

        /*
         * SmartDustPanel로 돌아갈 때
         * 현상 선택을 초기화한다.
         */
        if (deploymentController != null)
        {
            deploymentController.SetSelectedPhenomenon(
                PhenomenonType.None
            );
        }

        /*
         * Dropdown 표시도 None으로 되돌린다.
         * SetValueWithoutNotify를 사용해서
         * OnPhenomenonChanged가 중복 호출되지 않게 한다.
         */
        if (phenomenonDropdown != null)
        {
            phenomenonDropdown.SetValueWithoutNotify(
                (int)PhenomenonType.None
            );

            phenomenonDropdown.RefreshShownValue();
        }

        if (smartDustPanel != null)
        {
            smartDustPanel.SetActive(true);
        }

        if (phenomenonInfoUI != null)
        {
            phenomenonInfoUI.SetActive(false);
        }

        Debug.Log(
            "[Smart Dust] 현상 선택 화면 복귀 → 선택 초기화",
            this
        );
    }

    /// <summary>
    /// 현재 Origin 표시 갱신.
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


        phenomenonDropdown
            .onValueChanged
            .RemoveListener(
                OnPhenomenonChanged
            );
    }
}