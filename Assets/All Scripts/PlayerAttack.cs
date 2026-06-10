using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public int weaponType = 0;
    private float damage = 15f;
    private float attackCoolDown = 0.7f;
    private float lastAttackTime;
    private float attackRange = 2.5f;

    [Header("References")]
    public Camera playerCamera;
    private Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
        UpdateWeaponStats();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Attack();
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            InteractWithDoor();
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            EnemySimple[] enemies = FindObjectsOfType<EnemySimple>();
            foreach (EnemySimple e in enemies)
            {
                e.TakeDamage(999);
            }
            Debug.Log("K нажата — урон 999 всем зомби (EnemySimple)");
        }
    }

    public void Attack()
    {
        if (Time.time - lastAttackTime < attackCoolDown) return;

        RaycastHit hit;
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, attackRange))
        {
            Debug.Log($"Raycast попал в: {hit.collider.name}, тег: {hit.collider.tag}");

            if (hit.transform.CompareTag("Enemy"))
            {
                Debug.Log("Попали во врага!");

                // Для зомби с EnemySimple (лесные зомби)
                EnemySimple enemySimple = hit.transform.GetComponent<EnemySimple>();
                if (enemySimple != null)
                {
                    enemySimple.TakeDamage(damage);
                    Debug.Log($"Атака! Урон {damage} по EnemySimple");
                }

                // Для зомби с EnemyAITrigger (общага)
                EnemyAITrigger enemyTrigger = hit.transform.GetComponent<EnemyAITrigger>();
                if (enemyTrigger != null)
                {
                    enemyTrigger.TakeDamage(damage);
                    Debug.Log($"Атака! Урон {damage} по EnemyAITrigger");
                }
            }
        }
        else
        {
            Debug.Log("Raycast никуда не попал");
        }

        lastAttackTime = Time.time;
        animator.SetTrigger("Attack");
    }

    public void InteractWithDoor()
    {
        RaycastHit hit;
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, 3f))
        {
            DoorController door = hit.transform.GetComponent<DoorController>();
            if (door != null)
            {
                door.ToggleDoor();
                return;
            }

            if (hit.transform.parent != null)
            {
                door = hit.transform.parent.GetComponent<DoorController>();
                if (door != null)
                {
                    door.ToggleDoor();
                }
            }
        }
    }

    public void SetWeaponType(int type)
    {
        weaponType = type;
        UpdateWeaponStats();
        animator.SetInteger("WeaponType", weaponType);
        Debug.Log($"Оружие сменено на тип {type}");
    }

    public void UpdateWeaponStats()
    {
        animator.SetInteger("WeaponType", weaponType);
        switch (weaponType)
        {
            case 0: // Руки
                damage = 25f;
                attackCoolDown = 0.7f;
                attackRange = 2.5f;
                break;
            case 1: // Нож
                damage = 45f;
                attackCoolDown = 0.5f;
                attackRange = 2.5f;
                break;
            case 2: // Огнетушитель
                damage = 35f;
                attackCoolDown = 1f;
                attackRange = 2.8f;
                break;
        }
        Debug.Log($"Оружие обновлено: тип={weaponType}, урон={damage}");
    }
}