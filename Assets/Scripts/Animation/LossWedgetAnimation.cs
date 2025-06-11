using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class LossWedget : MonoBehaviour
{
    [Header("Stars Configuration")]

    public int StarsToShow;
    public int score;

    [Header("Top Section")]
    public GameObject ribbon;
    public GameObject LossSkull;

    [Header("Stars")]
    public GameObject star1Empty;
    public GameObject star1Full;
    public GameObject star2Empty;
    public GameObject star2Full;
    public GameObject star3Empty;
    public GameObject star3Full;

    [Header("Score Section")]
    public GameObject scoretext;
    public GameObject scoreNumber;
    public GameObject coinIcon;
    public GameObject coinText;

    [Header("Particle Effects")]
    [SerializeField] private ParticleSystem sparkleYellow1;
    [SerializeField] private ParticleSystem sparkleYellow2;
    [SerializeField] private ParticleSystem sparkleYellow3;
    [SerializeField] private ParticleSystem shineYellow;
    [SerializeField] private ParticleSystem cardglowType03;

    [Header("Additional References")]
    public GameObject backgroundAlphaSetting;
    public GameObject FirstGameview;

    private Text scoreNumberText;

    private Dictionary<GameObject, (Vector3 position, Vector3 scale)> originalTransforms;

    void Awake()
{
        StarsToShow = GameManager.Instance.stars;
    StoreOriginalTransforms();
    InitializeParticleSystems();

    // Get Text component of scoreNumber
    scoreNumberText = scoreNumber.GetComponent<Text>();

    if (scoreNumberText != null)
    {
        scoreNumberText.text = GameManager.Instance.Score.ToString();
    }
    else
    {
        Debug.LogWarning("ScoreNumber does not have a Text component!");
    }
}


    public void StoreOriginalTransforms()
    {
        originalTransforms = new Dictionary<GameObject, (Vector3, Vector3)>();

        StringBuilder log = new StringBuilder();
        log.AppendLine($"=== UI Element Transforms ===");

        StoreAndLogTransform(ribbon, "Ribbon", log);
        StoreAndLogTransform(LossSkull, "Loss Skull", log);
        StoreAndLogTransform(star1Empty, "Star 1 Empty", log);
        StoreAndLogTransform(star1Full, "Star 1 Full", log);
        StoreAndLogTransform(star2Empty, "Star 2 Empty", log);
        StoreAndLogTransform(star2Full, "Star 2 Full", log);
        StoreAndLogTransform(star3Empty, "Star 3 Empty", log);
        StoreAndLogTransform(star3Full, "Star 3 Full", log);
        StoreAndLogTransform(scoretext, "Score Text", log);
        StoreAndLogTransform(scoreNumber, "Score Number", log);
        StoreAndLogTransform(coinIcon, "Coin Icon", log);
        StoreAndLogTransform(coinText, "Coin Text", log);

        Debug.Log(log.ToString());
    }

    void StoreAndLogTransform(GameObject obj, string elementName, StringBuilder log)
    {
        if (obj != null)
        {
            Vector3 position = obj.transform.localPosition;
            Vector3 scale = obj.transform.localScale;
            originalTransforms[obj] = (position, scale);
            log.AppendLine($"{elementName}:\n  Position: {position}\n  Scale: {scale}");
        }
        else
        {
            log.AppendLine($"WARNING: {elementName} is null!");
        }
    }

    void InitializeElements()
    {
        transform.localPosition = new Vector3(0f, 2827f, 0f);

        SetZeroScale(ribbon);
        SetZeroScale(LossSkull);
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
    }    void AnimateVictoryScreen()
    {
        Sequence sequence = DOTween.Sequence();

        sequence.Append(transform.DOLocalMove(new Vector3(0f, -52f, 0f), 1f).SetEase(Ease.OutQuad));

        // Play initial glow effect
        if (cardglowType03 != null)
        {
            cardglowType03.gameObject.SetActive(true);
            cardglowType03.Play();
        }

        sequence.Append(AnimateToOriginal(ribbon, 0.5f));
        sequence.Append(AnimateToOriginal(LossSkull, 0.5f))
            .OnComplete(() => {
                if (cardglowType03) cardglowType03.Play();
                if (shineYellow) shineYellow.Play();
            });

        // Animate empty stars and full stars based on StarsToShow
        sequence.Append(AnimateToOriginal(star1Empty, 0.3f, Ease.OutBack));
        if (StarsToShow >= 1)
        {
            sequence.Append(AnimateToOriginal(star1Full, 0.3f, Ease.OutElastic))
                .OnComplete(() => PlayStarParticles(1));
        }

        sequence.Append(AnimateToOriginal(star2Empty, 0.3f, Ease.OutBack));
        if (StarsToShow >= 2)
        {
            sequence.Append(AnimateToOriginal(star2Full, 0.3f, Ease.OutElastic))
                .OnComplete(() => PlayStarParticles(2));
        }

        sequence.Append(AnimateToOriginal(star3Empty, 0.3f, Ease.OutBack));
        if (StarsToShow >= 3)
        {
            sequence.Append(AnimateToOriginal(star3Full, 0.3f, Ease.OutElastic))
                .OnComplete(() => PlayStarParticles(3));
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

    private void InitializeParticleSystems()
    {
        if (sparkleYellow1 != null)
        {
            sparkleYellow1.Stop();
            var main = sparkleYellow1.main;
            main.playOnAwake = false;
        }
        if (sparkleYellow2 != null)
        {
            sparkleYellow2.Stop();
            var main = sparkleYellow2.main;
            main.playOnAwake = false;
        }
        if (sparkleYellow3 != null)
        {
            sparkleYellow3.Stop();
            var main = sparkleYellow3.main;
            main.playOnAwake = false;
        }
        if (shineYellow != null)
        {
            shineYellow.Stop();
            var main = shineYellow.main;
            main.playOnAwake = false;
        }
        if (cardglowType03 != null)
        {
            cardglowType03 .Stop();
            var main = cardglowType03.main;
            main.playOnAwake = false;
        }
    }

    private void PlayParticleEffect(ParticleSystem particleSystem, float delay = 0f)
    {
        if (particleSystem != null && !particleSystem.isPlaying)
        {
            if (delay > 0)
                StartCoroutine(PlayParticleWithDelay(particleSystem, delay));
            else
                particleSystem.Play();
        }
    }

    private IEnumerator PlayParticleWithDelay(ParticleSystem particleSystem, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (particleSystem != null && !particleSystem.isPlaying)
        {
            particleSystem.Play();
        }
    }

    private void PlayStarParticles(int starNumber)
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
                break;
        }
    }
}