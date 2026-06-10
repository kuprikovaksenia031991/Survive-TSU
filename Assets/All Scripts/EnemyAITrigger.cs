using UnityEngine;
using UnityEngine.AI;

public class EnemyAITrigger : MonoBehaviour
{
    [Header("Movement")]
    public float stopDistance = 1.5f;
    public float speedWalk = 2f;
    public float speedRun = 5f;

    [Header("Combat")]
    public float damage = 10f;
    public float attackCooldown = 1.5f;

    [Header("Stats")]
    public float health = 80f;
    public float viewRadius = 30f;

    public bool isEnemyTrigger = true;
    public float startWaitTime = 4f;

    private Transform player;
    private NavMeshAgent agent;
    private Animator animator;
    private float lastAttackTime;
    private bool isActive = false;

    void Start()
    {
        player = GameObject.FindWithTag("Player")?.transform;
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        if (agent != null)
        {
            agent.stoppingDistance = stopDistance;
            agent.speed = speedWalk;
        }

        if (isEnemyTrigger)
        {
            isActive = false;
            if (agent != null) agent.isStopped = true;
        }
        else
        {
            isActive = true;
        }
    }

    void Update()
    {
        if (!isActive) return;
        if (player == null || health <= 0) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= viewRadius)
        {
            if (agent != null)
            {
                agent.speed = speedRun;
                agent.SetDestination(player.position);
                agent.isStopped = false;
            }
        }

        if (distanceToPlayer <= stopDistance)
        {
            TryAttack();
        }

        UpdateAnimator();
    }

    public void Activate()
    {
        isActive = true;
        if (agent != null)
        {
            agent.isStopped = false;
            agent.speed = speedRun;
        }
        Debug.Log("Зомби активирован!");
    }

    void TryAttack()
    {
        if (Time.time - lastAttackTime < attackCooldown) return;

        if (animator != null) animator.SetTrigger("Attack");

        PlayerStats stats = player?.GetComponent<PlayerStats>();
        if (stats != null)
        {
            stats.TakeDamage(damage);
            lastAttackTime = Time.time;
        }
    }

    void UpdateAnimator()
    {
        if (animator == null || agent == null) return;
        animator.SetFloat("Speed", agent.velocity.magnitude);
    }

    public void TakeDamage(float dmg)
    {
        health -= dmg;
        Debug.Log($"Зомби получил урон {dmg}. Осталось HP: {health}");

        if (health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("Зомби убит!");
        if (animator != null) animator.SetTrigger("Die");
        if (agent != null) agent.enabled = false;
        Destroy(gameObject, 2f);
    }
}