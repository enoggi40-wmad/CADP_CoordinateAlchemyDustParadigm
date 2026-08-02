using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

public class BurnableObject : MonoBehaviour
{
    [Header("State Transition")]

    [SerializeField]
    private GameObject nextStatePrefab;

    [SerializeField]
    private float transitionDelay = 3f;

    [SerializeField]
    private bool autoStartTransition = false;


    [Header("State Alignment")]

    [Tooltip("다음 상태 오브젝트 생성 후 Y축으로 추가 보정할 높이")]
    [SerializeField]
    private float nextStateSpawnHeightOffset = 0f;


    [Header("Fire VFX")]

    [SerializeField]
    private GameObject fireVfxPrefab;

    [SerializeField]
    private Transform fireVfxPoint;

    [SerializeField]
    private bool stopFireVfxOnTransition = false;


    [Header("Runtime State")]

    [SerializeField]
    private bool isIgnited = false;

    [SerializeField]
    private bool isTransitionRunning = false;


    private GameObject activeFireVfx;


    public bool IsIgnited
    {
        get
        {
            return isIgnited;
        }
    }


    private void Start()
    {
        if (autoStartTransition)
        {
            StartTransitionCountdown();
        }
    }


    public void ReceiveHeat()
    {
        if (isIgnited)
            return;

        TryIgnite();
    }


    public bool TryIgnite()
    {
        if (isIgnited)
            return false;

        if (nextStatePrefab == null)
        {
            Debug.LogWarning(
                $"[BurnableObject] {name}에 Next State Prefab이 없습니다.",
                this
            );

            return false;
        }

        isIgnited = true;

        StartFireVfx();
        StartTransitionCountdown();

        Debug.Log(
            $"[BurnableObject] {name} 점화 시작.",
            this
        );

        return true;
    }


    private void StartFireVfx()
    {
        if (activeFireVfx != null)
            return;

        if (fireVfxPrefab == null)
        {
            Debug.LogWarning(
                $"[BurnableObject] {name}: Fire VFX Prefab이 없습니다.",
                this
            );

            return;
        }

        if (fireVfxPoint == null)
        {
            Debug.LogWarning(
                $"[BurnableObject] {name}: Fire VFX Point가 없습니다.",
                this
            );

            return;
        }

        activeFireVfx =
            Instantiate(
                fireVfxPrefab,
                fireVfxPoint.position,
                fireVfxPoint.rotation,
                fireVfxPoint
            );

        activeFireVfx.transform.localPosition =
            Vector3.zero;

        activeFireVfx.transform.localRotation =
            Quaternion.identity;
    }


    private void StartTransitionCountdown()
    {
        if (isTransitionRunning)
            return;

        if (nextStatePrefab == null)
            return;

        isTransitionRunning = true;

        StartCoroutine(
            TransitionRoutine()
        );
    }


    private IEnumerator TransitionRoutine()
    {
        yield return new WaitForSeconds(
            transitionDelay
        );

        ReplaceWithNextState();
    }


    private void ReplaceWithNextState()
    {
        if (nextStatePrefab == null)
            return;

        Transform currentModel = transform;
        Transform currentRoot = currentModel.parent;

        if (currentRoot == null)
            return;

        Transform rootParent =
            currentRoot.parent;


        // 현재 상태의 충격 손상 비율 저장
        ImpactDurability currentImpactDurability =
            currentRoot.GetComponent<ImpactDurability>();

        float inheritedDamageRatio = 0f;

        if (currentImpactDurability != null)
        {
            inheritedDamageRatio =
                currentImpactDurability.DamageRatio;
        }


        Bounds currentBounds =
            GetCombinedRendererBounds(
                currentRoot.gameObject
            );


        if (activeFireVfx != null &&
            !stopFireVfxOnTransition)
        {
            activeFireVfx.transform.SetParent(
                null,
                true
            );
        }


        GameObject nextStateObject =
            Instantiate(
                nextStatePrefab,
                currentRoot.position,
                currentRoot.rotation,
                rootParent
            );


        nextStateObject.transform.localScale =
            currentRoot.localScale;


        Bounds nextBounds =
            GetCombinedRendererBounds(
                nextStateObject
            );


        Vector3 currentAnchor =
            new Vector3(
                currentBounds.center.x,
                currentBounds.min.y,
                currentBounds.center.z
            );

        Vector3 nextAnchor =
            new Vector3(
                nextBounds.center.x,
                nextBounds.min.y,
                nextBounds.center.z
            );


        Vector3 correction =
            currentAnchor - nextAnchor;

        correction.y +=
            nextStateSpawnHeightOffset;

        nextStateObject.transform.position +=
            correction;


        // 다음 상태에 기존 손상 비율 계승
        ImpactDurability nextImpactDurability =
            nextStateObject.GetComponent<ImpactDurability>();

        if (nextImpactDurability != null)
        {
            nextImpactDurability.SetDamageRatio(
                inheritedDamageRatio
            );

            Debug.Log(
                $"[BurnableObject] 충격 손상률 계승: " +
                $"{inheritedDamageRatio:P0}",
                nextImpactDurability
            );
        }


        // 기존 Fire VFX 인계 코드
        if (activeFireVfx != null &&
            !stopFireVfxOnTransition)
        {
            BurnableObject nextBurnable =
                nextStateObject
                    .GetComponentInChildren<BurnableObject>();

            if (nextBurnable != null)
            {
                nextBurnable.AdoptFireVfx(
                    activeFireVfx
                );

                activeFireVfx = null;
            }
        }


        if (activeFireVfx != null &&
            stopFireVfxOnTransition)
        {
            Destroy(activeFireVfx);
            activeFireVfx = null;
        }


        Destroy(
            currentRoot.gameObject
        );
    }


    private Bounds GetCombinedRendererBounds(
        GameObject targetObject)
    {
        MeshRenderer[] meshRenderers =
            targetObject
                .GetComponentsInChildren<MeshRenderer>();


        if (meshRenderers.Length == 0)
        {
            Debug.LogWarning(
                $"[BurnableObject] {targetObject.name}에서 MeshRenderer를 찾을 수 없습니다.",
                targetObject
            );

            return new Bounds(
                targetObject.transform.position,
                Vector3.zero
            );
        }


        Bounds combinedBounds =
            meshRenderers[0].bounds;


        for (int i = 1;
             i < meshRenderers.Length;
             i++)
        {
            combinedBounds.Encapsulate(
                meshRenderers[i].bounds
            );
        }


        return combinedBounds;
    }


    public void AdoptFireVfx(
        GameObject fireVfx)
    {
        if (fireVfx == null)
            return;

        if (fireVfxPoint == null)
            return;


        activeFireVfx = fireVfx;
        isIgnited = true;


        activeFireVfx.transform.SetParent(
            fireVfxPoint,
            true
        );


        activeFireVfx.transform.position =
            fireVfxPoint.position;

        activeFireVfx.transform.rotation =
            fireVfxPoint.rotation;


        VisualEffect visualEffect =
            activeFireVfx
                .GetComponentInChildren<VisualEffect>();


        if (visualEffect != null)
        {
            visualEffect.Play();
        }
    }
}