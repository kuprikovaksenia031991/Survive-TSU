using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine.InputSystem.XR;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    [Header("Movement")]
    public float stopDistance = 1.5f;

    [Header("Combat")]
    public float damage = 10f;
    public float attackCooldown = 3.2f;

    [Header("Stats")]
    public float health = 100f;

    public bool isEnemyTrigger = false;
    public float startWaitTime = 4f;
    public float timeToRotate = 2f;
    public float speedWalk = 2f;
    public float speedRun = 3f;
    public float maxChaseDistance = 6f; // ����������, ������ �������� �� ������� ������ �������

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

    private Transform player;
    private NavMeshAgent agent;

    public Transform guardRoom;
   // public DoorController temporaryDoor;
    public string enemyTag;

    public LayerMask playerMask;
    public LayerMask obstacleMask;

    Animator animator;

    public int weaponType = 0;
    private float lastAttackTime;

    void Start()
    {
       // temporaryDoor.ToggleDoor();
        enemyTag = transform.tag;

        mPlayerPosition = Vector3.zero;
        mIsPatrol = true;
        mCaughtPlayer = false;
        mPlayerInRange = false;
        mWaitTime = startWaitTime;
        mTimeToRotate = timeToRotate;

        mCurrentWaypointIndex = 0;

        player = GameObject.FindWithTag("Player").transform;

        animator = GetComponent<Animator>();
        animator.SetInteger("WeaponType", weaponType); //��������� ������ ��� ����� - �����

        agent = GetComponent<NavMeshAgent>();

        //���� ��������� - �������� ������ �������� �� ��������� ���������� �� �������, �� ��� �� ����� �������� �� ������ ���� ����� �� ��������
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

        float distanceToRoom = Vector3.Distance(transform.position, guardRoom.position);
        if (!mIsPatrol && distanceToRoom > maxChaseDistance)
        {
            mIsPatrol = true;

            mPlayerInRange = false;
            mPlayerNear = false;
            mCaughtPlayer = false;

            agent.isStopped = false;
            agent.speed = speedWalk;

            agent.SetDestination(guardRoom.position);

            return;
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

        // ���� ������� ������ �� ������� � ������������
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

        // ���� ����� ����� � ����������
        if (distanceToPlayer > stopDistance)
        {
            agent.isStopped = false;

            Move(speedRun);
            agent.SetDestination(player.position);
        }
        else
        {
            // ��������������� ��� �����
            agent.isStopped = true;

            TryAttack();
        }
    }

    private void Patrolling()
    {
        if (guardRoom == null)
            return;

        agent.isStopped = false;
        agent.speed = speedWalk;

        // ��������� ��������� ������� ������
        if (mPlayerNear)
        {
            agent.SetDestination(playerLastPosition);

            if (!agent.pathPending &&
                agent.remainingDistance <= 0.5f)
            {
                mPlayerNear = false;

                // ������������ �����
                //  agent.SetDestination(guardRoom.position);
            }

            return;
        }

        // ������ ���� �����
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
        // ����������� ����� ���� � ��������� ��������� ����� ������
        agent.SetDestination(checkPosition);

        // ���� ����� ����� ����� �� ���� ����� (�������� ������ 0.5 ������)
        if (!agent.pathPending && agent.remainingDistance <= 0.5f)
        {
            // ����� ������, �� ������ ��� ���. �� �������� "������������" (���� ����� mWaitTime)
            Stop();
            mWaitTime -= Time.deltaTime;

            if (mWaitTime <= 0)
            {
                // ����� �������� �����, ����� ������ � ������������ � �������� ��������������
                mPlayerNear = false;
                Move(speedWalk);
                // GoToNextWaypoint(); // ���� � ��������� ����� ������
                mWaitTime = startWaitTime; // ���������� ������ �������� �� �������
                mTimeToRotate = timeToRotate;
            }
        }
    }

    void EnviromentView()
    {
        // ���� ������ � �������
        Collider[] playerInRange = Physics.OverlapSphere(transform.position, viewRadius, playerMask);

        if (playerInRange.Length > 0)
        {
            Transform targetPlayer = playerInRange[0].transform;
            Vector3 dirToPlayer = (targetPlayer.position - transform.position).normalized;

            // ��������� ����� ������ (����)
            if (Vector3.Angle(transform.forward, dirToPlayer) < viewAngle / 2f)
            {
                float dstToPlayer = Vector3.Distance(transform.position, targetPlayer.position);

                // ��������� ����� (Raycast)
                if (!Physics.Raycast(transform.position, dirToPlayer, dstToPlayer, obstacleMask))
                {
                    // ����� ����� ������ ����� ������
                    mPlayerInRange = true;
                    mIsPatrol = false; // �������� ����� ������

                    // ��������� ��������� ������� ������� ������
                    mPlayerPosition = targetPlayer.position;
                    playerLastPosition = targetPlayer.position; // ���������� ��� ��������
                    return;
                }
            }
        }

        // ���� ����� ����� ���� � ����� �� ����� ������ ����� ������ (����� �� ����� ��� ������)
        if (mPlayerInRange)
        {
            mPlayerInRange = false;

            // ���������� ��������� �����
            playerLastPosition = mPlayerPosition;

            // ���� ���������
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

            Debug.Log("���� �ܨ�! ����: " + damage);

            lastAttackTime = Time.time;
        }
    }

    public void TakeDamage(float dmg)
    {
        health -= dmg;

        Debug.Log(
            "���� ������� ����: " +
            dmg +
            ". HP �����: " +
            health
        );

        if (health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("���� ����!");

        agent.enabled = false;

        Destroy(gameObject);
    }
}
