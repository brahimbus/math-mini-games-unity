using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    private AudioSource audioSource;
    private AudioSource backgroundMusicSource; // Separate source for background music

    // List of key → AudioClip pairs editable in Inspector
    public List<NumberAudioPair> numberClips;

    [Header("Game Sound Effects")]
    public AudioClip winSound;

    public AudioClip loseSound;
    public AudioClip correctSound;
    public AudioClip falseSound;
    public AudioClip backgroundMusic;

    // Optional: runtime Dictionary for faster lookup
    private Dictionary<string, AudioClip> numberClipMap;

    void Awake()
    {
        // Implement singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Initialize audio sources
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                Debug.LogError("AudioSource component missing on AudioManager!");
                audioSource = gameObject.AddComponent<AudioSource>();
            }

            // Setup background music source
            backgroundMusicSource = gameObject.AddComponent<AudioSource>();
            backgroundMusicSource.loop = true;
            backgroundMusicSource.playOnAwake = false;

            if (numberClips == null || numberClips.Count == 0)
            {
                Debug.LogWarning("Number clips list is empty or not set!");
            }

            // Build dictionary for fast lookup
            numberClipMap = new Dictionary<string, AudioClip>();
            foreach (var pair in numberClips)
            {
                if (!numberClipMap.ContainsKey(pair.key))
                {
                    numberClipMap.Add(pair.key, pair.clip);
                }
                else
                {
                    Debug.LogWarning($"Duplicate key found in NumberAudioPair list: {pair.key}. Ignoring duplicate.");
                }
            }
        }
        else
        {
            // If an instance already exists, destroy this one
            Destroy(gameObject);
            return;
        }
    }

    // Play sound based on the key passed
    public void PlaySoundForKey(string key)
    {
        if (audioSource == null)
        {
            Debug.LogError("AudioSource is null in AudioManager!");
            return;
        }

        if (numberClipMap == null || numberClipMap.Count == 0)
        {
            Debug.LogError("Number clip map is not initialized!");
            return;
        }

        if (numberClipMap.TryGetValue(key, out AudioClip clip))
        {
            if (clip == null)
            {
                Debug.LogError($"Audio clip for key '{key}' is null!");
                return;
            }

            audioSource.clip = clip;
            audioSource.Play();
        }
        else
        {
            Debug.LogWarning($"Audio clip not found for key: {key}");
        }
    }

    // Play one-shot audio clips (unchanged)
    public void PlayOneShot(AudioClip clip)
    {
        if (audioSource == null)
        {
            Debug.LogError("AudioSource is null in AudioManager!");
            return;
        }

        if (clip == null)
        {
            Debug.LogError("Audio clip is null!");
            return;
        }

        audioSource.PlayOneShot(clip);
    }

    public void PlayWinSound()
    {
        if (winSound != null)
        {
            audioSource.PlayOneShot(winSound);
        }
        else
        {
            Debug.LogWarning("Win sound clip is not assigned!");
        }
    }

    public void PlayLoseSound()
    {
        if (loseSound != null)
        {
            audioSource.PlayOneShot(loseSound);
        }
        else
        {
            Debug.LogWarning("Lose sound clip is not assigned!");
        }
    }

    public void PlayCorrectSound()
    {
        if (correctSound != null)
        {
            audioSource.PlayOneShot(correctSound);
        }
        else
        {
            Debug.LogWarning("Correct sound clip is not assigned!");
        }
    }

    public void PlayFalseSound()
    {
        if (falseSound != null)
        {
            audioSource.PlayOneShot(falseSound);
        }
        else
        {
            Debug.LogWarning("False sound clip is not assigned!");
        }
    }

    public void PlayBackgroundMusic()
    {
        if (backgroundMusic != null)
        {
            backgroundMusicSource.clip = backgroundMusic;
            backgroundMusicSource.Play();
        }
        else
        {
            Debug.LogWarning("Background music clip is not assigned!");
        }
    }

    public void StopBackgroundMusic()
    {
        backgroundMusicSource.Stop();
    }

    public void SetBackgroundMusicVolume(float volume)
    {
        backgroundMusicSource.volume = Mathf.Clamp01(volume);
    }

    public void SetSoundEffectsVolume(float volume)
    {
        audioSource.volume = Mathf.Clamp01(volume);
    }
}
