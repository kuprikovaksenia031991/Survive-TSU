using UnityEngine;

public class NoteController : MonoBehaviour
{
    public string noteText = "Здесь будет текст записки";
    public float readDistance = 3f;
    public GameObject noteUI; // UI-панель с текстом

    private bool isReading = false;
    private Transform player;
    private PlayerNoteReader playerReader;

    void Start()
    {
        player = GameObject.FindWithTag("Player").transform;
        playerReader = player.GetComponent<PlayerNoteReader>();
    }

    void Update()
    {
        if (player == null || playerReader == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= readDistance && Input.GetKeyDown(KeyCode.E) && !isReading)
        {
            StartReading();
        }
    }

    void StartReading()
    {
        isReading = true;
        playerReader.OpenNote(noteText, this);
    }

    public void CloseNote()
    {
        isReading = false;
    }
}