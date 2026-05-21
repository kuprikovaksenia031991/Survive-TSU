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
    private float maxChaseDistance = 2.5f;

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
    private bool mIsPatrol;
    private bool mCaughtPlayer;

    private Transform player;
    private NavMeshAgent agent;

    public Transform guardRoom;
    public string enemyTag;

    public LayerMask playerMask;
    public LayerMask obstacleMask;

    Animator animator;

    public int weaponType = 0;
    private float lastAttackTime;

    void Start()
    {
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
        EnviromentView();

        if (!mIsPatrol)
        {
            Chasing();
        }
        else
        {
            Patrolling();
        }
        /*
        if (player == null)
            return;

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

        */
        float distance =
            Vector3.Distance(transform.position, player.position);
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
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(enemyTag))
        {
            GameObject enemyGuard = GameObject.Find(enemyTag);
            if (enemyGuard != null)
            {
                mPlayerNear = true;
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(enemyTag))
        {
            GameObject enemyGuard_ = GameObject.Find(enemyTag);
            if (enemyGuard_ != null)
            {
                mPlayerNear = false;
            }
        }
    }
    private void Chasing()
    {
        float distanceToRoom = Vector3.Distance(transform.position, guardRoom.position);

        // Если зомби отошёл слишком далеко — возвращаемся
        if (distanceToRoom > maxChaseDistance + 5f)
        {
            Patrolling();
            return;
        }
        mPlayerNear = false;
        playerLastPosition = Vector3.zero;

        if (!mCaughtPlayer)
        {
            Move(speedRun);
            agent.SetDestination(mPlayerPosition);
        }
        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            if (mWaitTime <= 0 && !mCaughtPlayer && Vector3.Distance(transform.position, 
                         GameObject.FindGameObjectWithTag("Player").transform.position) >= 6f)
            {
                mIsPatrol = true;
                mPlayerNear = false;
                Move(speedWalk);
                mTimeToRotate = timeToRotate;
                mWaitTime = startWaitTime;
                agent.SetDestination(waypoints[mCurrentWaypointIndex].position);
            }
            else
            {
                if (Vector3.Distance(transform.position, 
                        GameObject.FindGameObjectWithTag("Player").transform.position) >= 2.5f)
                {
                    Stop();
                    mWaitTime -= Time.deltaTime;
                }
            }
        }
    }
    private void Patrolling()
    {
        if (mPlayerNear)
        {
            if (mTimeToRotate <= 0)
            {
                Move(speedWalk);
                LookingPlayer(playerLastPosition);
                
            }
            else
            {
                Stop();
                mTimeToRotate -= Time.deltaTime;
            }
        }
        else
        {
            mPlayerNear = false;
            playerLastPosition = Vector3.zero;
            agent.SetDestination(waypoints[mCurrentWaypointIndex].position);
            if (agent.remainingDistance <= agent.stoppingDistance)
            {
                if (mWaitTime <= 0)
                {
                    NextPoint();
                    Move(speedWalk);
                    mWaitTime = startWaitTime;
                }
                else
                {
                    Stop();
                    mWaitTime -= Time.deltaTime;
                }
            }
        }
    }
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
    private void NextPoint()
    {
        Vector3 randomOffset = new Vector3(Random.Range(-1, 1f), 0, Random.Range(-1f, 1f)); // 1f - радиус комнаты
        Vector3 nextPoint = guardRoom.position + randomOffset;
        agent.SetDestination(nextPoint);
    }
        
    void CaughtPlayer()
    {
        mCaughtPlayer = true;
    }
    void LookingPlayer(Vector3 player)
    {
        agent.SetDestination(player);
        if (Vector3.Distance(transform.position, player) <= 0.3) ;
        {
            if (mWaitTime <= 0)
            {
                mPlayerNear = false;
                Move(speedWalk);
                agent.SetDestination(waypoints[mCurrentWaypointIndex].position);
                mWaitTime = startWaitTime;
                mTimeToRotate = timeToRotate;
            }
            else
            {
                Stop();
                mWaitTime -= Time.deltaTime;
            }
        }
    }
    void EnviromentView()
    {
        Collider[] playerInRange = Physics.OverlapSphere(transform.position, viewRadius, playerMask);
        for (int i = 0; i < playerInRange.Length; i++)
        {
            Transform player = playerInRange[i].transform;
            Vector3 dirToPlayer = (player.position - transform.position).normalized;
            if (Vector3.Angle(transform.forward, dirToPlayer)< viewAngle/2f)
            {
                float dstToPlayer = Vector3.Distance(transform.position, player.position);
                if (!Physics.Raycast(transform.position, dirToPlayer, dstToPlayer, obstacleMask))
                {
                    mPlayerInRange = true;
                    mIsPatrol = false;
                }
                else
                {
                    mPlayerInRange = false;
                }
            }
            if (Vector3.Distance(transform.position, player.position) > viewRadius)
            {
                mPlayerInRange = false;
            }
        }
        if (mPlayerInRange)
        {
            mPlayerPosition = player.transform.position;
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