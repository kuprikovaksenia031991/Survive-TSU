using UnityEngine;

public class TriggerTester : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"TriggerTester: {other.name} вошёл в триггер! Тег: {other.tag}");
    }

    void OnTriggerExit(Collider other)
    {
        Debug.Log($"TriggerTester: {other.name} вышел из триггера");
    }
}