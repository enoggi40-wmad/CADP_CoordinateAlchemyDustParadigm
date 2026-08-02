using TMPro;
using UnityEngine;

/// <summary>
/// EnvironmentConditionSystem.cs의 현재 환경값을
/// UI Text로 표시한다.
///
/// 표시 항목:
/// - 습도
/// - 온도
/// - 강수 강도
/// - 바람 세기와 방향
/// </summary>
public class EnvironmentConditionUI : MonoBehaviour
{
    [Header("References")]

    [Tooltip("현재 환경값을 관리하는 시스템")]
    [SerializeField]
    private EnvironmentConditionSystem environmentConditionSystem;

    [Tooltip("습도를 표시할 TMP Text")]
    [SerializeField]
    private TMP_Text humidityText;

    [Tooltip("온도를 표시할 TMP Text")]
    [SerializeField]
    private TMP_Text temperatureText;

    [Tooltip("비의 강도를 표시할 TMP Text")]
    [SerializeField]
    private TMP_Text rainText;

    [Tooltip("바람 세기와 방향을 표시할 TMP Text")]
    [SerializeField]
    private TMP_Text windText;


    [Header("Refresh Settings")]

    [Tooltip("환경 UI 갱신 간격")]
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
        refreshTimer = 0f;

        RefreshUI();
    }


    private void Update()
    {
        refreshTimer -= Time.deltaTime;

        if (refreshTimer > 0f)
            return;

        refreshTimer = refreshInterval;

        RefreshUI();
    }


    /// <summary>
    /// 비어 있는 참조를 자동으로 찾는다.
    /// </summary>
    private void FindMissingReferences()
    {
        if (environmentConditionSystem == null)
        {
            environmentConditionSystem =
                FindFirstObjectByType<EnvironmentConditionSystem>();
        }
    }


    /// <summary>
    /// 현재 환경값을 UI에 표시한다.
    /// </summary>
    private void RefreshUI()
    {
        if (environmentConditionSystem == null)
            return;

        UpdateHumidityText();
        UpdateTemperatureText();
        UpdateRainText();
        UpdateWindText();
    }


    /// <summary>
    /// 현재 습도를 표시한다.
    /// </summary>
    private void UpdateHumidityText()
    {
        if (humidityText == null)
            return;

        int humidityPercent =
            Mathf.RoundToInt(
                environmentConditionSystem.Humidity * 100f
            );

        humidityText.text =
            $"Humidity  {humidityPercent}%";
    }


    /// <summary>
    /// 현재 주변 온도를 표시한다.
    /// </summary>
    private void UpdateTemperatureText()
    {
        if (temperatureText == null)
            return;

        temperatureText.text =
            $"Temp  " +
            $"{environmentConditionSystem.AmbientTemperature:F1}°C";
    }


    /// <summary>
    /// 현재 강수 강도를 표시한다.
    /// </summary>
    private void UpdateRainText()
    {
        if (rainText == null)
            return;

        int rainPercent =
            Mathf.RoundToInt(
                environmentConditionSystem.RainIntensity * 100f
            );

        rainText.text =
            $"Rain  {rainPercent}%";
    }


    /// <summary>
    /// 현재 바람 세기와 방향을 표시한다.
    /// </summary>
    private void UpdateWindText()
    {
        if (windText == null)
            return;

        float windStrength =
            environmentConditionSystem.WindStrength;

        Vector3 windDirection =
            environmentConditionSystem.WindDirection;

        if (windStrength <= 0.01f ||
            windDirection.sqrMagnitude < 0.0001f)
        {
            windText.text =
                "Wind  Calm";

            return;
        }

        string directionName =
            GetCompassDirectionName(
                windDirection
            );

        windText.text =
            $"Wind  {windStrength:F1} m/s {directionName}";
    }


    /// <summary>
    /// 월드 방향을 8방향 문자열로 변환한다.
    ///
    /// 기준:
    /// +Z = N
    /// +X = E
    /// -Z = S
    /// -X = W
    /// </summary>
    private string GetCompassDirectionName(
        Vector3 direction)
    {
        float angle =
            Mathf.Atan2(
                direction.x,
                direction.z
            ) *
            Mathf.Rad2Deg;

        if (angle < 0f)
        {
            angle += 360f;
        }

        string[] directionNames =
        {
            "N",
            "NE",
            "E",
            "SE",
            "S",
            "SW",
            "W",
            "NW"
        };

        int index =
            Mathf.RoundToInt(
                angle / 45f
            ) % 8;

        return directionNames[index];
    }
}