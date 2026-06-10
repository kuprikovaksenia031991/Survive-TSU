using UnityEngine;

public class ZombieSpawner : MonoBehaviour
{
    [Header("Префаб зомби")]
    public GameObject zombiePrefab;

    [Header("Настройки этого спавнера")]
    public int zombiesToSpawn = 3;        // Сколько зомби выйдет из этого спавнера
    public float spawnDelay = 2f;         // Задержка перед первым спавном
    public float spawnInterval = 3f;      // Интервал между спавнами

    private int spawnedCount = 0;
    private bool isSpawning = true;

    void Start()
    {
        InvokeRepeating("TrySpawn", spawnDelay, spawnInterval);
    }

    void TrySpawn()
    {
        if (!isSpawning) return;
        if (zombiePrefab == null)
        {
            Debug.LogWarning("Зомби префаб не подключён!");
            return;
        }

        if (spawnedCount >= zombiesToSpawn)
        {
            isSpawning = false;
            CancelInvoke("TrySpawn");
            Debug.Log($"Спавнер {gameObject.name} завершил работу, выпущено {spawnedCount} зомби");
            return;
        }

        Vector3 randomPos = transform.position + Random.insideUnitSphere * 2f;
        randomPos.y = 0;

        GameObject newZombie = Instantiate(zombiePrefab, randomPos, Quaternion.identity);
        spawnedCount++;

        // Добавляем компонент для уведомления о смерти (если нет)
        var deathScript = newZombie.GetComponent<ZombieDeathNotify>();
        if (deathScript == null)
            deathScript = newZombie.AddComponent<ZombieDeathNotify>();
        deathScript.Init(this);

        Debug.Log($"Спавнер {gameObject.name}: выпущен зомби {spawnedCount}/{zombiesToSpawn}");
    }

    public void OnZombieDied()
    {
        // Можно добавить логику, если нужно
        Debug.Log($"Зомби из спавнера {gameObject.name} умер");
    }
}