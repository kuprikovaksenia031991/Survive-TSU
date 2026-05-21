using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem.XR;

public class EnemyAITrigger : MonoBehaviour
{
    [Header("Movement")]
    public float stopDistance = 1.5f;

    [Header("Combat")]
    public float damage = 10f;
    public float attackCooldown = 3.2f;

    [Header("Stats")]
    public float health = 100f;

    public bool isEnemyTrigger = true;
    public float startWaitTime = 4f;
    public float timeToRotate = 2f;
    public float speedWalk = 6f;
    public float speedRun = 9f;

    public float viewRadius = 15f;
    public float viewAngle = 90f;
    public float meshResolution = 1f;
    public float edgeIterations = 4f;
    public float edgeDistance = 0.5f;

    public Transform[] waypoints;

    private int mCurrentWaypointIndex;
    private Vector3 playerLastPosition = Vector3.zero;
    private Vector3 mPlayerPosition;

    private float mWaitTime;
    private float mTimeToRotate;
    private bool mPlayerInRange;
    private bool mPlayerNear;
    private bool mIsPatrol;
    private bool mCaughtPlayer;

    private Transform player;
    private Transform roommate;
    private NavMeshAgent agent;

    public LayerMask playerMask;
    public LayerMask obstacleMask;

    Animator animator;

    public int weaponType = 0;
    private float lastAttackTime;

    void Start()
    {
        mPlayerPosition = Vector3.zero;
        mIsPatrol = true;
        mCaughtPlayer = false;
        mPlayerInRange = false;
        mWaitTime = startWaitTime;
        mTimeToRotate = timeToRotate;

        mCurrentWaypointIndex = 0;

        player = GameObject.FindWithTag("Player").transform;
        roommate = GameObject.FindWithTag("Roommate Trigger").transform;

        animator = GetComponent<Animator>();
        animator.SetInteger("WeaponType", weaponType); //начальное оружие для зомби - укусы

        agent = GetComponent<NavMeshAgent>();

        //если противник - участник первой катсцены со студентом выбегающим из комнаты, то ему не нужно нападать на игрока пока сосед не погибнет
        if (isEnemyTrigger)
        {
            return;
        }
        agent.isStopped = false;
        agent.speed = speedWalk;
        agent.SetDestination(waypoints[mCurrentWaypointIndex].position);

        agent.stoppingDistance = stopDistance;
    }

    void Update()
    {
       
        if (player == null)
            return;
        //пока сосед не придет и не "погибнет" противник не нападает на героя
        if (Vector3.Distance(transform.position, roommate.position) > 1.2f
            && startWaitTime > 0)
            return;
        startWaitTime-= Time.deltaTime;
        if (startWaitTime > 3)
        {
            return;
        }
        // AI PATHFINDING
        agent.SetDestination(player.position);

        float distance =
            Vector3.Distance(transform.position, player.position);

        // Поворот к игроку
        Vector3 lookPos = player.position - transform.position;
        lookPos.y = 0;

        if (lookPos != Vector3.zero)
        {
            Quaternion rot =
                Quaternion.LookRotation(lookPos);

            transform.rotation =
                Quaternion.Slerp(
                    transform.rotation,
                    rot,
                    Time.deltaTime * 8f
                );
        }

        // Attack
        if (distance <= stopDistance + 0.3f)
        {
            TryAttack();
        }
        UpdateAnimator();
        
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
        health -= dmg;

        Debug.Log(
            "Враг получил урон: " +
            dmg +
            ". HP врага: " +
            health
        );

        if (health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("ВРАГ УБИТ!");

        agent.enabled = false;

        Destroy(gameObject);
    }
}
