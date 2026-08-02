using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// ImpactDurability의 충격 누적 기능을
/// 임시로 테스트하기 위한 디버그 스크립트다.
///
/// 실제 게임 시스템이 완성되면 제거한다.
/// </summary>
public class ImpactDurabilityDebug : MonoBehaviour
{
    [Header("References")]

    [Tooltip("테스트할 ImpactDurability")]
    [SerializeField]
    private ImpactDurability impactDurability;


    [Header("Debug Input")]

    [Tooltip("충격을 줄 테스트 키")]
    [SerializeField]
    private Key impactKey = Key.K;

    [Tooltip("키를 한 번 누를 때 전달할 충격량")]
    [SerializeField]
    private float testImpactStrength = 10f;


    private void Awake()
    {
        if (impactDurability == null)
        {
            impactDurability =
                GetComponent<ImpactDurability>();
        }
    }


    private void Update()
    {
        if (Keyboard.current == null)
            return;

        if (!Keyboard.current[impactKey].wasPressedThisFrame)
            return;

        ApplyTestImpact();
    }


    private void ApplyTestImpact()
    {
        if (impactDurability == null)
        {
            Debug.LogWarning(
                "[Impact Debug] ImpactDurability가 연결되지 않았습니다.",
                this
            );

            return;
        }


        impactDurability.ReceiveImpact(
            testImpactStrength,
            transform.forward
        );
    }
}