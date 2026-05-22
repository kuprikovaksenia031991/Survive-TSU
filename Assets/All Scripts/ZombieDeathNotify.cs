using UnityEngine;

public class ZombieDeathNotify : MonoBehaviour
{
    private ZombieSpawner spawner;

    public void Init(ZombieSpawner s)
    {
        spawner = s;
    }

    void OnDestroy()
    {
        if (spawner != null)
            spawner.OnZombieDied();
    }
}