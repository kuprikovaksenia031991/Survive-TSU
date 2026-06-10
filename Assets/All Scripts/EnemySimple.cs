using UnityEngine;

public class EnemySimple : MonoBehaviour
{
    public static System.Action OnZombieDied;

    [Header("Movement")]
    public float speed = 5f;
    public float stopDistance = 1.5f;

    [Header("Combat")]
    public float damage = 10f;
    public float attackCooldown = 1.5f;

    [Header("Stats")]
    public float health = 80f;
    public float viewRadius = 20f;

    private Transform player;
    private Animator animator;
    private float lastAttackTime;
    private float groundY;
    private bool isDead = false;
    private static int deathCounter = 0;  // Для отладки

    void Start()
    {
        player = GameObject.FindWithTag("Player")?.transform;
        animator = GetComponent<Animator>();
        groundY = transform.position.y;
        Debug.Log($"Зомби {gameObject.name} активирован, здоровье: {health}");
    }

    void Update()
    {
        if (player == null || health <= 0 || isDead) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= viewRadius)
        {
            Vector3 direction = (player.position - transform.position).normalized;
            direction.y = 0;

            Vector3 newPos = transform.position + direction * speed * Time.deltaTime;
            newPos.y = groundY;
            transform.position = newPos;

            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation,
                    Quaternion.LookRotation(direction), Time.deltaTime * 8f);
            }

            if (animator != null)
                animator.SetFloat("Speed", speed);

            if (distanceToPlayer <= stopDistance)
            {
                TryAttack();
            }
        }
        else
        {
            if (animator != null)
                animator.SetFloat("Speed", 0);
        }
    }

    void TryAttack()
    {
        if (Time.time - lastAttackTime < attackCooldown) return;

        if (animator != null)
            animator.SetTrigger("Attack");

        PlayerStats stats = player?.GetComponent<PlayerStats>();
        if (stats != null)
        {
            stats.TakeDamage(damage);
            lastAttackTime = Time.time;
        }
    }

    public void TakeDamage(float dmg)
    {
        if (isDead) return;

        health -= dmg;
        Debug.Log($"Зомби {gameObject.name} получил урон {dmg}. Осталось HP: {health}");

        if (health <= 0)
            Die();
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        deathCounter++;
        Debug.Log($"✅ Зомби {gameObject.name} убит! Всего убито: {deathCounter}");

        OnZombieDied?.Invoke();

        if (animator != null)
            animator.SetTrigger("Die");

        Destroy(gameObject, 2f);
    }
}