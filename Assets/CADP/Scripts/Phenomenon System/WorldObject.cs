using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 월드 오브젝트의 고정 속성과
/// 현재 상태를 저장하고 제공한다.
///
/// 현상 실행 로직은 각 Phenomenon Executor가 담당한다.
/// </summary>
public class WorldObject : MonoBehaviour
{
    [Header("Object Properties")]

    [SerializeField]
    private List<ObjectProperty> properties = new();


    [Header("Current States")]

    [SerializeField]
    private List<ObjectState> currentStates = new();


    /// <summary>
    /// 특정 속성을 가지고 있는지 확인한다.
    /// </summary>
    public bool HasProperty(
        ObjectProperty property)
    {
        return properties.Contains(
            property
        );
    }


    /// <summary>
    /// 현재 특정 상태인지 확인한다.
    /// </summary>
    public bool HasState(
        ObjectState state)
    {
        return currentStates.Contains(
            state
        );
    }


    /// <summary>
    /// 상태를 추가한다.
    /// </summary>
    public void AddState(
        ObjectState state)
    {
        if (currentStates.Contains(state))
            return;


        currentStates.Add(
            state
        );


        Debug.Log(
            $"[WorldObject] {name}: {state} 상태 추가",
            this
        );
    }


    /// <summary>
    /// 상태를 제거한다.
    /// </summary>
    public void RemoveState(
        ObjectState state)
    {
        if (!currentStates.Contains(state))
            return;


        currentStates.Remove(
            state
        );


        Debug.Log(
            $"[WorldObject] {name}: {state} 상태 제거",
            this
        );
    }
}