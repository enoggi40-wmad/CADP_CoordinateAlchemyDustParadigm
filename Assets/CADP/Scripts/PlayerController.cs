using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]

    // 평상시 걷는 속도
    [SerializeField]
    private float walkSpeed = 3f;

    // 왼쪽 Shift를 누르고 이동할 때의 달리기 속도
    [SerializeField]
    private float runSpeed = 6f;

    [Header("Jump")]

    // 플레이어가 점프할 수 있는 높이
    [SerializeField]
    private float jumpHeight = 1.5f;

    // 플레이어에게 적용되는 중력값
    // 아래 방향이므로 음수 값을 사용한다.
    [SerializeField]
    private float gravity = -9.81f;

    [Header("Animation")]

    // 플레이어 캐릭터의 Animator
    [SerializeField]
    private Animator animator;

    // 애니메이션 파라미터가 부드럽게 변경되는 시간
    [SerializeField]
    private float animationDampTime = 0.1f;

    // 플레이어 이동을 담당하는 CharacterController
    private CharacterController characterController;

    // 점프와 중력 계산에 사용하는 수직 속도
    private Vector3 velocity;

    private void Awake()
    {
        // 같은 Player 오브젝트에 붙어 있는
        // CharacterController를 가져온다.
        characterController =
            GetComponent<CharacterController>();
    }

    private void Update()
    {
        // 매 프레임 플레이어 이동과 점프를 처리한다.
        Move();
    }

    private void Move()
    {
        // CharacterController가 바닥에 닿아 있는지 확인한다.
        bool isGrounded =
            characterController.isGrounded;

        // 바닥에 닿아 있는데 아래 방향 속도가 남아 있다면
        // 약한 음수값을 유지해 지면에 안정적으로 붙도록 한다.
        if (isGrounded && velocity.y < 0f)
        {
            velocity.y = -2f;
        }

        // 키보드의 WASD 입력값을 가져온다.
        Vector2 moveInput = GetMoveInput();

        // 이동 입력이 실제로 들어오고 있는지 확인한다.
        bool hasMoveInput =
            moveInput.sqrMagnitude > 0f;

        // 이동 중 왼쪽 Shift를 누르면 달리기 상태가 된다.
        bool isRunning =
            hasMoveInput &&
            Keyboard.current != null &&
            Keyboard.current.leftShiftKey.isPressed;

        // 현재 상태에 따라 걷기 또는 달리기 속도를 선택한다.
        float currentSpeed =
            isRunning
                ? runSpeed
                : walkSpeed;

        // 플레이어가 바라보는 방향을 기준으로
        // 앞뒤와 좌우 이동 방향을 계산한다.
        Vector3 moveDirection =
            transform.right * moveInput.x +
            transform.forward * moveInput.y;

        // 대각선 이동 속도가 더 빨라지지 않도록 정규화한다.
        if (moveDirection.sqrMagnitude > 1f)
        {
            moveDirection.Normalize();
        }

        // 점프 입력과 바닥 상태를 확인한다.
        if (Keyboard.current != null &&
            Keyboard.current.spaceKey.wasPressedThisFrame &&
            isGrounded)
        {
            // 원하는 점프 높이에 필요한 초기 수직 속도를 계산한다.
            velocity.y =
                Mathf.Sqrt(
                    jumpHeight * -2f * gravity
                );

            // Animator가 연결되어 있다면
            // Jump Trigger를 실행한다.
            if (animator != null)
            {
                animator.SetTrigger("Jump");
            }
        }

        // 매 프레임 수직 속도에 중력을 누적한다.
        velocity.y += gravity * Time.deltaTime;

        // 수평 이동 속도를 계산한다.
        Vector3 finalMove =
            moveDirection * currentSpeed;

        // 계산된 수직 속도를 최종 이동값에 넣는다.
        finalMove.y = velocity.y;

        // CharacterController를 사용해 실제로 이동한다.
        characterController.Move(
            finalMove * Time.deltaTime
        );

        // 현재 이동 입력을 Animator에 전달한다.
        UpdateAnimation(
            moveInput,
            isRunning
        );
    }

    private Vector2 GetMoveInput()
    {
        // 기본 이동 입력값
        Vector2 input = Vector2.zero;

        // 키보드 장치가 없다면 입력 없이 반환한다.
        if (Keyboard.current == null)
            return input;

        // W 키는 앞으로 이동
        if (Keyboard.current.wKey.isPressed)
        {
            input.y += 1f;
        }

        // S 키는 뒤로 이동
        if (Keyboard.current.sKey.isPressed)
        {
            input.y -= 1f;
        }

        // A 키는 왼쪽으로 이동
        if (Keyboard.current.aKey.isPressed)
        {
            input.x -= 1f;
        }

        // D 키는 오른쪽으로 이동
        if (Keyboard.current.dKey.isPressed)
        {
            input.x += 1f;
        }

        // 대각선 입력이 더 강해지지 않도록 정규화한다.
        return input.normalized;
    }

    private void UpdateAnimation(
        Vector2 moveInput,
        bool isRunning)
    {
        // Animator가 연결되지 않았다면 실행하지 않는다.
        if (animator == null)
            return;

        // 달릴 때는 입력값을 그대로 사용하고,
        // 걸을 때는 절반값을 사용한다.
        float animationMultiplier =
            isRunning
                ? 1f
                : 0.5f;

        // Blend Tree에 전달할 좌우 이동값
        float targetMoveX =
            moveInput.x * animationMultiplier;

        // Blend Tree에 전달할 앞뒤 이동값
        float targetMoveY =
            moveInput.y * animationMultiplier;

        // MoveX 값을 부드럽게 변경한다.
        animator.SetFloat(
            "MoveX",
            targetMoveX,
            animationDampTime,
            Time.deltaTime
        );

        // MoveY 값을 부드럽게 변경한다.
        animator.SetFloat(
            "MoveY",
            targetMoveY,
            animationDampTime,
            Time.deltaTime
        );
    }
}