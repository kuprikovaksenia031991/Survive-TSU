using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class RoommateAI : MonoBehaviour
{
    // Start is called before the first frame update
    public float speedWalk = 3f;
    public float startWaitTime = 4f;
    public float distanceToDie = 3f;
    private int countDoorOpen = 0;
    public bool playerDetected = false;
    bool wokeUp = false;
    public float sitDownOffset = 0.8f; // насколько садится вниз
    public float sitMoveTime = 0.4f;   // скорость присаживания
    public float sitTime = 1f;       // сколько сидит
    public float jumpForward = 0.3f;   // насколько спрыгивает вправо
    public float jumpTime = 0.4f;      // скорость спрыгивания

    public DoorController door;
    public NavMeshAgent agent;
    public Animator animator; // для всей анимации
    private Transform player;
    private Transform enemy;

    //��� ���������� �������� �������
    public AnimationFix animFix;

    [Header("Vision Settings")]
    public float viewDistance = 15f;
    public float viewAngle = 45f;
    public LayerMask playerLayer;
    public LayerMask obstacleLayer;


    void Start()
    {
        player = GameObject.FindWithTag("Player").transform;
        enemy = GameObject.FindWithTag("EnemyTrigger").transform;
        agent = GetComponent<NavMeshAgent>();
        agent.enabled = false; //отключаем физику во время сна

        animator = GetComponent<Animator>();
        animator.SetTrigger("Dead"); // для видимости сна бота

        animFix = GetComponent<AnimationFix>();
        animFix.OnDead(); // отключаем коллидер во время сна
    }

    // Update is called once per frame
    void Update()
    {
        DetectPlayer();
        if (playerDetected && !wokeUp)
        {
            AnimationTrigger(); // бот просыпается
            StartCoroutine(EnableAgent()); //появляется физика(вместес ней навигация ии)
        }

        if (playerDetected && countDoorOpen <= 1 && agent.enabled && agent.isOnNavMesh)
        {
            ChaseEnemy();
            if (startWaitTime <= 0)
            {
                door.ToggleDoor();
                startWaitTime = 4.8f;
            }
            else
            {
                startWaitTime -= Time.deltaTime;
            }
        }
        if (Vector3.Distance(enemy.position, transform.position) <= distanceToDie)
        {
            Die();
        }

    }
    void DetectPlayer()
    {
        if (player == null) return;
        if (playerDetected) return;
        if (startWaitTime > 0)
        {
            startWaitTime -= Time.deltaTime;
            return;
        }
        // Расстояние от игрока до бота
        Vector3 directionToBot = transform.position - player.position; //направление от игрока к боту
        float distanceToPlayer = directionToBot.magnitude;
        directionToBot.y = 0; // убираем разницу высоты (это не важно)
        // находится ли бот в пределах видимости по расстоянию
        if (distanceToPlayer > viewDistance)
        {
            playerDetected = false;
            return;
        }
        // находится ли бот в пределах видимости по направлению взгляда
        float angleToPlayer = Vector3.Angle(player.forward, directionToBot.normalized);

        if (angleToPlayer > viewAngle / 2f)
        {
            playerDetected = false;
            return;
        }
        playerDetected = true;
    }
    void ChaseEnemy()
    {
        if (!agent.enabled || !agent.isOnNavMesh)
    {
        if (NavMesh.SamplePosition(transform.position + Vector3.right * 1f, out NavMeshHit hit, 3f, NavMesh.AllAreas))
            agent.Warp(hit.position);
        return;
    }
        animator.SetFloat("VelX", 0.7f);
        agent.isStopped = false;
        agent.speed = speedWalk;
        agent.SetDestination(enemy.position);
    }

    void AnimationTrigger()
    {
        animFix.OnRespawn();
        animator.SetTrigger("Sit");
        wokeUp = true;
    }
    void Die()
    {
        animFix.OnDead(); // убираем физику
        animator.SetTrigger("Dead");

        agent.enabled = false;

        //отключаем скрипт бота чтобы не обновлять его после смерти
        this.enabled = false;
    }

    //принудительный пропуск кадров для корректной анимации вставания
    IEnumerator EnableAgent()
{
    // сразу при пробуждении: садим попой на кровать (вниз) + поворот
    Vector3 sitStart = transform.position;
    Vector3 sitTarget = sitStart + Vector3.down * sitDownOffset; // садится вниз
    sitTarget += Vector3.right * 1f; // сдвиг по X в плюс при посадке
    Quaternion startRot = transform.rotation;
    Quaternion targetRot = Quaternion.LookRotation(Vector3.right); // поворот вправо

    float t = 0;
    while (t < 1f)
    {
        t += Time.deltaTime / sitMoveTime;
        transform.position = Vector3.Lerp(sitStart, sitTarget, t);
        transform.rotation = Quaternion.Slerp(startRot, targetRot, t);
        yield return null;
    }

    yield return new WaitForSeconds(sitTime); // сидит на кровати
    // плавно опускается ещё на 0.4f вниз перед вставанием
    Vector3 downStart = transform.position;
    Vector3 downTarget = downStart + Vector3.down * 0.4f;
    float td = 0;
    while (td < 1f)
    {
        td += Time.deltaTime / sitMoveTime;
        transform.position = Vector3.Lerp(downStart, downTarget, td);
        yield return null;
    }
    yield return new WaitForSeconds(0.1f); // ждём перед вставанием
    animator.SetTrigger("StandUp");
    // вставание: спрыгивает вправо и на пол
    Vector3 jumpStart = transform.position;
    Vector3 jumpTarget = jumpStart + Vector3.right * jumpForward; // спрыгивает вправо
    t = 0;
    while (t < 1f)
    {
        t += Time.deltaTime / jumpTime;
        transform.position = Vector3.Lerp(jumpStart, jumpTarget, t);
        yield return null;
    }

    agent.enabled = true;
    yield return null;
    if (agent.isOnNavMesh) ChaseEnemy();
}
}



