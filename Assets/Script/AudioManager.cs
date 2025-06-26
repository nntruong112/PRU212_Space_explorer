using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    private static AudioManager instance;
    private AudioSource musicSourceA;
    private AudioSource musicSourceB;
    private AudioSource currentSource;
    private AudioSource nextSource;

    [Header("Music Clips")]
    public AudioClip stageMusic;
    public AudioClip bossMusic;

    [Header("Settings")]
    public float musicVolume = 0.5f;
    public float fadeDuration = 2f;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(instance.gameObject); // ✅ Destroy old AudioManager
        }

        instance = this;
        DontDestroyOnLoad(gameObject); // ✅ Persist this one

        musicSourceA = gameObject.AddComponent<AudioSource>();
        musicSourceB = gameObject.AddComponent<AudioSource>();

        musicSourceA.loop = true;
        musicSourceB.loop = true;

        musicSourceA.volume = 0f;
        musicSourceB.volume = 0f;

        musicSourceA.playOnAwake = false;
        musicSourceB.playOnAwake = false;

        currentSource = musicSourceA;
        nextSource = musicSourceB;

        if (stageMusic != null)
        {
            StartCoroutine(CrossfadeTo(stageMusic)); // ✅ Begin playback
        }
    }


    public static void PlayStageMusic()
    {
        if (instance != null && instance.stageMusic != null)
        {
            instance.StopAllCoroutines();
            AudioManager.ChangeMusic(instance.stageMusic, instance.musicVolume); // ✅ STATIC CALL
        }
    }



    private IEnumerator CrossfadeTo(AudioClip newClip)
    {
        nextSource.clip = newClip;
        nextSource.volume = 0f;
        nextSource.Play();

        float timer = 0f;
        while (timer < fadeDuration)
        {
            float t = timer / fadeDuration;
            currentSource.volume = Mathf.Lerp(musicVolume, 0, t);
            nextSource.volume = Mathf.Lerp(0, musicVolume, t);
            timer += Time.deltaTime;
            yield return null;
        }

        currentSource.Stop();
        AudioSource temp = currentSource;
        currentSource = nextSource;
        nextSource = temp;

        currentSource.volume = musicVolume;
    }

    /// <summary>
    /// Call this to fade from current music to the boss track.
    /// </summary>
    public static void TransitionToBossMusic()
    {
        if (instance != null && instance.bossMusic != null)
        {
            instance.StopAllCoroutines();
            instance.StartCoroutine(instance.CrossfadeTo(instance.bossMusic));
        }
    }

    /// <summary>
    /// Play 2D sound effect at position with optional volume.
    /// </summary>
    public static void PlayClip(AudioClip clip, Vector3 position, float volume = 1f)
    {
        if (clip == null) return;

        GameObject tempGO = new GameObject("TempAudio");
        tempGO.transform.position = position;

        AudioSource source = tempGO.AddComponent<AudioSource>();
        source.clip = clip;
        source.volume = volume;
        source.Play();

        Destroy(tempGO, clip.length);
    }

    /// <summary>
    /// Change music immediately (no fade).
    /// </summary>
    public static void ChangeMusic(AudioClip newMusic, float volume = 0.5f)
    {
        if (instance != null)
        {
            instance.StopAllCoroutines();
            instance.musicSourceA.Stop();
            instance.musicSourceB.Stop();

            instance.currentSource = instance.musicSourceA;
            instance.currentSource.clip = newMusic;
            instance.currentSource.loop = true;
            instance.musicVolume = volume;
            instance.currentSource.volume = volume;
            instance.currentSource.Play();
        }
    }

    /// <summary>
    /// Stop all music playback.
    /// </summary>
    public static void StopMusic()
    {
        if (instance != null)
        {
            instance.StopAllCoroutines();
            instance.musicSourceA.Stop();
            instance.musicSourceB.Stop();
        }
    }

    /// <summary>
    /// Set global music volume (affects fade-in/out target).
    /// </summary>
    public static void SetMusicVolume(float volume)
    {
        if (instance != null)
        {
            instance.musicVolume = volume;
            instance.currentSource.volume = volume;
        }
    }

    public static float GetVolume()
    {
        return instance != null ? instance.musicVolume : 0.5f;
    }

    public static void DestroyInstance()
    {
        if (instance != null)
        {
            Destroy(instance.gameObject);
            instance = null;
        }
    }



}
