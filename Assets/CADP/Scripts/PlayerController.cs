using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 3f;
    public float runSpeed = 6f;

    [Header("Mouse Look")]
    public float mouseSensitivity = 120f;
    public Transform cameraPivot;
    public float minPitch = -30f;
    public float maxPitch = 45f;

    [Header("Jump")]
    public float jumpHeight = 1.5f;
    public float gravity = -9.81f;

    [Header("Animation")]
    public Animator animator;
    public float animationDampTime = 0.1f;

    private CharacterController controller;
    private Vector3 velocity;

    private float pitch = 0f;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        RotateByMouse();
        Move();
    }

    private void RotateByMouse()
    {
        if (Mouse.current == null)
            return;

        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        float mouseX = mouseDelta.x * mouseSensitivity * Time.deltaTime;
        float mouseY = mouseDelta.y * mouseSensitivity * Time.deltaTime;

        // 좌우 회전은 Player 본체
        transform.Rotate(Vector3.up * mouseX);

        // 상하 회전은 CameraPivot만
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        if (cameraPivot != null)
        {
            cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }
    }

    private void Move()
    {
        bool isGrounded = controller.isGrounded;

        if (isGrounded && velocity.y < 0f)
        {
            velocity.y = -2f;
        }

        Vector2 input = GetMoveInput();

        bool hasMoveInput = input != Vector2.zero;
        bool isRunning = hasMoveInput && Keyboard.current.leftShiftKey.isPressed;

        float currentSpeed = isRunning ? runSpeed : walkSpeed;

        Vector3 moveDir =
            transform.right * input.x +
            transform.forward * input.y;

        moveDir.Normalize();

        if (Keyboard.current.spaceKey.wasPressedThisFrame && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

            if (animator != null)
                animator.SetTrigger("Jump");
        }

        velocity.y += gravity * Time.deltaTime;

        Vector3 finalMove = moveDir * currentSpeed;
        finalMove.y = velocity.y;

        controller.Move(finalMove * Time.deltaTime);

        UpdateAnimation(input, isRunning);
    }

    private Vector2 GetMoveInput()
    {
        Vector2 input = Vector2.zero;

        if (Keyboard.current.wKey.isPressed)
            input.y += 1f;

        if (Keyboard.current.sKey.isPressed)
            input.y -= 1f;

        if (Keyboard.current.aKey.isPressed)
            input.x -= 1f;

        if (Keyboard.current.dKey.isPressed)
            input.x += 1f;

        return input.normalized;
    }

    private void UpdateAnimation(Vector2 input, bool isRunning)
    {
        if (animator == null)
            return;

        float animMultiplier = isRunning ? 1f : 0.5f;

        float targetMoveX = input.x * animMultiplier;
        float targetMoveY = input.y * animMultiplier;

        animator.SetFloat("MoveX", targetMoveX, animationDampTime, Time.deltaTime);
        animator.SetFloat("MoveY", targetMoveY, animationDampTime, Time.deltaTime);
    }
}