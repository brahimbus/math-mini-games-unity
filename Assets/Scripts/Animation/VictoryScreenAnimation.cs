using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;
using System.Text;

public class VictoryScreenAnimation : MonoBehaviour
{
    [Header("Stars Configuration")]
    
    public int StarsToShow;

    [Header("Top Section")]
    public GameObject ribbon;
    public GameObject victoryHornsLeft;
    public GameObject victoryHornsRight;
    public GameObject victoryCup;

    [Header("Stars")]
    public GameObject star1Empty;
    public GameObject star1Full;
    public GameObject star2Empty;
    public GameObject star2Full;
    public GameObject star3Empty;
    public GameObject star3Full;

    [Header("Particle Effects")]
    [SerializeField] private ParticleSystem sparkleYellow1;
    [SerializeField] private ParticleSystem sparkleYellow2;
    [SerializeField] private ParticleSystem sparkleYellow3;
    [SerializeField] private ParticleSystem confettiBlastRainbow;
    [SerializeField] private ParticleSystem shineYellow;
    [SerializeField] private ParticleSystem cardglowType03;

    [Header("Score Section")]
    public GameObject scoretext;
    public GameObject scoreNumber;
    public GameObject coinIcon;
    public GameObject coinText;

    [Header("Additional References")]
    public GameObject backgroundAlphaSetting;
    public GameObject FirstGameview;

    private Dictionary<GameObject, (Vector3 position, Vector3 scale)> originalTransforms;

    private Text scoreNumberText;

void Awake()
{

    StarsToShow = GameManager.Instance.stars;
    StoreOriginalTransforms();

    // Get the Text component
    scoreNumberText = scoreNumber.GetComponent<Text>();

    // Set the text to current score
    if (scoreNumberText != null)
    {
        scoreNumberText.text = GameManager.Instance.Score.ToString();
    }
    else
    {
        Debug.LogWarning("ScoreNumber does not have a Text component!");
    }
}


    void StoreOriginalTransforms()
    {
        originalTransforms = new Dictionary<GameObject, (Vector3, Vector3)>();

        StringBuilder log = new StringBuilder();
        log.AppendLine("=== Victory Screen UI Element Transforms ===");

        void LogAndStore(GameObject obj, string name)
        {
            if (obj != null)
            {
                Vector3 position = obj.transform.localPosition;
                Vector3 scale = obj.transform.localScale;
                originalTransforms[obj] = (position, scale);
                log.AppendLine($"{name}:\n  Position: {position}\n  Scale: {scale}");
            }
            else
            {
                log.AppendLine($"WARNING: {name} is null!");
            }
        }

        LogAndStore(ribbon, "Ribbon");
        LogAndStore(victoryHornsLeft, "Victory Horns Left");
        LogAndStore(victoryHornsRight, "Victory Horns Right");
        LogAndStore(victoryCup, "Victory Cup");
        LogAndStore(star1Empty, "Star 1 Empty");
        LogAndStore(star1Full, "Star 1 Full");
        LogAndStore(star2Empty, "Star 2 Empty");
        LogAndStore(star2Full, "Star 2 Full");
        LogAndStore(star3Empty, "Star 3 Empty");
        LogAndStore(star3Full, "Star 3 Full");
        LogAndStore(scoretext, "Score Text");
        LogAndStore(scoreNumber, "Score Number");
        LogAndStore(coinIcon, "Coin Icon");
        LogAndStore(coinText, "Coin Text");

        Debug.Log(log.ToString());
    }

    void InitializeElements()
    {
        // Initialize particle systems
        if (sparkleYellow1 != null) sparkleYellow1.Stop();
        if (sparkleYellow2 != null) sparkleYellow2.Stop();
        if (sparkleYellow3 != null) sparkleYellow3.Stop();
        if (confettiBlastRainbow != null) confettiBlastRainbow.Stop();
        if (shineYellow != null) shineYellow.Stop();
        if (cardglowType03 != null) cardglowType03.Stop();

        transform.localPosition = new Vector3(0f, 2827f, 0f);

        SetZeroScale(ribbon);
        SetZeroScale(victoryHornsLeft);
        SetZeroScale(victoryHornsRight);
        SetZeroScale(victoryCup);
        SetZeroScale(star1Empty);
        SetZeroScale(star1Full);
        SetZeroScale(star2Empty);
        SetZeroScale(star2Full);
        SetZeroScale(star3Empty);
        SetZeroScale(star3Full);
        SetZeroScale(scoretext);
        SetZeroScale(scoreNumber);
        SetZeroScale(coinIcon);
        SetZeroScale(coinText);

        // Show background alpha setting and hide FirstGameview
        if (backgroundAlphaSetting != null)
        {
            backgroundAlphaSetting.SetActive(true);
        }

        if (FirstGameview != null)
        {
            FirstGameview.SetActive(false);
        }
    }

    void SetZeroScale(GameObject obj)
    {
        if (obj != null)
        {
            obj.transform.localScale = Vector3.zero;
        }
    }

    void PlayStarParticles(int starNumber)
    {
        switch (starNumber)
        {
            case 1:
                if (sparkleYellow1 != null)
                {
                    sparkleYellow1.gameObject.SetActive(true);
                    sparkleYellow1.Play();
                }
                break;
            case 2:
                if (sparkleYellow2 != null)
                {
                    sparkleYellow2.gameObject.SetActive(true);
                    sparkleYellow2.Play();
                }
                break;
            case 3:
                if (sparkleYellow3 != null)
                {
                    sparkleYellow3.gameObject.SetActive(true);
                    sparkleYellow3.Play();
                }
                if (confettiBlastRainbow != null)
                {
                    confettiBlastRainbow.gameObject.SetActive(true);
                    confettiBlastRainbow.Play();
                }
                break;
        }
    }

    void AnimateVictoryScreen()
{
    Sequence sequence = DOTween.Sequence();

    sequence.Append(transform.DOLocalMove(new Vector3(0f, -52f, 0f), 1.0f).SetEase(Ease.OutQuad));

    // Play initial effects
    if (cardglowType03 != null)
    {
        cardglowType03.startColor = new Color(1f, 0.682f, 0.098f);
        cardglowType03.gameObject.SetActive(true);
        cardglowType03.Play();
    }

    sequence.Append(AnimateToOriginal(ribbon, 0.5f));
    sequence.Join(AnimateToOriginal(victoryHornsLeft, 0.5f));
    sequence.Join(AnimateToOriginal(victoryHornsRight, 0.5f));
    sequence.Append(AnimateToOriginal(victoryCup, 0.5f));

    // **Play win sound here**
    sequence.AppendCallback(() => {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayWinSound();
        }
    });

    // Shine effect when cup appears
    if (shineYellow != null) 
    {
        sequence.AppendCallback(() => {
            shineYellow.startColor = new Color(0.682f, 0.592f, 0.153f);
            shineYellow.gameObject.SetActive(true);
            shineYellow.Play();
        });
    }

    // ... continue with stars and other animations
    sequence.Append(AnimateToOriginal(star1Empty, 0.3f, Ease.OutBack));
    if (StarsToShow >= 1)
    {
        sequence.Append(AnimateToOriginal(star1Full, 0.3f, Ease.OutElastic));
        sequence.AppendCallback(() => PlayStarParticles(1));
    }
        
    sequence.Append(AnimateToOriginal(star2Empty, 0.3f, Ease.OutBack));
    if (StarsToShow >= 2)
    {
        sequence.Append(AnimateToOriginal(star2Full, 0.3f, Ease.OutElastic));
        sequence.AppendCallback(() => PlayStarParticles(2));
    }
        
    sequence.Append(AnimateToOriginal(star3Empty, 0.3f, Ease.OutBack));
    if (StarsToShow >= 3)
    {
        sequence.Append(AnimateToOriginal(star3Full, 0.3f, Ease.OutElastic));
        sequence.AppendCallback(() => PlayStarParticles(3));
    }

    sequence.Append(AnimateToOriginal(scoretext, 0.5f));
    sequence.Join(AnimateToOriginal(scoreNumber, 0.5f));
    sequence.Join(AnimateToOriginal(coinIcon, 0.5f));
    sequence.Join(AnimateToOriginal(coinText, 0.5f));
}


    Tweener AnimateToOriginal(GameObject obj, float duration, Ease easeType = Ease.OutBounce)
    {
        if (obj != null && originalTransforms.ContainsKey(obj))
        {
            return obj.transform.DOScale(originalTransforms[obj].scale, duration).SetEase(easeType);
        }
        return null;
    }

    public void PlayAnimation()
    {
        DOTween.KillAll();
        InitializeElements();
        AnimateVictoryScreen();
    }
}