using UnityEngine;
using TMPro;

public class FinalDialogue : MonoBehaviour
{
    [Header("Настройки")]
    public GameObject interactHint;
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public GameObject scientist;  // Учёный (повернётся к игроку)

    [Header("После диалога")]
    public string nextSceneName = "";  // Какая сцена загружается после финала (если нужно)

    private bool isPlayerNear = false;
    private bool dialogueActive = false;
    private int currentLine = 0;
    private Transform player;

    private string[] dialogueLines = new string[]
    {
        "Учёный: Ты вернулся! О, нет... А где же Виктор? Он не справился",
        "Игрок: Записки у меня. Я собрал их все за него.",
        "Учёный: (Берёт записки, дрожащими руками листает)... Это невероятно. Хауэр был гением.",
        "Игрок: Что там? Я не всё понял из этих формул.",
        "Учёный: Слушай внимательно. Вирус создали в 'Генетексе' — это гибрид бешенства, ковида и CRISPR.",
        "Игрок: CRISPR... Это же редактирование генов?",
        "Учёный: Да. Вирус перезаписывает воспоминания и отключает страх. Вот почему они не чувствуют боли.",
        "Игрок: Но как его остановить?",
        "Учёный: Хауэр вывел формулу. Три компонента: теломираза, хелатор железа и gRNA-ловушка на фуллерене C₆₀.",
        "Игрок: А четвёртый? Там написано про спинномозговую жидкость.",
        "Учёный: (Пауза) Спинномозговая жидкость донора с мутацией HLA-G. Это редчайший фенотип.",
        "Игрок: И где нам такого найти?",
        "Учёный: (Смотрит на игрока) Срочно! Срочно дай свою руку!... (Делает укол. Проводит экспресс-тест)",
        "Учёный: Не может быть!..Ты...Ты — носитель HLA-G. Именно поэтому ты не заразился за всё это время.",
        "Игрок: (Шок) Я... я могу быть донором?",
        "Учёный: Да. Если согласишься, из твоей спинномозговой жидкости мы сделаем вакцину.",
        "Учёный: Это единственный способ остановить эпидемию. Но процедура опасна.",
        "Игрок: (Твёрдо) Если это спасёт людей — делай.",
        "Учёный: Ты не представляешь, как я благодарен... (Встаёт, смотрит в сторону)",
        "Учёный: Ты герой. Настоящий герой.",
        "(Учёный кивает и уходит в сторону лаборатории.)",
    };

    void Start()
    {
        if (interactHint != null)
            interactHint.SetActive(false);
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
    }

    void Update()
    {
        if (isPlayerNear && Input.GetKeyDown(KeyCode.E) && !dialogueActive)
        {
            StartDialogue();
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

    void StartDialogue()
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

        ShowLine();
    }

    void LookAtPlayer()
    {
        Vector3 lookPos = player.position - transform.position;
        lookPos.y = 0;
        if (lookPos != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(lookPos);

        if (scientist != null && scientist != gameObject)
        {
            Vector3 lookPos2 = player.position - scientist.transform.position;
            lookPos2.y = 0;
            if (lookPos2 != Vector3.zero)
                scientist.transform.rotation = Quaternion.LookRotation(lookPos2);
        }
    }

    void ShowLine()
    {
        if (currentLine < dialogueLines.Length)
        {
            if (currentLine == dialogueLines.Length - 1)
            {
                dialogueText.text = dialogueLines[currentLine];
            }
            else
            {
                dialogueText.text = dialogueLines[currentLine] + "\n\n<size=60%>Нажмите [E] чтобы продолжить</size>";
            }
        }
        else
        {
            EndDialogue();
        }
    }

    void NextLine()
    {
        if (currentLine < dialogueLines.Length - 1)
        {
            currentLine++;
            ShowLine();
        }
        else
        {
            EndDialogue();
        }
    }

    void EndDialogue()
    {
        dialogueActive = false;
        dialoguePanel.SetActive(false);

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (interactHint != null)
            interactHint.SetActive(false);

        GetComponent<Collider>().enabled = false;

        if (scientist != null)
            StartCoroutine(ScientistWalkAway());

        Debug.Log("Финальный диалог завершён!");

        if (!string.IsNullOrEmpty(nextSceneName))
        {
            GameManager.Instance?.ChangeScene(nextSceneName);
        }
        else
        {
            HintManager.Instance?.ShowHint("Вы спасли человечество. Спасибо за игру!", 5f);
            StartCoroutine(ExitGameDelayed());
        }
    }

    System.Collections.IEnumerator ScientistWalkAway()
    {
        yield return new WaitForSeconds(2f);

        Vector3 walkDirection = new Vector3(20, 0, 0);
        float walkTime = 4f;
        float elapsed = 0;
        Vector3 startPos = scientist.transform.position;
        Vector3 endPos = startPos + walkDirection;

        while (elapsed < walkTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / walkTime;
            scientist.transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        Destroy(scientist);
    }

    System.Collections.IEnumerator ExitGameDelayed()
    {
        yield return new WaitForSeconds(6f);
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}