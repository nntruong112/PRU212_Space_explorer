using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.Collections;

public class MainMenuController : MonoBehaviour
{
    [Header("Audio")]
    public AudioClip backgroundMusic;
    private AudioSource audioSource;

    [Header("UI Windows")]
    public GameObject mainMenuWindow;
    public GameObject highScoreWindow;
    public GameObject instructionWindow;

    [Header("Transition")]
    public GameObject circleTransitionObject; // Assign CircleMask (CanvasGroup + Animator)
    public Animator circleTransitionAnimator; // Animator on CircleMask
    public float transitionDuration = 1f;     // Must match animation length

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
        if (mainMenuWindow != null)
        {
            mainMenuWindow?.SetActive(true);
            highScoreWindow?.SetActive(false);
            instructionWindow?.SetActive(false);
        }
        if (backgroundMusic != null)
            audioSource.Play();

        // Just hide circle visually, don’t disable it
        if (circleTransitionObject != null)
        {
            var cg = circleTransitionObject.GetComponent<CanvasGroup>();
            if (cg != null)
                cg.alpha = 0f;
        }
    }

    public void StartGame()
    {

        Debug.Log("Animator is " + (circleTransitionAnimator != null));
        Debug.Log("Object is " + (circleTransitionObject != null));

        if (circleTransitionAnimator != null && circleTransitionObject != null)
        {
            targetScene = "StageMenu";
            StartCoroutine(PlayTransitionAndLoadScene());
        }
        else
        {
            SceneManager.LoadScene("StageMenu");
        }
    }

    private IEnumerator PlayTransitionAndLoadScene()
    {
        Debug.Log("running");

        var cg = circleTransitionObject.GetComponent<CanvasGroup>();
        if (cg != null) cg.alpha = 1f;

        circleTransitionAnimator.SetTrigger("StartOut");
        Debug.Log("running2");

        yield return new WaitForSeconds(transitionDuration);

        SceneManager.LoadScene(targetScene);
    }

    public void ShowInstruction()
    {
        mainMenuWindow?.SetActive(false);
        instructionWindow?.SetActive(true);
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void unShowInstruction()
    {
        mainMenuWindow?.SetActive(true);
        instructionWindow?.SetActive(false);
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void ShowHighScore()
    {
        mainMenuWindow?.SetActive(false);
        highScoreWindow?.SetActive(true);
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void unShowHighScore()
    {
        mainMenuWindow?.SetActive(true);
        highScoreWindow?.SetActive(false);
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
