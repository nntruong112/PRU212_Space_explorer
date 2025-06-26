using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class SceneBackgroundSet
{
    public string sceneName;
    public GameObject[] backgrounds; // [Easy, Medium, Hard]
}

public class BackgroundManager : MonoBehaviour
{
    [Header("Scene Background Configurations")]
    public List<SceneBackgroundSet> sceneBackgrounds;

    [Header("Fade Settings")]
    public Image fadeOverlayImage;
    public float transitionDuration = 1.5f;

    private GameObject[] currentSceneBackgrounds;
    private int currentBackgroundIndex = -1;

    void Start()
    {
        if (fadeOverlayImage != null)
            fadeOverlayImage.color = new Color(0f, 0f, 0f, 0f);

        LoadSceneSpecificBackgrounds();
        UpdateBackground(GameManager.Instance.difficultyLevel); // Show initial
    }

    void Update()
    {
        if (currentSceneBackgrounds == null || currentSceneBackgrounds.Length == 0)
            return;

        int level = GameManager.Instance != null ? GameManager.Instance.difficultyLevel : 0;

        if (level != currentBackgroundIndex)
        {
            UpdateBackground(level);
        }
    }

    void LoadSceneSpecificBackgrounds()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        foreach (SceneBackgroundSet set in sceneBackgrounds)
        {
            if (set.sceneName == sceneName)
            {
                currentSceneBackgrounds = set.backgrounds;

                foreach (GameObject bg in currentSceneBackgrounds)
                    if (bg != null) bg.SetActive(false);

                return;
            }
        }

        Debug.LogWarning("No background set found for scene: " + sceneName);
    }

    void UpdateBackground(int difficulty)
    {
        int index = Mathf.Clamp(difficulty, 0, currentSceneBackgrounds.Length - 1);
        StartCoroutine(SwitchWithFade(index));
    }

    IEnumerator SwitchWithFade(int newIndex)
    {
        yield return StartCoroutine(FadeInOverlay());
        SetBackground(newIndex);
        yield return StartCoroutine(FadeOutOverlay());
    }

    void SetBackground(int index)
    {
        if (currentSceneBackgrounds == null || index == currentBackgroundIndex)
            return;

        for (int i = 0; i < currentSceneBackgrounds.Length; i++)
            currentSceneBackgrounds[i].SetActive(i == index);

        currentBackgroundIndex = index;
    }

    IEnumerator FadeInOverlay()
    {
        float t = 0f;
        while (t < transitionDuration)
        {
            float alpha = Mathf.Lerp(0f, 1f, t / transitionDuration);
            if (fadeOverlayImage != null)
                fadeOverlayImage.color = new Color(0f, 0f, 0f, alpha);

            t += Time.deltaTime;
            yield return null;
        }

        if (fadeOverlayImage != null)
            fadeOverlayImage.color = new Color(0f, 0f, 0f, 1f);
    }

    IEnumerator FadeOutOverlay()
    {
        float t = 0f;
        while (t < transitionDuration)
        {
            float alpha = Mathf.Lerp(1f, 0f, t / transitionDuration);
            if (fadeOverlayImage != null)
                fadeOverlayImage.color = new Color(0f, 0f, 0f, alpha);

            t += Time.deltaTime;  
            yield return null;
        }

        if (fadeOverlayImage != null)
            fadeOverlayImage.color = new Color(0f, 0f, 0f, 0f);
    }
}
