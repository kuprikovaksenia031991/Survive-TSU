using UnityEngine;
using UnityEngine.AI;                       

[RequireComponent(typeof(NavMeshAgent))]
public class WarriorHumanAI : MonoBehaviour
{
    [Header("Movement")]
    public float stopDistance = 1.5f;

    [Header("Combat")]
    public float damage = 10f;
    public float attackCooldown = 2f;
    public int weaponType = 0;

    [Header("Stats")]
    public float health = 100f;

    private Transform enemy;
    private Transform target;
    private Transform player;
    private NavMeshAgent agent;

    Animator animator;

    private bool firstInteraction = true;
    private float startWaitTime = 5f;
    private float lastAttackTime;

    void Start()
    {
        player = GameObject.FindWithTag("Player").transform;
        enemy = GameObject.FindWithTag("Enemy").transform;

        agent = GetComponent<NavMeshAgent>();

        agent.stoppingDistance = stopDistance;

        animator = GetComponent<Animator>();
        //начальное оружие - руки
        animator.SetInteger("WeaponType", weaponType);
        animator.SetTrigger("Sit");
    }

    void Update()
    {
        if (enemy == null)
            return;
        if (Vector3.Distance(player.position, transform.position) < (stopDistance + 3f) && firstInteraction)
        {
            agent.SetDestination(player.position);
            UpdateAnimator();

        }
        if (startWaitTime <= 0)
            firstInteraction = false;
        else
            startWaitTime -= Time.deltaTime;
        if (Vector3.Distance(player.position, transform.position) > 15f && !firstInteraction)
        {
            agent.SetDestination(player.position);
            target = player;

        }
        else if (Vector3.Distance(enemy.position, transform.position) < 5f)
        {
            agent.SetDestination(enemy.position); // AI PATHFINDING
            target = enemy;
        }
        float distance =
            Vector3.Distance(transform.position, enemy.position);

        // Поворот к игроку
        Vector3 lookPos = target.position - transform.position;
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
        UpdateAnimator();
        // Attack
        if (distance <= stopDistance + 0.3f)
        {
            TryAttack();
        }
        if (enemy == null)
            enemy = GameObject.FindWithTag("Enemy").transform;
    }
    void UpdateAnimator()
    {
        Vector3 velocity = agent.velocity;
        Vector3 horizontalVelocity = new Vector3(velocity.x, 0, velocity.z);
        float speed = horizontalVelocity.magnitude;
        animator.SetFloat("VelX", horizontalVelocity.x);
        animator.SetFloat("VelY", horizontalVelocity.z);
        animator.SetFloat("Speed", speed);
    }
    void TryAttack()
    {
        if (Time.time - lastAttackTime < attackCooldown)
            return;
        animator.SetTrigger("Attack");
        float stats =
            enemy.GetComponent<EnemyAI>().health;

        if (stats != null)
        {
            enemy.GetComponent<EnemyAI>().TakeDamage(damage);

            Debug.Log("СОЮЗНИК АТАКУЕТ! Урон: " + damage);

            lastAttackTime = Time.time;
        }
    }

    public void TakeDamage(float dmg)
    {
        health -= dmg;

        Debug.Log(
            "Союзник получил урон: " +
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
        Debug.Log("Союзник убит УБИТ!");
        animator.SetBool("IsDead", true);

        agent.enabled = false;

        Destroy(gameObject);
    }
}
