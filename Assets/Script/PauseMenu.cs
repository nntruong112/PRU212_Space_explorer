using UnityEngine;
using UnityEngine.SceneManagement; // Importing SceneManager for scene management

public class PauseMenu : MonoBehaviour
{
    [SerializeField] GameObject pauseMenu; // Reference to the pause menu UI

    public void Pause()
    {
        pauseMenu.SetActive(true); // Show the pause menu
        Time.timeScale = 0f; // Pause the game by setting time scale to 0
    }

    public void Home()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu"); // Load the main menu scene
    }

    public void Resume()
    {
        Time.timeScale = 1f; // Resume the game
        gameObject.SetActive(false); // Hide the pause menu
    }

    public void Restart()
    {
        Time.timeScale = 1f; // Resume the game
        // Add logic to restart the current level or scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
