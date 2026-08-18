using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Air Pressure Gradient 전용 UI.
///
/// 역할:
/// - Slider 범위를 Controller에서 가져온다.
/// - Slider 입력을 Controller에 전달한다.
/// - Controller의 현재 설정값을 UI에 표시한다.
///
/// Air Pressure 설정값과 범위의 실제 관리는
/// AirPressureGradientController가 담당한다.
/// </summary>
public class AirPressureSettingsUI : MonoBehaviour
{
    [Header("Controller")]

    [SerializeField]
    private AirPressureGradientController controller;


    [Header("Radius")]

    [SerializeField]
    private Slider radiusSlider;

    [SerializeField]
    private TMP_Text radiusValueText;


    [Header("Length")]

    [SerializeField]
    private Slider lengthSlider;

    [SerializeField]
    private TMP_Text lengthValueText;


    [Header("Pressure")]

    [SerializeField]
    private Slider pressureSlider;

    [SerializeField]
    private TMP_Text pressureValueText;


    [Header("Duration")]

    [SerializeField]
    private Slider durationSlider;

    [SerializeField]
    private TMP_Text durationValueText;


    private void Start()
    {
        if (controller == null)
            return;

        SetupSliders();
        RegisterEvents();
        RefreshUI();
    }


    private void OnDestroy()
    {
        UnregisterEvents();
    }


    /// <summary>
    /// Slider의 Min / Max를
    /// Controller에서 가져와 설정한다.
    ///
    /// UI에서는 범위를 직접 관리하지 않는다.
    /// </summary>
    private void SetupSliders()
    {
        if (radiusSlider != null)
        {
            radiusSlider.minValue =
                controller.MinRadius;

            radiusSlider.maxValue =
                controller.MaxRadius;

            radiusSlider.SetValueWithoutNotify(
                controller.Radius
            );
        }


        if (lengthSlider != null)
        {
            lengthSlider.minValue =
                controller.MinLength;

            lengthSlider.maxValue =
                controller.MaxLength;

            lengthSlider.SetValueWithoutNotify(
                controller.Length
            );
        }


        if (pressureSlider != null)
        {
            pressureSlider.minValue =
                controller.MinPressure;

            pressureSlider.maxValue =
                controller.MaxPressure;

            pressureSlider.SetValueWithoutNotify(
                controller.Pressure
            );
        }


        if (durationSlider != null)
        {
            durationSlider.minValue =
                controller.MinDuration;

            durationSlider.maxValue =
                controller.MaxDuration;

            durationSlider.SetValueWithoutNotify(
                controller.Duration
            );
        }
    }


    /// <summary>
    /// Slider 입력과 Controller 변경 이벤트를 연결한다.
    /// </summary>
    private void RegisterEvents()
    {
        if (radiusSlider != null)
        {
            radiusSlider.onValueChanged.AddListener(
                controller.SetRadius
            );
        }


        if (lengthSlider != null)
        {
            lengthSlider.onValueChanged.AddListener(
                controller.SetLength
            );
        }


        if (pressureSlider != null)
        {
            pressureSlider.onValueChanged.AddListener(
                controller.SetPressure
            );
        }


        if (durationSlider != null)
        {
            durationSlider.onValueChanged.AddListener(
                controller.SetDuration
            );
        }


        /*
         * Controller 값이 변경되면
         * UI 전체를 갱신한다.
         */
        controller.OnSettingsChanged +=
            HandleSettingsChanged;
    }


    /// <summary>
    /// 등록했던 이벤트를 해제한다.
    /// </summary>
    private void UnregisterEvents()
    {
        if (controller == null)
            return;


        if (radiusSlider != null)
        {
            radiusSlider.onValueChanged.RemoveListener(
                controller.SetRadius
            );
        }


        if (lengthSlider != null)
        {
            lengthSlider.onValueChanged.RemoveListener(
                controller.SetLength
            );
        }


        if (pressureSlider != null)
        {
            pressureSlider.onValueChanged.RemoveListener(
                controller.SetPressure
            );
        }


        if (durationSlider != null)
        {
            durationSlider.onValueChanged.RemoveListener(
                controller.SetDuration
            );
        }


        controller.OnSettingsChanged -=
            HandleSettingsChanged;
    }


    /// <summary>
    /// Controller의 설정값이 변경되었을 때 호출된다.
    /// </summary>
    private void HandleSettingsChanged()
    {
        RefreshUI();
    }


    /// <summary>
    /// Controller의 현재 값을 기준으로
    /// Slider와 Text를 갱신한다.
    /// </summary>
    private void RefreshUI()
    {
        if (controller == null)
            return;


        if (radiusSlider != null)
        {
            radiusSlider.SetValueWithoutNotify(
                controller.Radius
            );
        }


        if (lengthSlider != null)
        {
            lengthSlider.SetValueWithoutNotify(
                controller.Length
            );
        }


        if (pressureSlider != null)
        {
            pressureSlider.SetValueWithoutNotify(
                controller.Pressure
            );
        }


        if (durationSlider != null)
        {
            durationSlider.SetValueWithoutNotify(
                controller.Duration
            );
        }


        if (radiusValueText != null)
        {
            radiusValueText.text =
                $"{controller.Radius:F1}";
        }


        if (lengthValueText != null)
        {
            lengthValueText.text =
                $"{controller.Length:F1}";
        }


        if (pressureValueText != null)
        {
            pressureValueText.text =
                $"{controller.Pressure:F0} Pa";
        }


        if (durationValueText != null)
        {
            durationValueText.text =
                $"{controller.Duration:F1} s";
        }
    }
}