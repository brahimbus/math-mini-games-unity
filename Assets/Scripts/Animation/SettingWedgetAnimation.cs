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
    [SerializeField] private RectTransform soundVolumeKnob;
    [SerializeField] private RectTransform musicVolumeKnob;

    private Vector3 hidePosition = new Vector3(1719, -110.800003f, 0);
    private Vector3 showPosition = new Vector3(-35, -110.800003f, 0);
    private float animationDuration = 0.5f;
    
    // Volume control variables
    private float minX = -170.88f;
    private float maxX = 175f;
    private bool isDraggingSound = false;
    private bool isDraggingMusic = false;

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

        // Initialize volume knob positions
        if (soundVolumeKnob != null)
        {
            SetInitialVolume(soundVolumeKnob);
        }
        if (musicVolumeKnob != null)
        {
            SetInitialVolume(musicVolumeKnob);
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

    public void OnSoundVolumeBeginDrag()
    {
        isDraggingSound = true;
    }

    public void OnMusicVolumeBeginDrag()
    {
        isDraggingMusic = true;
    }

    public void OnVolumeEndDrag()
    {
        isDraggingSound = false;
        isDraggingMusic = false;
    }

    private void Update()
    {
        if (isDraggingSound)
        {
            UpdateVolumeKnob(soundVolumeKnob);
        }
        if (isDraggingMusic)
        {
            UpdateVolumeKnob(musicVolumeKnob);
        }
    }

    private void UpdateVolumeKnob(RectTransform knob)
    {
        if (knob == null) return;

        Vector2 mousePos = Input.mousePosition;
        Vector2 localPoint;

        RectTransform parentRect = knob.parent as RectTransform;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, mousePos, Camera.main, out localPoint))
        {
            float newX = Mathf.Clamp(localPoint.x, minX, maxX);
            Vector2 currentPos = knob.anchoredPosition;
            currentPos.x = newX;
            knob.anchoredPosition = currentPos;

            float volumeValue = Mathf.InverseLerp(minX, maxX, newX);
            if (knob == soundVolumeKnob)
            {
                AudioListener.volume = volumeValue;
            }
        }
    }

    private void SetInitialVolume(RectTransform knob)
    {
        Vector2 pos = knob.anchoredPosition;
        pos.x = minX;
        knob.anchoredPosition = pos;
    }
}