using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ExitDoorSimple : MonoBehaviour
{
    public string sceneToLoad = "Forest";
    public string promptMessage = "ֽאזלטעו [E] קעמבû גמיעט";
    public float interactionDistance = 3f;

    private bool isTransitioning = false;
    private TextMeshProUGUI promptText;

    void Start()
    {
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

    void Update()
    {
        if (isTransitioning) return;

        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.transform.position);

        if (distance <= interactionDistance)
        {
            promptText.text = promptMessage;

            if (Input.GetKeyDown(KeyCode.E))
            {
                StartCoroutine(LoadScene());
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