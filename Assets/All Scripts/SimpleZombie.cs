using UnityEngine;

public class SimpleZombie : MonoBehaviour
{
    [Header("Настройки движения")]
    public float speed = 2.5f;
    public float damage = 10f;
    public float attackCooldown = 1.5f;
    public float attackRange = 1.8f;
    public float health = 80f;

    [Header("Активация")]
    public bool activateByDoor = true;      // Ждать открытия двери?
    public DoorController linkedDoor;       // Дверь, после открытия которой зомби активируется
    public float activationDelay = 1f;      // Задержка перед активацией после открытия двери

    [Header("Преследование")]
    public float chaseSpeed = 4f;           // Скорость при преследовании
    public float walkSpeed = 2f;            // Скорость до активации

    private Transform player;
    private Animator animator;
    private float lastAttackTime;
    private bool isDead = false;
    private bool isActivated = false;        // Активирован ли зомби (начинает преследование)
    private float currentSpeed;

    void Start()
    {
        player = GameObject.FindWithTag("Player")?.transform;
        animator = GetComponent<Animator>();
        currentSpeed = walkSpeed;

        // Если активация по двери, зомби сначала стоит
        if (activateByDoor && linkedDoor != null)
        {
            isActivated = false;
            currentSpeed = 0;
            if (animator != null) animator.SetFloat("Speed", 0);
            Debug.Log($"{gameObject.name}: Ожидает открытия двери {linkedDoor.name}");
        }
        else
        {
            isActivated = true;
        }
    }

    void Update()
    {
        if (isDead || player == null) return;

        // Проверяем активацию по двери
        if (!isActivated && activateByDoor && linkedDoor != null)
        {
            if (linkedDoor.IsOpen)
            {
                StartCoroutine(ActivateWithDelay());
                return;
            }
        }

        if (!isActivated) return;

        // ===== ПРЕСЛЕДОВАНИЕ (после активации) =====
        float distance = Vector3.Distance(transform.position, player.position);

        // Всегда двигаемся к игроку после активации
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0;
        transform.position += direction * currentSpeed * Time.deltaTime;

        // Поворот к игроку
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation,
                Quaternion.LookRotation(direction), Time.deltaTime * 8f);
        }

        // Анимация движения
        if (animator != null) animator.SetFloat("Speed", currentSpeed);

        // Атака
        if (distance <= attackRange)
        {
            TryAttack();
        }
    }

    System.Collections.IEnumerator ActivateWithDelay()
    {
        Debug.Log($"{gameObject.name}: Дверь открыта! Активация через {activationDelay} секунд");
        yield return new WaitForSeconds(activationDelay);

        isActivated = true;
        currentSpeed = chaseSpeed;

        if (animator != null)
        {
            animator.SetTrigger("StandUp");
            animator.SetFloat("Speed", currentSpeed);
        }

        Debug.Log($"{gameObject.name}: Активирован! Начинает преследование");
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
            Debug.Log($"{gameObject.name} атакует! Урон: {damage}");
        }
    }

    public void TakeDamage(float dmg)
    {
        if (isDead) return;

        health -= dmg;
        Debug.Log($"{gameObject.name} получил урон {dmg}. Осталось HP: {health}");

        if (health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;
        Debug.Log($"{gameObject.name} убит!");

        if (animator != null) animator.SetTrigger("Die");

        Destroy(gameObject, 2f);
    }
}