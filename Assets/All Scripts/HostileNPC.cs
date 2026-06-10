using UnityEngine;
using TMPro;

public class HostileNPC : MonoBehaviour
{
    [Header("Настройки диалога")]
    public GameObject interactHint;
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public string[] firstDialogueLines;

    [Header("Зомби у стола")]
    public GameObject[] zombiesToSpawn;

    [Header("После убийства зомби")]
    public GameObject secondNPC;
    public GameObject lootTable;
    public Item rewardNote;

    private bool isPlayerNear = false;
    private bool dialogueActive = false;
    private bool zombiesKilled = false;
    private int currentLine = 0;
    private Transform player;
    private int zombiesRemaining = 0;

    void Start()
    {
        if (interactHint != null)
            interactHint.SetActive(false);
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
        if (lootTable != null)
            lootTable.SetActive(false);

        // Подписываемся на событие смерти зомби
        EnemySimple.OnZombieDied += CheckZombiesDefeated;
    }

    void OnDestroy()
    {
        EnemySimple.OnZombieDied -= CheckZombiesDefeated;
    }

    void Update()
    {
        if (isPlayerNear && Input.GetKeyDown(KeyCode.E) && !dialogueActive)
        {
            if (zombiesKilled)
                StartSecondDialogue();
            else
                StartFirstDialogue();
        }

        if (dialogueActive && Input.GetKeyDown(KeyCode.E))
        {
            NextLine();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;
            if (interactHint != null)
                interactHint.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
            if (interactHint != null)
                interactHint.SetActive(false);
        }
    }

    void StartFirstDialogue()
    {
        dialogueActive = true;
        currentLine = 0;

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        dialoguePanel.SetActive(true);
        interactHint.SetActive(false);

        player = GameObject.FindWithTag("Player").transform;
        LookAtPlayer();

        ShowCurrentLine();
    }

    void StartSecondDialogue()
    {
        dialogueActive = true;
        currentLine = 0;

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        dialoguePanel.SetActive(true);
        interactHint.SetActive(false);

        ShowSecondDialogueLine();
    }

    void ShowCurrentLine()
    {
        if (currentLine < firstDialogueLines.Length)
        {
            dialogueText.text = firstDialogueLines[currentLine] + "\n\n<size=60%>Нажмите [E] чтобы продолжить</size>";
        }
        else
        {
            EndFirstDialogue();
        }
    }

    void ShowSecondDialogueLine()
    {
        string[] lines = new string[]
        {
            "Студент: Ты справился! Круто.",
            "Студент: Забирай свою записку.",
            "Игрок: Спасибо! Это очень поможет."
        };

        if (currentLine < lines.Length)
        {
            dialogueText.text = lines[currentLine] + "\n\n<size=60%>Нажмите [E] чтобы продолжить</size>";
        }
        else
        {
            EndSecondDialogue();
        }
    }

    void NextLine()
    {
        currentLine++;

        if (!zombiesKilled)
            ShowCurrentLine();
        else
            ShowSecondDialogueLine();
    }

    void EndFirstDialogue()
    {
        dialogueActive = false;
        dialoguePanel.SetActive(false);

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Активируем зомби
        zombiesRemaining = 0;
        foreach (GameObject zombie in zombiesToSpawn)
        {
            if (zombie != null)
            {
                zombie.SetActive(true);
                zombiesRemaining++;
            }
        }

        if (interactHint != null)
            interactHint.SetActive(false);

        Debug.Log($"Активировано {zombiesRemaining} зомби! Убейте их и вернитесь.");
        HintManager.Instance?.ShowHint($"Убейте {zombiesRemaining} зомби у стола и вернитесь к студентам", 3f);
    }

    void CheckZombiesDefeated()
    {
        if (zombiesRemaining <= 0) return; // Уже убиты

        zombiesRemaining--;
        Debug.Log($"Зомби осталось: {zombiesRemaining}");

        if (zombiesRemaining <= 0)
        {
            zombiesKilled = true;
            if (interactHint != null)
                interactHint.SetActive(true);
            HintManager.Instance?.ShowHint("Все зомби убиты! Вернитесь к студентам.", 2f);
            Debug.Log("✅ Все зомби убиты!");
        }
    }

    void EndSecondDialogue()
    {
        dialogueActive = false;
        dialoguePanel.SetActive(false);

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Активируем лут
        if (lootTable != null)
            lootTable.SetActive(true);

        // Добавляем записку
        if (rewardNote != null && Inventory.Instance != null)
        {
            Inventory.Instance.AddItem(rewardNote, 1);
            HintManager.Instance?.ShowHint("Вы получили записку! Нажмите [I] для инвентаря", 3f);
        }

        // Убираем студентов
        if (secondNPC != null)
            Destroy(secondNPC);
        Destroy(gameObject);

        Debug.Log("Записка получена!");
    }

    void LookAtPlayer()
    {
        Vector3 lookPos = player.position - transform.position;
        lookPos.y = 0;
        if (lookPos != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(lookPos);

        if (secondNPC != null)
        {
            Vector3 lookPos2 = player.position - secondNPC.transform.position;
            lookPos2.y = 0;
            if (lookPos2 != Vector3.zero)
                secondNPC.transform.rotation = Quaternion.LookRotation(lookPos2);
        }
    }
}