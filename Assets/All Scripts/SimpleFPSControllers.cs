using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class SimpleFPSControllers : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 5f;
    private float gravity = -9.8f;
    public float jumpHeight = 0.4f;

    [Header("Mouse")]
    public float mouseSensitivity = 1.7f;

    [Header("Jump Buffer (seconds)")]
    [SerializeField] private float jumpBufferTime = 0.1f;

    [Header("Crouch")]
    public float crouchHeight = 1f;
    public float standingHeight = 1.8f;
    public float crouchSpeed = 2f;
    public float crouchTransitionSpeed = 10f;

    private Transform playerCamera;
    private CharacterController controller;
    private float xRotation = 0f;
    private Vector3 playerVelocity;

    [Header("Animator")]
    [SerializeField] private Animator animator;

    // Состояния
    private bool isDead = false;
    private bool isGrounded;
    private bool isAttacking = false;

    private Vector2 currentMoveInput;
    private Vector2 currentLookDelta;

    private float jumpBufferCounter = 0f;
    private bool jumpConsumed = true;

    // Приседание
    private bool isCrouching = false;
    private float originalHeight;
    private float originalSpeed;
    private float targetHeight;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        playerCamera = GetComponentInChildren<Camera>().transform;
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Сохраняем оригинальные значения
        originalHeight = standingHeight;
        originalSpeed = speed;
        targetHeight = standingHeight;

        if (controller != null)
        {
            controller.height = standingHeight;
        }
    }

    public void SetInput(Vector2 moveInput, Vector2 lookDelta, bool jumpPressed)
    {
        currentLookDelta = lookDelta;
        currentMoveInput = moveInput;

        if (jumpPressed && !jumpConsumed)
        {
            jumpBufferCounter = jumpBufferTime;
            jumpConsumed = true;
        }
        else if (!jumpPressed)
        {
            jumpConsumed = false;
        }
    }

    void UpdateAnimator()
    {
        if (animator == null)
            return;
        Vector3 horizontalVel = new Vector3(controller.velocity.x, 0, controller.velocity.z);
        float speed = horizontalVel.magnitude;

        animator.SetFloat("Speed", speed);
        animator.SetFloat("VelX", horizontalVel.x);
        animator.SetFloat("VelY", horizontalVel.z);
        animator.SetFloat("Speed", horizontalVel.magnitude);
        animator.SetBool("IsGrounded", isGrounded);
        animator.SetBool("IsDead", isDead);
        animator.SetBool("IsCrouching", isCrouching);
    }

    void HandleCrouch()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            isCrouching = !isCrouching;

            if (isCrouching)
            {
                targetHeight = crouchHeight;
                speed = crouchSpeed;
            }
            else
            {
                targetHeight = standingHeight;
                speed = originalSpeed;
            }
        }

        if (controller != null)
        {
            float newHeight = Mathf.Lerp(controller.height, targetHeight, Time.deltaTime * crouchTransitionSpeed);
            controller.height = newHeight;

            Vector3 newCenter = controller.center;
            newCenter.y = newHeight / 2f;
            controller.center = newCenter;
        }
    }

    void Update()
    {
        // Обработка приседания
        HandleCrouch();

        if (jumpBufferCounter > 0)
            jumpBufferCounter -= Time.deltaTime;

        isGrounded = controller.isGrounded;

        ApplyGravity();

        bool shouldJump = jumpBufferCounter > 0 && isGrounded;
        if (shouldJump)
        {
            playerVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            jumpBufferCounter = 0f;
            animator.SetTrigger("Jump");
        }

        Vector3 moveDirection = transform.right * currentMoveInput.x + transform.forward * currentMoveInput.y;
        Vector3 motion = (moveDirection * speed + playerVelocity) * Time.deltaTime;

        controller.Move(motion);

        UpdateAnimator();
        Look(currentLookDelta);
    }

    void ApplyGravity()
    {
        if (isGrounded && playerVelocity.y < 0)
            playerVelocity.y = 0f;
        else
        {
            playerVelocity.y += gravity * Time.deltaTime;
        }
    }

    public void Look(Vector2 mouse)
    {
        float mouseX = mouse.x * mouseSensitivity;
        float mouseY = mouse.y * mouseSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        playerCamera.localRotation = Quaternion.Euler(xRotation, 0, 0);
        transform.Rotate(Vector3.up * mouseX);
    }

    public Camera GetPlayerCamera()
    {
        return GetComponentInChildren<Camera>();
    }
}