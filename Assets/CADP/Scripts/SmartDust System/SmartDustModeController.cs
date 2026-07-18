using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class SmartDustModeController : MonoBehaviour
{
    [Header("Controllers")]
    [SerializeField]
    private SmartDustDeploymentController deploymentController;

    [SerializeField]
    private SmartDustPreviewController previewController;

    [Header("UI")]
    [SerializeField]
    private GameObject smartDustPanel;

    [SerializeField]
    private TMP_Dropdown phenomenonDropdown;

    [SerializeField]
    private TMP_Text originText;

    [Header("Input")]
    [SerializeField]
    private Key toggleModeKey = Key.G;

    [SerializeField]
    private Key toggleOriginKey = Key.Tab;

    private bool isSmartDustModeActive;

    public bool IsSmartDustModeActive => isSmartDustModeActive;

    private void Start()
    {
        SetupDropdown();

        RefreshOriginText();

        SetSmartDustMode(false);
    }

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        // G키로 Smart Dust Mode 켜기/끄기
        if (Keyboard.current[toggleModeKey].wasPressedThisFrame)
        {
            ToggleSmartDustMode();
        }

        // Smart Dust Mode가 아닐 때는 아래 입력 무시
        if (!isSmartDustModeActive)
            return;

        // Tab키로 Player / LockedObject 원점 변경
        if (Keyboard.current[toggleOriginKey].wasPressedThisFrame)
        {
            ToggleOriginMode();
        }
    }

    private void ToggleSmartDustMode()
    {
        SetSmartDustMode(!isSmartDustModeActive);
    }

    private void SetSmartDustMode(bool active)
    {
        isSmartDustModeActive = active;

        // Smart Dust UI
        if (smartDustPanel != null)
        {
            smartDustPanel.SetActive(active);
        }

        // 범위 프리뷰
        if (previewController != null)
        {
            previewController.SetVisible(active);
        }

        // Dropdown 조작 가능 여부
        if (phenomenonDropdown != null)
        {
            phenomenonDropdown.interactable = active;
        }

        SetCursorState(active);

        Debug.Log(
            active
                ? "[Smart Dust] Mode ON"
                : "[Smart Dust] Mode OFF"
        );
    }

    private void SetCursorState(bool smartDustModeActive)
    {
        if (smartDustModeActive)
        {
            // UI 조작 가능
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            // 일반 플레이 시점 조작
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void ToggleOriginMode()
    {
        if (deploymentController == null)
        {
            Debug.LogWarning(
                "[Smart Dust] DeploymentController가 연결되지 않았습니다."
            );

            return;
        }

        bool changed =
            deploymentController.TryToggleOriginMode();

        if (!changed)
        {
            Debug.LogWarning(
                "[Smart Dust] Lock-On 대상이 없어 Player 원점을 유지합니다."
            );
        }

        RefreshOriginText();
    }

    private void SetupDropdown()
    {
        if (phenomenonDropdown == null)
            return;

        phenomenonDropdown.ClearOptions();

        phenomenonDropdown.AddOptions(
            new System.Collections.Generic.List<string>
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

        phenomenonDropdown.onValueChanged.RemoveListener(
            OnPhenomenonChanged
        );

        phenomenonDropdown.onValueChanged.AddListener(
            OnPhenomenonChanged
        );

        OnPhenomenonChanged(phenomenonDropdown.value);
    }

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
                $"[Smart Dust] 잘못된 현상 Dropdown 값: {index}"
            );

            return;
        }

        PhenomenonType selectedPhenomenon =
            (PhenomenonType)index;

        deploymentController.SetSelectedPhenomenon(
            selectedPhenomenon
        );

        Debug.Log(
            $"[Smart Dust] Phenomenon: {selectedPhenomenon}"
        );
    }

    private void OnDestroy()
    {
        if (phenomenonDropdown != null)
        {
            phenomenonDropdown.onValueChanged.RemoveListener(
                OnPhenomenonChanged
            );
        }
    }

    private void RefreshOriginText()
    {
        if (originText == null || deploymentController == null)
            return;

        originText.text =
            deploymentController.OriginMode ==
            CoordinateOriginMode.Player
                ? "Player"
                : "Locked Object";
    }
}