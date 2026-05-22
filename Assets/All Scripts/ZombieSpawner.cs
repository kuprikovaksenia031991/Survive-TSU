using UnityEngine;

public class ZombieSpawner : MonoBehaviour
{
    [Header("Префаб зомби")]
    public GameObject zombiePrefab;

    [Header("Настройки этого спавнера")]
    public float spawnInterval = 5f;
    public float spawnRadius = 3f;

    [Header("Глобальные настройки (общие для всех спавнеров)")]
    public static int totalMaxZombies = 10;  // Общий максимум зомби на всю сцену
    private static int currentTotalZombies = 0;  // Сколько зомби сейчас всего

    void Start()
    {
        InvokeRepeating("TrySpawn", 1f, spawnInterval);
    }

    void TrySpawn()
    {
        if (zombiePrefab == null)
        {
            Debug.LogWarning("Зомби префаб не подключён!");
            return;
        }

        // Проверяем общий лимит
        if (currentTotalZombies >= totalMaxZombies) return;

        Vector3 randomPos = transform.position + Random.insideUnitSphere * spawnRadius;
        randomPos.y = 0;

        GameObject newZombie = Instantiate(zombiePrefab, randomPos, Quaternion.identity);
        currentTotalZombies++;

        // Подписка на смерть
        var deathScript = newZombie.GetComponent<ZombieDeathNotify>();
        if (deathScript == null)
            deathScript = newZombie.AddComponent<ZombieDeathNotify>();
        deathScript.Init(this);
    }

    public void OnZombieDied()
    {
        currentTotalZombies--;
    }
}