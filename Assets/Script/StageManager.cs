using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class StageManager : MonoBehaviour
{
    [Header("Transition")]
    public GameObject circleTransitionObject;        // Assign UI Mask object (e.g. full-screen panel with CanvasGroup + Animator)
    public Animator circleTransitionAnimator;        // Animator with "StartIn" and "StartOut" triggers
    public float transitionDuration = 1f;            // Must match your animation clip duration

    [Header("Stage Menu Music")]
    public AudioClip backgroundMusic;
    private AudioSource audioSource;

    private string targetScene;

    private void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = backgroundMusic;
        audioSource.loop = true;
        audioSource.playOnAwake = false;
    }

    private void Start()
    {
        if (circleTransitionAnimator != null && circleTransitionObject != null)
        {
            StartCoroutine(PlayStartInWithDelay());
        }

        if (backgroundMusic != null)
            audioSource.Play();
    }

    private IEnumerator PlayStartInWithDelay()
    {
        CanvasGroup cg = circleTransitionObject.GetComponent<CanvasGroup>();
        if (cg != null)
            cg.alpha = 1f;

        circleTransitionAnimator.ResetTrigger("StartOut"); // safety
        circleTransitionAnimator.SetTrigger("StartIn");

        yield return new WaitForSeconds(transitionDuration);

        if (cg != null)
            cg.alpha = 0f; // hide mask
    }

    public void LoadStage1()
    {
        StartSceneTransition("Map1");
    }

    public void LoadStage2()
    {
        StartSceneTransition("Map2");
    }

    public void LoadStage3()
    {
        StartSceneTransition("Map3");
    }

    private void StartSceneTransition(string sceneName)
    {
        targetScene = sceneName;

        if (circleTransitionAnimator != null && circleTransitionObject != null)
        {
            StartCoroutine(PlayStartOutAndLoadScene());
        }
        else
        {
            SceneManager.LoadScene(sceneName);
        }
    }

    private IEnumerator PlayStartOutAndLoadScene()
    {
        CanvasGroup cg = circleTransitionObject.GetComponent<CanvasGroup>();
        if (cg != null)
            cg.alpha = 1f;

        circleTransitionAnimator.ResetTrigger("StartIn"); // safety
        circleTransitionAnimator.SetTrigger("StartOut");

        yield return new WaitForSeconds(transitionDuration);

        SceneManager.LoadScene(targetScene);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
