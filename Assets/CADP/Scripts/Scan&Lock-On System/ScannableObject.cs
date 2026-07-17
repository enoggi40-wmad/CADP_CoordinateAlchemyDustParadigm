using UnityEngine;

public class ScannableObject : MonoBehaviour, IScannable
{
    [Header("Scan Info")]
    public string scanName = "Unknown Object";

    public string GetScanName()
    {
        return scanName;
    }
}