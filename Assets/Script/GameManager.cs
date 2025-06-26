using UnityEngine;
using System.Collections;  // Needed for IEnumerator
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public float score = 0f;
    public int difficultyLevel = 1;

    public Text scoreText;
    public GameObject gameOverWindow;
    public Text finalScoreText;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {   if(gameOverWindow!= null)
        gameOverWindow.SetActive(false);
    }

    void Update()
    {  
        if(scoreText != null) 
        scoreText.text = Mathf.RoundToInt(score).ToString();

        UpdateDifficulty();
    }

    void UpdateDifficulty()
    {
        if (score >= 50000)
            difficultyLevel = 3;
        else if (score >= 20000)
            difficultyLevel = 2;
        else if (score >= 5000)
            difficultyLevel = 1;
        else
            difficultyLevel = 0;
    }

    /// <summary>
    /// Adds points to the score and updates difficulty if needed.
    /// </summary>
    public void AddScore(float amount)
    {
        score += amount;
        UpdateDifficulty(); // Optional here if you want to instantly apply the change
    }

    // --- New code for ship respawn ---

    /// <summary>
    /// Starts the respawn coroutine for the given ship GameObject.
    /// </summary>
    public void RespawnShip(GameObject ship, float delay = 1f, Vector3 respawnPosition = default, Quaternion respawnRotation = default)
    {
        StartCoroutine(RespawnCoroutine(ship, delay, respawnPosition, respawnRotation));
    }

    private IEnumerator RespawnCoroutine(GameObject ship, float delay, Vector3 respawnPosition, Quaternion respawnRotation)
    {
        // If respawnPosition/respawnRotation not supplied, use zero/default
        if (respawnPosition == default) respawnPosition = Vector3.zero;
        if (respawnRotation == default) respawnRotation = Quaternion.identity;

        // Disable the ship immediately
        if (ship != null)
            ship.SetActive(false);

        // Wait for delay seconds
        yield return new WaitForSeconds(delay);

        if (ship == null)
            yield break;

        // Reset position and rotation
        ship.transform.position = respawnPosition;
        ship.transform.rotation = respawnRotation;

        // Enable the ship again
        ship.SetActive(true);
        PlayerScript ps = ship.GetComponent<PlayerScript>();
        if (ps != null)
        {
            ps.StartCoroutine(ps.BlinkDuringInvulnerability());
        }
    }

    public void HandlePlayerCollision(GameObject player, AudioClip collisionSound, GameObject explosionPrefab, float respawnDelay, Vector3 respawnPosition)
    {
        // Disable laser if available
        PlayerScript playerScript = player.GetComponent<PlayerScript>();
        if (playerScript != null)
        {
            playerScript.DisableLaser(); 
        }

        // Play collision sound
        AudioManager.PlayClip(collisionSound, player.transform.position);

        // Spawn explosion
        if (explosionPrefab != null)
        {
            GameObject.Instantiate(explosionPrefab, player.transform.position, Quaternion.identity);
        }

        // Respawn the player
        if (player.GetComponent<PlayerScript>().currentLives > 0)
        {
            RespawnShip(player, respawnDelay, respawnPosition, Quaternion.identity);
        }
    }

    //bye bye
    public void HandleGameOver()
    {
        AudioManager.StopMusic();

        float finalScore = score;
        string dateTime = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
        string newEntry = Mathf.RoundToInt(finalScore).ToString() + "|" + dateTime;


        // Load existing top 5
        List<(int, string)> highScores = new List<(int, string)>();

        for (int i = 0; i < 5; i++)
        {
            string saved = PlayerPrefs.GetString("HighScore" + i, "");
            if (!string.IsNullOrEmpty(saved))
            {
                string[] parts = saved.Split('|');
                if (parts.Length == 2 && int.TryParse(parts[0], out int score))
                {
                    highScores.Add((score, parts[1]));
                }
            }
        }

        // Add new score
        highScores.Add((Mathf.RoundToInt(finalScore), dateTime));

        // Sort and keep top 5
        highScores.Sort((a, b) => b.Item1.CompareTo(a.Item1)); // Descending
        while (highScores.Count > 5)
            highScores.RemoveAt(highScores.Count - 1);

        // Save back
        for (int i = 0; i < highScores.Count; i++)
        {
            PlayerPrefs.SetString("HighScore" + i, highScores[i].Item1 + "|" + highScores[i].Item2);
        }

        PlayerPrefs.Save();

        // Show game over window
        if (gameOverWindow != null)
            gameOverWindow.SetActive(true);

        if (scoreText != null)
            scoreText.gameObject.SetActive(false);

        if (finalScoreText != null)
            finalScoreText.text = "Score: " + Mathf.RoundToInt(score).ToString();
    }


}
