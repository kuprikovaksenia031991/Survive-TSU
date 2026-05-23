using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ExitDoorSimple : MonoBehaviour
{
    public string sceneToLoad = "Forest";
    public float fadeDuration = 1f;
    public float interactionDistance = 3f;

    private bool isTransitioning = false;
    private CanvasGroup fadeCanvasGroup;
    private TextMeshProUGUI promptText;

    void Start()
    {
        // Создаём Canvas для UI
        GameObject canvasObj = new GameObject("ExitDoorCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        // Панель затемнения
        GameObject fadeObj = new GameObject("FadePanel");
        fadeObj.transform.SetParent(canvasObj.transform, false);
        Image fadeImage = fadeObj.AddComponent<Image>();
        fadeImage.color = new Color(0, 0, 0, 0);
        RectTransform fadeRect = fadeObj.GetComponent<RectTransform>();
        fadeRect.anchorMin = Vector2.zero;
        fadeRect.anchorMax = Vector2.one;
        fadeRect.sizeDelta = Vector2.zero;
        fadeImage.raycastTarget = false;
        fadeCanvasGroup = fadeObj.AddComponent<CanvasGroup>();
        fadeCanvasGroup.alpha = 0;

        // Текст подсказки
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

    void Update()
    {
        if (isTransitioning) return;

        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) return;

        Camera cam = player.GetComponentInChildren<Camera>();
        if (cam == null) return;

        float distance = Vector3.Distance(transform.position, player.transform.position);

        if (distance <= interactionDistance)
        {
            promptText.text = "Нажмите [E] чтобы покинуть общежитие";

            if (Input.GetKeyDown(KeyCode.E))
            {
                StartCoroutine(LoadSceneWithFade());
            }
        }
        else
        {
            promptText.text = "";
        }
    }

    IEnumerator LoadSceneWithFade()
    {
        isTransitioning = true;
        promptText.text = "";

        // Затемнение
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Clamp01(elapsed / fadeDuration);
            yield return null;
        }

        // Загрузка сцены
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneToLoad);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }
}