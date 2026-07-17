using UnityEngine;
using UnityEngine.InputSystem;

public class ObjectHighlighter : MonoBehaviour
{
    [Header("Outline")]
    public Outline outline;

    [Header("Colors")]
    public Color scanColor = Color.cyan;
    public Color lockedColor = Color.yellow;

    [Header("Width")]
    public float scanWidth = 3f;
    public float lockedWidth = 5f;

    [Header("Debug Test")]
    public bool enableDebugKeys = false;

    private HighlightState currentState = HighlightState.None;

    private void Awake()
    {
        if (outline == null)
        {
            outline = GetComponent<Outline>();
        }

        if (outline == null)
        {
            Debug.LogWarning(gameObject.name + " : Outline component not found.");
        }
    }

    private void Start()
    {
        SetHighlight(HighlightState.None, true);
    }

    public void SetHighlight(HighlightState state)
    {
        SetHighlight(state, false);
    }

    public void SetHighlight(HighlightState state, bool forceUpdate)
    {
        if (outline == null)
        {
            Debug.LogWarning(gameObject.name + " has no Outline component.");
            return;
        }

        if (!forceUpdate && currentState == state)
            return;

        currentState = state;

        if (state == HighlightState.None)
        {
            outline.enabled = false;
            return;
        }

        // Quick Outline은 enabled 되는 순간 내부 Material을 붙이므로,
        // 먼저 켜고 그 다음 색/두께를 확실히 다시 넣는다.
        outline.enabled = true;
        outline.OutlineMode = Outline.Mode.OutlineAll;

        if (state == HighlightState.Scan)
        {
            outline.OutlineColor = scanColor;
            outline.OutlineWidth = scanWidth;
        }
        else if (state == HighlightState.Locked)
        {
            outline.OutlineColor = lockedColor;
            outline.OutlineWidth = lockedWidth;
        }
    }

    private void Update()
    {
        if (!enableDebugKeys)
            return;

        if (Keyboard.current == null)
            return;

        if (Keyboard.current.digit7Key.wasPressedThisFrame || Keyboard.current.numpad7Key.wasPressedThisFrame)
        {
            SetHighlight(HighlightState.Scan, true);
        }

        if (Keyboard.current.digit8Key.wasPressedThisFrame || Keyboard.current.numpad8Key.wasPressedThisFrame)
        {
            SetHighlight(HighlightState.Locked, true);
        }

        if (Keyboard.current.digit9Key.wasPressedThisFrame || Keyboard.current.numpad9Key.wasPressedThisFrame)
        {
            SetHighlight(HighlightState.None, true);
        }
    }
}