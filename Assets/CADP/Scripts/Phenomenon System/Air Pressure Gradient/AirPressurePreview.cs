using UnityEngine;

/// <summary>
/// Air Pressure Gradient의 공기관 Preview를 관리한다.
///
/// 역할:
/// 1. Smart Dust Sphere 위치를 따라간다.
/// 2. AirPressureGradientController의 Length / Radius 범위를
///    비율값으로 변환한다.
/// 3. Smart Dust의 실제 크기를 기준으로
///    공기관 Preview 크기를 계산한다.
///
/// Air Pressure의 실제 설정값과 범위는
/// AirPressureGradientController가 유일하게 관리한다.
/// </summary>
public class AirPressurePreview : MonoBehaviour
{
    [Header("References")]

    [Tooltip("Air Pressure 설정값과 범위를 관리하는 컨트롤러")]
    [SerializeField]
    private AirPressureGradientController controller;

    [Tooltip("Smart Dust의 실제 Radius를 관리하는 컨트롤러")]
    [SerializeField]
    private SmartDustDeploymentController deploymentController;

    [Tooltip("실제로 Scale이 변경되는 공기관 Mesh")]
    [SerializeField]
    private Transform airTubeMesh;

    [Tooltip("실제로 움직이는 Smart Dust Sphere")]
    [SerializeField]
    private Transform smartDustSphere;


    [Header("Mesh Scale Correction")]

    [Tooltip(
        "계산된 실제 길이를 현재 Mesh의 X Scale로 " +
        "변환하기 위한 모델 보정값"
    )]
    [SerializeField]
    private float lengthMeshScaleFactor = 1f;

    [Tooltip(
        "계산된 실제 반경을 현재 Mesh의 Y/Z Scale로 " +
        "변환하기 위한 모델 보정값"
    )]
    [SerializeField]
    private float radiusMeshScaleFactor = 1f;


    private void Update()
    {
        FollowSmartDustSphere();
        UpdateTubeScale();
    }


    /// <summary>
    /// Smart Dust 크기와
    /// Air Pressure의 현재 설정값을 이용해
    /// 공기관 Preview Scale을 계산한다.
    /// </summary>
    private void UpdateTubeScale()
    {
        if (controller == null ||
            deploymentController == null ||
            airTubeMesh == null)
        {
            return;
        }


        /*
         * Smart Dust Radius는 실제 월드 반경.
         *
         * 예:
         * Radius 1 → 반경 1m
         * Radius 5 → 반경 5m
         */
        float smartDustRadius =
            deploymentController.Radius;


        /*
         * Smart Dust Sphere의 실제 지름.
         */
        float smartDustDiameter =
            smartDustRadius * 2f;


        /*
         * Controller가 관리하는 Length 범위를 기준으로
         * 현재 Length가 어느 정도인지 0~1로 변환한다.
         *
         * MinLength → 0
         * MaxLength → 1
         */
        float lengthRatio =
            Mathf.InverseLerp(
                controller.MinLength,
                controller.MaxLength,
                controller.Length
            );


        /*
         * 현재 Length 비율을
         * Smart Dust Sphere 지름에 적용한다.
         *
         * Length 최대
         * → Sphere 지름 100% 사용
         */
        float actualTubeLength =
            smartDustDiameter *
            lengthRatio;


        /*
         * Radius도 Controller 범위를 기준으로
         * 0~1 비율값으로 변환한다.
         */
        float radiusRatio =
            Mathf.InverseLerp(
                controller.MinRadius,
                controller.MaxRadius,
                controller.Radius
            );


        /*
         * 현재 Radius 비율을
         * Smart Dust Sphere 반경에 적용한다.
         *
         * Radius 최대
         * → Sphere 반경 100% 사용
         */
        float actualTubeRadius =
            smartDustRadius *
            radiusRatio;


        /*
         * 현재 Mesh 모델의 원본 크기가
         * Unity 월드 1m와 정확히 일치하지 않을 수 있으므로
         * 마지막에 Mesh 보정값을 적용한다.
         */
        Vector3 scale =
            airTubeMesh.localScale;


        // 현재 모델 기준 X = 길이
        scale.x =
            actualTubeLength *
            lengthMeshScaleFactor;


        // 현재 모델 기준 Y/Z = 반경
        scale.y =
            actualTubeRadius *
            radiusMeshScaleFactor;

        scale.z =
            actualTubeRadius *
            radiusMeshScaleFactor;


        airTubeMesh.localScale =
            scale;
    }


    /// <summary>
    /// Air Pressure Preview 중심을
    /// 현재 Smart Dust Sphere 중심 위치에 맞춘다.
    /// </summary>
    private void FollowSmartDustSphere()
    {
        if (smartDustSphere == null)
            return;

        transform.position =
            smartDustSphere.position;
    }
}