using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class HighScoreDisplay : MonoBehaviour
{
    public Text[] scoreTextLines; // Assign 5 Text objects in order in Inspector
    public GameObject mainMenuWindow;
    private void Start()
    {
        mainMenuWindow.SetActive(false);
        Canvas.ForceUpdateCanvases();
        LoadHighScores();
    }

    void LoadHighScores()
    {
        for (int i = 0; i < scoreTextLines.Length; i++)
        {
            string data = PlayerPrefs.GetString("HighScore" + i, "");
            if (!string.IsNullOrEmpty(data))
            {
                string[] parts = data.Split('|');
                if (parts.Length == 2)
                {
                    string score = parts[0];
                    string date = parts[1];
                    scoreTextLines[i].text = $"{i + 1}. {score} pts on {date}";
                }
                else
                {
                    scoreTextLines[i].text = $"{i + 1}. ---";
                }
            }
            else
            {
                scoreTextLines[i].text = $"{i + 1}. ---";
            }
        }
    }
}
