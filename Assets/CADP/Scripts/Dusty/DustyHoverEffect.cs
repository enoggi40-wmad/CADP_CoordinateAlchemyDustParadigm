using UnityEngine;

/// <summary>
/// Dusty의 외형을 위아래로 천천히 움직여
/// 공중에 떠 있는 느낌을 만든다.
///
/// 실제 이동을 담당하는 SmartDustCarrier 루트가 아니라
/// 자식 오브젝트인 VisualRoot에 붙여야 한다.
/// </summary>
public class DustyHoverEffect : MonoBehaviour
{
    [Header("Hover Settings")]

    [Tooltip("기준 위치에서 위아래로 움직이는 최대 거리")]
    [SerializeField]
    private float hoverHeight = 0.12f;

    [Tooltip("위아래 움직임의 속도")]
    [SerializeField]
    private float hoverSpeed = 1.5f;


    // 게임 시작 시 VisualRoot의 원래 로컬 위치
    private Vector3 initialLocalPosition;


    private void Awake()
    {
        // 현재 위치를 부유 움직임의 기준점으로 저장한다.
        initialLocalPosition = transform.localPosition;
    }


    private void Update()
    {
        /*
         * Sin 값을 사용하면 -1부터 1까지
         * 부드럽게 반복되는 움직임을 만들 수 있다.
         */
        float verticalOffset =
            Mathf.Sin(Time.time * hoverSpeed) *
            hoverHeight;

        // 원래 위치에서 Y축만 위아래로 움직인다.
        transform.localPosition =
            initialLocalPosition +
            Vector3.up * verticalOffset;
    }
}