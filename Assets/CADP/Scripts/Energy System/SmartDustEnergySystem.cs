using System;
using UnityEngine;

/// <summary>
/// 플레이어가 보유한 Smart Dust 에너지를 관리한다.
///
/// 환경이나 현상별 계산은 담당하지 않고,
/// 에너지 보유량 확인, 소비, 회복만 담당한다.
/// </summary>
public class SmartDustEnergySystem : MonoBehaviour
{
    [Header("Energy")]

    [Tooltip("최대 Smart Dust 에너지")]
    [Min(0.01f)]
    [SerializeField]
    private float maxEnergy = 500f;

    [Tooltip("게임 시작 시 에너지를 최대치로 채울지 여부")]
    [SerializeField]
    private bool startWithFullEnergy = true;

    [Tooltip("현재 Smart Dust 에너지")]
    [Min(0f)]
    [SerializeField]
    private float currentEnergy = 500f;


    /// <summary>
    /// 에너지 값이 변경되었을 때 호출된다.
    ///
    /// 첫 번째 값: 현재 에너지
    /// 두 번째 값: 최대 에너지
    /// </summary>
    public event Action<float, float> OnEnergyChanged;


    public float MaxEnergy
    {
        get
        {
            return maxEnergy;
        }
    }


    public float CurrentEnergy
    {
        get
        {
            return currentEnergy;
        }
    }


    public float EnergyRatio
    {
        get
        {
            if (maxEnergy <= 0f)
                return 0f;

            return currentEnergy / maxEnergy;
        }
    }


    private void Awake()
    {
        maxEnergy =
            Mathf.Max(
                0.01f,
                maxEnergy
            );

        if (startWithFullEnergy)
        {
            currentEnergy =
                maxEnergy;
        }
        else
        {
            currentEnergy =
                Mathf.Clamp(
                    currentEnergy,
                    0f,
                    maxEnergy
                );
        }

        NotifyEnergyChanged();
    }


#if UNITY_EDITOR
    private void OnValidate()
    {
        maxEnergy =
            Mathf.Max(
                0.01f,
                maxEnergy
            );

        currentEnergy =
            Mathf.Clamp(
                currentEnergy,
                0f,
                maxEnergy
            );
    }
#endif


    /// <summary>
    /// 지정된 에너지를 소비할 수 있는지 확인한다.
    /// </summary>
    public bool CanConsume(
        float amount)
    {
        if (amount < 0f)
            return false;

        return currentEnergy >= amount;
    }


    /// <summary>
    /// 에너지가 충분하면 차감하고 true를 반환한다.
    /// 부족하면 차감하지 않고 false를 반환한다.
    /// </summary>
    public bool TryConsume(
        float amount)
    {
        if (amount < 0f)
        {
            Debug.LogWarning(
                "[Smart Dust Energy] 음수 에너지는 소비할 수 없습니다.",
                this
            );

            return false;
        }

        if (!CanConsume(amount))
        {
            Debug.Log(
                $"[Smart Dust Energy] 에너지 부족 | " +
                $"필요: {amount:F1} | " +
                $"현재: {currentEnergy:F1}",
                this
            );

            return false;
        }

        currentEnergy -=
            amount;

        currentEnergy =
            Mathf.Max(
                0f,
                currentEnergy
            );

        Debug.Log(
            $"[Smart Dust Energy] 에너지 소비 | " +
            $"소비: {amount:F1} | " +
            $"남은 에너지: {currentEnergy:F1}/{maxEnergy:F1}",
            this
        );

        NotifyEnergyChanged();

        return true;
    }


    /// <summary>
    /// 지정된 양만큼 에너지를 회복한다.
    /// </summary>
    public void RestoreEnergy(
        float amount)
    {
        if (amount <= 0f)
            return;

        currentEnergy =
            Mathf.Clamp(
                currentEnergy + amount,
                0f,
                maxEnergy
            );

        Debug.Log(
            $"[Smart Dust Energy] 에너지 회복 | " +
            $"회복: {amount:F1} | " +
            $"현재: {currentEnergy:F1}/{maxEnergy:F1}",
            this
        );

        NotifyEnergyChanged();
    }


    /// <summary>
    /// 에너지를 최대치로 회복한다.
    /// </summary>
    public void RestoreToFull()
    {
        currentEnergy =
            maxEnergy;

        NotifyEnergyChanged();
    }


    /// <summary>
    /// 현재 에너지를 직접 설정한다.
    /// 디버그, 세이브 로드 등에 사용할 수 있다.
    /// </summary>
    public void SetCurrentEnergy(
        float newEnergy)
    {
        currentEnergy =
            Mathf.Clamp(
                newEnergy,
                0f,
                maxEnergy
            );

        NotifyEnergyChanged();
    }


    private void NotifyEnergyChanged()
    {
        OnEnergyChanged?.Invoke(
            currentEnergy,
            maxEnergy
        );
    }
}