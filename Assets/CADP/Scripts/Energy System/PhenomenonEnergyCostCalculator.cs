using UnityEngine;

/// <summary>
/// 현재 환경 조건을 바탕으로
/// 현상별 최종 에너지 소비량을 계산한다.
///
/// 실제 에너지를 차감하지는 않는다.
/// 비용 계산만 담당한다.
/// </summary>
public class PhenomenonEnergyCostCalculator : MonoBehaviour
{
    [Header("References")]

    [Tooltip("현재 월드 환경 정보를 관리하는 시스템")]
    [SerializeField]
    private EnvironmentConditionSystem environmentConditionSystem;


    [Header("Thermal Ignition Cost")]

    [Tooltip("Thermal Ignition의 기본 에너지 비용")]
    [Min(0f)]
    [SerializeField]
    private float thermalIgnitionBaseCost = 50f;

    [Tooltip("이 습도 이하에서는 추가 비용이 발생하지 않는다")]
    [Range(0f, 1f)]
    [SerializeField]
    private float thermalNeutralHumidity = 0.4f;

    [Tooltip("습도가 최대일 때 적용할 최대 비용 배율")]
    [Min(1f)]
    [SerializeField]
    private float thermalMaxHumidityMultiplier = 1.5f;

    [Tooltip("이 온도 이상에서는 추가 비용이 발생하지 않는다")]
    [SerializeField]
    private float thermalNeutralTemperature = 20f;

    [Tooltip("이 온도 이하에서 최대 저온 비용 배율이 적용된다")]
    [SerializeField]
    private float thermalColdTemperature = -10f;

    [Tooltip("매우 낮은 온도에서 적용할 최대 비용 배율")]
    [Min(1f)]
    [SerializeField]
    private float thermalColdMultiplier = 1.25f;


    private void Awake()
    {
        if (environmentConditionSystem == null)
        {
            environmentConditionSystem =
                FindFirstObjectByType<EnvironmentConditionSystem>();
        }
    }


    /// <summary>
    /// 특정 현상의 최종 에너지 비용을 계산한다.
    /// </summary>
    public PhenomenonEnergyCostResult Calculate(
        PhenomenonType phenomenonType)
    {
        float baseCost =
            GetBaseCost(
                phenomenonType
            );

        float environmentMultiplier =
            GetEnvironmentMultiplier(
                phenomenonType
            );

        return new PhenomenonEnergyCostResult(
            baseCost,
            environmentMultiplier
        );
    }


    /// <summary>
    /// 현상별 기본 비용을 반환한다.
    /// </summary>
    private float GetBaseCost(
        PhenomenonType phenomenonType)
    {
        switch (phenomenonType)
        {
            case PhenomenonType.ThermalIgnition:
                return thermalIgnitionBaseCost;

            case PhenomenonType.AirPressureGradient:
            case PhenomenonType.MoistureCondensation:
            case PhenomenonType.ElectricDischarge:
            case PhenomenonType.HeatExtraction:
                return 0f;

            default:
                return 0f;
        }
    }


    /// <summary>
    /// 현재 환경에 따른 현상별 비용 배율을 계산한다.
    /// </summary>
    private float GetEnvironmentMultiplier(
        PhenomenonType phenomenonType)
    {
        if (environmentConditionSystem == null)
        {
            Debug.LogWarning(
                "[Energy Cost] EnvironmentConditionSystem이 없어 " +
                "환경 배율을 1로 처리합니다.",
                this
            );

            return 1f;
        }

        switch (phenomenonType)
        {
            case PhenomenonType.ThermalIgnition:
                return CalculateThermalIgnitionMultiplier();

            default:
                return 1f;
        }
    }


    /// <summary>
    /// 주변 습도와 온도를 기반으로
    /// Thermal Ignition의 비용 배율을 계산한다.
    ///
    /// Wet 상태는 이 계산에 포함하지 않는다.
    /// Wet 여부는 WorldObject가 점화 가능 여부로 판단한다.
    /// </summary>
    private float CalculateThermalIgnitionMultiplier()
    {
        float harmfulHumidity =
            Mathf.InverseLerp(
                thermalNeutralHumidity,
                1f,
                environmentConditionSystem.Humidity
            );

        float humidityMultiplier =
            Mathf.Lerp(
                1f,
                thermalMaxHumidityMultiplier,
                harmfulHumidity
            );


        float coldRatio =
            Mathf.InverseLerp(
                thermalNeutralTemperature,
                thermalColdTemperature,
                environmentConditionSystem.AmbientTemperature
            );

        float temperatureMultiplier =
            Mathf.Lerp(
                1f,
                thermalColdMultiplier,
                coldRatio
            );


        float finalMultiplier =
            humidityMultiplier *
            temperatureMultiplier;


        Debug.Log(
            $"[Thermal Energy Cost] " +
            $"Humidity Multiplier: x{humidityMultiplier:F2} | " +
            $"Temperature Multiplier: x{temperatureMultiplier:F2} | " +
            $"Final: x{finalMultiplier:F2}",
            this
        );


        return Mathf.Max(
            0f,
            finalMultiplier
        );
    }
}