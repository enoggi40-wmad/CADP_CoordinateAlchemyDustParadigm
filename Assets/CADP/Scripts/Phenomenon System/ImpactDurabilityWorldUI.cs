using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 개별 월드 오브젝트의 충격 내구도를
/// 월드 스페이스 Slider로 표시한다.
///
/// 권장 구조:
///
/// FallenBranch_Normal_Root
/// ├─ ImpactDurability.cs
/// └─ FallenBranch_Normal
///    └─ DurabilityCanvas
///       ├─ ImpactDurabilityWorldUI.cs
///       └─ DurabilitySlider
///
/// Canvas는 모델의 자식이지만,
/// 위치와 회전은 월드 기준으로 직접 보정한다.
/// </summary>
public class ImpactDurabilityWorldUI : MonoBehaviour
{
    [Header("References")]

    [Tooltip("표시할 ImpactDurability. 비어 있으면 부모에서 자동 탐색")]
    [SerializeField]
    private ImpactDurability targetDurability;

    [Tooltip("내구도를 표시할 Slider")]
    [SerializeField]
    private Slider durabilitySlider;

    [Tooltip("UI 위치 기준으로 사용할 모델 Renderer")]
    [SerializeField]
    private Renderer targetRenderer;

    [Tooltip("UI가 바라볼 카메라. 비어 있으면 Main Camera 사용")]
    [SerializeField]
    private Camera targetCamera;


    [Header("Position Settings")]

    [Tooltip("모델 Bounds 상단에서 UI를 추가로 띄울 월드 높이")]
    [SerializeField]
    private float worldHeightOffset = 0.15f;

    [Tooltip("Bounds 중심 기준 X/Z 월드 오프셋")]
    [SerializeField]
    private Vector2 worldHorizontalOffset = Vector2.zero;


    [Header("Rotation Settings")]

    [Tooltip("카메라의 회전을 그대로 따라갈지 여부")]
    [SerializeField]
    private bool faceCamera = true;

    [Tooltip("Canvas 앞면이 반대로 보일 경우 사용")]
    [SerializeField]
    private bool flipFacingDirection = false;


    [Header("Display Settings")]

    [Tooltip("Slider 값이 실제 내구도를 따라가는 속도")]
    [SerializeField]
    private float smoothSpeed = 8f;

    [Tooltip("내구도가 최대일 때 UI를 숨길지 여부")]
    [SerializeField]
    private bool hideWhenFull = false;


    private void Awake()
    {
        FindReferences();
        InitializeSlider();
    }


    private void LateUpdate()
    {
        UpdateWorldPosition();
        UpdateWorldRotation();
        UpdateSliderValue();
        UpdateVisibility();
    }


    /// <summary>
    /// 필요한 참조를 자동으로 찾는다.
    /// </summary>
    private void FindReferences()
    {
        if (targetDurability == null)
        {
            targetDurability =
                GetComponentInParent<ImpactDurability>();
        }

        if (durabilitySlider == null)
        {
            durabilitySlider =
                GetComponentInChildren<Slider>(true);
        }

        if (targetRenderer == null)
        {
            /*
             * Canvas 자신이나 UI Graphic이 아니라
             * 부모 모델 쪽의 Renderer를 찾는다.
             */
            Transform current =
                transform.parent;

            while (current != null)
            {
                Renderer renderer =
                    current.GetComponent<Renderer>();

                if (renderer != null &&
                    !(renderer is CanvasRenderer))
                {
                    targetRenderer = renderer;
                    break;
                }

                current = current.parent;
            }
        }

        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }
    }


    /// <summary>
    /// Slider의 기본 범위를 설정한다.
    /// </summary>
    private void InitializeSlider()
    {
        if (durabilitySlider == null)
            return;

        durabilitySlider.minValue = 0f;
        durabilitySlider.maxValue = 1f;
        durabilitySlider.interactable = false;

        if (targetDurability != null)
        {
            durabilitySlider.value =
                targetDurability.RemainingRatio;
        }
    }


    /// <summary>
    /// 모델의 Renderer Bounds 상단에
    /// Canvas를 월드 좌표로 배치한다.
    ///
    /// 부모 모델이 회전해도 로컬 축의 영향을 받지 않는다.
    /// </summary>
    private void UpdateWorldPosition()
    {
        if (targetRenderer == null)
            return;

        Bounds bounds =
            targetRenderer.bounds;

        Vector3 worldPosition =
            bounds.center;

        worldPosition.x +=
            worldHorizontalOffset.x;

        worldPosition.y =
            bounds.max.y +
            worldHeightOffset;

        worldPosition.z +=
            worldHorizontalOffset.y;

        transform.position =
            worldPosition;
    }


    /// <summary>
    /// 부모 모델의 회전을 무시하고
    /// 카메라의 월드 회전을 사용한다.
    /// </summary>
    private void UpdateWorldRotation()
    {
        if (!faceCamera ||
            targetCamera == null)
        {
            return;
        }

        Quaternion cameraRotation =
            targetCamera.transform.rotation;

        if (flipFacingDirection)
        {
            cameraRotation *=
                Quaternion.Euler(
                    0f,
                    180f,
                    0f
                );
        }

        transform.rotation =
            cameraRotation;
    }


    /// <summary>
    /// 현재 남은 내구도 비율을
    /// Slider에 반영한다.
    /// </summary>
    private void UpdateSliderValue()
    {
        if (targetDurability == null ||
            durabilitySlider == null)
        {
            return;
        }

        durabilitySlider.value =
            Mathf.MoveTowards(
                durabilitySlider.value,
                targetDurability.RemainingRatio,
                smoothSpeed * Time.deltaTime
            );
    }


    /// <summary>
    /// 내구도 최대 상태에서 UI를 숨길지 결정한다.
    /// </summary>
    private void UpdateVisibility()
    {
        if (targetDurability == null ||
            durabilitySlider == null)
        {
            return;
        }

        bool shouldShow =
            !hideWhenFull ||
            targetDurability.DamageRatio > 0.001f;

        if (durabilitySlider.gameObject.activeSelf !=
            shouldShow)
        {
            durabilitySlider.gameObject.SetActive(
                shouldShow
            );
        }
    }
}