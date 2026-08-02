using UnityEngine;

/// <summary>
/// 현재 월드의 환경 조건을 관리한다.
///
/// 비, 습도, 주변 온도, 바람처럼
/// 여러 현상이 공통으로 참고할 환경 데이터를 보관한다.
///
/// 이 스크립트는 현상별 효과를 직접 계산하지 않는다.
///
/// 예:
/// - Thermal Ignition은 비와 습도를 불리하게 해석
/// - Moisture Condensation은 습도를 유리하게 해석
/// - Air Pressure Gradient는 기존 바람을 참고
/// </summary>
public class EnvironmentConditionSystem : MonoBehaviour
{
    [Header("Rain")]

    [Tooltip("현재 비의 강도. 0은 비 없음, 1은 최대 강도")]
    [Range(0f, 1f)]
    [SerializeField]
    private float rainIntensity = 0f;


    [Header("Humidity")]

    [Tooltip("현재 공기 중 습도. 0은 매우 건조, 1은 포화 상태")]
    [Range(0f, 1f)]
    [SerializeField]
    private float humidity = 0.3f;


    [Header("Temperature")]

    [Tooltip("현재 주변 온도")]
    [SerializeField]
    private float ambientTemperature = 20f;


    [Header("Wind")]

    [Tooltip("현재 환경 바람의 월드 방향")]
    [SerializeField]
    private Vector3 windDirection = Vector3.forward;

    [Tooltip("현재 환경 바람의 세기")]
    [Min(0f)]
    [SerializeField]
    private float windStrength = 0f;


    /// <summary>
    /// 현재 비의 강도.
    /// 0에서 1 사이의 값이다.
    /// </summary>
    public float RainIntensity
    {
        get
        {
            return rainIntensity;
        }
    }


    /// <summary>
    /// 현재 습도.
    /// 0에서 1 사이의 값이다.
    /// </summary>
    public float Humidity
    {
        get
        {
            return humidity;
        }
    }


    /// <summary>
    /// 현재 주변 온도.
    /// </summary>
    public float AmbientTemperature
    {
        get
        {
            return ambientTemperature;
        }
    }


    /// <summary>
    /// 정규화된 환경 바람 방향.
    ///
    /// 바람 세기가 0이거나 방향값이 유효하지 않으면
    /// Vector3.zero를 반환한다.
    /// </summary>
    public Vector3 WindDirection
    {
        get
        {
            if (windStrength <= 0f ||
                windDirection.sqrMagnitude < 0.0001f)
            {
                return Vector3.zero;
            }

            return windDirection.normalized;
        }
    }


    /// <summary>
    /// 현재 환경 바람의 세기.
    /// </summary>
    public float WindStrength
    {
        get
        {
            return windStrength;
        }
    }


    /// <summary>
    /// 환경 바람의 실제 벡터.
    ///
    /// 방향과 세기를 함께 적용한 값이다.
    /// Air Pressure Gradient에서 기존 바람과
    /// 합성할 때 사용할 수 있다.
    /// </summary>
    public Vector3 WindVelocity
    {
        get
        {
            return WindDirection * windStrength;
        }
    }


    /// <summary>
    /// 현재 비가 내리고 있는지 여부.
    /// </summary>
    public bool IsRaining
    {
        get
        {
            return rainIntensity > 0.001f;
        }
    }


    private void Awake()
    {
        ValidateValues();
    }


#if UNITY_EDITOR
    private void OnValidate()
    {
        ValidateValues();
    }
#endif


    /// <summary>
    /// Inspector 또는 외부 코드에서 잘못된 값이 입력되어도
    /// 허용 범위 안으로 보정한다.
    /// </summary>
    private void ValidateValues()
    {
        rainIntensity =
            Mathf.Clamp01(
                rainIntensity
            );

        humidity =
            Mathf.Clamp01(
                humidity
            );

        windStrength =
            Mathf.Max(
                0f,
                windStrength
            );

        if (windDirection.sqrMagnitude < 0.0001f)
        {
            windDirection =
                Vector3.forward;
        }
    }


    /// <summary>
    /// 비의 강도를 변경한다.
    /// </summary>
    public void SetRainIntensity(
        float newRainIntensity)
    {
        rainIntensity =
            Mathf.Clamp01(
                newRainIntensity
            );
    }


    /// <summary>
    /// 습도를 변경한다.
    /// </summary>
    public void SetHumidity(
        float newHumidity)
    {
        humidity =
            Mathf.Clamp01(
                newHumidity
            );
    }


    /// <summary>
    /// 주변 온도를 변경한다.
    /// </summary>
    public void SetAmbientTemperature(
        float newTemperature)
    {
        ambientTemperature =
            newTemperature;
    }


    /// <summary>
    /// 환경 바람의 방향과 세기를 변경한다.
    /// </summary>
    public void SetWind(
        Vector3 newDirection,
        float newStrength)
    {
        if (newDirection.sqrMagnitude > 0.0001f)
        {
            windDirection =
                newDirection.normalized;
        }

        windStrength =
            Mathf.Max(
                0f,
                newStrength
            );
    }


    /// <summary>
    /// 모든 환경값을 중립적인 기본 상태로 되돌린다.
    /// </summary>
    public void ResetToDefault()
    {
        rainIntensity = 0f;
        humidity = 0.3f;
        ambientTemperature = 20f;
        windDirection = Vector3.forward;
        windStrength = 0f;
    }
}