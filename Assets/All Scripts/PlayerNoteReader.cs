using UnityEngine;
using UnityEngine.UI;

public class PlayerNoteReader : MonoBehaviour
{
    public GameObject notePanel; // UI панель
    public Text noteTextUI; // текстовое поле
    public SimpleFPSControllers fpsController; // отключение движений при чтении

    private NoteController currentNote;

    void Start()
    {
        notePanel.SetActive(false);
    }

    void Update()
    {
        if (notePanel.activeSelf && Input.GetKeyDown(KeyCode.E))
        {
            CloseNote();
        }
    }

    public void OpenNote(string text, NoteController note)
    {
        currentNote = note;
        noteTextUI.text = text;
        notePanel.SetActive(true);
        fpsController.enabled = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void CloseNote()
    {
        notePanel.SetActive(false);
        fpsController.enabled = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (currentNote != null)
            currentNote.CloseNote();
    }
}