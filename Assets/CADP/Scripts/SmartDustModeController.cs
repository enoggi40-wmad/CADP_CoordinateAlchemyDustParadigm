using UnityEngine;
using UnityEngine.InputSystem;

public class SmartDustModeController : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private SmartDustDeploymentController deploymentController;

    [SerializeField]
    private SmartDustPreviewController previewController;

    [Header("Mode State")]
    [SerializeField]
    private bool isSmartDustModeActive;

    public bool IsSmartDustModeActive =>
        isSmartDustModeActive;

    private void Start()
    {
        SetSmartDustMode(false);
    }

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        HandleModeInput();

        if (!isSmartDustModeActive)
            return;

        HandleSmartDustInput();
    }

    private void HandleModeInput()
    {
        if (Keyboard.current.gKey.wasPressedThisFrame)
        {
            ToggleSmartDustMode();
        }
    }

    private void HandleSmartDustInput()
    {
        if (Keyboard.current.tabKey.wasPressedThisFrame)
        {
            ToggleOriginMode();
        }
    }

    public void ToggleSmartDustMode()
    {
        SetSmartDustMode(!isSmartDustModeActive);
    }

    public void SetSmartDustMode(bool active)
    {
        isSmartDustModeActive = active;

        if (previewController != null)
        {
            previewController.SetVisible(active);
        }

        if (active)
        {
            Debug.Log(
                "Smart Dust Mode: ACTIVE",
                this
            );
        }
        else
        {
            Debug.Log(
                "Smart Dust Mode: INACTIVE",
                this
            );
        }
    }

    public void ToggleOriginMode()
    {
        if (!isSmartDustModeActive)
            return;

        if (deploymentController == null)
        {
            Debug.LogError(
                "SmartDustDeploymentController가 연결되지 않았습니다.",
                this
            );

            return;
        }

        deploymentController.TryToggleOriginMode();
    }

    public void SelectPhenomenon(
        PhenomenonType phenomenon)
    {
        if (!isSmartDustModeActive)
        {
            Debug.LogWarning(
                "Smart Dust Mode가 비활성화되어 있습니다.",
                this
            );

            return;
        }

        if (deploymentController == null)
            return;

        deploymentController.SetSelectedPhenomenon(
            phenomenon
        );
    }
}