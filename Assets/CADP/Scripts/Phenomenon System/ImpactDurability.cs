using UnityEngine;

/// <summary>
/// 오브젝트의 충격 내구도와 누적 충격을 관리한다.
///
/// 특정 현상이나 상태에 종속되지 않으며,
/// Normal, Burning, Burned, Wet 등
/// 어떤 상태 프리팹에도 사용할 수 있다.
///
/// 이 스크립트는 반드시 상태 프리팹의 Root에 붙인다.
/// </summary>
public class ImpactDurability : MonoBehaviour
{
    [Header("Durability")]

    [Tooltip("이 상태에서 견딜 수 있는 최대 누적 충격량")]
    [Min(0.01f)]
    [SerializeField]
    private float maxDurability = 100f;

    [Tooltip("현재까지 누적된 충격량")]
    [Min(0f)]
    [SerializeField]
    private float accumulatedImpact = 0f;


    [Header("Break Settings")]

    [Tooltip("내구도가 소진되었을 때 생성할 파괴 VFX")]
    [SerializeField]
    private GameObject breakVfxPrefab;

    [Tooltip("파괴 VFX 생성 위치. 비어 있으면 현재 Root 위치를 사용")]
    [SerializeField]
    private Transform breakVfxPoint;


    [Header("Runtime State")]

    [SerializeField]
    private bool isBroken = false;


    public float MaxDurability
    {
        get
        {
            return maxDurability;
        }
    }


    public float AccumulatedImpact
    {
        get
        {
            return accumulatedImpact;
        }
    }


    public float RemainingDurability
    {
        get
        {
            return Mathf.Max(
                0f,
                maxDurability - accumulatedImpact
            );
        }
    }


    /// <summary>
    /// 현재 손상 비율.
    ///
    /// 0 = 손상 없음
    /// 1 = 완전 파괴
    /// </summary>
    public float DamageRatio
    {
        get
        {
            if (maxDurability <= 0f)
                return 1f;

            return Mathf.Clamp01(
                accumulatedImpact / maxDurability
            );
        }
    }


    public bool IsBroken
    {
        get
        {
            return isBroken;
        }
    }


    private void Awake()
    {
        maxDurability =
            Mathf.Max(
                0.01f,
                maxDurability
            );

        accumulatedImpact =
            Mathf.Clamp(
                accumulatedImpact,
                0f,
                maxDurability
            );
    }


    /// <summary>
    /// 외부 시스템으로부터 충격을 전달받는다.
    /// </summary>
    public void ReceiveImpact(
        float strength,
        Vector3 direction)
    {
        if (isBroken)
            return;

        if (strength <= 0f)
            return;


        accumulatedImpact =
            Mathf.Clamp(
                accumulatedImpact + strength,
                0f,
                maxDurability
            );


        Debug.Log(
            $"[ImpactDurability] {name} | " +
            $"Impact: {strength:F2} | " +
            $"Damage: {accumulatedImpact:F2}/{maxDurability:F2} | " +
            $"Ratio: {DamageRatio:P0}",
            this
        );


        if (accumulatedImpact >= maxDurability)
        {
            Break(
                direction
            );
        }
    }


    /// <summary>
    /// 이전 상태 프리팹의 손상 비율을 계승한다.
    /// </summary>
    public void SetDamageRatio(
        float damageRatio)
    {
        if (isBroken)
            return;


        float clampedRatio =
            Mathf.Clamp01(
                damageRatio
            );


        accumulatedImpact =
            maxDurability *
            clampedRatio;


        Debug.Log(
            $"[ImpactDurability] {name} | " +
            $"Damage Ratio 계승: {clampedRatio:P0} | " +
            $"Damage: {accumulatedImpact:F2}/{maxDurability:F2}",
            this
        );


        if (accumulatedImpact >= maxDurability)
        {
            Break(
                Vector3.zero
            );
        }
    }


    /// <summary>
    /// 현재 누적 충격을 초기화한다.
    /// </summary>
    public void ResetImpact()
    {
        if (isBroken)
            return;


        accumulatedImpact = 0f;


        Debug.Log(
            $"[ImpactDurability] {name} | 충격 손상 초기화",
            this
        );
    }


    /// <summary>
    /// 최대 충격 내구도를 변경한다.
    ///
    /// preserveDamageRatio가 true라면
    /// 기존 손상 비율을 유지한다.
    /// </summary>
    public void SetMaxDurability(
        float newMaxDurability,
        bool preserveDamageRatio = true)
    {
        if (isBroken)
            return;


        float previousDamageRatio =
            DamageRatio;


        maxDurability =
            Mathf.Max(
                0.01f,
                newMaxDurability
            );


        if (preserveDamageRatio)
        {
            accumulatedImpact =
                maxDurability *
                previousDamageRatio;
        }
        else
        {
            accumulatedImpact =
                Mathf.Clamp(
                    accumulatedImpact,
                    0f,
                    maxDurability
                );
        }


        if (accumulatedImpact >= maxDurability)
        {
            Break(
                Vector3.zero
            );
        }
    }


    /// <summary>
    /// 내구도가 모두 소진되면
    /// 파괴 VFX를 생성하고 현재 Root를 제거한다.
    /// </summary>
    private void Break(
        Vector3 impactDirection)
    {
        if (isBroken)
            return;


        isBroken = true;


        CreateBreakVfx();


        Debug.Log(
            $"[ImpactDurability] {name} 파괴 | " +
            $"Direction: {impactDirection}",
            this
        );


        /*
         * 이 스크립트는 상태 프리팹의 Root에 붙이므로
         * 현재 씬에 존재하는 Root 인스턴스만 제거한다.
         *
         * Project 창의 프리팹 에셋을 참조하지 않기 때문에
         * Destroying assets is not permitted 오류가 발생하지 않는다.
         */
        Destroy(
            gameObject
        );
    }


    /// <summary>
    /// 파괴 VFX를 생성한다.
    /// </summary>
    private void CreateBreakVfx()
    {
        if (breakVfxPrefab == null)
            return;


        Vector3 spawnPosition =
            breakVfxPoint != null
                ? breakVfxPoint.position
                : transform.position;


        Quaternion spawnRotation =
            breakVfxPoint != null
                ? breakVfxPoint.rotation
                : transform.rotation;


        Instantiate(
            breakVfxPrefab,
            spawnPosition,
            spawnRotation
        );
    }

    /// <summary>
    /// 현재 남아 있는 내구도 비율.
    /// 1 = 최대 내구도, 0 = 파괴 상태
    /// </summary>
    public float RemainingRatio
    {
        get
        {
            return 1f - DamageRatio;
        }
    }
}