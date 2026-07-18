using UnityEngine;

public enum CoordinateOriginMode
{
    Player,
    LockedObject
}

public enum PhenomenonType
{
    None,
    MoistureCondensation,
    AirPressureGradient,
    ThermalIgnition,
    ElectricDischarge,
    HeatExtraction
}

public class SmartDustDeploymentController : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private Transform playerTransform;

    [SerializeField]
    private ScanLockOnSystem scanLockOnSystem;

    [Header("Coordinate Settings")]
    [SerializeField]
    private CoordinateOriginMode originMode =
        CoordinateOriginMode.Player;

    [SerializeField]
    private Vector3 coordinateOffset = new Vector3(0f, 1f, 3f);

    [Min(0.1f)]
    [SerializeField]
    private float radius = 2f;

    [Header("Phenomenon")]
    [SerializeField]
    private PhenomenonType selectedPhenomenon =
        PhenomenonType.None;

    public CoordinateOriginMode OriginMode => originMode;
    public Vector3 CoordinateOffset => coordinateOffset;
    public float Radius => radius;
    public PhenomenonType SelectedPhenomenon => selectedPhenomenon;

    public Vector3 WorldCenter
    {
        get
        {
            if (originMode == CoordinateOriginMode.LockedObject)
            {
                if (HasValidLockedTarget())
                {
                    return scanLockOnSystem.LockedTarget.position
                           + coordinateOffset;
                }
            }

            return GetPlayerRelativePosition();
        }
    }

    public bool HasValidLockedTarget()
    {
        return scanLockOnSystem != null
               && scanLockOnSystem.HasLockedTarget()
               && scanLockOnSystem.LockedTarget != null;
    }

    public void SetOriginMode(CoordinateOriginMode newMode)
    {
        if (newMode == CoordinateOriginMode.LockedObject
            && !HasValidLockedTarget())
        {
            Debug.LogWarning(
                "Lock-On 대상이 없어 LockedObject 기준으로 변경할 수 없습니다.",
                this
            );

            return;
        }

        originMode = newMode;

        Debug.Log($"Coordinate Origin: {originMode}", this);
    }

    public bool TryToggleOriginMode()
    {
        if (originMode == CoordinateOriginMode.Player)
        {
            if (!HasValidLockedTarget())
            {
                Debug.LogWarning(
                    "Lock-On 대상이 없어 Player 기준을 유지합니다.",
                    this
                );

                return false;
            }

            SetOriginMode(CoordinateOriginMode.LockedObject);
        }
        else
        {
            SetOriginMode(CoordinateOriginMode.Player);
        }

        return true;
    }

    public void SetCoordinateOffset(Vector3 newOffset)
    {
        coordinateOffset = newOffset;
    }

    public void SetRadius(float newRadius)
    {
        radius = Mathf.Max(0.1f, newRadius);
    }

    public void SetSelectedPhenomenon(
        PhenomenonType newPhenomenon)
    {
        selectedPhenomenon = newPhenomenon;

        Debug.Log(
            $"Selected Phenomenon: {selectedPhenomenon}",
            this
        );
    }

    private Vector3 GetPlayerRelativePosition()
    {
        if (playerTransform == null)
        {
            Debug.LogError(
                "SmartDustDeploymentController에 Player Transform이 연결되지 않았습니다.",
                this
            );

            return transform.position;
        }

        return playerTransform.TransformPoint(coordinateOffset);
    }

    private void OnDrawGizmosSelected()
    {
        if (playerTransform == null)
            return;

        Gizmos.DrawWireSphere(WorldCenter, radius);
    }
}