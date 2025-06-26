using UnityEngine;
using UnityEngine.UI;

public class MusicSettings : MonoBehaviour
{
    [SerializeField] private Slider musicSlider;

    private void Start()
    {
        // Optionally initialize slider to current volume
        if (musicSlider != null)
        {
            musicSlider.value = AudioManager.GetVolume(); // New helper function
        }

        musicSlider.onValueChanged.AddListener(SetVolume);
    }

    public void SetVolume(float value)
    {
        AudioManager.SetMusicVolume(value);
    }
}
