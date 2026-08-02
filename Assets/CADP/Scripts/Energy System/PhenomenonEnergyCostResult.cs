using System;
using UnityEngine;

/// <summary>
/// 특정 현상을 실행할 때 필요한
/// 에너지 비용 계산 결과를 보관한다.
/// </summary>
[Serializable]
public struct PhenomenonEnergyCostResult
{
    [SerializeField]
    private float baseCost;

    [SerializeField]
    private float environmentMultiplier;

    [SerializeField]
    private float finalCost;


    /// <summary>
    /// 환경 보정 전 기본 비용.
    /// </summary>
    public float BaseCost
    {
        get
        {
            return baseCost;
        }
    }


    /// <summary>
    /// 환경에 의해 적용된 비용 배율.
    ///
    /// 1 = 변화 없음
    /// 1.5 = 50% 증가
    /// 0.8 = 20% 감소
    /// </summary>
    public float EnvironmentMultiplier
    {
        get
        {
            return environmentMultiplier;
        }
    }


    /// <summary>
    /// 실제로 소비할 최종 에너지 비용.
    /// </summary>
    public float FinalCost
    {
        get
        {
            return finalCost;
        }
    }


    public PhenomenonEnergyCostResult(
        float newBaseCost,
        float newEnvironmentMultiplier)
    {
        baseCost =
            Mathf.Max(
                0f,
                newBaseCost
            );

        environmentMultiplier =
            Mathf.Max(
                0f,
                newEnvironmentMultiplier
            );

        finalCost =
            baseCost *
            environmentMultiplier;
    }
}