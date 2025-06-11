using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private AudioSource audioSource;

    // List of key → AudioClip pairs editable in Inspector
    public List<NumberAudioPair> numberClips;

    // Optional: runtime Dictionary for faster lookup
    private Dictionary<string, AudioClip> numberClipMap;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogError("AudioSource component missing on AudioManager!");
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        if (numberClips == null || numberClips.Count == 0)
        {
            Debug.LogError("Number clips list is empty or not set!");
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
}
