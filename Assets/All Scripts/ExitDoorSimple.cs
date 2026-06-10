using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ExitDoorSimple : MonoBehaviour
{
    public string sceneToLoad = "Inter_Forest_TSU";
    public string promptMessage = "Нажмите [E] чтобы войти";
    public float interactionDistance = 3f;

    [Header("Блокировка выхода")]
    public bool requireAllZombiesDead = true;
    public string requiredSceneName = "Abricos";  // Только в этой сцене работает блокировка
    public string blockedMessage = "Yбейте всех зомби, чтобы выйти. Не забудьте забрать записку";

    private bool isTransitioning = false;
    private TextMeshProUGUI promptText;
    private int zombiesTotal = 0;
    private int zombiesDead = 0;

    void Start()
    {
        // Подсчитываем зомби только если нужно
        if (requireAllZombiesDead && SceneManager.GetActiveScene().name == requiredSceneName)
        {
            EnemySimple[] allZombies = FindObjectsOfType<EnemySimple>();
            zombiesTotal = allZombies.Length;
            Debug.Log($"Всего зомби в сцене {requiredSceneName}: {zombiesTotal}");

            EnemySimple.OnZombieDied += OnZombieKilled;
        }

        // Создаём UI
        GameObject canvasObj = new GameObject("ExitDoorCanvas_" + sceneToLoad);
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        GameObject textObj = new GameObject("PromptText");
        textObj.transform.SetParent(canvasObj.transform, false);
        promptText = textObj.AddComponent<TextMeshProUGUI>();
        promptText.text = "";
        promptText.fontSize = 24;
        promptText.color = Color.white;
        promptText.alignment = TMPro.TextAlignmentOptions.Center;
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 0);
        textRect.anchorMax = new Vector2(0.5f, 0);
        textRect.pivot = new Vector2(0.5f, 0);
        textRect.anchoredPosition = new Vector2(0, 80);
        textRect.sizeDelta = new Vector2(600, 60);
    }

    void OnDestroy()
    {
        EnemySimple.OnZombieDied -= OnZombieKilled;
    }

    void OnZombieKilled()
    {
        zombiesDead++;
        Debug.Log($"Убито зомби: {zombiesDead}/{zombiesTotal}");

        if (zombiesDead >= zombiesTotal)
        {
            HintManager.Instance?.ShowHint("Все зомби убиты! Теперь можно выйти.", 3f);
        }
    }

    bool CanExit()
    {
        // Если не та сцена — выходим без блокировки
        if (SceneManager.GetActiveScene().name != requiredSceneName)
            return true;

        if (!requireAllZombiesDead) return true;
        return zombiesDead >= zombiesTotal;
    }

    void Update()
    {
        if (isTransitioning) return;

        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.transform.position);

        if (distance <= interactionDistance)
        {
            if (CanExit())
            {
                promptText.text = promptMessage;
                if (Input.GetKeyDown(KeyCode.E))
                {
                    StartCoroutine(LoadScene());
                }
            }
            else
            {
                promptText.text = blockedMessage;
                if (Input.GetKeyDown(KeyCode.E))
                {
                    HintManager.Instance?.ShowHint(blockedMessage, 2f);
                }
            }
        }
        else
        {
            promptText.text = "";
        }
    }

    IEnumerator LoadScene()
    {
        isTransitioning = true;
        promptText.text = "";

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Single);
        asyncLoad.allowSceneActivation = true;

        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        DynamicGI.UpdateEnvironment();
    }

}