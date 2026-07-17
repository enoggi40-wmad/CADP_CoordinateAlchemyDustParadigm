using System.Collections.Generic;
using UnityEngine;

public class WorldObject : MonoBehaviour
{
    [Header("Object Properties")]
    [SerializeField]
    private List<ObjectProperty> properties = new();

    [Header("Current States")]
    [SerializeField]
    private List<ObjectState> currentStates = new();

    public bool HasProperty(ObjectProperty property)
    {
        return properties.Contains(property);
    }

    public bool HasState(ObjectState state)
    {
        return currentStates.Contains(state);
    }

    public void AddState(ObjectState state)
    {
        if (currentStates.Contains(state))
        {
            return;
        }

        currentStates.Add(state);

        Debug.Log($"{name}: {state} 상태 추가");
    }

    public void RemoveState(ObjectState state)
    {
        if (!currentStates.Contains(state))
        {
            return;
        }

        currentStates.Remove(state);

        Debug.Log($"{name}: {state} 상태 제거");
    }
}