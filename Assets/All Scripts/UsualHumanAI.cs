using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class WarriorHumanAIs : MonoBehaviour
{
    [Header("Movement")]
    public float stopDistance = 1.5f;

    [Header("Combat")]
    public float damage = 10f;
    public float attackCooldown = 3.2f;

    [Header("Stats")]
    public float health = 100f;

    private Transform enemy;
    private Transform player;
    private NavMeshAgent agent;

    Animator animator;

    private float lastAttackTime;

    void Start()
    {
        player = GameObject.FindWithTag("Player").transform;
        enemy = GameObject.FindWithTag("Enenemy").transform;

        agent = GetComponent<NavMeshAgent>();

        agent.stoppingDistance = stopDistance;

        animator = GetComponent<Animator>();
        //animator.SetTrigger("")
    }

    void Update()
    {
        if (player == null)
            return;

        // AI PATHFINDING
        agent.SetDestination(enemy.position);

        float distance =
            Vector3.Distance(transform.position, enemy.position);

        Vector3 horizontalVelocity = new Vector3(transform.position.x, 0, transform.position.z);
        float speed = horizontalVelocity.magnitude;
        animator.SetFloat("VelX", horizontalVelocity.x);
        animator.SetFloat("VelY", horizontalVelocity.z);
        animator.SetFloat("Speed", speed);
        // Поворот к игроку
        Vector3 lookPos = enemy.position - transform.position;
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
