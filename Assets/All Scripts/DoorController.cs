using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(AudioSource))]
public class DoorController : MonoBehaviour
{
    public AudioClip openClip, closeClip;
    public float openAngle = -90f;
    public float speed = 3f;

    private AudioSource _asrc;
    private bool _isOpen;
    private Quaternion _closedRot, _openRot;

    // Вместо NavMeshLink используем NavMeshObstacle
    private NavMeshObstacle _obstacle;

    void Awake()
    {
        _asrc = GetComponent<AudioSource>();

        _closedRot = transform.localRotation;
        _openRot = _closedRot * Quaternion.Euler(0f, openAngle, 0f);

        // Добавляем NavMeshObstacle, если его нет
        _obstacle = GetComponent<NavMeshObstacle>();
        if (_obstacle == null)
            _obstacle = gameObject.AddComponent<NavMeshObstacle>();

        // Настройка препятствия
        _obstacle.carving = true; // ВАЖНО: Carving вырезает дыру в NavMesh
        _obstacle.enabled = true;  // Изначально закрыта -> препятствие активно
    
    }

    void Update()
    {
        // плавно вращаем
        var target = _isOpen ? _openRot : _closedRot;
        transform.localRotation = Quaternion.Slerp(transform.localRotation,
                                                   target,
                                                   Time.deltaTime * speed);
    }

    // вызывается из другого скрипта (например, от триггера игрока)
    public void ToggleDoor()
    {
        _isOpen = !_isOpen;

        // Управляем препятствием: если дверь открыта, препятствие выключается
        if (_obstacle != null)
        {
            _obstacle.enabled = !_isOpen;
        }

        // Обычный коллайдер для физики игрока
        var col = GetComponent<Collider>();
        if (col) col.enabled = !_isOpen;

        // звук
        _asrc.clip = _isOpen ? openClip : closeClip;
        _asrc.Play();
    }
}