using UnityEngine;
using TMPro;
using System.Collections;

public class HintManager : MonoBehaviour
{
    public static HintManager Instance;

    public GameObject hintPanel;
    public TextMeshProUGUI hintText;
    public float defaultDuration = 3f;

    private Coroutine currentCoroutine;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        if (hintPanel != null)
            hintPanel.SetActive(false);
    }

    public void ShowHint(string message, float duration = -1)
    {
        if (currentCoroutine != null)
            StopCoroutine(currentCoroutine);

        hintText.text = message;
        hintPanel.SetActive(true);

        float time = duration > 0 ? duration : defaultDuration;
        currentCoroutine = StartCoroutine(HideAfterDelay(time));
    }

    IEnumerator HideAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        hintPanel.SetActive(false);
        currentCoroutine = null;
    }
}