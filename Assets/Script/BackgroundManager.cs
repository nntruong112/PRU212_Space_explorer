using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BackgroundManager : MonoBehaviour
{
    public GameObject[] backgrounds; // Should be size 3
    public float transitionDuration = 1.5f;
    public float switchInterval = 60f; // me random change background every 60s

    public Image fadeOverlayImage; 

    private int currentBackgroundIndex = 0;
    private Coroutine switchRoutine;
    private bool isSwitchingRandom = false;

    void Start()
    {
        fadeOverlayImage.color = new Color(0f, 0f, 0f, 0f);
        //UpdateBackground(GameManager.Instance.difficultyLevel);
    }

    void Update()
    {
        int currentLevel = GameManager.Instance.difficultyLevel;
        if (currentLevel != currentBackgroundIndex)
        {
            UpdateBackground(currentLevel);
        }
    }

    void UpdateBackground(int difficulty)
    {
        if (difficulty < 3)
        {
            if (isSwitchingRandom && switchRoutine != null)
            {
                StopCoroutine(switchRoutine);
                isSwitchingRandom = false;
            }

            StartCoroutine(SwitchWithFade(difficulty));
        }
        else
        {
            if (!isSwitchingRandom)
            {
                isSwitchingRandom = true;
                switchRoutine = StartCoroutine(RandomSwitchRoutine());
            }
        }
    }

    IEnumerator SwitchWithFade(int newIndex)
    {
        yield return StartCoroutine(FadeInOverlay());

        SetBackground(newIndex);

        yield return StartCoroutine(FadeOutOverlay());
    }

    void SetBackground(int index)
    {
        if (index == currentBackgroundIndex) return;
        if (index < 0 || index >= backgrounds.Length) return;

        for (int i = 0; i < backgrounds.Length; i++)
        {
            backgrounds[i].SetActive(i == index);
        }

        currentBackgroundIndex = index;
    }

    IEnumerator RandomSwitchRoutine()
    {
        while (true)
        {
            int newIndex = Random.Range(0, backgrounds.Length);
            while (newIndex == currentBackgroundIndex)
                newIndex = Random.Range(0, backgrounds.Length);

            yield return StartCoroutine(SwitchWithFade(newIndex));

            yield return new WaitForSeconds(switchInterval);
        }
    }

    IEnumerator FadeInOverlay()
    {
        float t = 0f;
        while (t < transitionDuration)
        {
            float alpha = Mathf.Lerp(0f, 1f, t / transitionDuration);
            fadeOverlayImage.color = new Color(0f, 0f, 0f, alpha);
            t += Time.deltaTime;
            yield return null;
        }
        fadeOverlayImage.color = new Color(0f, 0f, 0f, 1f);
    }

    IEnumerator FadeOutOverlay()
    {
        float t = 0f;
        while (t < transitionDuration)
        {
            float alpha = Mathf.Lerp(1f, 0f, t / transitionDuration);
            fadeOverlayImage.color = new Color(0f, 0f, 0f, alpha);
            t += Time.deltaTime;
            yield return null;
        }
        fadeOverlayImage.color = new Color(0f, 0f, 0f, 0f);
    }
}
