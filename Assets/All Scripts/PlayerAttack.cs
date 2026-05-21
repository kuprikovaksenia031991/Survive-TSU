using UnityEditor.AnimatedValues;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerAttack : MonoBehaviour
{
    public int weaponType = 0; // 0 - unArmed 1 - pistol 2 - sword
    private float damage = 15f;
    private float attackCoolDown = 0.7f;
    private float lastAttackTime;
    private float attackRange = 3f;

    [Header("References")]
    public Camera playerCamera;         
    private Animator animator;
    void Awake()
    {
        animator = GetComponent<Animator>();
        UpdateWeaponStats();
    }
    public void Attack()
    {
        if (Time.time - lastAttackTime < attackCoolDown)
            return;
        RaycastHit hit;
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, attackRange))
        {
            if (hit.transform.CompareTag("Enemy"))
            {
                EnemyAI enemy = hit.transform.GetComponent<EnemyAI>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage);
                }
            }
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

            // Проверяем родителя (если попали в модель, а скрипт на петлях)
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
        bool weaponChanged = !(weaponType == type);
        animator.SetBool("WeaponChanged", weaponChanged);

        weaponType = type;
        UpdateWeaponStats();
        animator.SetInteger("WeaponType", weaponType);
    }
    public void UpdateWeaponStats()
    {
        //check weapong
        animator.SetInteger("WeaponType", weaponType);
        switch (weaponType)
        {
            case 0:
                damage = 15f;
                attackCoolDown = 1.3f;
                attackRange = 0.7f;
                break;
            case 1:
                damage = 50f;
                attackCoolDown = 0.7f;
                attackRange = 50f;
                break;
            case 2:
                damage = 80f;
                attackCoolDown = 3.2f;
                attackRange = 1f;
                break;
            default:
                break;
        }
    }
}