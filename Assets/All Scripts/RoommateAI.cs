using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class RoommateAI : MonoBehaviour
{
    // Start is called before the first frame update
    public float speedWalk = 3f;
    public float startWaitTime = 1f;
    public float distanceToDie = 3f;
    private int countDoorOpen = 0;
    private bool playerDetected = false;

    public DoorController door;
    public NavMeshAgent agent;
    public Animator animator;
    private Transform player;
    private Transform enemy;

    //для корректной анимации падения
    public AnimationFix animFix;

    [Header("Vision Settings")]
    public float viewDistance = 15f;
    public float viewAngle = 90f;
    public LayerMask playerLayer;
    public LayerMask obstacleLayer;


    void Start()
    {
        player = GameObject.FindWithTag("Player").transform;
        enemy = GameObject.FindWithTag("EnemyTrigger").transform;
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        animator.SetTrigger("Sit");
        animFix = GetComponent<AnimationFix>();
    }

    // Update is called once per frame
    void Update()
    {
        DetectPlayer();

        if (playerDetected && countDoorOpen <= 1)
        {
            ChaseEnemy();
            if (startWaitTime <= 0)
            {
                door.ToggleDoor();
                countDoorOpen += 1;
                startWaitTime = 4.8f;
            }
            else
            {
                startWaitTime -= Time.deltaTime;
            }
        }
        if (Vector3.Distance(enemy.position, transform.position) <= distanceToDie)
        {
            animFix.OnDead();
            transform.position += new Vector3(-1f, 0f, 0f);
            animator.SetTrigger("Dead");
            agent.enabled = false;
        }

    }
    void DetectPlayer()
    {
        if (player == null)
        {
            player = GameObject.FindWithTag("Player").transform;
            return;
        }
        if (startWaitTime > 0)
        {
            startWaitTime -= Time.deltaTime;
            return;
        }
        startWaitTime = 1.2f;
        // Получаем forward игрока (куда смотрит игрок)
        Vector3 playerForward = player.transform.forward;

        // Направление от игрока к боту
        Vector3 directionToRoommate = transform.position - player.transform.position;
        float distanceToRoommate = directionToRoommate.magnitude;

        // Проверяем дистанцию
        if (distanceToRoommate > viewDistance) return;

        // Проверяем угол (поле зрения)
        float angle = Vector3.Angle(playerForward, directionToRoommate);
        if (angle > viewAngle / 2f) return;

        // Проверяем есть ли препятствия между игроком и ботом
        if (Physics.Raycast(player.transform.position + Vector3.up,
            directionToRoommate.normalized, distanceToRoommate, obstacleLayer))
        {
            playerDetected = false;
            return; // Стена мешает
        }

        // Игрок обнаружен!
        if (!playerDetected)
        {
            playerDetected = true;
            animator.SetTrigger("StandUp");
            Debug.Log("Roommate");
        }
    }
    void ChaseEnemy()
    {
        animator.SetFloat("VelX", 0.5f);
        agent.isStopped = false;
        agent.speed = speedWalk;
        agent.SetDestination(enemy.position);
    }
    void OnDrawGizmos()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        Gizmos.color = playerDetected ? Color.red : Color.green;

        // Показываем поле зрения игрока
        Vector3 playerForward = player.transform.forward;
        Vector3 leftBoundary = Quaternion.Euler(0, -viewAngle / 2, 0) * playerForward;
        Vector3 rightBoundary = Quaternion.Euler(0, viewAngle / 2, 0) * playerForward;

        Gizmos.DrawRay(player.transform.position, leftBoundary * viewDistance);
        Gizmos.DrawRay(player.transform.position, rightBoundary * viewDistance);
    }
}


