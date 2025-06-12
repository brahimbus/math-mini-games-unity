using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;
using DG.Tweening;
using System.Threading.Tasks;

public class NumberSlotChecker : DigitSlot
{
    public KeyboardWidget keyboardWidget;

    [Header("Game Settings")]
    [SerializeField] private int maxRounds;
    [SerializeField] private float timeRemaining;
    
    [Header("UI Components")]
    [SerializeField] private TMP_Text ScoreText;
    [SerializeField] private TMP_Text lifeRestText;
    [SerializeField] private TMP_Text MessageText;
    [SerializeField] private TMP_Text timerText;
    
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
    private ParticleSystem.MainModule heartBreakMain;    [Header("Audio")]
    // Using AudioManager.Instance instead of serialized reference
    private AudioClip winVoice;
    private AudioClip loseVoice;

    private int round = 0;
    private int score = 0;
    private int correctAnswers = 0;
    private int previousStars = 0;
    private int livesRemaining;
    private int correctNumber;
    private bool isGameOver = false;
    private bool timerIsRunning = false;
    private int startingLives = 3;
    private int maxNumberRange;    void Awake()
    {
        LoadAudioClips();
        InitializeGameState();
        InitializeParticleSystems();
    }

    void Start()
    {
        maxRounds = GameManager.Instance.numQuestions;
        maxNumberRange = GameManager.Instance.maxNumberRange;
        timeRemaining = GameSessionData.completionTime * 60f;
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
    }    private void LoadAudioClips()
    {
        winVoice = Resources.Load<AudioClip>("Audio/win_voice");
        loseVoice = Resources.Load<AudioClip>("Audio/lose_voice");

        if (winVoice == null)
            Debug.LogError("Could not load win_voice.wav from Resources folder!");
        if (loseVoice == null)
            Debug.LogError("Could not load lose_voice.wav from Resources folder!");
    }

    private void InitializeGameState()
    {
        livesRemaining = startingLives;
        UpdateLivesDisplay();
        correctAnswers = 0;
        previousStars = 0;
        ResetStars();
        
        if (starsSlider != null)
        {
            starsSlider.value = 0f;
        }
        
        GenerateNewNumber();
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

    private IEnumerator DeactivateParticle(ParticleSystem particleSystem)
    {
        // Wait until the particle system has finished
        while (particleSystem.isPlaying)
        {
            yield return null;
        }
        
        // Wait a little extra time to ensure all particles are gone
        yield return new WaitForSeconds(0.1f);
        
        // Deactivate the game object
        particleSystem.gameObject.SetActive(false);
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
                }                if (winVoice != null)
                {
                    AudioManager.Instance.PlayOneShot(winVoice);
                }
            }
        }
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
    Debug.Log("Time is finished!");
    isGameOver = true;
    if (MessageText != null)
    {
        MessageText.text = "Time's Up!";
    }
    
    // Save remaining time (0 in this case since time is up)
    GameSessionData.completionTime = 0;
    
    GameManager.Instance.MiniGameComplete(score, correctAnswers, maxRounds - correctAnswers);
}    // OnValidate no longer needed since we're using the singleton pattern


    public void GenerateNewNumber()
{
    if (isGameOver) return;

    correctNumber = Random.Range(0, maxNumberRange);
    

    // Now generate a new set of symbols:
    string[] newSymbols = GenerateSymbolSet(correctNumber, 4); // 4 symbols total
    keyboardWidget.UpdateSymbols(newSymbols);

    Debug.Log($"Generated Number: {correctNumber}");
    StartCoroutine(WaitAndShowMessage(0.7f));
}

    private string[] GenerateSymbolSet(int correctNumber, int totalSymbols)
    {
        System.Collections.Generic.HashSet<int> symbolSet = new System.Collections.Generic.HashSet<int>();

    
        symbolSet.Add(correctNumber);

    
        while (symbolSet.Count < totalSymbols)
        {
            int randomNum = Random.Range(0, 21); 
            symbolSet.Add(randomNum); 
        }

    
        var symbolList = new System.Collections.Generic.List<string>();

        foreach (int num in symbolSet)
        {
            symbolList.Add(num.ToString());
        }

        // Shuffle the list so correctNumber is not always in first position
        for (int i = 0; i < symbolList.Count; i++)
        {
            int randIndex = Random.Range(i, symbolList.Count);
            (symbolList[i], symbolList[randIndex]) = (symbolList[randIndex], symbolList[i]);
        }

        return symbolList.ToArray();
    }    public void PlaySoundForNumber()
{
    string key = correctNumber.ToString();
    AudioManager.Instance.PlaySoundForKey(key);
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

    if (!int.TryParse(symbol, out int droppedNumber))
    {
        Debug.LogWarning("Dropped symbol is not a valid number.");
        return;
    }

    round++;
    await HandleAnswerCheck(droppedNumber);

    if (round >= maxRounds && livesRemaining > 0)
    {
        HandleVictory();
    }
}

private async Task HandleAnswerCheck(int droppedNumber)
{
    if (droppedNumber == correctNumber)
    {
        HandleCorrectAnswer();
    }
    else
    {
        HandleWrongAnswer(droppedNumber);
    }

   
    await Task.Delay(1000); 
}


    private void HandleCorrectAnswer()
    {
        score += 10;
        correctAnswers++;
        UpdateStarProgress();
        
        PlayRandomCorrectEmoji();
        AudioManager.Instance.PlayCorrectSound();
        
        ShowMessage("You answered right!");
        if (ScoreText != null)
        {
            ScoreText.text = $"{score}";
        }
        if (winVoice != null)
        {
            AudioManager.Instance.PlayOneShot(winVoice);
        }

        StartCoroutine(GenerateNewNumberAfterDelay(3f));
    }

    private void HandleWrongAnswer(int droppedNumber)
    {
        Debug.Log($"Wrong number. Expected {correctNumber}, got {droppedNumber}");
        livesRemaining = Mathf.Clamp(livesRemaining - 1, 0, startingLives);

        PlayParticleEffect(heartBreakParticle);
        AudioManager.Instance.PlayFalseSound();

        UpdateLivesDisplay();
        ShowMessage("Wrong answer!");

        if (loseVoice != null)
        {
            AudioManager.Instance.PlayOneShot(loseVoice);
        }

        if (livesRemaining <= 0)
        {
            HandleGameOver();
        }
        else
        {
            StartCoroutine(GenerateNewNumberAfterDelay(3f));
        }
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

    GameManager.Instance.MiniGameComplete(score, correctAnswers, maxRounds - correctAnswers);
}

    // Update HandleGameOver method:
private void HandleGameOver()
{
    isGameOver = true;
    timerIsRunning = false;
    Debug.Log("Game Over! No lives left.");
    if (MessageText != null)
    {
        MessageText.text = "Game Over!";
    }

    // Save remaining time in minutes
    GameSessionData.completionTime = timeRemaining / 60f;
    
    GameManager.Instance.MiniGameComplete(score, correctAnswers, maxRounds - correctAnswers);
}

    private void ShowMessage(string feedback)
    {
        if (MessageText != null)
        {
            MessageText.text = $"Round {round} of {maxRounds}\n{feedback}";
        }
    }

    private IEnumerator GenerateNewNumberAfterDelay(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        if (!isGameOver)
        {
            slotText.text = "";
            GenerateNewNumber();
        }
    }

    private IEnumerator WaitAndShowMessage(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        if (MessageText != null && !isGameOver)
        {
            ShowMessage("Listen carefully...");
            PlaySoundForNumber();
        }
    }

    private void UpdateLivesDisplay()
    {
        if (lifeRestText != null)
        {
            lifeRestText.text = $"{livesRemaining}";
        }
    }

   
}