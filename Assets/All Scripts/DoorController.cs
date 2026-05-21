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

    // NavMeshLink – «мост», который появляется/исчезает
    private NavMeshLink _link;

    void Awake()
    {
        _asrc = GetComponent<AudioSource>();

        _closedRot = transform.localRotation;
        _openRot = _closedRot * Quaternion.Euler(0f, openAngle, 0f);

        // создаём линк, если его нет в иерархии
        _link = GetComponent<NavMeshLink>();
        if (_link == null)
            _link = gameObject.AddComponent<NavMeshLink>();

        // указываем размеры – они должны покрывать проём двери
        _link.startPoint = new Vector3(0f, 0f, -0.5f);
        _link.endPoint = new Vector3(0f, 2f, -0.5f);
        _link.width = 1f;
        _link.costModifier = -1; // обычная стоимость
        _link.autoUpdate = true;  // линк будет следовать за трансформом
        _link.enabled = false;    // изначально закрыта → линк выключен
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

        // включаем/выключаем линк
        _link.enabled = _isOpen;

        // обычный коллайдер двери (чтобы физика не пропускала персонажа)
        var col = GetComponent<Collider>();
        if (col) col.enabled = !_isOpen;

        // звук
        _asrc.clip = _isOpen ? openClip : closeClip;
        _asrc.Play();
    }
}