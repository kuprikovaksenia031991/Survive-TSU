using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BloodEffect : MonoBehaviour
{
    public static BloodEffect Instance;
    
    [Header("Blood Settings")]
    public Material bloodMaterial;
    public float particleSize = 0.1f;
    public float lifetime = 1f;
    public float speed = 3f;
    public int particleCount = 10;
    
    void Awake()
    {
        Instance = this;
    }
    
    public void SpawnBlood(Vector3 position)
    {
        for (int i = 0; i < particleCount; i++)
        {
            // Создаём сферу
            GameObject blood = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            blood.transform.position = position;
            blood.transform.localScale = Vector3.one * particleSize;
            
            // Удаляем коллайдер (не нужен)
            Destroy(blood.GetComponent<Collider>());
            
            // Назначаем материал
            Renderer rend = blood.GetComponent<Renderer>();
            rend.material = bloodMaterial;
            
            // Добавляем физику
            Rigidbody rb = blood.AddComponent<Rigidbody>();
            rb.useGravity = true;
            
            // Случайное направление
            Vector3 dir = new Vector3(
                Random.Range(-1f, 1f),
                Random.Range(0.5f, 1f),
                Random.Range(-1f, 1f)
            ).normalized;
            rb.velocity = dir * speed;
            
            // Удаляем через время
            Destroy(blood, lifetime);
        }
    }
}