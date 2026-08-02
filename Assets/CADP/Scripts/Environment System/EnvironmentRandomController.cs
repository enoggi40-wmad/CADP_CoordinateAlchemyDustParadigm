using UnityEngine;

/// <summary>
/// EnvironmentConditionSystem의 환경값을
/// 일정 시간마다 새로운 목표값으로 설정하고,
/// 현재 값이 목표값을 향해 부드럽게 변화하도록 관리한다.
///
/// 환경값이 갑자기 튀지 않고
/// 시간에 따라 서서히 변하도록 만드는 용도다.
/// </summary>
public class EnvironmentRandomController : MonoBehaviour
{
    [Header("References")]

    [Tooltip("현재 환경 상태를 관리하는 시스템")]
    [SerializeField]
    private EnvironmentConditionSystem environmentConditionSystem;


    [Header("Change Timing")]

    [Tooltip("새로운 환경 목표를 선택하는 최소 대기 시간")]
    [Min(1f)]
    [SerializeField]
    private float minChangeInterval = 20f;

    [Tooltip("새로운 환경 목표를 선택하는 최대 대기 시간")]
    [Min(1f)]
    [SerializeField]
    private float maxChangeInterval = 45f;

    [Tooltip("현재 환경값이 목표값을 따라가는 속도")]
    [Min(0.01f)]
    [SerializeField]
    private float transitionSpeed = 0.08f;


    [Header("Humidity Range")]

    [Range(0f, 1f)]
    [SerializeField]
    private float minHumidity = 0.2f;

    [Range(0f, 1f)]
    [SerializeField]
    private float maxHumidity = 0.9f;


    [Header("Temperature Range")]

    [SerializeField]
    private float minTemperature = 5f;

    [SerializeField]
    private float maxTemperature = 30f;


    [Header("Rain Range")]

    [Range(0f, 1f)]
    [SerializeField]
    private float minRainIntensity = 0f;

    [Range(0f, 1f)]
    [SerializeField]
    private float maxRainIntensity = 1f;

    [Tooltip("환경 변경 시 비가 내릴 확률")]
    [Range(0f, 1f)]
    [SerializeField]
    private float rainChance = 0.25f;


    [Header("Wind Range")]

    [Min(0f)]
    [SerializeField]
    private float minWindStrength = 0f;

    [Min(0f)]
    [SerializeField]
    private float maxWindStrength = 5f;


    [Header("Runtime Target")]

    [SerializeField]
    private float targetHumidity;

    [SerializeField]
    private float targetTemperature;

    [SerializeField]
    private float targetRainIntensity;

    [SerializeField]
    private Vector3 targetWindDirection = Vector3.forward;

    [SerializeField]
    private float targetWindStrength;


    private float changeTimer;


    private void Awake()
    {
        if (environmentConditionSystem == null)
        {
            environmentConditionSystem =
                GetComponent<EnvironmentConditionSystem>();
        }

        ValidateValues();
        CreateNewTargetEnvironment();
    }


#if UNITY_EDITOR
    private void OnValidate()
    {
        ValidateValues();
    }
#endif


    private void Update()
    {
        if (environmentConditionSystem == null)
            return;

        UpdateEnvironmentTransition();
        UpdateChangeTimer();
    }


    /// <summary>
    /// 현재 환경값을 목표 환경값 쪽으로
    /// 서서히 이동시킨다.
    /// </summary>
    private void UpdateEnvironmentTransition()
    {
        float humidity =
            Mathf.MoveTowards(
                environmentConditionSystem.Humidity,
                targetHumidity,
                transitionSpeed * Time.deltaTime
            );

        float temperature =
            Mathf.MoveTowards(
                environmentConditionSystem.AmbientTemperature,
                targetTemperature,
                transitionSpeed * 20f * Time.deltaTime
            );

        float rainIntensity =
            Mathf.MoveTowards(
                environmentConditionSystem.RainIntensity,
                targetRainIntensity,
                transitionSpeed * Time.deltaTime
            );

        float windStrength =
            Mathf.MoveTowards(
                environmentConditionSystem.WindStrength,
                targetWindStrength,
                transitionSpeed * 5f * Time.deltaTime
            );

        Vector3 currentWindDirection =
            environmentConditionSystem.WindDirection;

        if (currentWindDirection == Vector3.zero)
        {
            currentWindDirection =
                targetWindDirection;
        }

        Vector3 windDirection =
            Vector3.Slerp(
                currentWindDirection,
                targetWindDirection,
                transitionSpeed * Time.deltaTime
            );

        environmentConditionSystem.SetHumidity(
            humidity
        );

        environmentConditionSystem.SetAmbientTemperature(
            temperature
        );

        environmentConditionSystem.SetRainIntensity(
            rainIntensity
        );

        environmentConditionSystem.SetWind(
            windDirection,
            windStrength
        );
    }


    /// <summary>
    /// 다음 환경 목표를 생성할 시간을 계산한다.
    /// </summary>
    private void UpdateChangeTimer()
    {
        changeTimer -=
            Time.deltaTime;

        if (changeTimer > 0f)
            return;

        CreateNewTargetEnvironment();
    }


    /// <summary>
    /// 새로운 무작위 환경 목표값을 생성한다.
    /// </summary>
    private void CreateNewTargetEnvironment()
    {
        targetHumidity =
            Random.Range(
                minHumidity,
                maxHumidity
            );

        targetTemperature =
            Random.Range(
                minTemperature,
                maxTemperature
            );

        bool shouldRain =
            Random.value <= rainChance;

        targetRainIntensity =
            shouldRain
                ? Random.Range(
                    Mathf.Max(0.1f, minRainIntensity),
                    maxRainIntensity
                )
                : 0f;

        targetWindDirection =
            CreateRandomHorizontalDirection();

        targetWindStrength =
            Random.Range(
                minWindStrength,
                maxWindStrength
            );

        changeTimer =
            Random.Range(
                minChangeInterval,
                maxChangeInterval
            );

        Debug.Log(
            $"[Environment Random] 새로운 환경 목표 | " +
            $"Humidity: {targetHumidity:F2} | " +
            $"Temperature: {targetTemperature:F1} | " +
            $"Rain: {targetRainIntensity:F2} | " +
            $"Wind Strength: {targetWindStrength:F1}",
            this
        );
    }


    /// <summary>
    /// 수평면 기준의 무작위 바람 방향을 생성한다.
    /// </summary>
    private Vector3 CreateRandomHorizontalDirection()
    {
        float angle =
            Random.Range(
                0f,
                360f
            );

        Quaternion rotation =
            Quaternion.Euler(
                0f,
                angle,
                0f
            );

        return rotation *
               Vector3.forward;
    }


    /// <summary>
    /// Inspector 값의 최소·최대 관계를 정리한다.
    /// </summary>
    private void ValidateValues()
    {
        minChangeInterval =
            Mathf.Max(
                1f,
                minChangeInterval
            );

        maxChangeInterval =
            Mathf.Max(
                minChangeInterval,
                maxChangeInterval
            );

        transitionSpeed =
            Mathf.Max(
                0.01f,
                transitionSpeed
            );

        minHumidity =
            Mathf.Clamp01(
                minHumidity
            );

        maxHumidity =
            Mathf.Clamp(
                maxHumidity,
                minHumidity,
                1f
            );

        if (maxTemperature < minTemperature)
        {
            maxTemperature =
                minTemperature;
        }

        minRainIntensity =
            Mathf.Clamp01(
                minRainIntensity
            );

        maxRainIntensity =
            Mathf.Clamp(
                maxRainIntensity,
                minRainIntensity,
                1f
            );

        minWindStrength =
            Mathf.Max(
                0f,
                minWindStrength
            );

        maxWindStrength =
            Mathf.Max(
                minWindStrength,
                maxWindStrength
            );
    }
}