using UnityEngine;

public class ZombieSpawner : MonoBehaviour
{
    [Header("Префаб зомби (перетащишь потом)")]
    public GameObject zombiePrefab;  // Сюда готового зомби

    [Header("Настройки спавна")]
    public int maxZombies = 3;
    public float spawnInterval = 5f;
    public float spawnRadius = 3f;

    private int currentZombieCount = 0;

    void Start()
    {
        InvokeRepeating("TrySpawn", 1f, spawnInterval);
    }

    void TrySpawn()
    {
        if (zombiePrefab == null)
        {
            Debug.LogWarning("Зомби префаб не подключён! Перетащи его в поле zombiePrefab");
            return;
        }

        if (currentZombieCount >= maxZombies) return;

        Vector3 randomPos = transform.position + Random.insideUnitSphere * spawnRadius;
        randomPos.y = 0;

        GameObject newZombie = Instantiate(zombiePrefab, randomPos, Quaternion.identity);
        currentZombieCount++;

        // Подписка на смерть (опционально)
        var deathScript = newZombie.GetComponent<ZombieDeathNotify>();
        if (deathScript == null)
            deathScript = newZombie.AddComponent<ZombieDeathNotify>();
        deathScript.Init(this);
    }

    public void OnZombieDied()
    {
        currentZombieCount--;
    }
}