using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    private static AudioManager instance;
    private AudioSource musicSourceA;
    private AudioSource musicSourceB;
    private AudioSource currentSource;
    private AudioSource nextSource;

    [Header("Legacy (Optional)")]
    public AudioClip backgroundMusic;

    [Header("Playlist Mode")]
    public AudioClip[] musicPlaylist;
    public float musicVolume = 0.5f;
    public float fadeDuration = 2f;

    private int currentTrackIndex = -1;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        musicSourceA = gameObject.AddComponent<AudioSource>();
        musicSourceB = gameObject.AddComponent<AudioSource>();

        musicSourceA.loop = false;
        musicSourceB.loop = false;

        musicSourceA.volume = 0f;
        musicSourceB.volume = 0f;

        musicSourceA.playOnAwake = false;
        musicSourceB.playOnAwake = false;

        currentSource = musicSourceA;
        nextSource = musicSourceB;

        if (musicPlaylist != null && musicPlaylist.Length > 0)
        {
            currentTrackIndex = Random.Range(0, musicPlaylist.Length); 
            AudioClip firstClip = musicPlaylist[currentTrackIndex];
            StartCoroutine(CrossfadeTo(firstClip));
        }
        else if (backgroundMusic != null)
        {
            currentSource.clip = backgroundMusic;
            currentSource.loop = true;
            currentSource.volume = musicVolume;
            currentSource.Play();
        }
    }

    private void PlayNextTrack()
    {
        currentTrackIndex = (currentTrackIndex + 1) % musicPlaylist.Length;
        AudioClip nextClip = musicPlaylist[currentTrackIndex];
        StartCoroutine(CrossfadeTo(nextClip));
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

        StartCoroutine(WaitAndPlayNext(currentSource.clip.length));
    }

    private IEnumerator WaitAndPlayNext(float delay)
    {
        yield return new WaitForSeconds(delay - fadeDuration);
        PlayNextTrack();
    }

    public static void PlayClip(AudioClip clip, Vector3 position)
    {
        if (clip == null) return;

        GameObject tempGO = new GameObject("TempAudio");
        tempGO.transform.position = position;

        AudioSource source = tempGO.AddComponent<AudioSource>();
        source.clip = clip;
        source.Play();

        Destroy(tempGO, clip.length);
    }

    public static void ChangeMusic(AudioClip newMusic, float volume = 0.5f)
    {
        if (instance != null)
        {
            instance.StopAllCoroutines();
            instance.musicSourceA.Stop();
            instance.musicSourceB.Stop();

            instance.musicPlaylist = null;

            instance.currentSource = instance.musicSourceA;
            instance.currentSource.clip = newMusic;
            instance.currentSource.loop = true;
            instance.currentSource.volume = volume;
            instance.currentSource.Play();
        }
    }

    public static void StopMusic()
    {
        if (instance != null)
        {
            instance.StopAllCoroutines();
            instance.musicSourceA.Stop();
            instance.musicSourceB.Stop();
        }
    }
    public static void ResumeRandomMusic()
    {
        if (instance == null) return;

        // If playlist is set, pick a random track and fade to it
        if (instance.musicPlaylist != null && instance.musicPlaylist.Length > 0)
        {
            instance.currentTrackIndex = Random.Range(0, instance.musicPlaylist.Length);
            AudioClip firstClip = instance.musicPlaylist[instance.currentTrackIndex];
            instance.StartCoroutine(instance.CrossfadeTo(firstClip));
        }
        else if (instance.backgroundMusic != null)
        {
            instance.currentSource.clip = instance.backgroundMusic;
            instance.currentSource.loop = true;
            instance.currentSource.volume = instance.musicVolume;
            instance.currentSource.Play();
        }
    }

}
