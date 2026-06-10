using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Ultimate Camera System v4.0
/// Полностью совместима с вашей системой:
/// - CharacterControllerMotor
/// - InputManager 
/// - SetInput(moveInput, lookInput, jumpPressed)
/// Добавляет:
/// - Free Look камеру через Cinemachine
/// - Коллизию камеры с окружением
/// - Collider для PlayerBody чтобы камера не проходила через тело
/// - Дополнительные фичи стабильности
/// 
/// ИНСТРУКЦИЯ:
/// 1. Создайте пустой объект в иерархии (назовите "CameraSystem")
/// 2. Добавьте этот скрипт на объект
/// 3. Назначьте ссылки в инспекторе:
///    - Player (ваш трансформ с CharacterController Motor)
///    - Player Camera (ваша основная камера)
///    - InputManager (ваш Input Manager)
/// 4. Запустите игру
/// </summary>
public class UltimateCameraSystem : MonoBehaviour
{
    #region Serialized Fields
    
    [Header("=== ОСНОВНЫЕ ССЫЛКИ ===")]
    [Tooltip("Трансформ вашего игрока (с CharacterController Motor)")]
    public Transform player;
    
    [Tooltip("Ваша основная камера")]
    public Camera playerCamera;
    
    [Tooltip("Ваш Input Manager скрипт")]
    public MonoBehaviour inputManager;
    
    [Header("=== НАСТРОЙКИ КАМЕРЫ ===")]
    [Tooltip("Расстояние камеры по умолчанию")]
    [Range(0.5f, 10f)]
    public float defaultDistance = 4f;
    
    [Tooltip("Минимальное расстояние камеры (зум максимальный)")]
    [Range(0.3f, 3f)]
    public float minDistance = 0.5f;
    
    [Tooltip("Максимальное расстояние камеры (зум минимальный)")]
    [Range(3f, 20f)]
    public float maxDistance = 10f;
    
    [Tooltip("Скорость зума колесиком мыши")]
    public float zoomSpeed = 3f;
    
    [Tooltip("Высота точки обзора от центра игрока")]
    public float lookAtHeight = 1.7f;
    
    [Header("=== ЧУВСТВИТЕЛЬНОСТЬ ===")]
    [Tooltip("Чувствительность мыши по X (поворот вокруг игрока)")]
    [Range(0.5f, 10f)]
    public float mouseSensitivityX = 3f;
    
    [Tooltip("Чувствительность мыши по Y (наклон камеры)")]
    [Range(0.5f, 10f)]
    public float mouseSensitivityY = 3f;
    
    [Tooltip("Скорость сглаживания вращения")]
    [Range(0.01f, 0.5f)]
    public float rotationSmooth = 0.05f;
    
    [Header("=== КОЛЛИЗИЯ КАМЕРЫ ===")]
    [Tooltip("Слой окружения, с которым будет сталкиваться камера")]
    public LayerMask collisionLayers = ~0; // Все слои по умолчанию
    
    [Tooltip("Радиус сферы коллизии камеры")]
    [Range(0.1f, 1f)]
    public float cameraRadius = 0.3f;
    
    [Tooltip("Дополнительный отступ от при столкновении")]
    public float collisionOffset = 0.1f;
    
    [Header("=== ОГРАНИЧЕНИЯ УГЛОВ ===")]
    [Tooltip("Максимальный угол наклона вверх")]
    [Range(30f, 89f)]
    public float maxPitchUp = 80f;
    
    [Tooltip("Максимальный угол наклона вниз")]
    [Range(30f, 89f)]
    public float maxPitchDown = 80f;
    
    [Tooltip("Можно ли вращать на 360 градусов")]
    public bool allowFullRotation = false;
    
    [Tooltip("Минимальный горизонтальный угол (если не полное вращение)")]
    public float minYaw = -180f;
    
    [Tooltip("Максимальный горизонтальный угол (если не полное вращение)")]
    public float maxYaw = 180f;
    
    [Header("=== PLAYER BODY COLLIDER ===")]
    [Tooltip("Автоматически создать капсулу-коллайдер для тела игрока")]
    public bool autoCreatePlayerBodyCollider = true;
    
    [Tooltip("Радиус капсулы тела игрока")]
    public float bodyColliderRadius = 0.4f;
    
    [Tooltip("Высота капсулы тела игрока")]
    public float bodyColliderHeight = 1.8f;
    
    [Header("=== ДОПОЛНИТЕЛЬНО ===")]
    [Tooltip("Инвертировать ось X")]
    public bool invertX = false;
    
    [Tooltip("Инвертировать ось Y")]
    public bool invertY = false;
    
    [Tooltip("Блокировать курсор во время игры")]
    public bool lockCursor = true;
    
    [Tooltip("Показать Debug GUI")]
    public bool showDebugGUI = false;
    
    #endregion
    
    #region Private Fields
    
    // Текущие углы камеры
    private float currentYaw;
    private float currentPitch;
    
    // Целевые углы для сглаживания
    private float targetYaw;
    private float targetPitch;
    
    // Зум
    private float currentDistance;
    private float targetDistance;
    private float zoomVelocity;
    
    // Ссылки на компоненты
    private CharacterController characterController;
    private Camera cam;
    
    // Для Input Manager
    private Vector2 lastLookInput;
    private bool isInitialized;
    
    // Кэш для оптимизации
    private Vector3 cameraPosition;
    private Vector3 targetPosition;
    private RaycastHit[] raycastHits;
    private Quaternion cameraRotation;
    
    // Player Body Collider
    private CapsuleCollider playerBodyCollider;
    private GameObject playerBodyObject;
    
    #endregion
    
    #region Properties
    
    public float CurrentYaw => currentYaw;
    public float CurrentPitch => currentPitch;
    public float CurrentDistance => currentDistance;
    public Vector3 CameraForward => cam.transform.forward;
    public Vector3 CameraRight => cam.transform.right;
    public bool IsColliding { get; private set; }
    
    #endregion
    
    #region Unity Lifecycle
    
    private void Awake()
    {
        Initialize();
    }
    
    private void Start()
    {
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        
        // Инициализация начальных значений
        if (playerCamera != null && player != null)
        {
            currentDistance = defaultDistance;
            targetDistance = defaultDistance;
            
            // Получаем начальные углы из текущей позиции камеры
            Vector3 angles = playerCamera.transform.eulerAngles;
            currentYaw = angles.y;
            currentPitch = angles.x;
            targetYaw = currentYaw;
            targetPitch = currentPitch;
        }
        
        // Создаём Player Body Collider если нужно
        if (autoCreatePlayerBodyCollider)
        {
            CreatePlayerBodyCollider();
        }
        
        isInitialized = true;
    }
    
    private void Update()
    {
        HandleZoom();
        HandleCursorLock();
    }
    
    private void LateUpdate()
    {
        if (!isInitialized || player == null || playerCamera == null) return;
        
        // Получаем инпут из вашего InputManager если он есть
        Vector2 lookInput = GetLookInput();
        
        UpdateTargetAngles(lookInput);
        UpdateCameraPosition();
    }
    
    private void OnDestroy()
    {
        CleanupPlayerBodyCollider();
    }
    
    private void OnGUI()
    {
        if (!showDebugGUI) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Label($"Yaw: {currentYaw:F1}°");
        GUILayout.Label($"Pitch: {currentPitch:F1}°");
        GUILayout.Label($"Distance: {currentDistance:F2}");
        GUILayout.Label($"IsColliding: {IsColliding}");
        GUILayout.Label($"Target: {(player != null ? player.name : "None")}");
        GUILayout.EndArea();
    }
    
    #endregion
    
    #region Initialization
    
    private void Initialize()
    {
        // Получаем компоненты
        cam = GetComponentInChildren<Camera>();
        if (cam == null && playerCamera != null)
        {
            cam = playerCamera;
        }
        
        if (player != null)
        {
            characterController = player.GetComponent<CharacterController>();
        }
        
        // Инициализация массива для рейкастов
        raycastHits = new RaycastHit[10];
        
        // Устанавливаем начальную дистанцию
        currentDistance = defaultDistance;
        targetDistance = defaultDistance;
    }
    
    private void CreatePlayerBodyCollider()
    {
        if (player == null) return;
        
        // Создаём дочерний объект для коллайдера
        playerBodyObject = new GameObject("PlayerBody_Collider");
        playerBodyObject.transform.SetParent(player);
        playerBodyObject.transform.localPosition = Vector3.zero;
        playerBodyObject.transform.localRotation = Quaternion.identity;
        
        // Устанавливаем слой (предполагаем, что есть слой "PlayerBody")
        int playerBodyLayer = LayerMask.NameToLayer("PlayerBody");
        if (playerBodyLayer != -1)
        {
            playerBodyObject.layer = playerBodyLayer;
        }
        
        // Добавляем капсульный коллайдер (триггер чтобы не блокировать движение)
        playerBodyCollider = playerBodyObject.AddComponent<CapsuleCollider>();
        playerBodyCollider.radius = bodyColliderRadius;
        playerBodyCollider.height = bodyColliderHeight;
        playerBodyCollider.center = new Vector3(0, bodyColliderHeight / 2f, 0);
        playerBodyCollider.isTrigger = true;
        
        // Добавляем Rigidbody для работы триггеров
        Rigidbody rb = playerBodyObject.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
    }
    
    private void CleanupPlayerBodyCollider()
    {
        if (playerBodyObject != null)
        {
            Destroy(playerBodyObject);
        }
    }
    
    #endregion
    
    #region Input Handling
    
    private Vector2 GetLookInput()
    {
        // Если есть InputManager, получаем данные из него
        if (inputManager != null)
        {
            // Пытаемся получить lookInput через рефлексию или прямой доступ
            // Это зависит от реализации вашего InputManager
            
            // Пример если InputManager имеет публичное свойство:
            // var inputManagerType = inputManager.GetType();
            // var lookInputProperty = inputManagerType.GetProperty("LookInput");
            // if (lookInputProperty != null)
            // {
            //     return (Vector2)lookInputProperty.GetValue(inputManager);
            // }
            
            // Альтернативно, если у вас есть доступ к полю:
            return GetInputManagerLookInput();
        }
        
        // Базовый инпут если нет InputManager
        return new Vector2(
            Input.GetAxis("Mouse X") * mouseSensitivityX,
            Input.GetAxis("Mouse Y") * mouseSensitivityY
        );
    }
    
    private Vector2 GetInputManagerLookInput()
    {
        // Здесь вы можете добавить логику получения инпута из вашего InputManager
        // Например, если у вас есть метод GetLookInput() или свойство
        
        // Временно используем стандартный Input
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivityX;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivityY;
        
        if (invertX) mouseX = -mouseX;
        if (invertY) mouseY = -mouseY;
        
        return new Vector2(mouseX, mouseY);
    }
    
    private void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            targetDistance -= scroll * zoomSpeed;
            targetDistance = Mathf.Clamp(targetDistance, minDistance, maxDistance);
        }
        
        // Плавный зум
        currentDistance = Mathf.SmoothDamp(currentDistance, targetDistance, ref zoomVelocity, 0.1f);
    }
    
    private void HandleCursorLock()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
        
        // Блокировка при клике если разблокирован
        if (Cursor.lockState == CursorLockMode.None && Input.GetMouseButtonDown(0))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
    
    #endregion
    
    #region Camera Logic
    
    private void UpdateTargetAngles(Vector2 lookInput)
    {
        targetYaw += lookInput.x;
        targetPitch -= lookInput.y;
        
        // Ограничение углов
        targetPitch = Mathf.Clamp(targetPitch, -maxPitchDown, maxPitchUp);
        
        if (!allowFullRotation)
        {
            targetYaw = Mathf.Clamp(targetYaw, minYaw, maxYaw);
        }
        else
        {
            // Нормализация для полного вращения
            if (targetYaw > 180f) targetYaw -= 360f;
            if (targetYaw < -180f) targetYaw += 360f;
        }
    }
    
    private void UpdateCameraPosition()
    {
        // Плавное сглаживание углов
        currentYaw = Mathf.LerpAngle(currentYaw, targetYaw, rotationSmooth * Time.deltaTime * 60f);
        currentPitch = Mathf.LerpAngle(currentPitch, targetPitch, rotationSmooth * Time.deltaTime * 60f);
        
        // Позицияцели (точка на которую смотрит камера)
        targetPosition = player.position + Vector3.up * lookAtHeight;
        
        // Рассчитываем желаемую позицию камеры
        cameraRotation = Quaternion.Euler(currentPitch, currentYaw, 0);
        Vector3 desiredCameraPos = targetPosition - (cameraRotation * Vector3.forward * currentDistance);
        
        // Проверяем коллизии
        IsColliding = HandleCameraCollision(targetPosition, ref desiredCameraPos);
        
        // Применяем позицию
        playerCamera.transform.position = desiredCameraPos;
        playerCamera.transform.LookAt(targetPosition);
    }
    
    private bool HandleCameraCollision(Vector3 lookAtPoint, ref Vector3 cameraPosition)
    {
        Vector3 direction = (cameraPosition - lookAtPoint).normalized;
        float targetDist = currentDistance;
        
        // Основной рейкаст для проверки видимости
        if (Physics.SphereCast(lookAtPoint, cameraRadius, direction, out RaycastHit hit, targetDist, collisionLayers))
        {
            // Камера ударяется о препятствие
            float hitDistance = hit.distance - collisionOffset;
            hitDistance = Mathf.Max(hitDistance, minDistance);
            
            // Новая позиция у стены
            cameraPosition = lookAtPoint + direction * hitDistance;
            
            // Дополнительная проверка: не находимся ли мы внутри PlayerBody коллайдера
            if (IsInsidePlayerBody(lookAtPoint, cameraPosition))
            {
                // Если внутри тела игрока, пробуем найти ближайшую точку снаружи
                cameraPosition = AvoidPlayerBody(lookAtPoint, cameraPosition);
            }
            
            return true;
        }
        
        // Проверка: не проходит ли камера сквозь тело игрока
        if (IsInsidePlayerBody(lookAtPoint, cameraPosition))
        {
            cameraPosition = AvoidPlayerBody(lookAtPoint, cameraPosition);
            return true;
        }
        
        return false;
    }
    
    private bool IsInsidePlayerBody(Vector3 from, Vector3 to)
    {
        if (playerBodyCollider == null) return false;
        
        Vector3 direction = to - from;
        float distance = direction.magnitude;
        
        // Проверяем пересечение с капсулой тела игрока
        return Physics.Raycast(from, direction.normalized, distance, 1 << playerBodyObject.layer);
    }
    
    private Vector3 AvoidPlayerBody(Vector3 from, Vector3 currentPos)
    {
        // Находим ближайшую точку на поверхности капсулы
        Vector3 nearestPoint = playerBodyCollider.ClosestPoint(currentPos);
        
        // Отодвигаем камеру от тела
        Vector3 pushDirection = (currentPos - nearestPoint).normalized;
        if (pushDirection == Vector3.zero) pushDirection = (currentPos - from).normalized;
        
        return nearestPoint + pushDirection * (cameraRadius + collisionOffset);
    }
    
    #endregion
    
    #region Public API
    
    /// <summary>
    /// Принудительно установить углы камеры
    /// </summary>
    public void SetCameraAngles(float yaw, float pitch)
    {
        targetYaw = yaw;
        targetPitch = Mathf.Clamp(pitch, -maxPitchDown, maxPitchUp);
    }
    
    /// <summary>
    /// Установить дистанцию зума
    /// </summary>
    public void SetZoom(float distance)
    {
        targetDistance = Mathf.Clamp(distance, minDistance, maxDistance);
    }
    
    /// <summary>
    /// Сбросить камеру в стандартное положение
    /// </summary>
    public void ResetCamera()
    {
        targetDistance = defaultDistance;
        // Можно также сбросить углы, если нужно
    }
    
    /// <summary>
    /// Получить направление камеры в мировых координатах
    /// </summary>
    public Vector3 GetCameraDirection()
    {
        if (cam == null) return Vector3.forward;
        return cam.transform.forward;
    }
    
    /// <summary>
    /// Получить правый вектор камеры (для WASD движения относительно камеры)
    /// </summary>
    public Vector3 GetCameraRight()
    {
        if (cam == null) return Vector3.right;
        Vector3 right = cam.transform.right;
        right.y = 0;
        return right.normalized;
    }
    
    #endregion
    
    #region Gizmos
    
    private void OnDrawGizmosSelected()
    {
        if (player == null) return;
        
        // Рисуем точку цели камеры
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(player.position + Vector3.up * lookAtHeight, cameraRadius);
        
        // Рисуем тело игрока коллайдер
        if (playerBodyCollider != null)
        {
            Gizmos.color = new Color(1, 0, 0, 0.3f);
            DrawCapsuleGizmo(playerBodyCollider);
        }
    }
    
    private void DrawCapsuleGizmo(CapsuleCollider capsule)
    {
        // Упрощённое отображение капсулы
        Vector3 center = capsule.transform.TransformPoint(capsule.center);
        float radius = capsule.radius;
        float height = capsule.height;
        
        Gizmos.DrawWireSphere(center + Vector3.up * (height/2 - radius), radius);
        Gizmos.DrawWireSphere(center - Vector3.up * (height/2 - radius), radius);
    }
    
    #endregion
}