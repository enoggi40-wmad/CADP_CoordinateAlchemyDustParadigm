using UnityEngine;

public class SmartDustPreviewController : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private SmartDustDeploymentController deploymentController;

    [SerializeField]
    private GameObject visualObject;

    [Header("State")]
    [SerializeField]
    private bool showPreview;

    private void Start()
    {
        ApplyVisibility();
    }

    private void Update()
    {
        if (!showPreview)
            return;

        if (deploymentController == null)
            return;

        UpdatePreviewTransform();
    }

    public void SetVisible(bool visible)
    {
        showPreview = visible;
        ApplyVisibility();

        if (visible)
        {
            UpdatePreviewTransform();
        }
    }

    private void ApplyVisibility()
    {
        if (visualObject != null)
        {
            visualObject.SetActive(showPreview);
        }
    }

    private void UpdatePreviewTransform()
    {
        if (deploymentController == null)
            return;

        transform.position =
            deploymentController.WorldCenter;

        float diameter =
            deploymentController.Radius * 2f;

        transform.localScale =
            Vector3.one * diameter;
    }
}