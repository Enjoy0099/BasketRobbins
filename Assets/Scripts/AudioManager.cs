using BasketRobbins;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Sources")]
    public AudioSource musicSource;
    public AudioSource[] sfxSources;
    private int currentIndex = 0;

    [Header("Audio Clips")]
    public AudioClip BGMusic;
    public AudioClip Score_SwishSound;
    public AudioClip Score_NormalSound;
    public AudioClip ballBounceSound;
    public AudioClip ballRimHitSound;

    [Header("Ducker")]
    public AudioDucker audioDucker;

    [Header("Settings")]
    public bool isMusicMuted = false;
    public bool isSFXMuted = false;

    [Range(0f, 1f)] public float musicVolume = 0.29f;
    [Range(0f, 1f)] public float sfxVolume = 1f;


    void Start()
    {
        isMusicMuted = PlayerPrefs.GetInt(GameConstants.Prefs.MusicMuted, 0) == 1;
        isSFXMuted = PlayerPrefs.GetInt(GameConstants.Prefs.SFXMuted, 0) == 1;
        musicVolume = PlayerPrefs.GetFloat(GameConstants.Prefs.MusicVolume, musicVolume);
        sfxVolume = PlayerPrefs.GetFloat(GameConstants.Prefs.SFXVolume, sfxVolume);
    }


    // ------------------ Toggle ------------------

    public void ToggleMusic()
    {
        isMusicMuted = !isMusicMuted;
        musicSource.volume = isMusicMuted ? 0f : musicVolume;
    }

    public void ToggleSFX()
    {
        isSFXMuted = !isSFXMuted;
    }

    // ------------------ Sliders ------------------
    public void SetMusicVolume(float value)
    {
        musicVolume = value;
        if (!isMusicMuted)
            musicSource.volume = musicVolume;
    }

    public void SetSFXVolume(float value)
    {
        sfxVolume = value;
    }

    // ------------------ MUSIC ------------------

    public void PlayMusic()
    {
        musicSource.clip = BGMusic;
        musicSource.loop = true;
        musicSource.volume = isMusicMuted ? 0f : musicVolume;
        musicSource.Play();
    }

    // ------------------ GENERIC SFX PLAYER ------------------

    private AudioSource GetAvailableSource()
    {
        if (sfxSources == null || sfxSources.Length == 0)
        {
            Debug.LogWarning("No SFX Sources assigned!");
            return null;
        }


        currentIndex = (currentIndex + 1) % sfxSources.Length;
        return sfxSources[currentIndex];
    }

    private void PlaySFX(AudioClip clip, float volume = 1f, float pitchMin = 1f, float pitchMax = 1f, bool duck = true)
    {
        if (isSFXMuted) return;

        AudioSource source = GetAvailableSource();
        if (source == null) return;
        source.pitch = 1f;

        source.pitch = Random.Range(pitchMin, pitchMax);
        float finalVolume = volume * sfxVolume;
        source.PlayOneShot(clip, finalVolume);

        if (duck && audioDucker != null)
            audioDucker.Duck();
    }

    // ------------------ GAME EVENTS ------------------

    public void PlaySwish()
    {
        PlaySFX(Score_SwishSound, 1f, 0.95f, 1.05f);
    }

    public void PlayNormal()
    {
        PlaySFX(Score_NormalSound, 0.9f, 0.95f, 1.05f);
    }

    public void PlayRimHit()
    {
        PlaySFX(ballRimHitSound, 1f, 0.9f, 1.1f);
    }

    public void PlayBounce(float impactForce)
    {
        if (isSFXMuted || impactForce < 1f) return; // ignore tiny hits

        float minForce = 1f;
        float maxForce = 15f;

        float normalized = Mathf.InverseLerp(minForce, maxForce, impactForce);

        float volume = Mathf.Lerp(0.1f, 1f, normalized);
        float pitch = Mathf.Lerp(0.8f, 1.2f, normalized);

        AudioSource source = GetAvailableSource();
        if (source == null) return;
        source.pitch = 1f;

        source.pitch = pitch;
        float finalVolume = volume * sfxVolume;
        source.PlayOneShot(ballBounceSound, finalVolume);

        if (audioDucker != null)
            audioDucker.Duck();
    }


    public void SaveSettings()
    {
        PlayerPrefs.SetInt(GameConstants.Prefs.MusicMuted, isMusicMuted ? 1 : 0);
        PlayerPrefs.SetInt(GameConstants.Prefs.SFXMuted, isSFXMuted ? 1 : 0);
        PlayerPrefs.SetFloat(GameConstants.Prefs.MusicVolume, musicVolume);
        PlayerPrefs.SetFloat(GameConstants.Prefs.SFXVolume, sfxVolume);
    }
}