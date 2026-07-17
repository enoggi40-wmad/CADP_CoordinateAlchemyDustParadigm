using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;

public class ScanLockOnSystem : MonoBehaviour
{
    [Header("Scan")]
    public Camera playerCamera;
    public float scanDistance = 12f;
    public LayerMask scannableLayer;

    [Header("UI")]
    public TMP_Text targetText;
    public Image crosshairImage;

    [Header("Crosshair Colors")]
    public Color normalColor = new Color(1f, 1f, 1f, 0.6f);
    public Color scanColor = new Color(0f, 1f, 1f, 0.9f);
    public Color lockedColor = new Color(1f, 0.85f, 0.1f, 1f);

    private Transform currentTarget;
    private IScannable currentScannable;
    private ObjectHighlighter currentHighlighter;

    private Transform lockedTarget;
    private IScannable lockedScannable;
    private ObjectHighlighter lockedHighlighter;

    private ObjectHighlighter previousScanHighlighter;

    public Transform LockedTarget => lockedTarget;

    private void Start()
    {
        ShowNormalUI();
    }

    private void Update()
    {
        Scan();

        if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
        {
            LockOn();
        }

        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            ClearLockOn();
        }
    }

    private void Scan()
    {
        currentTarget = null;
        currentScannable = null;
        currentHighlighter = null;

        if (playerCamera == null)
            return;

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        bool foundScannable = false;

        if (Physics.Raycast(ray, out RaycastHit hit, scanDistance, scannableLayer))
        {
            IScannable scannable = hit.collider.GetComponentInParent<IScannable>();

            if (scannable != null)
            {
                foundScannable = true;

                MonoBehaviour scannableBehaviour = scannable as MonoBehaviour;

                if (scannableBehaviour != null)
                {
                    currentTarget = scannableBehaviour.transform;
                }
                else
                {
                    currentTarget = hit.collider.transform;
                }

                currentScannable = scannable;
                currentHighlighter = hit.collider.GetComponentInParent<ObjectHighlighter>();

                HandleHighlight(scannable.GetScanName());

                Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.cyan);
            }
        }

        if (!foundScannable)
        {
            HandleNoScanTarget();
            Debug.DrawRay(ray.origin, ray.direction * scanDistance, Color.white);
        }
    }

    private void HandleHighlight(string scanName)
    {
        bool isLockedObject = lockedHighlighter != null && lockedHighlighter == currentHighlighter;

        // 이전 Scan 대상과 현재 Scan 대상이 다를 때만 이전 Scan 하이라이트를 끈다.
        if (previousScanHighlighter != null &&
            previousScanHighlighter != currentHighlighter &&
            previousScanHighlighter != lockedHighlighter)
        {
            previousScanHighlighter.SetHighlight(HighlightState.None);
        }

        if (isLockedObject)
        {
            ShowLockedUI(scanName);

            if (currentHighlighter != null)
            {
                currentHighlighter.SetHighlight(HighlightState.Locked);
            }
        }
        else
        {
            ShowScanUI(scanName);

            if (currentHighlighter != null)
            {
                currentHighlighter.SetHighlight(HighlightState.Scan);
                previousScanHighlighter = currentHighlighter;
            }
        }
    }

    private void HandleNoScanTarget()
    {
        // 아무것도 보고 있지 않을 때,
        // 이전 Scan 대상이 Lock 대상이 아니라면 Scan 하이라이트를 끈다.
        if (previousScanHighlighter != null && previousScanHighlighter != lockedHighlighter)
        {
            previousScanHighlighter.SetHighlight(HighlightState.None);
            previousScanHighlighter = null;
        }

        if (lockedTarget != null && lockedScannable != null)
        {
            ShowLockedUI(lockedScannable.GetScanName());
        }
        else
        {
            ShowNormalUI();
        }
    }

    private void LockOn()
    {
        if (currentTarget == null || currentScannable == null)
        {
            Debug.Log("Lock-on failed: No scannable target.");
            return;
        }

        // 기존 Lock 대상이 있으면 끈다.
        if (lockedHighlighter != null)
        {
            lockedHighlighter.SetHighlight(HighlightState.None, true);
        }

        lockedTarget = currentTarget;
        lockedScannable = currentScannable;
        lockedHighlighter = currentHighlighter;

        // Lock 대상은 더 이상 Scan 대상이 아니라 Locked 대상이다.
        previousScanHighlighter = null;

        if (lockedHighlighter != null)
        {
            lockedHighlighter.SetHighlight(HighlightState.Locked, true);
        }

        ShowLockedUI(lockedScannable.GetScanName());

        Debug.Log("Lock-on: " + lockedScannable.GetScanName());
    }

    private void ClearLockOn()
    {
        if (lockedHighlighter != null)
        {
            lockedHighlighter.SetHighlight(HighlightState.None, true);
        }

        lockedTarget = null;
        lockedScannable = null;
        lockedHighlighter = null;
        previousScanHighlighter = null;

        ShowNormalUI();

        Debug.Log("Lock-on cleared.");
    }

    private void ShowNormalUI()
    {
        if (targetText != null)
            targetText.text = "";

        if (crosshairImage != null)
            crosshairImage.color = normalColor;
    }

    private void ShowScanUI(string targetName)
    {
        if (targetText != null)
            targetText.text = "Scan: " + targetName;

        if (crosshairImage != null)
            crosshairImage.color = scanColor;
    }

    private void ShowLockedUI(string targetName)
    {
        if (targetText != null)
            targetText.text = "Locked: " + targetName;

        if (crosshairImage != null)
            crosshairImage.color = lockedColor;
    }

    public Vector3 GetLockedPosition()
    {
        if (lockedTarget != null)
            return lockedTarget.position;

        return playerCamera.transform.position + playerCamera.transform.forward * 5f;
    }

    public bool HasLockedTarget()
    {
        return lockedTarget != null;
    }
}