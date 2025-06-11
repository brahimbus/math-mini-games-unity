using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;
using DG.Tweening;
using System.Threading.Tasks;

public class NumberSlotChecker1 : DigitSlot
{
    public KeyboardWidget keyboardWidget;

    [Header("Game Settings")]
    [SerializeField] private float timeRemaining;
    
    [Header("UI Components")]
    [SerializeField] private TMP_Text ScoreText;
    [SerializeField] private TMP_Text lifeRestText;
    [SerializeField] private TMP_Text MessageText;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text number1Text;
    [SerializeField] private TMP_Text number2Text;

    [Header("Star System")]
    [SerializeField] private Slider starsSlider;
    [SerializeField] private Image[] starImages;
    [SerializeField] private float starAnimDuration = 0.5f;
    [SerializeField] private float starPopScale = 1.2f;
    [SerializeField] private float oneStarThreshold = 0.2f;
    [SerializeField] private float twoStarThreshold = 0.71f;
    [SerializeField] private float threeStarThreshold = 0.915f;

    [Header("Particle Systems")]
    [SerializeField] private ParticleSystem bigStarFilledParticle;
    [SerializeField] private ParticleSystem emojiThumbsUpParticle;
    [SerializeField] private ParticleSystem emojiStarstruckParticle;
    [SerializeField] private ParticleSystem sparkleYellow1Particle;
    [SerializeField] private ParticleSystem sparkleYellow2Particle;
    [SerializeField] private ParticleSystem sparkleYellow3Particle;
    [SerializeField] private ParticleSystem heartBreakParticle;

    private ParticleSystem.MainModule bigStarFilledMain;
    private ParticleSystem.MainModule emojiThumbsUpMain;
    private ParticleSystem.MainModule emojiStarstruckMain;
    private ParticleSystem.MainModule sparkleYellow1Main;
    private ParticleSystem.MainModule sparkleYellow2Main;
    private ParticleSystem.MainModule sparkleYellow3Main;
    private ParticleSystem.MainModule heartBreakMain;

    private int round = 0;
    private int score = 0;
    private int correctAnswers = 0;
    private int wrongAnswers = 0;
    private int previousStars = 0;
    private int livesRemaining;
    private int number1;
    private int number2;
    private bool isGameOver = false;
    private bool timerIsRunning = false;
    private int startingLives = 3;
    private int maxRounds;
    private int maxNumberRange;

    void Start()
    {
        keyboardWidget.symbols = new string[] { ">", "<", "=" };
        maxRounds = GameManager.Instance.numQuestions;
        maxNumberRange = GameManager.Instance.maxNumberRange;
        timeRemaining = GameSessionData.completionTime * 60f; // Convert minutes to seconds
        InitializeParticleSystems();
        InitializeGameState();
    }
    

    void Update()
    {
        if (timerIsRunning)
        {
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
                UpdateTimerDisplay();
            }
            else
            {
                timeRemaining = 0;
                timerIsRunning = false;
                HandleTimeUp();
            }
        }
    }

    private void InitializeGameState()
    {
        livesRemaining = startingLives;
        UpdateLivesDisplay();
        correctAnswers = 0;
        wrongAnswers = 0;
        previousStars = 0;
        ResetStars();
        
        if (starsSlider != null)
        {
            starsSlider.value = 0f;
        }
        
        GenerateNewNumbers();
        timerIsRunning = true;
        UpdateTimerDisplay();
    }

    private void InitializeParticleSystems()
    {
        if (bigStarFilledParticle != null)
        {
            bigStarFilledMain = bigStarFilledParticle.main;
            bigStarFilledMain.playOnAwake = false;
            bigStarFilledParticle.Stop();
        }
        if (emojiThumbsUpParticle != null)
        {
            emojiThumbsUpMain = emojiThumbsUpParticle.main;
            emojiThumbsUpMain.playOnAwake = false;
            emojiThumbsUpParticle.Stop();
        }
        if (emojiStarstruckParticle != null)
        {
            emojiStarstruckMain = emojiStarstruckParticle.main;
            emojiStarstruckMain.playOnAwake = false;
            emojiStarstruckParticle.Stop();
        }
        if (sparkleYellow1Particle != null)
        {
            sparkleYellow1Main = sparkleYellow1Particle.main;
            sparkleYellow1Main.playOnAwake = false;
            sparkleYellow1Particle.Stop();
        }
        if (sparkleYellow2Particle != null)
        {
            sparkleYellow2Main = sparkleYellow2Particle.main;
            sparkleYellow2Main.playOnAwake = false;
            sparkleYellow2Particle.Stop();
        }
        if (sparkleYellow3Particle != null)
        {
            sparkleYellow3Main = sparkleYellow3Particle.main;
            sparkleYellow3Main.playOnAwake = false;
            sparkleYellow3Particle.Stop();
        }
        if (heartBreakParticle != null)
        {
            heartBreakMain = heartBreakParticle.main;
            heartBreakMain.playOnAwake = false;
            heartBreakParticle.Stop();
        }
    }

    private void ResetStars()
    {
        if (starImages != null)
        {
            foreach (var star in starImages)
            {
                if (star != null)
                {
                    star.transform.localScale = Vector3.one;
                    star.color = new Color(1f, 1f, 1f, 0.3f);
                    star.transform.rotation = Quaternion.identity;
                }
            }
        }
    }

    private void UpdateStarProgress()
    {
        if (starsSlider != null)
        {
            float targetProgress = (float)correctAnswers / maxRounds;
            
            DOTween.To(() => starsSlider.value, x => starsSlider.value = x, targetProgress, 0.5f)
                   .SetEase(Ease.OutCubic);

            int currentStars = CalculateStars();
            if (currentStars > previousStars)
            {
                AnimateStars(previousStars, currentStars);
                previousStars = currentStars;
            }
        }
    }

    private void AnimateStars(int fromStar, int toStar)
    {
        for (int i = fromStar; i < toStar; i++)
        {
            if (starImages != null && i < starImages.Length && starImages[i] != null)
            {
                Sequence starSequence = DOTween.Sequence();
                
                starSequence.Append(starImages[i].transform.DOScale(Vector3.one * starPopScale, starAnimDuration)
                    .SetEase(Ease.OutBack));
                
                starSequence.Append(starImages[i].transform.DOScale(Vector3.one, starAnimDuration * 0.5f)
                    .SetEase(Ease.OutBounce));

                starImages[i].DOColor(Color.white, starAnimDuration)
                    .SetEase(Ease.OutCubic);

                starImages[i].transform.DORotate(new Vector3(0, 0, 360), starAnimDuration, RotateMode.FastBeyond360)
                    .SetEase(Ease.OutBack);

                starSequence.Insert(0, starImages[i].DOColor(Color.yellow, starAnimDuration * 0.5f)
                    .SetEase(Ease.OutFlash, 2, 0))
                    .Append(starImages[i].DOColor(Color.white, starAnimDuration * 0.5f));

                // Play emoji first
                PlayRandomCorrectEmoji();
                
                // Play star particles with delay
                PlayParticleEffect(bigStarFilledParticle, 2f); // 1 second delay

                // Play corresponding sparkle effect based on star number with slight delay
                switch (i)
                {
                    case 0:
                        PlayParticleEffect(sparkleYellow1Particle, 1.2f);
                        break;
                    case 1:
                        PlayParticleEffect(sparkleYellow2Particle, 1.2f);
                        break;
                    case 2:
                        PlayParticleEffect(sparkleYellow3Particle, 1.2f);
                        break;
                }
            }
        }
    }

    public void GenerateNewNumbers()
    {
        if (isGameOver) return;

        number1 = Random.Range(0, maxNumberRange + 1);
        number2 = Random.Range(0, maxNumberRange + 1);
        
        if (number1Text != null) number1Text.text = number1.ToString();
        if (number2Text != null) number2Text.text = number2.ToString();

        Debug.Log($"Generated Numbers: {number1} and {number2}");
        ShowMessage("Compare these numbers");
    }

    public override async void OnDrop(PointerEventData eventData)
    {
        if (isGameOver) return;

        base.OnDrop(eventData);

        if (slotText == null)
        {
            Debug.LogError("SlotText reference is missing!");
            return;
        }

        string droppedSymbol = symbol;
        round++;
        await HandleAnswerCheck(droppedSymbol);

        if (round >= maxRounds)
        {
            HandleVictory();
        }
    }

    private async Task HandleAnswerCheck(string droppedSymbol)
    {
        bool isCorrect = false;

        if (number1 > number2 && droppedSymbol == ">")
            isCorrect = true;
        else if (number1 < number2 && droppedSymbol == "<")
            isCorrect = true;
        else if (number1 == number2 && droppedSymbol == "=")
            isCorrect = true;

        if (isCorrect)
        {
            HandleCorrectAnswer();
        }
        else
        {
            HandleWrongAnswer(droppedSymbol);
        }

        await Task.Delay(1000);
    }

    private void HandleCorrectAnswer()
    {
        score += 10;
        correctAnswers++;
        UpdateStarProgress();
        
        PlayRandomCorrectEmoji();
        
        ShowMessage("Correct!");
        if (ScoreText != null)
        {
            ScoreText.text = $"{score}";
        }
        StartCoroutine(GenerateNewNumbersAfterDelay(2f));
    }

    private void HandleWrongAnswer(string droppedSymbol)
    {
        livesRemaining = Mathf.Clamp(livesRemaining - 1, 0, startingLives);
        wrongAnswers++;
        
        PlayParticleEffect(heartBreakParticle);
        
        UpdateLivesDisplay();
        ShowMessage("Wrong answer!");
        
        StartCoroutine(GenerateNewNumbersAfterDelay(2f));
    }

    private void UpdateTimerDisplay()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(timeRemaining / 60f);
            int seconds = Mathf.FloorToInt(timeRemaining % 60f);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    private void HandleTimeUp()
    {
        isGameOver = true;
        timerIsRunning = false;
        if (MessageText != null)
        {
            MessageText.text = "Time's Up!";
        }

        // Save remaining time (0 in this case since time is up)
        GameSessionData.completionTime = 0;
        
        GameManager.Instance.MiniGameComplete(score, correctAnswers, wrongAnswers);
    }

    private int CalculateStars()
    {
        float progress = (float)correctAnswers / maxRounds;
        
        if (progress >= threeStarThreshold)
            return 3;
        else if (progress >= twoStarThreshold)
            return 2;
        else if (progress >= oneStarThreshold)
            return 1;
        else
            return 0;
    }

    private void HandleVictory()
    {
        isGameOver = true;
        timerIsRunning = false;
        
        int finalStars = CalculateStars();
        
        if (MessageText != null)
        {
            MessageText.text = $"Congratulations! You Won!\nYou earned {finalStars} stars!";
        }

        // Save remaining time in minutes
        GameSessionData.completionTime = timeRemaining / 60f;
        
        GameManager.Instance.MiniGameComplete(score, correctAnswers, wrongAnswers);
    }


    private void ShowMessage(string feedback)
    {
        if (MessageText != null)
        {
            MessageText.text = $"Round {round} of {maxRounds}\n{feedback}";
        }
    }

    private IEnumerator GenerateNewNumbersAfterDelay(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        if (!isGameOver)
        {
            slotText.text = "";
            GenerateNewNumbers();
        }
    }

    private void UpdateLivesDisplay()
    {
        if (lifeRestText != null)
        {
            lifeRestText.text = $"{livesRemaining}";
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

    private void PlayRandomCorrectEmoji()
    {
        if (Random.value > 0.5f)
        {
            PlayParticleEffect(emojiThumbsUpParticle);
        }
        else
        {
            PlayParticleEffect(emojiStarstruckParticle);
        }
    }
}