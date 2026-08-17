using System;
using UnityEngine;

/// <summary>
/// 현재 선택된 현상에 따라
/// 현상별 설정 Panel을 활성화/비활성화한다.
///
/// 또한 설정 UI의 표시 상태가 변경되면
/// OnSettingsVisibilityChanged 이벤트를 발생시킨다.
///
/// 예:
/// AirPressureGradient 선택
/// → Air Pressure Gradient Panel ON
/// → OnSettingsVisibilityChanged(AirPressureGradient, true)
///
/// G키로 PhenomenonInfoUI가 닫힘
/// → OnDisable()
/// → OnSettingsVisibilityChanged(AirPressureGradient, false)
/// </summary>
public class PhenomenonSettingsUIController : MonoBehaviour
{
    [Header("References")]

    [Tooltip("현재 선택된 현상을 관리하는 컨트롤러")]
    [SerializeField]
    private SmartDustDeploymentController deploymentController;


    [Header("Phenomenon Panels")]

    [Tooltip("Thermal Ignition 전용 설정 Panel")]
    [SerializeField]
    private GameObject thermalIgnitionPanel;

    [Tooltip("Air Pressure Gradient 전용 설정 Panel")]
    [SerializeField]
    private GameObject airPressureGradientPanel;


    /// <summary>
    /// 현상 설정 UI의 활성 상태가 변경될 때 발생한다.
    ///
    /// 첫 번째 값:
    /// 현재 선택된 PhenomenonType
    ///
    /// 두 번째 값:
    /// true  = 설정 UI가 열림
    /// false = 설정 UI가 닫힘
    /// </summary>
    public event Action<PhenomenonType, bool>
        OnSettingsVisibilityChanged;


    private void Awake()
    {
        /*
         * Inspector에서 연결하지 않았을 경우
         * 자동으로 DeploymentController를 찾는다.
         */
        if (deploymentController == null)
        {
            deploymentController =
                FindFirstObjectByType<SmartDustDeploymentController>();
        }
    }


    private void OnEnable()
    {
        /*
         * PhenomenonInfoUI가 다시 활성화되면
         * 현재 선택된 현상에 맞는 Panel을 표시한다.
         */
        Refresh();
    }


    private void OnDisable()
    {
        /*
         * G키 등으로 PhenomenonInfoUI가 닫히면
         * Preview 시스템에게
         * "설정 화면이 닫혔다"고 알려준다.
         */
        PhenomenonType selectedPhenomenon =
            deploymentController != null
                ? deploymentController.SelectedPhenomenon
                : PhenomenonType.None;

        OnSettingsVisibilityChanged?.Invoke(
            selectedPhenomenon,
            false
        );
    }


    /// <summary>
    /// 현재 선택된 현상에 따라
    /// 전용 설정 Panel을 갱신한다.
    /// </summary>
    public void Refresh()
    {
        if (deploymentController == null)
            return;


        PhenomenonType selectedPhenomenon =
            deploymentController.SelectedPhenomenon;


        /*
         * Thermal Ignition Panel
         */
        if (thermalIgnitionPanel != null)
        {
            thermalIgnitionPanel.SetActive(
                selectedPhenomenon ==
                PhenomenonType.ThermalIgnition
            );
        }


        /*
         * Air Pressure Gradient Panel
         */
        if (airPressureGradientPanel != null)
        {
            airPressureGradientPanel.SetActive(
                selectedPhenomenon ==
                PhenomenonType.AirPressureGradient
            );
        }


        /*
         * None이 아니라 실제 현상이 선택되어 있다면
         * 해당 현상의 설정 UI가 열려 있는 상태다.
         */
        bool hasSettingsOpen =
            selectedPhenomenon !=
            PhenomenonType.None;


        /*
         * PreviewController 등 외부 시스템에게
         * 현재 설정 화면 상태를 알린다.
         */
        OnSettingsVisibilityChanged?.Invoke(
            selectedPhenomenon,
            hasSettingsOpen
        );
    }
}