using System;
using UnityEngine;

/// <summary>
/// Air Pressure Gradient 현상의 설정값을 관리한다.
///
/// 실제 공기관 Mesh 조절이나 물리 효과는 담당하지 않고,
/// Radius / Length / Pressure / Duration / Direction 값을 보관한다.
/// </summary>
public class AirPressureGradientController : MonoBehaviour
{
    [Header("Air Pressure Settings")]

    [Tooltip("공기관 반지름")]
    [SerializeField]
    private float radius = 0.5f;

    [Tooltip("공기관 길이")]
    [SerializeField]
    private float length = 8f;

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

    [SerializeField]
    private Vector2 radiusRange =
        new Vector2(0.1f, 5f);

    [SerializeField]
    private Vector2 lengthRange =
        new Vector2(1f, 20f);

    [SerializeField]
    private Vector2 pressureRange =
        new Vector2(10f, 1000f);

    [SerializeField]
    private Vector2 durationRange =
        new Vector2(0.1f, 10f);


    public float Radius => radius;
    public float Length => length;
    public float Pressure => pressure;
    public float Duration => duration;

    public float MinRadius => radiusRange.x;
    public float MaxRadius => radiusRange.y;

    public float MinLength => lengthRange.x;
    public float MaxLength => lengthRange.y;

    public float MinPressure => pressureRange.x;
    public float MaxPressure => pressureRange.y;

    public float MinDuration => durationRange.x;
    public float MaxDuration => durationRange.y;

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
    /// 설정값이 변경되었을 때
    /// Preview나 UI가 갱신할 수 있도록 알린다.
    /// </summary>
    public event Action OnSettingsChanged;


    private void Awake()
    {
        ValidateValues();
    }


    public void SetRadius(float value)
    {
        radius = Mathf.Clamp(
            value,
            radiusRange.x,
            radiusRange.y
        );

        NotifyChanged();
    }


    public void SetLength(float value)
    {
        length = Mathf.Clamp(
            value,
            lengthRange.x,
            lengthRange.y
        );

        NotifyChanged();
    }


    public void SetPressure(float value)
    {
        pressure = Mathf.Clamp(
            value,
            pressureRange.x,
            pressureRange.y
        );

        NotifyChanged();
    }


    public void SetDuration(float value)
    {
        duration = Mathf.Clamp(
            value,
            durationRange.x,
            durationRange.y
        );

        NotifyChanged();
    }


    public void SetDirection(Vector3 value)
    {
        if (value.sqrMagnitude <= 0.0001f)
            return;

        direction = value.normalized;

        NotifyChanged();
    }


    private void ValidateValues()
    {
        radius = Mathf.Clamp(
            radius,
            radiusRange.x,
            radiusRange.y
        );

        length = Mathf.Clamp(
            length,
            lengthRange.x,
            lengthRange.y
        );

        pressure = Mathf.Clamp(
            pressure,
            pressureRange.x,
            pressureRange.y
        );

        duration = Mathf.Clamp(
            duration,
            durationRange.x,
            durationRange.y
        );

        if (direction.sqrMagnitude <= 0.0001f)
        {
            direction = Vector3.forward;
        }
        else
        {
            direction.Normalize();
        }
    }


    private void NotifyChanged()
    {
        OnSettingsChanged?.Invoke();
    }

    /// <summary>
    /// Inspector에서 값을 직접 변경했을 때도
    /// Preview가 즉시 갱신되도록 한다.
    /// </summary>
    private void OnValidate()
    {
        ValidateValues();

        // OnSettingsChanged가 null이 아니면 Invoke
        OnSettingsChanged?.Invoke();
    }
}