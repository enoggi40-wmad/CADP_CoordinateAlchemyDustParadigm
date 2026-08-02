using UnityEngine;

/// <summary>
/// Smart Dust 영역에서 발생하는
/// Thermal Ignition 현상의 시각적 Field를 담당한다.
///
/// 실제 대상 탐색과 점화 판정은
/// PhenomenonExecutor.cs와 ThermalIgnitionExecutor.cs가 담당한다.
///
/// 이 클래스는:
/// - 중심 / 반경 저장
/// - Field VFX 생성
/// - 일정 시간 후 제거
/// 만 담당한다.
/// </summary>
public class ThermalIgnitionField : MonoBehaviour
{
    [Header("Field Settings")]

    [Tooltip("Thermal Ignition Field가 유지되는 시간")]
    [SerializeField]
    private float duration = 2f;


    [Header("Field VFX")]

    [SerializeField]
    private GameObject fieldVfxPrefab;

    [Tooltip("VFX 원본 크기가 맞춰져 있는 기준 반경")]
    [SerializeField]
    private float fieldVfxBaseRadius = 1f;


    private float radius;

    private GameObject createdFieldVfx;


    public float Radius
    {
        get
        {
            return radius;
        }
    }


    /// <summary>
    /// Smart Dust 전개 정보로 Field를 초기화한다.
    ///
    /// 실제 현상 적용은 하지 않고
    /// Field VFX만 생성한다.
    /// </summary>
    public void Initialize(
        Vector3 center,
        float newRadius)
    {
        transform.position = center;

        radius = newRadius;

        CreateFieldVfx();


        Debug.Log(
            $"[Thermal Ignition Field] 활성화 | " +
            $"Center: {center}, Radius: {radius}",
            this
        );


        Destroy(
            gameObject,
            duration
        );
    }


    /// <summary>
    /// Thermal Ignition Field 전용 VFX를 생성한다.
    /// </summary>
    private void CreateFieldVfx()
    {
        if (fieldVfxPrefab == null)
        {
            Debug.LogWarning(
                "[Thermal Ignition Field] Field VFX Prefab이 연결되지 않았습니다.",
                this
            );

            return;
        }


        createdFieldVfx =
            Instantiate(
                fieldVfxPrefab,
                transform.position,
                Quaternion.identity,
                transform
            );


        createdFieldVfx.transform.localPosition =
            Vector3.zero;


        float scaleMultiplier =
            radius /
            Mathf.Max(
                fieldVfxBaseRadius,
                0.01f
            );


        createdFieldVfx.transform.localScale *=
            scaleMultiplier;
    }
}