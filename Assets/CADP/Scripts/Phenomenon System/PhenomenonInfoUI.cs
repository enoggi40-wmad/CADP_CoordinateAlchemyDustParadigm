using TMPro;
using UnityEngine;

/// <summary>
/// 현재 선택된 현상 정보와
/// Smart Dust 에너지 정보를 UI에 표시한다.
///
/// 표시 항목:
/// - 현재 선택된 현상 이름
/// - 현재 보유 에너지
/// - 현재 환경을 반영한 예상 에너지 소비량
/// </summary>
public class PhenomenonInfoUI : MonoBehaviour
{
    [Header("References")]

    [Tooltip("현재 선택된 현상을 관리하는 컨트롤러")]
    [SerializeField]
    private SmartDustDeploymentController deploymentController;

    [Tooltip("현상별 에너지 비용을 계산하는 시스템")]
    [SerializeField]
    private PhenomenonEnergyCostCalculator energyCostCalculator;

    [Tooltip("현재 Smart Dust 에너지를 관리하는 시스템")]
    [SerializeField]
    private SmartDustEnergySystem smartDustEnergySystem;


    [Header("UI")]

    [Tooltip("현재 선택된 현상 이름")]
    [SerializeField]
    private TMP_Text phenomenonLabelText;

    [Tooltip("현재 보유 에너지")]
    [SerializeField]
    private TMP_Text energyRemainingText;

    [Tooltip("현재 선택된 현상의 예상 소비 에너지")]
    [SerializeField]
    private TMP_Text energyCostText;


    [Header("Refresh")]

    [Tooltip("환경 변화에 따른 비용 재계산 주기")]
    [Min(0.05f)]
    [SerializeField]
    private float refreshInterval = 0.2f;


    private float refreshTimer;


    private void Awake()
    {
        FindMissingReferences();
    }


    private void OnEnable()
    {
        SubscribeEvents();

        refreshTimer = 0f;

        RefreshAll();
    }


    private void OnDisable()
    {
        UnsubscribeEvents();
    }


    private void Update()
    {
        /*
         * 환경값은 시간에 따라 계속 변하므로
         * 일정 간격마다 예상 비용을 다시 계산한다.
         */
        refreshTimer -= Time.deltaTime;

        if (refreshTimer > 0f)
            return;

        refreshTimer = refreshInterval;

        RefreshEnergyCost();
    }


    /// <summary>
    /// 비어 있는 시스템 참조를 자동으로 찾는다.
    /// </summary>
    private void FindMissingReferences()
    {
        if (deploymentController == null)
        {
            deploymentController =
                FindFirstObjectByType<SmartDustDeploymentController>();
        }

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
    /// 필요한 이벤트를 구독한다.
    /// </summary>
    private void SubscribeEvents()
    {
        if (deploymentController != null)
        {
            deploymentController.OnSelectedPhenomenonChanged -=
                HandlePhenomenonChanged;

            deploymentController.OnSelectedPhenomenonChanged +=
                HandlePhenomenonChanged;
        }

        if (smartDustEnergySystem != null)
        {
            smartDustEnergySystem.OnEnergyChanged -=
                HandleEnergyChanged;

            smartDustEnergySystem.OnEnergyChanged +=
                HandleEnergyChanged;
        }
    }


    /// <summary>
    /// 이벤트 구독 해제.
    /// </summary>
    private void UnsubscribeEvents()
    {
        if (deploymentController != null)
        {
            deploymentController.OnSelectedPhenomenonChanged -=
                HandlePhenomenonChanged;
        }

        if (smartDustEnergySystem != null)
        {
            smartDustEnergySystem.OnEnergyChanged -=
                HandleEnergyChanged;
        }
    }


    /// <summary>
    /// Dropdown에서 현상이 변경되었을 때 호출된다.
    /// </summary>
    private void HandlePhenomenonChanged(
        PhenomenonType phenomenonType)
    {
        RefreshPhenomenonName();
        RefreshEnergyCost();
    }


    /// <summary>
    /// 현재 에너지가 변경되었을 때 호출된다.
    /// </summary>
    private void HandleEnergyChanged(
        float currentEnergy,
        float maxEnergy)
    {
        UpdateEnergyRemainingText(
            currentEnergy,
            maxEnergy
        );
    }


    /// <summary>
    /// UI 전체를 현재 값으로 갱신한다.
    /// </summary>
    private void RefreshAll()
    {
        RefreshPhenomenonName();
        RefreshEnergyRemaining();
        RefreshEnergyCost();
    }


    /// <summary>
    /// 현재 선택된 현상 이름을 갱신한다.
    /// </summary>
    private void RefreshPhenomenonName()
    {
        if (phenomenonLabelText == null ||
            deploymentController == null)
        {
            return;
        }

        PhenomenonType selectedPhenomenon =
            deploymentController.SelectedPhenomenon;

        phenomenonLabelText.text =
            GetPhenomenonDisplayName(
                selectedPhenomenon
            );
    }


    /// <summary>
    /// 현재 보유 에너지 표시.
    /// </summary>
    private void RefreshEnergyRemaining()
    {
        if (smartDustEnergySystem == null)
            return;

        UpdateEnergyRemainingText(
            smartDustEnergySystem.CurrentEnergy,
            smartDustEnergySystem.MaxEnergy
        );
    }


    private void UpdateEnergyRemainingText(
        float currentEnergy,
        float maxEnergy)
    {
        if (energyRemainingText == null)
            return;

        energyRemainingText.text =
            $"{Mathf.CeilToInt(currentEnergy)} / " +
            $"{Mathf.CeilToInt(maxEnergy)}";
    }


    /// <summary>
    /// 현재 환경을 반영하여
    /// 선택된 현상의 예상 소비량을 표시한다.
    /// </summary>
    private void RefreshEnergyCost()
    {
        if (energyCostText == null ||
            deploymentController == null ||
            energyCostCalculator == null)
        {
            return;
        }

        PhenomenonType selectedPhenomenon =
            deploymentController.SelectedPhenomenon;

        if (selectedPhenomenon ==
            PhenomenonType.None)
        {
            energyCostText.text =
                "--";

            return;
        }

        PhenomenonEnergyCostResult result =
            energyCostCalculator.Calculate(
                selectedPhenomenon
            );

        /*
         * 아직 에너지 비용이 구현되지 않은 현상은
         * Calculator에서 0을 반환하므로 -- 표시.
         */
        if (result.FinalCost <= 0f)
        {
            energyCostText.text =
                "--";

            return;
        }

        energyCostText.text =
            $"{Mathf.CeilToInt(result.FinalCost)}";
    }


    /// <summary>
    /// Enum 이름을 플레이어용 표시 문자열로 변환한다.
    /// </summary>
    private string GetPhenomenonDisplayName(
        PhenomenonType phenomenonType)
    {
        switch (phenomenonType)
        {
            case PhenomenonType.None:
                return "None";

            case PhenomenonType.MoistureCondensation:
                return "Moisture Condensation";

            case PhenomenonType.AirPressureGradient:
                return "Air Pressure Gradient";

            case PhenomenonType.ThermalIgnition:
                return "Thermal Ignition";

            case PhenomenonType.ElectricDischarge:
                return "Electric Discharge";

            case PhenomenonType.HeatExtraction:
                return "Heat Extraction";

            default:
                return "Unknown";
        }
    }
}