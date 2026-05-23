using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class OpeningCutscene : MonoBehaviour
{
    [Header("Персонажи")]
    public GameObject player;
    public GameObject neighbourRoom;
    public GameObject sickNeighbour;
    public GameObject zombie;

    [Header("Двери")]
    public DoorController roomDoor;

    [Header("Аниматоры")]
    public Animator playerAnimator;
    public Animator neighbourAnimator;
    public Animator zombieAnimator;

    [Header("Звуки")]
    public AudioSource audioSource;
    public AudioClip screamSound;
    public AudioClip doorOpenSound;
    public AudioClip doorCloseSound;

    [Header("Точки")]
    public Transform playerBedPosition;
    public Transform neighbourBedPosition;
    public Transform neighbourRunTarget;
    public Transform neighbourDeathPosition;
    public Transform zombiePosition;
    public Transform sickNeighbourBedPosition;

    [Header("Диалог")]
    public GameObject dialoguePanel;
    public string[] neighbourLines;
    public string[] playerLines;

    private SimpleFPSController fpsController;
    private PlayerAttack playerAttack;
    private TextMeshProUGUI dialogueText;

    void Start()
    {
        fpsController = player.GetComponent<SimpleFPSController>();
        playerAttack = player.GetComponent<PlayerAttack>();

        if (fpsController != null) fpsController.enabled = false;
        if (playerAttack != null) playerAttack.enabled = false;

        // Отключаем RoommateAI
        RoommateAI roommateAI = sickNeighbour.GetComponent<RoommateAI>();
        if (roommateAI != null) roommateAI.enabled = false;

        // Находим текст внутри панели диалога
        if (dialoguePanel != null)
        {
            dialogueText = dialoguePanel.GetComponentInChildren<TextMeshProUGUI>();
            if (dialogueText == null)
                Debug.LogError("TextMeshPro не найден внутри DialoguePanel!");
            else
                Debug.Log("DialogueText найден: " + dialogueText.name);
        }

        // Все на места
        player.transform.position = playerBedPosition.position;
        player.transform.rotation = playerBedPosition.rotation;

        neighbourRoom.transform.position = neighbourBedPosition.position;
        sickNeighbour.transform.position = sickNeighbourBedPosition.position;
        zombie.transform.position = zombiePosition.position;

        dialoguePanel.SetActive(false);

        StartCoroutine(CutsceneSequence());
    }

    IEnumerator CutsceneSequence()
    {
        // Оба встают
        yield return new WaitForSeconds(0.5f);
        if (playerAnimator != null) playerAnimator.SetTrigger("StandUp");
        yield return new WaitForSeconds(0.5f);
        if (neighbourAnimator != null) neighbourAnimator.SetTrigger("StandUp");

        yield return new WaitForSeconds(2f);

        // ГГ поворачивается к соседу
        Vector3 lookTarget = new Vector3(neighbourRoom.transform.position.x,
            player.transform.position.y, neighbourRoom.transform.position.z);
        player.transform.LookAt(lookTarget);

        yield return new WaitForSeconds(0.5f);

        // Сосед убегает
        if (roomDoor != null) roomDoor.ToggleDoor();
        if (audioSource != null && doorOpenSound != null)
            audioSource.PlayOneShot(doorOpenSound);

        yield return new WaitForSeconds(0.5f);

        if (neighbourAnimator != null)
            neighbourAnimator.SetBool("Walking", true);

        float runTime = 0f;
        float runDuration = 2.5f;
        Vector3 startPos = neighbourRoom.transform.position;

        while (runTime < runDuration)
        {
            runTime += Time.deltaTime;
            float t = runTime / runDuration;

            Vector3 newPos = Vector3.Lerp(startPos, neighbourRunTarget.position, t);
            newPos.y = startPos.y;
            neighbourRoom.transform.position = newPos;

            Vector3 moveDir = (neighbourRunTarget.position - neighbourRoom.transform.position).normalized;
            moveDir.y = 0;
            if (moveDir.magnitude > 0.1f)
                neighbourRoom.transform.rotation = Quaternion.LookRotation(moveDir);

            yield return null;
        }

        if (neighbourAnimator != null)
            neighbourAnimator.SetBool("Walking", false);

        if (roomDoor != null) roomDoor.ToggleDoor();
        if (audioSource != null && doorCloseSound != null)
            audioSource.PlayOneShot(doorCloseSound);

        yield return new WaitForSeconds(0.3f);

        // Крик
        if (audioSource != null && screamSound != null)
            audioSource.PlayOneShot(screamSound);

        yield return new WaitForSeconds(screamSound != null ? screamSound.length : 2f);

        // Сосед мёртв, зомби ест
        neighbourRoom.transform.position = neighbourDeathPosition.position;
        neighbourRoom.transform.rotation = Quaternion.Euler(90f, 0, 0);
        if (neighbourAnimator != null) neighbourAnimator.SetTrigger("Dead");

        zombie.transform.position = zombiePosition.position;
        if (zombieAnimator != null) zombieAnimator.SetTrigger("Eat");

        yield return new WaitForSeconds(2f);

        // Включаем управление и подсказку
        if (fpsController != null) fpsController.enabled = true;
        if (playerAttack != null) playerAttack.enabled = true;

        if (dialogueText != null)
        {
            dialogueText.text = "Нужно выйти из комнаты и узнать, что случилось...";
            dialoguePanel.SetActive(true);
            yield return new WaitForSeconds(3f);
            dialoguePanel.SetActive(false);
        }

        // Зомби начинает атаковать
        EnemyAITrigger enemyAI = zombie.GetComponent<EnemyAITrigger>();
        if (enemyAI != null) enemyAI.enabled = true;

        Debug.Log("Убейте зомби, затем подойдите к больному соседу и нажмите E");

        // Ждём, пока игрок убьёт зомби
        EnemyAITrigger zAI = zombie.GetComponent<EnemyAITrigger>();
        Debug.Log("Проверка зомби: zombie=" + (zombie != null) + ", zAI=" + (zAI != null) + ", health=" + (zAI != null ? zAI.health : -1));

        // Если зомби уже мёртв — воскрешаем
        if (zAI != null && zAI.health <= 0)
        {
            zAI.health = 80;
            Debug.Log("Зомби был мёртв, воскресили до 80 HP");
        }

        while (zombie != null && zAI != null && zAI.health > 0)
        {
            yield return new WaitForSeconds(0.3f);
            if (zombie != null)
                zAI = zombie.GetComponent<EnemyAITrigger>();
        }

        Debug.Log("Зомби побеждён!");

        // Подсказка подойти к соседу
        if (dialogueText != null)
        {
            dialogueText.text = "Убейте зомби, используя ЛКМ, а затем подойдите к соседу и нажмите E для диалога";
            dialoguePanel.SetActive(true);
        }

        // Ждём нажатия E рядом с соседом
        bool dialogueStarted = false;
        while (!dialogueStarted)
        {
            float distToSick = Vector3.Distance(player.transform.position, sickNeighbour.transform.position);
            if (distToSick < 3f && Input.GetKeyDown(KeyCode.E))
            {
                dialogueStarted = true;
            }
            yield return null;
        }

        if (dialogueText != null)
        {
            dialoguePanel.SetActive(false);
        }

        Debug.Log("ПЕРЕХОДИМ К ДИАЛОГУ");

        // Телепортируем больного соседа к игроку
        sickNeighbour.transform.position = player.transform.position + player.transform.forward * 2f;
        sickNeighbour.transform.LookAt(player.transform);

        Animator sickAnim = sickNeighbour.GetComponent<Animator>();
        if (sickAnim != null)
        {
            sickAnim.SetBool("Walking", false);
            sickAnim.SetTrigger("StandUp");
        }

        yield return new WaitForSeconds(1f);

        // ДИАЛОГ
        if (dialogueText != null)
        {
            dialoguePanel.SetActive(true);

            // Все реплики по порядку
            string[] allLines = new string[neighbourLines.Length + playerLines.Length];
            int idx = 0;
            int maxLines = Mathf.Max(neighbourLines.Length, playerLines.Length);
            for (int i = 0; i < maxLines; i++)
            {
                if (i < neighbourLines.Length)
                {
                    allLines[idx] = neighbourLines[i];
                    idx++;
                }
                if (i < playerLines.Length)
                {
                    allLines[idx] = playerLines[i];
                    idx++;
                }
            }

            for (int i = 0; i < allLines.Length; i++)
            {
                if (string.IsNullOrEmpty(allLines[i])) continue;

                dialogueText.text = allLines[i] + "\n\n<size=60%>Нажмите Е чтобы продолжить</size>";

                while (!Input.GetKeyDown(KeyCode.E))
                {
                    yield return null;
                }

                yield return new WaitForSeconds(0.2f);
            }

            dialoguePanel.SetActive(false);
        }

        Debug.Log("ДИАЛОГ ЗАВЕРШЁН. КАТ-СЦЕНА ОКОНЧЕНА.");
    }
}