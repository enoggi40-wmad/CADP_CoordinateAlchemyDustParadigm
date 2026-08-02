using TMPro;
using UnityEngine;

/// <summary>
/// SmartDustEnergySystem.cs의 현재 에너지를
/// UI Text로 표시한다.
/// </summary>
public class SmartDustEnergyUI : MonoBehaviour
{
    [Header("References")]

    [Tooltip("플레이어의 Smart Dust 에너지를 관리하는 시스템")]
    [SerializeField]
    private SmartDustEnergySystem smartDustEnergySystem;

    [Tooltip("현재 에너지와 최대 에너지를 표시할 TMP Text")]
    [SerializeField]
    private TMP_Text energyRemainText;


    private void Awake()
    {
        FindMissingReferences();
    }


    private void OnEnable()
    {
        SubscribeEvents();
        RefreshUI();
    }


    private void OnDisable()
    {
        UnsubscribeEvents();
    }


    /// <summary>
    /// 비어 있는 참조를 자동으로 찾는다.
    /// </summary>
    private void FindMissingReferences()
    {
        if (smartDustEnergySystem == null)
        {
            smartDustEnergySystem =
                FindFirstObjectByType<SmartDustEnergySystem>();
        }

        if (energyRemainText == null)
        {
            TMP_Text[] texts =
                GetComponentsInChildren<TMP_Text>(true);

            foreach (TMP_Text text in texts)
            {
                if (text.name == "EnergyRemainText")
                {
                    energyRemainText = text;
                    break;
                }
            }
        }
    }


    /// <summary>
    /// 에너지 변경 이벤트를 구독한다.
    /// </summary>
    private void SubscribeEvents()
    {
        if (smartDustEnergySystem == null)
            return;

        smartDustEnergySystem.OnEnergyChanged -=
            HandleEnergyChanged;

        smartDustEnergySystem.OnEnergyChanged +=
            HandleEnergyChanged;
    }


    /// <summary>
    /// 이벤트 구독을 해제한다.
    /// </summary>
    private void UnsubscribeEvents()
    {
        if (smartDustEnergySystem == null)
            return;

        smartDustEnergySystem.OnEnergyChanged -=
            HandleEnergyChanged;
    }


    /// <summary>
    /// 에너지가 변경되었을 때 UI를 갱신한다.
    /// </summary>
    private void HandleEnergyChanged(
        float currentEnergy,
        float maxEnergy)
    {
        UpdateEnergyText(
            currentEnergy,
            maxEnergy
        );
    }


    /// <summary>
    /// 현재 시스템 값을 기준으로 UI를 즉시 갱신한다.
    /// </summary>
    private void RefreshUI()
    {
        if (smartDustEnergySystem == null)
            return;

        UpdateEnergyText(
            smartDustEnergySystem.CurrentEnergy,
            smartDustEnergySystem.MaxEnergy
        );
    }


    /// <summary>
    /// 에너지 값을 Text에 표시한다.
    /// </summary>
    private void UpdateEnergyText(
        float currentEnergy,
        float maxEnergy)
    {
        if (energyRemainText == null)
            return;

        energyRemainText.text =
            $"{Mathf.CeilToInt(currentEnergy)} / " +
            $"{Mathf.CeilToInt(maxEnergy)}";
    }
}