using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ButtonAnimation : MonoBehaviour
{
    private Button button;
    private Vector3 originalScale;

    [Header("UI References")]
    [SerializeField] private GameObject settingsWidget;
    [SerializeField] private GameObject backgroundAlphaSetting;
    [SerializeField] private Button closeButton;

    [Header("Volume Controls")]
    [SerializeField] private Slider soundVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;

    private Vector3 hidePosition = new Vector3(1719, -110.800003f, 0);
    private Vector3 showPosition = new Vector3(-35, -110.800003f, 0);
    private float animationDuration = 0.5f;

    private const string SOUND_VOLUME_KEY = "SoundVolume";
    private const string MUSIC_VOLUME_KEY = "MusicVolume";

    void Start()
    {
        button = GetComponent<Button>();
        originalScale = transform.localScale;
        button.onClick.AddListener(AnimateClick);

        // Setup close button
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseSettings);
        }

        // Initialize settings widget position
        if (settingsWidget != null)
        {
            settingsWidget.transform.localPosition = hidePosition;
        }

        // Setup volume sliders
        if (soundVolumeSlider != null)
        {
            soundVolumeSlider.onValueChanged.AddListener(OnSoundVolumeChanged);
            LoadAndSetVolume(soundVolumeSlider, true);
        }
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            LoadAndSetVolume(musicVolumeSlider, false);
        }
    }

    void AnimateClick()
    {
        transform.DOKill(); // Stop any ongoing animations
        transform.localScale = originalScale;

        // Button scale animation
        transform.DOScale(originalScale * 1.3f, 0.1f)
            .OnComplete(() =>
                transform.DOScale(originalScale, 0.1f)
            );

        // Animate settings widget
        if (settingsWidget != null)
        {
            settingsWidget.transform.DOLocalMove(showPosition, animationDuration)
                .SetEase(Ease.OutBack);
        }

        // Show background alpha setting
        if (backgroundAlphaSetting != null)
        {
            backgroundAlphaSetting.SetActive(true);
        }
    }

    void CloseSettings()
    {
        // Animate settings widget back to hide position
        if (settingsWidget != null)
        {
            settingsWidget.transform.DOLocalMove(hidePosition, animationDuration)
                .SetEase(Ease.InBack);
        }

        // Hide background alpha setting
        if (backgroundAlphaSetting != null)
        {
            backgroundAlphaSetting.SetActive(false);
        }
    }

    private void OnSoundVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSoundEffectsVolume(value);
            PlayerPrefs.SetFloat(SOUND_VOLUME_KEY, value);
            PlayerPrefs.Save();
            Debug.Log($"Sound Effects Volume Changed and Saved: {value}");
        }
    }

    private void OnMusicVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetBackgroundMusicVolume(value);
            PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, value);
            PlayerPrefs.Save();
            Debug.Log($"Background Music Volume Changed and Saved: {value}");
        }
    }

    private void LoadAndSetVolume(Slider slider, bool isSound)
    {
        slider.minValue = 0f;
        slider.maxValue = 1f;

        string key = isSound ? SOUND_VOLUME_KEY : MUSIC_VOLUME_KEY;
        float savedVolume = PlayerPrefs.GetFloat(key, 1f); // Default to full volume if not set
        slider.value = savedVolume;

        if (AudioManager.Instance != null)
        {
            if (isSound)
            {
                AudioManager.Instance.SetSoundEffectsVolume(savedVolume);
            }
            else
            {
                AudioManager.Instance.SetBackgroundMusicVolume(savedVolume);
            }
        }
    }
}