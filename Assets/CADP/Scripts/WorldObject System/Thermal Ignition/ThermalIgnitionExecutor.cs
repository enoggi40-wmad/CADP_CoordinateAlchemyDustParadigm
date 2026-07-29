using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Thermal Ignition 현상을 실행한다.
///
/// Smart Dust 전개 범위 안의 Collider를 검색하고,
/// 해당 오브젝트가 가연성이면서 BurnableObject를 가지고 있다면
/// 점화를 시도한다.
/// </summary>
public class ThermalIgnitionExecutor : MonoBehaviour
{
    [Header("Detection Settings")]

    [Tooltip("Thermal Ignition이 검사할 오브젝트 레이어")]
    [SerializeField]
    private LayerMask targetLayerMask = ~0;

    [Tooltip("Trigger Collider도 점화 대상으로 검사할지 결정")]
    [SerializeField]
    private bool includeTriggerColliders;


    /// <summary>
    /// 지정된 중심과 반경을 기준으로
    /// Thermal Ignition 현상을 실행한다.
    /// </summary>
    public void Execute(Vector3 center, float radius)
    {
        // 잘못된 반경으로 검사하지 않도록 방어한다.
        if (radius <= 0f)
        {
            Debug.LogWarning(
                $"[Thermal Ignition] 반경이 올바르지 않습니다: {radius}",
                this
            );

            return;
        }

        QueryTriggerInteraction triggerInteraction =
            includeTriggerColliders
                ? QueryTriggerInteraction.Collide
                : QueryTriggerInteraction.Ignore;

        // Smart Dust 구체 범위와 겹치는 Collider들을 찾는다.
        Collider[] detectedColliders =
            Physics.OverlapSphere(
                center,
                radius,
                targetLayerMask,
                triggerInteraction
            );

        /*
         * 하나의 오브젝트에 Collider가 여러 개 붙어 있으면
         * 같은 BurnableObject가 여러 번 검색될 수 있다.
         *
         * HashSet을 사용해 같은 대상은 한 번만 처리한다.
         */
        HashSet<BurnableObject> processedBurnableObjects =
            new HashSet<BurnableObject>();

        int ignitedCount = 0;

        foreach (Collider detectedCollider in detectedColliders)
        {
            if (detectedCollider == null)
                continue;

            /*
             * Collider가 모델의 자식에 붙어 있을 수 있으므로
             * 부모 방향으로 BurnableObject를 검색한다.
             */
            BurnableObject burnableObject =
                detectedCollider.GetComponentInParent<BurnableObject>();

            // BurnableObject가 없다면 발화 대상이 아니다.
            if (burnableObject == null)
                continue;

            // 이미 처리한 오브젝트라면 중복 점화를 막는다.
            if (!processedBurnableObjects.Add(burnableObject))
                continue;

            /*
             * WorldObject의 속성까지 확인한다.
             *
             * BurnableObject와 WorldObject가 같은 오브젝트에 있거나,
             * 부모 계층에 있을 수 있으므로 부모 방향으로 검색한다.
             */
            WorldObject worldObject =
                burnableObject.GetComponentInParent<WorldObject>();

            if (worldObject == null)
            {
                Debug.LogWarning(
                    $"[Thermal Ignition] " +
                    $"{burnableObject.name}에 WorldObject가 없습니다.",
                    burnableObject
                );

                continue;
            }

            // Flammable 속성이 없는 오브젝트는 점화하지 않는다.
            if (!worldObject.HasProperty(ObjectProperty.Flammable))
                continue;

            // 젖은 오브젝트는 현재 규칙상 점화할 수 없다.
            if (worldObject.HasState(ObjectState.Wet))
                continue;

            // 실제 VFX 생성과 중복 점화 처리는 BurnableObject가 담당한다.
            bool ignitionSucceeded =
                burnableObject.TryIgnite();

            if (ignitionSucceeded)
            {
                ignitedCount++;
            }
        }

        Debug.Log(
            $"[Thermal Ignition] 실행 완료 | " +
            $"감지 Collider: {detectedColliders.Length}, " +
            $"점화 성공: {ignitedCount}",
            this
        );
    }


    /// <summary>
    /// Scene 뷰에서 마지막 설정값을 보는 용도가 아니라,
    /// 선택된 오브젝트 위치를 기준으로 검사 형태를 확인하기 위한 Gizmo다.
    ///
    /// 실제 실행 중심은 외부에서 전달되므로
    /// 이 Gizmo는 참고용이다.
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(
            transform.position,
            1f
        );
    }
}