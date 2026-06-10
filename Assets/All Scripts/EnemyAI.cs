using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine.InputSystem.XR;
using UnityEditor.UI;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    private Vector3 uiOffset = new Vector3(0, 2.5f, 0); // Выше головы врага
    // UI and for UI
    private int maxEnemies = 4;
    private int currentEnemies = 0;
    private Slider hpSlider;
private CanvasGroup canvasGroup;
private float damageTimer;
private float hideDelay = 3f;
    // Spawner
    private GameObject enemyPrefab;
    [SerializeField] private Transform enemyParent;


    [Header("Movement")]
    public float stopDistance = 1.5f;

    [Header("Combat")]
    public float damage = 7f;
    public float attackCooldown = 3.2f;

    [Header("Stats")]
    public float health = 100f;

    public bool isEnemyTrigger = false;
    public bool isEnemySpawn = false;
    public float startWaitTime = 4f;
    public float timeToRotate = 2f;
    public float speedWalk = 2f;
    public float speedRun = 3f;
    public float maxChaseDistance = 6f; // Расстояние, дальше которого от комнаты нельзя убегать
    float distanceToRoom = 0;

    public float viewRadius = 15f;
    public float viewAngle = 90f;
    public float meshResolution = 1f;
    public float edgeIterations = 4f;
    public float edgeDistance = 0.5f;

    public Transform[] waypoints = new Transform[6];

    private int mCurrentWaypointIndex;
    private Vector3 playerLastPosition = Vector3.zero;
    private Vector3 mPlayerPosition;

    private float mWaitTime;
    private float mTimeToRotate;
    private bool mPlayerInRange;
    private bool mPlayerNear;
    private bool mIsPatrol = true;
    private bool mCaughtPlayer;

    private float maxHP = 100f;
    private float currentHP;

    private Transform player;
    private Transform enemyHead;
    private NavMeshAgent agent;

    public Transform guardRoom;
    //public DoorController temporaryDoor;
    public string enemyTag;
    [Header("Effects")]
    public float knockbackForce = 3f;
    public float flashDuration = 0.1f;
    // Вспышка при ударе
    private Material bodyMaterial;
    private Color originalColor;
    private float flashTimer;

    public LayerMask playerMask;
    public LayerMask obstacleMask;

    Animator animator;

    public int weaponType = 0;
    private float lastAttackTime;
    
    // Эффекты полоски HP
private Image glowImage;
private Image damageFlashImage;
private Color originalFillColor;
private float glowTimer;
private float glowDuration = 0.3f;
private float damageFlashTimer;
private float damageFlashDuration = 0.15f;
   

    void Start()
    {
        CreateHPBar();
        //temporaryDoor.ToggleDoor();
        enemyTag = transform.tag;
        currentHP = maxHP;

        mPlayerPosition = Vector3.zero;
        mIsPatrol = true;
        mCaughtPlayer = false;
        mPlayerInRange = false;
        mWaitTime = startWaitTime;
        mTimeToRotate = timeToRotate;

        mCurrentWaypointIndex = 0;

        player = GameObject.FindWithTag("Player").transform;

        animator = GetComponent<Animator>();
        animator.SetInteger("WeaponType", weaponType); //начальное оружие для зомби - укусы

        agent = GetComponent<NavMeshAgent>();

        //если противник - участник первой катсцены со студентом выбегающим из комнаты, то ему не нужно нападать на игрока пока сосед не погибнет
        if (isEnemyTrigger)
        {
            agent.isStopped = true;
        }

        else if (waypoints.Length > 0)
        {
            agent.speed = speedWalk;
            // agent.SetDestination(waypoints[mCurrentWaypointIndex].position);
            agent.stoppingDistance = stopDistance;
        }
    }

    void Update()
    {
        if (canvasGroup != null && canvasGroup.alpha <= 0)
        canvasGroup.alpha = 1f;

        // Таймер скрытия HP
        if (damageTimer > 0)
    {
        damageTimer -= Time.deltaTime;
        if (damageTimer <= 0 && canvasGroup != null)
            canvasGroup.alpha = 0f;
    }
        if (enemyHead != null)
            transform.position = enemyHead.position + Vector3.up * 0.01f;

        if (guardRoom != null)
        {
            distanceToRoom = Vector3.Distance(transform.position, guardRoom.position);
        }
        if (guardRoom != null && !mIsPatrol)
        {
            if (distanceToRoom > maxChaseDistance)
            {
                mIsPatrol = true;
                mPlayerInRange = false;
                mPlayerNear = false;
                mCaughtPlayer = false;

                agent.isStopped = false;
                agent.speed = speedRun;
                agent.SetDestination(guardRoom.position);
                return;
            }
        }
        EnviromentView();
        if (!mIsPatrol)
        {
            Chasing();
        }
        else
        {
            Patrolling();
        }

        float distance =
            Vector3.Distance(transform.position, player.position);
        if (mPlayerInRange && distance <= stopDistance + 0.3f)
        {
            TryAttack();
        }
        UpdateAnimator();
        // Эффект подсветки (Glow)
    if (glowTimer > 0 && glowImage != null)
    {
        glowTimer -= Time.deltaTime;
        float alpha = glowTimer / glowDuration;
        glowImage.color = new Color(1f, 0.5f, 0f, alpha * 0.8f);
    }
    
    // Эффект вспышки урона
    if (damageFlashTimer > 0 && damageFlashImage != null)
    {
        damageFlashTimer -= Time.deltaTime;
        float alpha = damageFlashTimer / damageFlashDuration;
        damageFlashImage.color = new Color(1f, 1f, 0f, alpha);
        
        if (damageFlashTimer <= 0)
        {
            damageFlashImage.color = new Color(1f, 1f, 0f, 0f);
        }
    }
    }
    void UpdateAnimator()
    {
        if (animator == null)
            return;
        Vector3 velocity = agent.velocity;
        Vector3 horizontalVel = new Vector3(velocity.x, 0, velocity.z);
        float speed = horizontalVel.magnitude;

        animator.SetFloat("Speed", speed);
        animator.SetFloat("VelX", horizontalVel.x);
        animator.SetFloat("VelY", horizontalVel.z);
        animator.SetFloat("Speed", horizontalVel.magnitude);
    }
    private void Chasing()
    {
        if (player == null)
            return;

        float distanceToPlayer =
            Vector3.Distance(transform.position, player.position);

        // Если слишком далеко от комнаты — возвращаемся
        if (guardRoom != null)
        {
            float distanceToRoom =
                Vector3.Distance(transform.position, guardRoom.position);

            if (distanceToRoom > maxChaseDistance)
            {
                mIsPatrol = true;
                mPlayerInRange = false;
                mPlayerNear = false;

                agent.isStopped = false;
                agent.speed = speedWalk;
                //agent.SetDestination(guardRoom.position);

                return;
            }
        }
        // Если игрок рядом — преследуем
        if (distanceToPlayer > stopDistance)
        {
            agent.isStopped = false;

            Move(speedRun);
            agent.SetDestination(player.position);
        }
        else
        {
            // Останавливаемся для атаки
            agent.isStopped = true;

            TryAttack();
        }
    }
void LateUpdate()
{
    if (hpSlider != null)
    {
        // Позиция над головой врага
        hpSlider.transform.position = transform.position + Vector3.up * 2.5f;
    }
}    private void Patrolling()
    {
        agent.isStopped = false;
        agent.speed = speedWalk;

        // Проверяем последнюю позицию игрока
        if (mPlayerNear)
        {
            agent.SetDestination(playerLastPosition);

            if (!agent.pathPending &&
                agent.remainingDistance <= 0.5f)
            {
                mPlayerNear = false;

                // Возвращаемся домой
                //  agent.SetDestination(guardRoom.position);
            }

            return;
        }
        if (guardRoom != null)
            // Просто идем домой
            agent.SetDestination(guardRoom.position);
    }

    // private void GoToNextWaypoint()
    // {
    //     if (waypoints.Length > 0)
    //     {
    //         agent.SetDestination(waypoints[mCurrentWaypointIndex].position);
    //         mCurrentWaypointIndex = (mCurrentWaypointIndex + 1) % waypoints.Length;
    //     }
    // }

    void Move(float speed)
    {
        agent.isStopped = false;
        agent.speed = speed;
    }
    void Stop()
    {
        agent.isStopped = true;
        agent.speed = 0;
    }
    //private void NextPoint()
    //{
    //    if (guardRoom != null)
    //    {
    //        Vector3 randomOffset = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));
    //        Vector3 nextPoint = guardRoom.position + randomOffset;
    //        agent.SetDestination(nextPoint);
    //    }
    //    else
    //    {
    //        GoToNextWaypoint();
    //    }
    //}

    void CaughtPlayer()
    {
        mCaughtPlayer = true;
    }
    void LookingPlayer(Vector3 checkPosition)
    {
        // Приказываем зомби идти к последней известной точке игрока
        agent.SetDestination(checkPosition);

        // Если зомби почти дошел до этой точки (осталось меньше 0.5 метров)
        if (!agent.pathPending && agent.remainingDistance <= 0.5f)
        {
            // Зомби пришел, но игрока там нет. Он начинает "оглядываться" (ждет время mWaitTime)
            Stop();
            mWaitTime -= Time.deltaTime;

            if (mWaitTime <= 0)
            {
                // Время ожидания вышло, зомби сдался и возвращается к обычному патрулированию
                mPlayerNear = false;
                Move(speedWalk);
                // GoToNextWaypoint(); // Идет к следующей точке обхода
                mWaitTime = startWaitTime; // Сбрасываем таймер ожидания на будущее
                mTimeToRotate = timeToRotate;
            }
        }
    }

    void EnviromentView()
    {
        // Ищем игрока в радиусе
        Collider[] playerInRange = Physics.OverlapSphere(transform.position, viewRadius, playerMask);

        if (playerInRange.Length > 0)
        {
            Transform targetPlayer = playerInRange[0].transform;
            Vector3 dirToPlayer = (targetPlayer.position - transform.position).normalized;

            // Проверяем конус зрения (угол)
            if (Vector3.Angle(transform.forward, dirToPlayer) < viewAngle / 2f)
            {
                float dstToPlayer = Vector3.Distance(transform.position, targetPlayer.position);

                // Проверяем стены (Raycast)
                if (!Physics.Raycast(transform.position, dirToPlayer, dstToPlayer, obstacleMask))
                {
                    // ЗОМБИ ВИДИТ ИГРОКА ПРЯМО СЕЙЧАС
                    mPlayerInRange = true;
                    mIsPatrol = false; // Включаем режим погони

                    // Постоянно обновляем текущую позицию игрока
                    mPlayerPosition = targetPlayer.position;
                    playerLastPosition = targetPlayer.position; // Запоминаем для будущего
                    return;
                }
            }
        }

        // ЕСЛИ ЗОМБИ ДОШЕЛ СЮДА — ЗОМБИ НЕ ВИДИТ ИГРОКА ПРЯМО СЕЙЧАС (зашел за стену или убежал)
        if (mPlayerInRange)
        {
            mPlayerInRange = false;

            // Запоминаем последнюю точку
            playerLastPosition = mPlayerPosition;

            // Идем проверить
            mPlayerNear = true;
        }
    }

    void TryAttack()
    {
        if (Time.time - lastAttackTime < attackCooldown)
            return;
        animator.SetTrigger("Attack");
        PlayerStats stats =
            player.GetComponent<PlayerStats>();

        if (stats != null)
        {
            stats.TakeDamage(damage);

            Debug.Log("ВРАГ БЬЁТ! Урон: " + damage);

            lastAttackTime = Time.time;
        }
    }

    public void TakeDamage(float dmg)
{
    currentHP -= dmg; // ИСПРАВЛЁНО: было damage, должно быть dmg
    currentHP = Mathf.Max(0, currentHP);
    
    // КРОВЬ
    if (BloodEffect.Instance != null)
    {
        Vector3 bloodPos = transform.position + Vector3.up * 1.5f;
        BloodEffect.Instance.SpawnBlood(bloodPos);
    }
    
    // Вспышка
    if (bodyMaterial != null)
    {
        bodyMaterial.color = Color.red;
        flashTimer = flashDuration;
    }
    if (hpSlider != null)
            hpSlider.value = currentHP / maxHP; // НОРМАЛИЗОВАТЬ 0-1
    
    // Показать полоску HP
    if (canvasGroup != null)
    {
        canvasGroup.alpha = 1;
        damageTimer = hideDelay;
    }

    if (currentHP <= 0)
        Die();
}

    void Die()
    {
        animator.SetTrigger("IsDead");
        Debug.Log("ВРАГ УБИТ!");

        agent.enabled = false;

        Destroy(gameObject, 5f);
        FindObjectOfType<EnemyAI>()?.EnemyDiedCounter();

        if (isEnemySpawn)
        {
            Vector3 posDied = transform.position;
            Invoke(nameof(SpawnEnemyDelayed), 15f);
        }
    }
    public void EnemyDiedCounter()
    {
        currentEnemies--;
    }
    // Spawner
    void SpawnEnemyDelayed()
    {
        SpawnEnemy(transform.position);
    }
    public void SpawnEnemy(Vector3 position)
    {
        if (currentEnemies >= maxEnemies)
        {
            Debug.Log("Максимум врагов!");
            return;
        }

        GameObject enemyNew = Instantiate(enemyPrefab, position, Quaternion.identity, enemyParent);
        currentEnemies++;
     
    }
    void CreateHPBar()
{
    // Создаём HPBar
    GameObject hpBar = new GameObject("HPBar");
    hpBar.transform.SetParent(transform);
    hpBar.transform.localPosition = new Vector3(0, 2.5f, 0);

    // Canvas
    Canvas canvas = hpBar.AddComponent<Canvas>();
    canvas.renderMode = RenderMode.WorldSpace;
    canvas.worldCamera = Camera.main;
    canvas.sortingOrder = 10;

    CanvasScaler scaler = hpBar.AddComponent<CanvasScaler>();
    scaler.dynamicPixelsPerUnit = 100;

    // CanvasGroup
    canvasGroup = hpBar.AddComponent<CanvasGroup>();

    // Slider объект
    GameObject sliderObj = new GameObject("Slider");
    sliderObj.transform.SetParent(hpBar.transform);
    sliderObj.transform.localPosition = Vector3.zero;
    sliderObj.transform.localScale = new Vector3(1, 1, 1);

    RectTransform sliderRT = sliderObj.AddComponent<RectTransform>();
    sliderRT.sizeDelta = new Vector2(0.5f, 0.05f);

    // Slider component
    hpSlider = sliderObj.AddComponent<Slider>();

    // Background
    GameObject bg = new GameObject("Background");
    bg.transform.SetParent(sliderObj.transform);
    bg.transform.localPosition = Vector3.zero;
    RectTransform bgRT = bg.AddComponent<RectTransform>();
    bgRT.sizeDelta = new Vector2(0.5f, 0.05f);
    bgRT.anchoredPosition = Vector2.zero;
    Image bgImg = bg.AddComponent<Image>();
    bgImg.color = new Color(0.3f, 0f, 0f, 0.8f);

    // Fill Area
    GameObject fillArea = new GameObject("Fill Area");
    fillArea.transform.SetParent(sliderObj.transform);
    fillArea.transform.localPosition = Vector3.zero;
    RectTransform faRT = fillArea.AddComponent<RectTransform>();
    faRT.sizeDelta = new Vector2(0.5f, 0.05f);
    faRT.anchoredPosition = Vector2.zero;

    // Fill (основной)
    GameObject fill = new GameObject("Fill");
    fill.transform.SetParent(fillArea.transform);
    fill.transform.localPosition = Vector3.zero;
    RectTransform fillRT = fill.AddComponent<RectTransform>();
    fillRT.sizeDelta = new Vector2(0.5f, 0.05f);
    fillRT.anchoredPosition = Vector2.zero;
    Image fillImg = fill.AddComponent<Image>();
    fillImg.color = new Color(0.8f, 0f, 0f, 1f);

    // ===== GLOW EFFECT (подсветка) =====
    GameObject glow = new GameObject("Glow");
    glow.transform.SetParent(fillArea.transform);
    glow.transform.localPosition = Vector3.zero;
    RectTransform glowRT = glow.AddComponent<RectTransform>();
    glowRT.sizeDelta = new Vector2(0.5f, 0.05f);
    glowRT.anchoredPosition = Vector2.zero;
    Image glowImg = glow.AddComponent<Image>();
    glowImg.color = new Color(1f, 0.5f, 0f, 0f);

    // ===== DAMAGE FLASH (вспышка урона) =====
    GameObject damageFlash = new GameObject("DamageFlash");
    damageFlash.transform.SetParent(sliderObj.transform);
    damageFlash.transform.localPosition = Vector3.zero;
    RectTransform dfRT = damageFlash.AddComponent<RectTransform>();
    dfRT.sizeDelta = new Vector2(0.5f, 0.05f);
    dfRT.anchoredPosition = Vector2.zero;
    Image dfImg = damageFlash.AddComponent<Image>();
    dfImg.color = new Color(1f, 1f, 0f, 0f);

    // Slider settings
    hpSlider.fillRect = fillRT;
    hpSlider.targetGraphic = fillImg;
    hpSlider.direction = Slider.Direction.LeftToRight;
    hpSlider.minValue = 0;
    hpSlider.maxValue = 1;
    hpSlider.value = 1;

    // Скрыть по умолчанию
    canvasGroup.alpha = 0;
    canvasGroup.blocksRaycasts = false;
    
    // Сохраняем ссылки для эффектов
    glowImage = glowImg;
    damageFlashImage = dfImg;
    originalFillColor = fillImg.color;
}
}
