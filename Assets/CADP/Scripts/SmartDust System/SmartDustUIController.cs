using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SmartDustUIController : MonoBehaviour
{
    [Header("Controller")]

    // 좌표와 반경 값을 실제로 저장하는 DeploymentController
    [SerializeField]
    private SmartDustDeploymentController deploymentController;

    [Header("Coordinate Inputs")]

    // X 좌표 입력칸
    [SerializeField]
    private TMP_InputField xInput;

    // Y 좌표 입력칸
    [SerializeField]
    private TMP_InputField yInput;

    // Z 좌표 입력칸
    [SerializeField]
    private TMP_InputField zInput;

    [Header("Radius UI")]

    // Smart Dust 전개 반경을 조절하는 Slider
    [SerializeField]
    private Slider radiusSlider;

    // 현재 반경 값을 숫자로 보여주는 Text
    [SerializeField]
    private TMP_Text radiusValueText;

    [Header("Radius Settings")]

    // Slider가 사용할 최소 반경
    [SerializeField]
    private float minimumRadius = 0.1f;

    // Slider가 사용할 최대 반경
    [SerializeField]
    private float maximumRadius = 20f;

    private void Start()
    {
        // UI에 초기값을 표시한다.
        InitializeUI();

        // UI 이벤트를 등록한다.
        RegisterListeners();
    }

    private void InitializeUI()
    {
        // DeploymentController가 없다면
        // 초기값을 가져올 수 없다.
        if (deploymentController == null)
        {
            Debug.LogWarning(
                "[Smart Dust UI] DeploymentController가 연결되지 않았습니다.",
                this
            );

            return;
        }

        // 현재 저장된 좌표 오프셋을 가져온다.
        Vector3 offset =
            deploymentController.CoordinateOffset;

        // 이벤트를 발생시키지 않고
        // X, Y, Z 입력칸에 초기값을 표시한다.
        if (xInput != null)
        {
            xInput.SetTextWithoutNotify(
                FloatToText(offset.x)
            );
        }

        if (yInput != null)
        {
            yInput.SetTextWithoutNotify(
                FloatToText(offset.y)
            );
        }

        if (zInput != null)
        {
            zInput.SetTextWithoutNotify(
                FloatToText(offset.z)
            );
        }

        // Radius Slider의 범위를 설정한다.
        if (radiusSlider != null)
        {
            radiusSlider.minValue = minimumRadius;
            radiusSlider.maxValue = maximumRadius;

            // Slider 값을 변경하되
            // onValueChanged 이벤트는 발생시키지 않는다.
            radiusSlider.SetValueWithoutNotify(
                deploymentController.Radius
            );
        }

        // 현재 반경 값을 Text에 표시한다.
        RefreshRadiusText(
            deploymentController.Radius
        );
    }

    private void RegisterListeners()
    {
        // 좌표 입력값이 바뀔 때마다
        // DeploymentController의 좌표를 갱신한다.
        if (xInput != null)
        {
            xInput.onValueChanged.AddListener(
                OnCoordinateChanged
            );
        }

        if (yInput != null)
        {
            yInput.onValueChanged.AddListener(
                OnCoordinateChanged
            );
        }

        if (zInput != null)
        {
            zInput.onValueChanged.AddListener(
                OnCoordinateChanged
            );
        }

        // Radius Slider가 움직일 때마다
        // DeploymentController의 반경을 갱신한다.
        if (radiusSlider != null)
        {
            radiusSlider.onValueChanged.AddListener(
                OnRadiusChanged
            );
        }
    }

    private void OnCoordinateChanged(string unusedValue)
    {
        // DeploymentController가 없다면
        // 좌표 값을 저장할 수 없다.
        if (deploymentController == null)
            return;

        // X, Y, Z 중 하나라도 숫자로 읽을 수 없다면
        // 좌표를 갱신하지 않는다.
        if (!TryReadFloat(xInput, out float x))
            return;

        if (!TryReadFloat(yInput, out float y))
            return;

        if (!TryReadFloat(zInput, out float z))
            return;

        // 입력된 좌표를 DeploymentController에 저장한다.
        deploymentController.SetCoordinateOffset(
            new Vector3(x, y, z)
        );
    }

    private void OnRadiusChanged(float radius)
    {
        // DeploymentController가 없다면
        // 반경 값을 저장할 수 없다.
        if (deploymentController == null)
            return;

        // Slider 값을 설정한 최소값과 최대값 사이로 제한한다.
        radius = Mathf.Clamp(
            radius,
            minimumRadius,
            maximumRadius
        );

        // 실제 Smart Dust 반경을 갱신한다.
        deploymentController.SetRadius(radius);

        // UI에 현재 반경 숫자를 표시한다.
        RefreshRadiusText(radius);
    }

    private void RefreshRadiusText(float radius)
    {
        // 반경 표시용 Text가 없다면 실행하지 않는다.
        if (radiusValueText == null)
            return;

        // 예: Radius: 3.5
        radiusValueText.text =
            $"Radius: {FloatToText(radius)}";
    }

    private bool TryReadFloat(
        TMP_InputField inputField,
        out float value)
    {
        // 변환 실패 시 사용할 기본값
        value = 0f;

        // InputField가 없다면 실패 처리한다.
        if (inputField == null)
            return false;

        // 점(.)을 소수점으로 사용하는 형식으로 먼저 변환한다.
        if (float.TryParse(
                inputField.text,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out value))
        {
            return true;
        }

        // 사용자 PC의 지역 설정으로 한 번 더 시도한다.
        return float.TryParse(
            inputField.text,
            out value
        );
    }

    private string FloatToText(float value)
    {
        // 불필요한 소수점 아래 0을 제거한다.
        //
        // 예:
        // 3.00 → 3
        // 2.50 → 2.5
        return value.ToString(
            "0.##",
            CultureInfo.InvariantCulture
        );
    }

    private void OnDestroy()
    {
        // 오브젝트가 파괴될 때
        // 등록했던 UI 이벤트를 모두 해제한다.

        if (xInput != null)
        {
            xInput.onValueChanged.RemoveListener(
                OnCoordinateChanged
            );
        }

        if (yInput != null)
        {
            yInput.onValueChanged.RemoveListener(
                OnCoordinateChanged
            );
        }

        if (zInput != null)
        {
            zInput.onValueChanged.RemoveListener(
                OnCoordinateChanged
            );
        }

        if (radiusSlider != null)
        {
            radiusSlider.onValueChanged.RemoveListener(
                OnRadiusChanged
            );
        }
    }

    /// <summary>
    /// 현재 원점 모드에 저장된 좌표를
    /// X, Y, Z 입력칸에 다시 표시한다.
    /// </summary>
    public void RefreshCoordinateInputs()
    {
        if (deploymentController == null)
            return;

        // 현재 모드의 전용 좌표를 가져온다.
        Vector3 offset =
            deploymentController.CoordinateOffset;

        // SetTextWithoutNotify를 사용해야
        // onValueChanged가 호출되지 않는다.
        if (xInput != null)
        {
            xInput.SetTextWithoutNotify(
                FloatToText(offset.x)
            );
        }

        if (yInput != null)
        {
            yInput.SetTextWithoutNotify(
                FloatToText(offset.y)
            );
        }

        if (zInput != null)
        {
            zInput.SetTextWithoutNotify(
                FloatToText(offset.z)
            );
        }
    }
}