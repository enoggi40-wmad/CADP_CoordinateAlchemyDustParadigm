using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 실제 Smart Dust 영역 안의 WorldObject를 수집하고,
/// 현재 선택된 현상의 Executor에게 전달한다.
///
/// 현상 실행 전:
/// 1. 현상별 에너지 비용 계산
/// 2. 플레이어의 보유 에너지 확인
/// 3. 에너지 차감
/// 4. 현상 실행
///
/// 범위 안에 반응 대상이 없더라도
/// 현상 자체가 발생하면 에너지는 소비된다.
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


    [Header("Smart Dust Runtime")]

    [Tooltip("실제로 이동 중인 Smart Dust 구체의 Transform")]
    [SerializeField]
    private Transform smartDustSphereTransform;


    [Header("Energy System")]

    [Tooltip("환경을 반영하여 현상별 에너지 비용을 계산하는 시스템")]
    [SerializeField]
    private PhenomenonEnergyCostCalculator energyCostCalculator;

    [Tooltip("플레이어의 Smart Dust 에너지 보유량을 관리하는 시스템")]
    [SerializeField]
    private SmartDustEnergySystem smartDustEnergySystem;


    [Header("Phenomenon Executors")]

    [Tooltip("Thermal Ignition 현상 실행기")]
    [SerializeField]
    private ThermalIgnitionExecutor thermalIgnitionExecutor;


    [Header("Input")]

    [Tooltip("선택된 현상을 실행할 키")]
    [SerializeField]
    private Key executeKey = Key.Enter;


    private void Awake()
    {
        FindMissingReferences();
    }


    private void Update()
    {
        HandleExecuteInput();
    }


    /// <summary>
    /// 비어 있는 공통 시스템 참조를 자동으로 찾는다.
    /// </summary>
    private void FindMissingReferences()
    {
        if (energyCostCalculator == null)
        {
            energyCostCalculator =
                FindFirstObjectByType<PhenomenonEnergyCostCalculator>();
        }

        if (smartDustEnergySystem == null)
        {
            smartDustEnergySystem =
                FindFirstObjectByType<SmartDustEnergySystem>();
        }
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
        if (!ValidateCommonReferences())
            return;

        PhenomenonType selectedPhenomenon =
            deploymentController.SelectedPhenomenon;

        /*
         * 현상이 선택되지 않은 경우에는
         * 비용 계산과 에너지 차감을 하지 않는다.
         */
        if (selectedPhenomenon == PhenomenonType.None)
        {
            Debug.Log(
                "[Phenomenon] 선택된 현상이 없습니다.",
                this
            );

            return;
        }

        /*
         * 아직 구현되지 않은 현상 역시
         * 에너지를 소비하지 않는다.
         */
        if (!IsPhenomenonImplemented(selectedPhenomenon))
        {
            Debug.Log(
                $"[Phenomenon] {selectedPhenomenon}은 아직 구현되지 않았습니다.",
                this
            );

            return;
        }

        /*
         * WorldCenter는 목표 위치이고,
         * 실제 범위 판정은 현재 움직이고 있는
         * Smart Dust Sphere의 월드 위치를 사용한다.
         */
        Vector3 center =
            smartDustSphereTransform.position;

        float radius =
            deploymentController.Radius;

        /*
         * 현상 실행에 필요한 Executor가 정상적으로
         * 연결되어 있는지 먼저 확인한다.
         *
         * Executor가 없으면 에너지를 소비하지 않는다.
         */
        if (!ValidatePhenomenonExecutor(selectedPhenomenon))
            return;

        /*
         * 현재 환경을 반영한 최종 에너지 비용을 계산하고,
         * 에너지가 충분한 경우 실제로 차감한다.
         *
         * 차감에 실패하면 현상은 실행되지 않는다.
         */
        if (!TryConsumePhenomenonEnergy(
                selectedPhenomenon,
                out PhenomenonEnergyCostResult costResult))
        {
            return;
        }

        /*
         * 대상 수와 무관하게 현상은 이미 결제되었다.
         *
         * 따라서 범위 안 WorldObject가 0개여도
         * Field는 생성되고 에너지는 되돌아오지 않는다.
         */
        List<WorldObject> targets =
            CollectWorldObjectsInRange(
                center,
                radius
            );

        Debug.Log(
            $"[Phenomenon] 실행 | " +
            $"Type: {selectedPhenomenon} | " +
            $"Center: {center} | " +
            $"Radius: {radius:F2} | " +
            $"Targets: {targets.Count} | " +
            $"Base Cost: {costResult.BaseCost:F1} | " +
            $"Environment Multiplier: x{costResult.EnvironmentMultiplier:F2} | " +
            $"Final Cost: {costResult.FinalCost:F1}",
            this
        );

        // 현상이 정상적으로 실행된 뒤
        // Smart Dust 선택/UI 상태 초기화
        ExecutePhenomenon(
            selectedPhenomenon,
            center,
            radius,
            targets
        );

        if (smartDustModeController != null)
        {
            smartDustModeController
                .ResetAfterPhenomenonExecution();
        }
    }


    /// <summary>
    /// 현상 실행에 필요한 공통 참조를 검사한다.
    /// </summary>
    private bool ValidateCommonReferences()
    {
        if (deploymentController == null)
        {
            Debug.LogWarning(
                "[Phenomenon] SmartDustDeploymentController.cs가 연결되지 않았습니다.",
                this
            );

            return false;
        }

        if (smartDustSphereTransform == null)
        {
            Debug.LogWarning(
                "[Phenomenon] Smart Dust Sphere Transform이 연결되지 않았습니다.",
                this
            );

            return false;
        }

        if (energyCostCalculator == null)
        {
            Debug.LogWarning(
                "[Phenomenon] PhenomenonEnergyCostCalculator.cs가 연결되지 않았습니다.",
                this
            );

            return false;
        }

        if (smartDustEnergySystem == null)
        {
            Debug.LogWarning(
                "[Phenomenon] SmartDustEnergySystem.cs가 연결되지 않았습니다.",
                this
            );

            return false;
        }

        return true;
    }


    /// <summary>
    /// 현재 구현된 현상인지 확인한다.
    /// </summary>
    private bool IsPhenomenonImplemented(
        PhenomenonType phenomenonType)
    {
        switch (phenomenonType)
        {
            case PhenomenonType.ThermalIgnition:
                return true;

            default:
                return false;
        }
    }


    /// <summary>
    /// 선택된 현상의 Executor가 연결되어 있는지 확인한다.
    ///
    /// Executor가 없으면 에너지를 소비하지 않는다.
    /// </summary>
    private bool ValidatePhenomenonExecutor(
        PhenomenonType phenomenonType)
    {
        switch (phenomenonType)
        {
            case PhenomenonType.ThermalIgnition:

                if (thermalIgnitionExecutor == null)
                {
                    Debug.LogWarning(
                        "[Phenomenon] ThermalIgnitionExecutor.cs가 연결되지 않았습니다.",
                        this
                    );

                    return false;
                }

                return true;

            default:
                return false;
        }
    }


    /// <summary>
    /// 환경을 반영한 현상 비용을 계산하고,
    /// 플레이어의 에너지를 소비한다.
    /// </summary>
    private bool TryConsumePhenomenonEnergy(
        PhenomenonType phenomenonType,
        out PhenomenonEnergyCostResult costResult)
    {
        costResult =
            energyCostCalculator.Calculate(
                phenomenonType
            );

        float finalCost =
            costResult.FinalCost;

        if (finalCost <= 0f)
        {
            Debug.LogWarning(
                $"[Phenomenon] {phenomenonType}의 최종 비용이 " +
                $"{finalCost:F1}입니다. 비용 설정을 확인하세요.",
                this
            );

            return false;
        }

        if (!smartDustEnergySystem.TryConsume(finalCost))
        {
            Debug.Log(
                $"[Phenomenon] 에너지가 부족하여 실행할 수 없습니다. | " +
                $"Type: {phenomenonType} | " +
                $"필요 에너지: {finalCost:F1} | " +
                $"현재 에너지: {smartDustEnergySystem.CurrentEnergy:F1}",
                this
            );

            return false;
        }

        return true;
    }


    /// <summary>
    /// 선택된 현상의 실제 Executor를 호출한다.
    /// </summary>
    private void ExecutePhenomenon(
        PhenomenonType phenomenonType,
        Vector3 center,
        float radius,
        List<WorldObject> targets)
    {
        switch (phenomenonType)
        {
            case PhenomenonType.ThermalIgnition:

                ExecuteThermalIgnition(
                    center,
                    radius,
                    targets
                );

                break;

            default:

                Debug.LogWarning(
                    $"[Phenomenon] 실행할 수 없는 현상입니다: {phenomenonType}",
                    this
                );

                break;
        }
    }


    /// <summary>
    /// 현재 실제 Smart Dust 구체 안의
    /// WorldObject들을 수집한다.
    ///
    /// 하나의 WorldObject가 여러 Collider를 가지고 있어도
    /// 한 번만 반환한다.
    /// </summary>
    private List<WorldObject> CollectWorldObjectsInRange(
        Vector3 center,
        float radius)
    {
        Collider[] hitColliders =
            Physics.OverlapSphere(
                center,
                radius,
                ~0,
                QueryTriggerInteraction.Ignore
            );

        HashSet<WorldObject> uniqueObjects =
            new HashSet<WorldObject>();

        foreach (Collider hitCollider in hitColliders)
        {
            if (hitCollider == null)
                continue;

            WorldObject worldObject =
                hitCollider.GetComponentInParent<WorldObject>();

            if (worldObject == null)
                continue;

            if (!uniqueObjects.Add(worldObject))
                continue;

            Debug.Log(
                $"[Phenomenon] 범위 안 대상: {worldObject.name}",
                worldObject
            );
        }

        return new List<WorldObject>(
            uniqueObjects
        );
    }


    /// <summary>
    /// Thermal Ignition 실행기에게
    /// 현재 실제 Smart Dust 범위 안의 대상만 전달한다.
    /// </summary>
    private void ExecuteThermalIgnition(
        Vector3 center,
        float radius,
        List<WorldObject> targets)
    {
        thermalIgnitionExecutor.Execute(
            center,
            radius,
            targets
        );
    }
}