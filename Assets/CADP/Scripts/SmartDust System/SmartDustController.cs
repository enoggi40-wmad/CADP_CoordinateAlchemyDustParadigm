using UnityEngine;

/// <summary>
/// Smart Dust 구체의 표시 상태와 크기만 관리한다.
///
/// 로봇 이동은 SmartDustRobotController.cs가 담당하며,
/// 로봇 모델의 표시 상태에는 영향을 주지 않는다.
/// </summary>
public class SmartDustController : MonoBehaviour
{
    [Header("References")]

    [Tooltip("Smart Dust의 반경 정보를 관리하는 컨트롤러")]
    [SerializeField]
    private SmartDustDeploymentController deploymentController;

    [Tooltip("로봇이 들고 있는 Smart Dust 구체 오브젝트")]
    [SerializeField]
    private GameObject smartDustSphere;


    [Header("State")]

    [SerializeField]
    private bool isVisible;


    public bool IsVisible => isVisible;


    private void Start()
    {
        ApplyVisibility();
        RefreshSphereScale();
    }

    private void Update()
    {
        if (!isVisible)
            return;

        RefreshSphereScale();
    }

    /// <summary>
    /// Smart Dust 구체의 표시 여부를 설정한다.
    /// 로봇 모델은 항상 그대로 유지된다.
    /// </summary>
    public void SetVisible(bool visible)
    {
        isVisible = visible;

        ApplyVisibility();

        if (visible)
        {
            RefreshSphereScale();
        }
    }


    /// <summary>
    /// 현재 설정된 반경에 맞춰 구체의 크기를 갱신한다.
    /// UI에서 Radius가 변경될 때 호출하면 된다.
    /// </summary>
    public void RefreshSphereScale()
    {
        if (deploymentController == null)
        {
            Debug.LogWarning(
                "[Smart Dust] DeploymentController가 연결되지 않았습니다.",
                this
            );

            return;
        }

        if (smartDustSphere == null)
        {
            Debug.LogWarning(
                "[Smart Dust] SmartDustSphere가 연결되지 않았습니다.",
                this
            );

            return;
        }

        float diameter = deploymentController.Radius * 2f;

        smartDustSphere.transform.localScale =
            Vector3.one * diameter;
    }


    /// <summary>
    /// 구체 오브젝트에만 활성 상태를 적용한다.
    /// </summary>
    private void ApplyVisibility()
    {
        if (smartDustSphere == null)
            return;

        smartDustSphere.SetActive(isVisible);
    }
}