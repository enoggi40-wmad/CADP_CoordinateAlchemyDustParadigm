using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Air Pressure Gradient 전용 UI.
///
/// Slider 값을 AirPressureGradientController에 전달하고,
/// 현재 값을 Text로 표시한다.
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


    private void SetupSliders()
    {
        radiusSlider.minValue = controller.MinRadius;
        radiusSlider.maxValue = controller.MaxRadius;

        lengthSlider.minValue = controller.MinLength;
        lengthSlider.maxValue = controller.MaxLength;

        pressureSlider.minValue = controller.MinPressure;
        pressureSlider.maxValue = controller.MaxPressure;

        durationSlider.minValue = controller.MinDuration;
        durationSlider.maxValue = controller.MaxDuration;


        radiusSlider.value = controller.Radius;
        lengthSlider.value = controller.Length;
        pressureSlider.value = controller.Pressure;
        durationSlider.value = controller.Duration;
    }


    private void RegisterEvents()
    {
        radiusSlider.onValueChanged.AddListener(
            controller.SetRadius
        );

        lengthSlider.onValueChanged.AddListener(
            controller.SetLength
        );

        pressureSlider.onValueChanged.AddListener(
            controller.SetPressure
        );

        durationSlider.onValueChanged.AddListener(
            controller.SetDuration
        );


        radiusSlider.onValueChanged.AddListener(
            _ => RefreshUI()
        );

        lengthSlider.onValueChanged.AddListener(
            _ => RefreshUI()
        );

        pressureSlider.onValueChanged.AddListener(
            _ => RefreshUI()
        );

        durationSlider.onValueChanged.AddListener(
            _ => RefreshUI()
        );
    }


    private void UnregisterEvents()
    {
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
    }


    private void RefreshUI()
    {
        radiusValueText.text =
            $"{controller.Radius:F1} m";

        lengthValueText.text =
            $"{controller.Length:F1} m";

        pressureValueText.text =
            $"{controller.Pressure:F0}";

        durationValueText.text =
            $"{controller.Duration:F1} s";
    }
}