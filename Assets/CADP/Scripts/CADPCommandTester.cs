using UnityEngine;
using UnityEngine.InputSystem;

public class CADPCommandTester : MonoBehaviour
{
    private void Start()
    {
        Debug.Log("CADP System Initialized");
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.enterKey.wasPressedThisFrame)
        {
            Debug.Log("CADP Command Test: Enter key pressed");
        }
    }
}