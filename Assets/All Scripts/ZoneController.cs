using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class ZoneController : MonoBehaviour
{
    // Start is called before the first frame update
    public string enemyTag;
    private Vector3 resetPosition;
    private NavMeshAgent enemyAgent;
    public bool isInside = true;
    void Start()
    {
        resetPosition = new Vector3(transform.position.x, 0f, transform.position.z);
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(enemyTag))
        {
            isInside = false;
            if (other.TryGetComponent<UnityEngine.AI.NavMeshAgent>(out var agent))
            {
                enemyAgent = agent;
                // Ищем ближайшую безопасную точку на NavMesh в радиусе 2 метров от resetPosition
                if (NavMesh.SamplePosition(resetPosition, out NavMeshHit hit, 0.1f, NavMesh.AllAreas))
                {
                    // Warp — это специальный телепорт для NavMeshAgent, который не ломает логику
                    agent.Warp(resetPosition);

                    agent.ResetPath(); // СТИРАЕМ СТАРУЮ ЦЕЛЬ! Теперь зомби просто стоит в центре.
                    Debug.Log($"Зомби успешно возвращен на NavMesh в точку: {hit.position}");
                }
                else
                {
                    // Если центр комнаты совсем заблокирован, телепортируем хотя бы просто в координаты
                    agent.Warp(resetPosition);
                    Debug.LogWarning("Центр комнаты вне сетки NavMesh! Проверьте запекание.");
                }
            }
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(enemyTag))
        {
            isInside = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!isInside)
        {
            enemyAgent.SetDestination(resetPosition);
        }
    }
}

