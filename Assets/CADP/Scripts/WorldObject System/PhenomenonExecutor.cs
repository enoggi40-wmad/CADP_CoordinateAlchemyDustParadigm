using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 현재 선택된 현상에 따라
/// 알맞은 현상 실행기로 요청을 전달한다.
/// </summary>
public class PhenomenonExecutor : MonoBehaviour
{
    [Header("References")]

    [Tooltip("Smart Dust 전개 위치, 반경, 선택 현상을 관리하는 컨트롤러")]
    [SerializeField]
    private SmartDustDeploymentController deploymentController;

    [Tooltip("Smart Dust 모드 활성 상태를 확인할 컨트롤러")]
    [SerializeField]
    private SmartDustModeController smartDustModeController;


    [Header("Phenomenon Executors")]

    [Tooltip("Thermal Ignition 현상 실행기")]
    [SerializeField]
    private ThermalIgnitionExecutor thermalIgnitionExecutor;


    [Header("Input")]

    [Tooltip("선택된 현상을 실행할 키")]
    [SerializeField]
    private Key executeKey = Key.Enter;


    private void Update()
    {
        HandleExecuteInput();
    }


    /// <summary>
    /// 현상 실행 입력을 확인한다.
    /// </summary>
    private void HandleExecuteInput()
    {
        if (Keyboard.current == null)
            return;

        if (!Keyboard.current[executeKey].wasPressedThisFrame)
            return;

        // Smart Dust 모드가 꺼져 있으면 실행하지 않는다.
        if (smartDustModeController != null &&
            !smartDustModeController.IsSmartDustModeActive)
        {
            Debug.Log(
                "[Phenomenon] Smart Dust 모드가 아니므로 실행하지 않습니다.",
                this
            );

            return;
        }

        ExecuteSelectedPhenomenon();
    }


    /// <summary>
    /// 현재 선택된 현상을 실행한다.
    /// </summary>
    public void ExecuteSelectedPhenomenon()
    {
        if (deploymentController == null)
        {
            Debug.LogWarning(
                "[Phenomenon] DeploymentController가 연결되지 않았습니다.",
                this
            );

            return;
        }

        Vector3 center =
            deploymentController.WorldCenter;

        float radius =
            deploymentController.Radius;

        PhenomenonType selectedPhenomenon =
            deploymentController.SelectedPhenomenon;

        switch (selectedPhenomenon)
        {
            case PhenomenonType.None:
                Debug.Log(
                    "[Phenomenon] 선택된 현상이 없습니다.",
                    this
                );
                break;

            case PhenomenonType.ThermalIgnition:
                ExecuteThermalIgnition(
                    center,
                    radius
                );
                break;

            case PhenomenonType.MoistureCondensation:
            case PhenomenonType.AirPressureGradient:
            case PhenomenonType.ElectricDischarge:
            case PhenomenonType.HeatExtraction:
                Debug.Log(
                    $"[Phenomenon] {selectedPhenomenon}은 아직 구현되지 않았습니다.",
                    this
                );
                break;

            default:
                Debug.LogWarning(
                    $"[Phenomenon] 알 수 없는 현상입니다: {selectedPhenomenon}",
                    this
                );
                break;
        }
    }


    /// <summary>
    /// Thermal Ignition 현상을 실행한다.
    /// </summary>
    private void ExecuteThermalIgnition(
        Vector3 center,
        float radius)
    {
        if (thermalIgnitionExecutor == null)
        {
            Debug.LogWarning(
                "[Phenomenon] ThermalIgnitionExecutor가 연결되지 않았습니다.",
                this
            );

            return;
        }

        thermalIgnitionExecutor.Execute(
            center,
            radius
        );
    }
}