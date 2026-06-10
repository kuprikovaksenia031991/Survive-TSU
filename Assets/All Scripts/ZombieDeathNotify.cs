using UnityEngine;

public class ZombieDeathNotify : MonoBehaviour
{
    private ZombieSpawner spawner;

    public void Init(ZombieSpawner owner)
    {
        spawner = owner;

        // Находим скрипт зомби и подписываемся на смерть
        EnemySimple enemy = GetComponent<EnemySimple>();
        if (enemy != null)
        {
            // Добавляем событие при смерти
            // EnemySimple.OnZombieDied += OnDeath;
        }
    }

    void OnDeath()
    {
        if (spawner != null)
            spawner.OnZombieDied();
        Destroy(gameObject, 2f);
    }

    void OnDestroy()
    {
        EnemySimple.OnZombieDied -= OnDeath;
    }
}