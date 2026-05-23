using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class ExitDoor : MonoBehaviour
{
    [Header("Настройки сцены")]
    public string sceneToLoad = "Forest";  // Название сцены для перехода

    [Header("Настройки затемнения")]
    public float fadeDuration = 1f;        // Длительность затемнения

    [Header("UI элементы")]
    public GameObject promptPanel;         // Панель с подсказкой "Нажми E"
    public GameObject confirmPanel;        // Панель с подтверждением "Вы уверены?"
    public Text promptText;                // Текст подсказки
    public Text confirmText;               // Текст подтверждения

    private bool isPlayerNear = false;
    private bool isTransitioning = false;
    private Canvas mainCanvas;
    private Image fadeImage;

    void Start()
    {
        // Ищем Canvas в сцене
        mainCanvas = FindObjectOfType<Canvas>();
        if (mainCanvas == null)
        {
            Debug.LogError("Canvas не найден в сцене! Создай Canvas через GameObject → UI → Canvas");
            return;
        }

        // Создаём панель затемнения
        CreateFadePanel();

        // Создаём UI панели если они не назначены
        if (promptPanel == null)
            CreatePromptPanel();
        if (confirmPanel == null)
            CreateConfirmPanel();

        // Скрываем панели в начале
        if (promptPanel != null) promptPanel.SetActive(false);
        if (confirmPanel != null) confirmPanel.SetActive(false);
    }

    void Update()
    {
        if (isPlayerNear && !isTransitioning)
        {
            // Показываем подсказку
            if (promptPanel != null && !promptPanel.activeSelf)
                promptPanel.SetActive(true);

            // Нажатие E
            if (Input.GetKeyDown(KeyCode.E))
            {
                ShowConfirmDialog();
            }
        }
        else
        {
            if (promptPanel != null && promptPanel.activeSelf)
                promptPanel.SetActive(false);
        }
    }

    void ShowConfirmDialog()
    {
        // Показываем окно подтверждения
        if (confirmPanel != null)
        {
            confirmPanel.SetActive(true);
        }
        if (promptPanel != null)
        {
            promptPanel.SetActive(false);
        }

        // Останавливаем игру на время показа окна (опционально)
        Time.timeScale = 0f;
    }

    public void ConfirmExit()
    {
        // Возвращаем время
        Time.timeScale = 1f;

        // Скрываем окно подтверждения
        if (confirmPanel != null)
            confirmPanel.SetActive(false);

        // Сохраняем игру перед переходом
        SaveGame();

        // Запускаем затемнение и переход
        StartCoroutine(LoadSceneWithFade());
    }

    public void CancelExit()
    {
        // Возвращаем время
        Time.timeScale = 1f;

        // Скрываем окно подтверждения
        if (confirmPanel != null)
            confirmPanel.SetActive(false);

        // Показываем снова подсказку
        if (promptPanel != null && isPlayerNear)
            promptPanel.SetActive(true);
    }

    void SaveGame()
    {
        // Сохраняем позицию игрока (если нужно вернуться когда-нибудь)
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerPrefs.SetFloat("PlayerPosX", player.transform.position.x);
            PlayerPrefs.SetFloat("PlayerPosY", player.transform.position.y);
            PlayerPrefs.SetFloat("PlayerPosZ", player.transform.position.z);
        }

        // Сохраняем флаг, что игрок покинул общежитие
        PlayerPrefs.SetInt("LeftDormitory", 1);
        PlayerPrefs.Save();

        Debug.Log("Игра сохранена перед выходом из общежития");
    }

    IEnumerator LoadSceneWithFade()
    {
        isTransitioning = true;

        // Затемнение
        if (fadeImage != null)
        {
            float elapsedTime = 0f;
            Color color = fadeImage.color;
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                float alpha = Mathf.Clamp01(elapsedTime / fadeDuration);
                color.a = alpha;
                fadeImage.color = color;
                yield return null;
            }
        }

        // Загружаем сцену
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneToLoad);

        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }

    void CreateFadePanel()
    {
        GameObject panel = new GameObject("FadePanel");
        panel.transform.SetParent(mainCanvas.transform, false);

        RectTransform rect = panel.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;

        fadeImage = panel.AddComponent<Image>();
        fadeImage.color = new Color(0, 0, 0, 0);
        fadeImage.raycastTarget = false;
    }

    void CreatePromptPanel()
    {
        // Создаём панель подсказки
        promptPanel = new GameObject("PromptPanel");
        promptPanel.transform.SetParent(mainCanvas.transform, false);

        RectTransform promptRect = promptPanel.AddComponent<RectTransform>();
        promptRect.anchorMin = new Vector2(0.5f, 0);
        promptRect.anchorMax = new Vector2(0.5f, 0);
        promptRect.pivot = new Vector2(0.5f, 0);
        promptRect.anchoredPosition = new Vector2(0, 100);
        promptRect.sizeDelta = new Vector2(400, 60);

        Image bg = promptPanel.AddComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.8f);

        GameObject textObj = new GameObject("PromptText");
        textObj.transform.SetParent(promptPanel.transform, false);
        promptText = textObj.AddComponent<Text>();
        promptText.text = "Нажмите [E] чтобы покинуть общежитие";
        promptText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        promptText.fontSize = 20;
        promptText.color = Color.white;
        promptText.alignment = TextAnchor.MiddleCenter;

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;

        promptPanel.SetActive(false);
    }

    void CreateConfirmPanel()
    {
        // Создаём панель подтверждения
        confirmPanel = new GameObject("ConfirmPanel");
        confirmPanel.transform.SetParent(mainCanvas.transform, false);

        RectTransform confirmRect = confirmPanel.AddComponent<RectTransform>();
        confirmRect.anchorMin = new Vector2(0.5f, 0.5f);
        confirmRect.anchorMax = new Vector2(0.5f, 0.5f);
        confirmRect.sizeDelta = new Vector2(500, 200);

        Image bg = confirmPanel.AddComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.9f);

        // Текст подтверждения
        GameObject textObj = new GameObject("ConfirmText");
        textObj.transform.SetParent(confirmPanel.transform, false);
        confirmText = textObj.AddComponent<Text>();
        confirmText.text = "Вы уверены, что хотите покинуть общежитие?\nВы больше не сможете вернуться!";
        confirmText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        confirmText.fontSize = 18;
        confirmText.color = Color.white;
        confirmText.alignment = TextAnchor.MiddleCenter;

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.offsetMin = new Vector2(10, 40);
        textRect.offsetMax = new Vector2(-10, -40);

        // Кнопка "Да"
        GameObject yesButton = CreateButton("YesButton", "Да", new Vector2(-120, -60), new Vector2(100, 40));
        yesButton.transform.SetParent(confirmPanel.transform, false);
        yesButton.GetComponent<Button>().onClick.AddListener(ConfirmExit);

        // Кнопка "Нет"
        GameObject noButton = CreateButton("NoButton", "Нет", new Vector2(120, -60), new Vector2(100, 40));
        noButton.transform.SetParent(confirmPanel.transform, false);
        noButton.GetComponent<Button>().onClick.AddListener(CancelExit);

        confirmPanel.SetActive(false);
    }

    GameObject CreateButton(string name, string buttonText, Vector2 position, Vector2 size)
    {
        GameObject button = new GameObject(name);

        RectTransform rect = button.AddComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Image img = button.AddComponent<Image>();
        img.color = new Color(0.3f, 0.3f, 0.3f);

        Button btn = button.AddComponent<Button>();

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(button.transform, false);
        Text text = textObj.AddComponent<Text>();
        text.text = buttonText;
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontSize = 16;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleCenter;

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;

        return button;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
            if (promptPanel != null) promptPanel.SetActive(false);
            if (confirmPanel != null) confirmPanel.SetActive(false);
            Time.timeScale = 1f;
        }
    }
}