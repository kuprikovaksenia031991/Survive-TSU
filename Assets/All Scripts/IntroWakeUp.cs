using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class IntroWakeUp : MonoBehaviour
{
    public Image blackScreen;
    public AudioSource audioSource;
    public AudioClip crashSound;
    public float fadeDuration = 1.5f;

    private bool crashed = false;
    private bool startedLoading = false;
    private float soundLength = 0f;
    private float soundStartTime = 0f;

    void Start()
    {
        blackScreen.color = Color.black;

        // Узнаем длину звука
        if (crashSound != null)
            soundLength = crashSound.length;

        StartCoroutine(PlayCrash());
    }

    System.Collections.IEnumerator PlayCrash()
    {
        yield return new WaitForSeconds(1.5f);

        if (audioSource != null && crashSound != null)
        {
            audioSource.PlayOneShot(crashSound);
            soundStartTime = Time.time;
        }

        crashed = true;
    }

    void Update()
    {
        if (!crashed) return;
        if (startedLoading) return;

        startedLoading = true;
        StartCoroutine(LoadAndFade());
    }

    System.Collections.IEnumerator LoadAndFade()
    {
        // Начинаем загрузку сцены в фоне
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Dormitory_Inside");
        asyncLoad.allowSceneActivation = false;

        // Ждём, пока сцена загрузится до 90%
        while (asyncLoad.progress < 0.9f)
        {
            yield return null;
        }

        // Ждём, пока звук доиграет ИЛИ прошло минимум 4 секунды от начала звука
        float elapsedSoundTime = Time.time - soundStartTime;
        float remainingSound = Mathf.Max(0, soundLength - elapsedSoundTime);

        if (remainingSound > 0)
        {
            yield return new WaitForSeconds(remainingSound);
        }

        // Дополнительная гарантия: ждём минимум до общей длительности 5 секунд
        float totalElapsed = Time.time - soundStartTime + 1.5f;
        if (totalElapsed < 5f)
        {
            yield return new WaitForSeconds(5f - totalElapsed);
        }

        // Плавно убираем чёрный экран
        float fadeTimer = 0f;
        while (fadeTimer < fadeDuration)
        {
            fadeTimer += Time.deltaTime;
            float alpha = 1 - (fadeTimer / fadeDuration);
            blackScreen.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        blackScreen.color = Color.clear;

        // Активируем сцену
        asyncLoad.allowSceneActivation = true;
    }
}