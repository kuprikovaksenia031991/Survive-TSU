using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class SimpleFPSControllers : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    private Transform playerCamera;
    private CharacterController controller;

    [Header("Movement")]
    public float speed = 5f;
    public float jumpHeight = 0.4f;
    private float gravity = -9.8f;

    [Header("Mouse Settings")]
    public float mouseSensitivity = 1.7f;
    public float minPitch = -89f;
    public float maxPitch = 89f;

    [Header("Camera Shake / Head Bobbing")]
    public bool enableHeadBob = true;
    public float bobFrequency = 5f;  // Скорость покачивания
    public float bobAmplitude = 0.05f; // Сила (высота) покачивания
    private float bobTimer = 0f;
    private Vector3 defaultCameraPos; // Начальная позиция камеры

    [Header("Jump Buffer")]
    [SerializeField] private float jumpBufferTime = 0.1f;

    private float xRotation = 0f;
    private Vector3 playerVelocity;
    private Vector2 currentMoveInput;
    private Vector2 currentLookDelta;
    private float jumpBufferCounter = 0f;
    private bool jumpConsumed = true;
    private bool isDead = false;
    private bool isGrounded;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        playerCamera = GetComponentInChildren<Camera>().transform;
        
        // Запоминаем стандартную позицию камеры, чтобы тряска была относительно неё
        if (playerCamera != null)
            defaultCameraPos = playerCamera.localPosition;

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void SetInput(Vector2 moveInput, Vector2 lookDelta, bool jumpPressed)
    {
        currentMoveInput = moveInput;
        currentLookDelta = lookDelta;

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

    void Update()
    {
        if (jumpBufferCounter > 0)
            jumpBufferCounter -= Time.deltaTime;

        isGrounded = controller.isGrounded;
        ApplyGravity();

        if (jumpBufferCounter > 0 && isGrounded)
        {
            playerVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            jumpBufferCounter = 0f;
            if (animator != null) animator.SetTrigger("Jump");
        }

        Vector3 moveDirection = transform.right * currentMoveInput.x + transform.forward * currentMoveInput.y;
        Vector3 motion = (moveDirection * speed + playerVelocity) * Time.deltaTime;

        controller.Move(motion);
        UpdateAnimator();
    }

    void LateUpdate()
    {
        HandleLook();
        if (enableHeadBob)
        {
            ApplyHeadBob();
        }
    }

    void ApplyGravity()
    {
        if (isGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = -2f;
        }
        else
        {
            playerVelocity.y += gravity * Time.deltaTime;
        }
    }

    private void HandleLook()
    {
        if (playerCamera == null) return;

        float yaw = currentLookDelta.x * mouseSensitivity;
        transform.Rotate(Vector3.up * yaw);

        xRotation -= currentLookDelta.y * mouseSensitivity; 
        xRotation = Mathf.Clamp(xRotation, minPitch, maxPitch);
        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }

    private void ApplyHeadBob()
    {
        if (playerCamera == null) return;

        // Тряска работает только если персонаж движется и стоит на земле
        float horizontalSpeed = new Vector3(controller.velocity.x, 0, controller.velocity.z).magnitude;

        if (isGrounded && horizontalSpeed > 0.1f)
        {
            // Считаем время с учетом скорости движения
            bobTimer += Time.deltaTime * bobFrequency;
            
            // Используем Sin для плавного движения вверх-вниз и влево-вправо
            float bobOffsetY = Mathf.Sin(bobTimer) * bobAmplitude;
            float bobOffsetX = Mathf.Cos(bobTimer * 0.5f) * bobAmplitude * 0.5f;

            Vector3 newPos = defaultCameraPos + new Vector3(bobOffsetX, bobOffsetY, 0);
            playerCamera.localPosition = newPos;
        }
        else
        {
            // Плавно возвращаем камеру в исходную позицию, когда стоим
            bobTimer = 0;
            playerCamera.localPosition = Vector3.Lerp(playerCamera.localPosition, defaultCameraPos, Time.deltaTime * 5f);
        }
    }

    void UpdateAnimator()
    {
        if (animator == null) return;
        Vector3 horizontalVel = new Vector3(controller.velocity.x, 0, controller.velocity.z);
        float currentSpeed = horizontalVel.magnitude;

        animator.SetFloat("Speed", currentSpeed);
        animator.SetFloat("VelX", horizontalVel.x);
        animator.SetFloat("VelY", horizontalVel.z);
        animator.SetBool("IsGrounded", isGrounded);
        animator.SetBool("IsDead", isDead);
    }
}