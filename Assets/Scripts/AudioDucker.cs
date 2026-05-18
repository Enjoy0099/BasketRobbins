using UnityEngine;
using System.Collections;

public class AudioDucker : MonoBehaviour
{
    public AudioSource musicSource;

    [Range(0f, 1f)]  public float duckAmount = 0.5f;   // how much to lower music
    public float duckTime = 0.2f;     // how long to stay low
    public float fadeDownSpeed = 2f; // slower drop
    public float fadeUpSpeed = 3f;  // faster recovery

    private float originalVolume;
    private float targetVolume;
    private float duckTimer;

    private bool isDucking = false;

    void Start()
    {
        originalVolume = musicSource.volume;
    }

    public void Duck()
    {
        duckTimer = duckTime;

        targetVolume = originalVolume * duckAmount;

        isDucking = true;
    }

    void Update()
    {
        if (!isDucking) return;

        // Fade DOWN
        if (duckTimer > 0)
        {
            duckTimer -= Time.deltaTime;

            musicSource.volume = Mathf.MoveTowards(
                musicSource.volume,
                targetVolume,
                fadeDownSpeed * Time.deltaTime
            );
        }
        else
        {
            // Fade UP
            musicSource.volume = Mathf.MoveTowards(
                musicSource.volume,
                originalVolume,
                fadeUpSpeed * Time.deltaTime
            );

            // Stop when reached
            if (Mathf.Approximately(musicSource.volume, originalVolume))
            {
                isDucking = false;
            }
        }
    }
}