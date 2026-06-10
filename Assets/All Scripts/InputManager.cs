using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    private PlayerInputs playerInput;
    private PlayerInputs.OnFootActions onFoot;

    [SerializeField] private SimpleFPSControllers motor;
    [SerializeField] private PlayerAttack playerAttack;

    void Awake()
    {
        playerInput = new PlayerInputs();
        onFoot = playerInput.OnFoot;

        if (motor == null)
            motor = GetComponent<SimpleFPSControllers>();
        if (playerAttack == null)
            playerAttack = GetComponent<PlayerAttack>();

        Debug.Log("InputManager Awake завершён");
    }

    private void OnEnable()
    {
        if (playerInput != null)
        {
            onFoot.Enable();
            Debug.Log("InputManager OnEnable: onFoot включён");
        }
    }

    private void OnDisable()
    {
        if (playerInput != null)
        {
            onFoot.Disable();
            Debug.Log("InputManager OnDisable: onFoot выключен");
        }
    }

    void Update()
    {
        if (motor == null || playerAttack == null)
        {
            return;
        }

        if (playerInput == null) return;

        try
        {
            Vector2 moveInput = onFoot.Move.ReadValue<Vector2>();
            Vector2 lookInput = onFoot.Look.ReadValue<Vector2>();
            bool jumpPressed = onFoot.Jump.triggered;
            bool attackPressed = onFoot.Attack.triggered;
            bool interactPressed = onFoot.Interact.triggered;

            motor.SetInput(moveInput, lookInput, jumpPressed);

            if (attackPressed)
                playerAttack.Attack();

            if (interactPressed)
            {
                playerAttack.InteractWithDoor();
                TryPickupItem();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"InputManager Update ошибка: {e.Message}");
        }
    }

    private void TryPickupItem()
    {
        if (motor == null) return;

        Camera cam = motor.GetPlayerCamera();
        if (cam == null) return;

        Vector3 screenCenter = new Vector3(Screen.width / 2, Screen.height / 2, 0);
        Ray ray = cam.ScreenPointToRay(screenCenter);

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 3f))
        {
            Debug.Log($"Raycast попал в: {hit.collider.name}, слой: {hit.collider.gameObject.layer}");

            // Ищем Pickup на объекте или на родителе
            Pickup pickup = hit.collider.GetComponent<Pickup>();
            if (pickup == null)
                pickup = hit.collider.GetComponentInParent<Pickup>();

            if (pickup != null)
            {
                Debug.Log("✅ Pickup найден, вызываем TryPickup()");
                pickup.TryPickup();
            }
            else
            {
                Debug.LogWarning($"❌ Pickup НЕ найден на {hit.collider.name}");
            }
        }
    }

    public Camera GetPlayerCamera()
    {
        if (motor != null)
            return motor.GetPlayerCamera();
        return null;
    }
}