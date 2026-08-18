using System;
using UnityEngine;

/// <summary>
/// Air Pressure Gradient 현상의 모든 설정값을 관리한다.
///
/// 이 스크립트가 Air Pressure 설정값의 유일한 기준이다.
///
/// 관리 항목:
/// - Radius
/// - Length
/// - Pressure
/// - Duration
/// - Direction
///
/// UI와 Preview는 자체적으로 범위를 가지지 않고
/// 이 Controller의 값과 범위를 사용한다.
/// </summary>
public class AirPressureGradientController : MonoBehaviour
{
    [Header("Air Pressure Settings")]

    [Tooltip("공기관 굵기 설정값")]
    [SerializeField]
    private float radius = 1f;

    [Tooltip("공기관 길이 설정값")]
    [SerializeField]
    private float length = 3f;

    [Tooltip("공기 압력")]
    [SerializeField]
    private float pressure = 300f;

    [Tooltip("공기 흐름 유지 시간")]
    [SerializeField]
    private float duration = 0.5f;

    [Tooltip("공기가 흐르는 방향")]
    [SerializeField]
    private Vector3 direction = Vector3.forward;


    [Header("Limits")]

    [Tooltip("Radius 최소 / 최대 설정값")]
    [SerializeField]
    private Vector2 radiusRange =
        new Vector2(0.1f, 5f);

    [Tooltip("Length 최소 / 최대 설정값")]
    [SerializeField]
    private Vector2 lengthRange =
        new Vector2(1f, 5f);

    [Tooltip("Pressure 최소 / 최대값")]
    [SerializeField]
    private Vector2 pressureRange =
        new Vector2(10f, 1000f);

    [Tooltip("Duration 최소 / 최대값")]
    [SerializeField]
    private Vector2 durationRange =
        new Vector2(0.1f, 10f);


    // =========================================================
    // Current Values
    // =========================================================

    public float Radius => radius;
    public float Length => length;
    public float Pressure => pressure;
    public float Duration => duration;


    // =========================================================
    // Ranges
    // =========================================================

    public float MinRadius => radiusRange.x;
    public float MaxRadius => radiusRange.y;

    public float MinLength => lengthRange.x;
    public float MaxLength => lengthRange.y;

    public float MinPressure => pressureRange.x;
    public float MaxPressure => pressureRange.y;

    public float MinDuration => durationRange.x;
    public float MaxDuration => durationRange.y;


    /// <summary>
    /// 공기가 흐르는 방향.
    /// 항상 정규화된 방향 Vector를 반환한다.
    /// </summary>
    public Vector3 Direction
    {
        get
        {
            if (direction.sqrMagnitude <= 0.0001f)
                return Vector3.forward;

            return direction.normalized;
        }
    }


    /// <summary>
    /// Air Pressure 설정값이 변경되었을 때 발생한다.
    ///
    /// UI / Preview / Energy 계산 등이
    /// 필요하면 이 이벤트를 구독할 수 있다.
    /// </summary>
    public event Action OnSettingsChanged;


    private void Awake()
    {
        ValidateValues();
    }


    // =========================================================
    // Setters
    // =========================================================

    public void SetRadius(float value)
    {
        radius =
            Mathf.Clamp(
                value,
                MinRadius,
                MaxRadius
            );

        NotifyChanged();
    }


    public void SetLength(float value)
    {
        length =
            Mathf.Clamp(
                value,
                MinLength,
                MaxLength
            );

        NotifyChanged();
    }


    public void SetPressure(float value)
    {
        pressure =
            Mathf.Clamp(
                value,
                MinPressure,
                MaxPressure
            );

        NotifyChanged();
    }


    public void SetDuration(float value)
    {
        duration =
            Mathf.Clamp(
                value,
                MinDuration,
                MaxDuration
            );

        NotifyChanged();
    }


    public void SetDirection(Vector3 value)
    {
        if (value.sqrMagnitude <= 0.0001f)
            return;

        direction =
            value.normalized;

        NotifyChanged();
    }


    // =========================================================
    // Validation
    // =========================================================

    /// <summary>
    /// 모든 현재 설정값이
    /// 지정된 범위를 벗어나지 않도록 보정한다.
    /// </summary>
    private void ValidateValues()
    {
        radius =
            Mathf.Clamp(
                radius,
                MinRadius,
                MaxRadius
            );

        length =
            Mathf.Clamp(
                length,
                MinLength,
                MaxLength
            );

        pressure =
            Mathf.Clamp(
                pressure,
                MinPressure,
                MaxPressure
            );

        duration =
            Mathf.Clamp(
                duration,
                MinDuration,
                MaxDuration
            );


        if (direction.sqrMagnitude <= 0.0001f)
        {
            direction =
                Vector3.forward;
        }
        else
        {
            direction.Normalize();
        }
    }


    /// <summary>
    /// 설정 변경을 다른 시스템에 알린다.
    /// </summary>
    private void NotifyChanged()
    {
        OnSettingsChanged?.Invoke();
    }


    /// <summary>
    /// Inspector에서 값을 직접 수정했을 때도
    /// 범위를 검사하고 변경 이벤트를 발생시킨다.
    /// </summary>
    private void OnValidate()
    {
        ValidateValues();

        OnSettingsChanged?.Invoke();
    }
}