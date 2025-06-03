using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class MainMenuController : MonoBehaviour
    {
        public AudioClip backgroundMusic;
        private AudioSource audioSource;    
        public GameObject mainMenuWindow, highScoreWindow, instructionWindow;

    private void Awake()
        {
            // Create and configure the AudioSource
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.clip = backgroundMusic;
            audioSource.loop = true;
            audioSource.playOnAwake = false;
        }

        private void Start()
        {
        if (highScoreWindow != null && mainMenuWindow != null)
        {
            mainMenuWindow.SetActive(true);
            highScoreWindow.SetActive(false);
        }
        if (backgroundMusic != null)    
            {
                audioSource.Play();
            }
        }

        public void StartGame()
        {
        AudioManager.ResumeRandomMusic();
        SceneManager.LoadScene("MainScene");
        }

    public void ShowInstruction()
    {
        if (instructionWindow != null && mainMenuWindow != null)
        {
            EventSystem.current.SetSelectedGameObject(null);

            // Then switch windows
            mainMenuWindow.SetActive(false);
            instructionWindow.SetActive(true);
        }
    }  

    public void ShowHighScore()
        {
        if (highScoreWindow != null && mainMenuWindow !=null)
            {
            EventSystem.current.SetSelectedGameObject(null);

            // Then switch windows
            mainMenuWindow.SetActive(false);
            highScoreWindow.SetActive(true);
            }
        }
        //hinh nhu ngu phap minh` sai sai
        public void unShowHighScore()
        {
        if (highScoreWindow != null && mainMenuWindow != null)
            {
            EventSystem.current.SetSelectedGameObject(null);
            mainMenuWindow.SetActive(true);
            highScoreWindow.SetActive(false);
            }
        }

    public void unShowInstruction()
    {
        if (instructionWindow != null && mainMenuWindow != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            mainMenuWindow.SetActive(true);
            instructionWindow.SetActive(false);
        }
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
