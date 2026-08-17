using UnityEngine;

/// <summary>
/// Air Pressure Gradient의 공기관 Preview를 관리한다.
///
/// 역할:
/// 1. Smart Dust Sphere 위치를 따라간다.
/// 2. Smart Dust Sphere 크기에 맞춰 공기관 전체 크기를 조절한다.
/// 3. Air Pressure Length / Radius 값을 Preview Scale에 반영한다.
/// 4. Length가 길어질수록 허용 가능한 Radius를 줄여
///    공기관이 Smart Dust 영역 안에서 자연스럽게 보이도록 한다.
///
/// 기준:
/// Smart Dust Radius = 1
/// Length = 1
/// Radius = 1.3
/// → 기본 공기관 크기
/// </summary>
public class AirPressurePreview : MonoBehaviour
{
    [Header("References")]

    [Tooltip("Air Pressure의 Length / Radius 값을 관리하는 컨트롤러")]
    [SerializeField]
    private AirPressureGradientController controller;

    [Tooltip("Smart Dust의 현재 Radius를 관리하는 컨트롤러")]
    [SerializeField]
    private SmartDustDeploymentController deploymentController;

    [Tooltip("실제로 Scale이 변경되는 공기관 Mesh")]
    [SerializeField]
    private Transform airTubeMesh;

    [Tooltip("실제로 움직이는 Smart Dust Sphere")]
    [SerializeField]
    private Transform smartDustSphere;


    [Header("Base Scale")]

    [Tooltip("Length = 1일 때 공기관 길이 Scale")]
    [SerializeField]
    private float baseLengthScale = 0.1f;

    [Tooltip("Radius = 1.3일 때 공기관 굵기 Scale")]
    [SerializeField]
    private float baseRadiusScale = 3f;


    [Header("Length Settings")]

    [Tooltip("Air Pressure Length 최소값")]
    [SerializeField]
    private float minimumLength = 1f;

    [Tooltip("Air Pressure Length 최대값")]
    [SerializeField]
    private float maximumLength = 20f;


    [Header("Radius Balance")]

    [Tooltip("Length가 최소일 때 허용되는 최대 Radius")]
    [SerializeField]
    private float maxRadiusAtMinLength = 1.3f;

    [Tooltip("Length가 최대일 때 허용되는 최대 Radius")]
    [SerializeField]
    private float maxRadiusAtMaxLength = 0.2f;


    private void Update()
    {
        FollowSmartDustSphere();
        UpdateTubeScale();
    }


    /// <summary>
    /// Smart Dust Sphere와 Air Pressure 설정값을 기준으로
    /// 공기관 Preview Scale을 갱신한다.
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
         * Smart Dust Sphere Radius.
         *
         * 현재 범위:
         * 1 ~ 5
         *
         * Sphere가 커질수록
         * 공기관 전체 기본 크기도 같이 커진다.
         */
        float smartDustRadius =
            deploymentController.Radius;


        /*
         * 현재 Length가
         * 1 ~ 20 범위에서 어느 정도인지
         * 0 ~ 1 값으로 변환한다.
         *
         * Length = 1  → 0
         * Length = 20 → 1
         */
        float lengthRatio =
            Mathf.InverseLerp(
                minimumLength,
                maximumLength,
                controller.Length
            );


        /*
         * Length가 길어질수록
         * 허용 가능한 최대 Radius를 줄인다.
         *
         * 예:
         * Length = 1
         * → Radius 최대 1.3
         *
         * Length = 20
         * → Radius 최대 0.2
         */
        float maxAllowedRadius =
            Mathf.Lerp(
                maxRadiusAtMinLength,
                maxRadiusAtMaxLength,
                lengthRatio
            );


        /*
         * 사용자가 설정한 Radius와
         * 현재 Length에서 허용되는 최대 Radius 중
         * 더 작은 값을 Preview에 사용한다.
         *
         * 실제 Controller.Radius 값 자체는 변경하지 않는다.
         */
        float previewRadius =
            Mathf.Min(
                controller.Radius,
                maxAllowedRadius
            );


        /*
         * Length Scale.
         *
         * Length = 1일 때
         * baseLengthScale을 기준으로 한다.
         *
         * Smart Dust Sphere가 커지면
         * 전체 Scale도 같이 커진다.
         */
        float targetLength =
            controller.Length *
            baseLengthScale *
            smartDustRadius;


        /*
         * Radius Scale.
         *
         * Radius = 1.3을 기준 크기로 보고
         * 현재 previewRadius가 몇 배인지 계산한다.
         */
        float radiusRatio =
            previewRadius /
            maxRadiusAtMinLength;


        float targetRadius =
            baseRadiusScale *
            radiusRatio *
            smartDustRadius;


        Vector3 scale =
            airTubeMesh.localScale;


        /*
         * 현재 모델 기준:
         *
         * X = Length
         * Y/Z = 굵기
         */
        scale.x =
            targetLength;

        scale.y =
            targetRadius;

        scale.z =
            targetRadius;


        airTubeMesh.localScale =
            scale;
    }


    /// <summary>
    /// Air Pressure Preview를
    /// Smart Dust Sphere 중심 위치에 맞춘다.
    /// </summary>
    private void FollowSmartDustSphere()
    {
        if (smartDustSphere == null)
            return;

        transform.position =
            smartDustSphere.position;
    }
}