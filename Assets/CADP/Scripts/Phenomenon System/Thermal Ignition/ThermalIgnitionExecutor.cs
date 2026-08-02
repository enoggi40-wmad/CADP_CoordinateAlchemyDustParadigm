using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Thermal Ignition 현상을 담당한다.
///
/// 범위 탐색은 PhenomenonExecutor.cs에서 이미 끝난 상태이며,
/// 이 클래스는 전달받은 WorldObject 중
/// 실제 점화 가능한 대상만 처리한다.
/// </summary>
public class ThermalIgnitionExecutor : MonoBehaviour
{
    [Header("References")]

    [Tooltip("생성할 Thermal Ignition Field Prefab")]
    [SerializeField]
    private ThermalIgnitionField thermalIgnitionFieldPrefab;


    /// <summary>
    /// Smart Dust 범위 안의 WorldObject들에게
    /// Thermal Ignition을 적용한다.
    /// </summary>
    public void Execute(
        Vector3 center,
        float radius,
        IReadOnlyList<WorldObject> targets)
    {
        /*
         * Thermal Ignition Field는
         * 현상의 시각적 표현용으로 생성한다.
         */
        CreateField(
            center,
            radius
        );


        if (targets == null ||
            targets.Count == 0)
        {
            Debug.Log(
                "[Thermal Ignition] 범위 안에 적용 가능한 WorldObject가 없습니다.",
                this
            );

            return;
        }


        foreach (WorldObject worldObject in targets)
        {
            if (worldObject == null)
                continue;


            TryIgniteWorldObject(
                worldObject
            );
        }
    }


    /// <summary>
    /// Thermal Ignition Field를 생성한다.
    /// </summary>
    private void CreateField(
        Vector3 center,
        float radius)
    {
        if (thermalIgnitionFieldPrefab == null)
        {
            Debug.LogWarning(
                "[Thermal Ignition] Field Prefab이 연결되지 않았습니다.",
                this
            );

            return;
        }


        ThermalIgnitionField createdField =
            Instantiate(
                thermalIgnitionFieldPrefab,
                center,
                Quaternion.identity
            );


        createdField.Initialize(
            center,
            radius
        );
    }


    /// <summary>
    /// 하나의 WorldObject에 점화를 시도한다.
    /// </summary>
    private void TryIgniteWorldObject(
        WorldObject worldObject)
    {
        /*
         * 불에 탈 수 있는 속성이 없는 오브젝트는 무시한다.
         */
        if (!worldObject.HasProperty(
                ObjectProperty.Flammable))
        {
            return;
        }


        /*
         * 젖어 있는 오브젝트는 현재 규칙상
         * 점화되지 않는다.
         */
        if (worldObject.HasState(
                ObjectState.Wet))
        {
            Debug.Log(
                $"[Thermal Ignition] {worldObject.name}은 Wet 상태라 점화되지 않습니다.",
                worldObject
            );

            return;
        }


        /*
         * WorldObject와 같은 오브젝트에 붙어 있는
         * BurnableObject를 찾는다.
         */
        BurnableObject burnableObject =
            worldObject.GetComponent<BurnableObject>();


        /*
         * 구조에 따라 BurnableObject가
         * 자식에 있을 경우까지 대응한다.
         */
        if (burnableObject == null)
        {
            burnableObject =
                worldObject
                    .GetComponentInChildren<BurnableObject>();
        }


        if (burnableObject == null)
        {
            Debug.LogWarning(
                $"[Thermal Ignition] {worldObject.name}은 Flammable이지만 BurnableObject.cs가 없습니다.",
                worldObject
            );

            return;
        }


        bool ignited =
            burnableObject.TryIgnite();


        if (ignited)
        {
            Debug.Log(
                $"[Thermal Ignition] 점화 성공: {worldObject.name}",
                worldObject
            );
        }
    }
}