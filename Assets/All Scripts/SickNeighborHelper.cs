using UnityEngine;
using UnityEngine.AI;

public class SickNeighborHelper : MonoBehaviour
{
    [Header("Настройки помощи")]
    public float damage = 15f;
    public float attackCooldown = 1.5f;
    public float attackRange = 5f; // Увеличил радиус атаки

    private GameObject enemyToHelp;
    private Animator animator;
    private NavMeshAgent agent;
    private float lastAttackTime;
    private bool isHelping = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();

        if (agent != null)
        {
            agent.isStopped = true; // Сосед не двигается, стоит на месте
        }

        if (animator != null)
            animator.SetTrigger("StandUp");
    }

    public void SetEnemy(GameObject enemy)
    {
        enemyToHelp = enemy;
        isHelping = true;
        Debug.Log($"SickNeighborHelper: Активирован! Буду атаковать если враг подойдёт близко");
    }

    void Update()
    {
        if (!isHelping || enemyToHelp == null || !enemyToHelp.activeSelf)
        {
            return;
        }

        // Просто проверяем расстояние до врага
        float distance = Vector3.Distance(transform.position, enemyToHelp.transform.position);

        // Если враг в радиусе атаки - бьём
        if (distance <= attackRange)
        {
            TryAttack();
        }

        // Поворот к врагу (чтобы смотреть в сторону зомби)
        if (enemyToHelp != null)
        {
            Vector3 lookPos = enemyToHelp.transform.position - transform.position;
            lookPos.y = 0;
            if (lookPos != Vector3.zero)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookPos), Time.deltaTime * 5f);
        }
    }

    void TryAttack()
    {
        if (Time.time - lastAttackTime < attackCooldown) return;

        if (animator != null)
            animator.SetTrigger("Attack");

        EnemyAITrigger enemy = enemyToHelp?.GetComponent<EnemyAITrigger>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            Debug.Log($"Больной сосед помогает! Урон: {damage}");
            lastAttackTime = Time.time;
        }
    }
}