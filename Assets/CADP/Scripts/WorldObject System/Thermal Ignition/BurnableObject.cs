using UnityEngine;

/// <summary>
/// 불이 붙을 수 있는 오브젝트의 점화 상태와
/// Fire VFX 생성을 관리한다.
///
/// 현재 단계에서는:
/// - 중복 점화 방지
/// - Fire VFX 생성
/// 만 처리한다.
///
/// 연소 시간, Burned 상태, 확산 기능은 이후 단계에서 추가한다.
/// </summary>
public class BurnableObject : MonoBehaviour
{
    [Header("Fire VFX")]

    [Tooltip("점화될 때 생성할 불 VFX 프리팹")]
    [SerializeField]
    private GameObject fireVfxPrefab;

    [Tooltip("불 VFX가 생성될 위치")]
    [SerializeField]
    private Transform fireVfxPoint;

    [Tooltip("생성되는 불 VFX의 크기 배율")]
    [SerializeField]
    private float fireVfxScale = 1f;


    [Header("Runtime State")]

    [Tooltip("현재 오브젝트가 점화된 상태인지 표시")]
    [SerializeField]
    private bool isIgnited;


    /// <summary>
    /// 현재 점화 여부
    /// </summary>
    public bool IsIgnited
    {
        get
        {
            return isIgnited;
        }
    }


    /// <summary>
    /// 오브젝트 점화를 시도한다.
    ///
    /// 이미 점화된 경우 false를 반환하고,
    /// 새로 점화된 경우 Fire VFX를 생성한 뒤 true를 반환한다.
    /// </summary>
    public bool TryIgnite()
    {
        // 이미 점화된 오브젝트에는
        // 불 VFX를 중복 생성하지 않는다.
        if (isIgnited)
        {
            return false;
        }

        if (fireVfxPrefab == null)
        {
            Debug.LogWarning(
                $"[BurnableObject] {name}에 Fire VFX Prefab이 연결되지 않았습니다.",
                this
            );

            return false;
        }

        // 별도의 생성 지점이 없다면
        // 현재 오브젝트 위치를 대신 사용한다.
        Transform spawnPoint =
            fireVfxPoint != null
                ? fireVfxPoint
                : transform;

        // Fire VFX를 점화 대상의 자식으로 생성한다.
        GameObject createdFireVfx =
            Instantiate(
                fireVfxPrefab,
                spawnPoint.position,
                spawnPoint.rotation,
                spawnPoint
            );

        /*
         * 프리팹 자체에 설정된 기본 크기는 유지하면서,
         * 오브젝트별 fireVfxScale을 곱한다.
         */
        createdFireVfx.transform.localScale *=
            Mathf.Max(0f, fireVfxScale);

        isIgnited = true;

        Debug.Log(
            $"[BurnableObject] {name} 점화 성공",
            this
        );

        return true;
    }
}