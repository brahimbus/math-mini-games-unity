using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using DG.Tweening;
using System.Threading.Tasks;
using System.Linq;

public class NumberSlotChecker2 : DigitSlot
{
    public KeyboardWidget keyboardWidget;

    [Header("Game Settings")]
    [SerializeField] private float timeRemaining;


    [Header("UI Components")]
    [SerializeField] private TMP_Text ScoreText;
    [SerializeField] private TMP_Text MessageText;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private Button backButton;

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

    private float TimeCompleted { get; set; }
    private int round = 0;
    private int score = 0;
    private int correctAnswers = 0;
    private int previousStars = 0;
    private bool timerIsRunning = false;
    private List<int> droppedNumbers = new List<int>();
    private string Sequance = "";
    private int maxRounds;
    private int maxNumberRange;
    private int numbersRequired;
    private Dictionary<int, KeyboardButton> usedButtons = new Dictionary<int, KeyboardButton>();

    private ParticleSystem.MainModule bigStarFilledMain;
    private ParticleSystem.MainModule emojiThumbsUpMain;
    private ParticleSystem.MainModule emojiStarstruckMain;
    private ParticleSystem.MainModule sparkleYellow1Main;
    private ParticleSystem.MainModule sparkleYellow2Main;
    private ParticleSystem.MainModule sparkleYellow3Main;
    private ParticleSystem.MainModule heartBreakMain;

    void Awake()
    {
        InitializeParticleSystems();
    }

    void Start()
    {
        maxRounds = GameManager.Instance.numQuestions;
        maxNumberRange = GameManager.Instance.maxNumberRange;
        numbersRequired = GameManager.Instance.maxNumbersInSequence;
        timeRemaining = GameSessionData.completionTime * 60f;
        GenerateUniqueSymbols();
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

    private void GenerateUniqueSymbols()
{
    if (numbersRequired > maxNumberRange)
    {
        Debug.LogError("numbersRequired exceeds the range of unique numbers!");
        return;
    }

    // Generate a new list of random unique numbers
    List<int> numbers = new List<int>();
    while (numbers.Count < numbersRequired)
    {
        int randomNum = Random.Range(1, maxNumberRange + 1);
        if (!numbers.Contains(randomNum))
        {
            numbers.Add(randomNum);
        }
    }

    // Convert to string array and update keyboard
    string[] newSymbols = numbers.Select(n => n.ToString()).ToArray();
    keyboardWidget.UpdateSymbols(newSymbols);
}

    private void InitializeGameState()
    {
        correctAnswers = 0;
        previousStars = 0;
        ResetStars();
        droppedNumbers.Clear();
        usedButtons.Clear();
        Sequance = "";
        TimeCompleted = 0f;

        if (starsSlider != null)
        {
            starsSlider.value = 0f;
        }

        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(HandleBackButton);
        }

        timerIsRunning = true;
        UpdateTimerDisplay();
        ShowMessage($"order from largest to smallest");
    }

    public override void OnDrop(PointerEventData eventData)
    {
        base.OnDrop(eventData);

        if (slotText == null)
        {
            Debug.LogError("SlotText reference is missing!");
            return;
        }

        GhostButtonController.Instance.Hide();

        if (int.TryParse(symbol, out int droppedNumber))
        {
            if (droppedNumbers.Count < numbersRequired)
            {
                KeyboardButton keyButton = eventData.pointerDrag.GetComponent<KeyboardButton>();
                if (keyButton != null)
                {
                    usedButtons[droppedNumbers.Count] = keyButton;
                    keyButton.gameObject.SetActive(false);
                }

                droppedNumbers.Add(droppedNumber);
                UpdateSequenceDisplay();

                if (droppedNumbers.Count == numbersRequired)
                {
                    CheckSequence();
                }
            }
        }
    }

    private void HandleBackButton()
    {
        if (droppedNumbers.Count > 0)
        {
            int lastIndex = droppedNumbers.Count - 1;

            if (usedButtons.ContainsKey(lastIndex))
            {
                KeyboardButton keyButton = usedButtons[lastIndex];
                if (keyButton != null)
                {
                    keyButton.gameObject.SetActive(true);
                }
                usedButtons.Remove(lastIndex);
            }

            droppedNumbers.RemoveAt(lastIndex);
            UpdateSequenceDisplay();

            if (droppedNumbers.Count == 0)
            {
                ShowMessage($"Drop {numbersRequired} numbers to order them from largest to smallest");
            }
        }
    }

    private void UpdateSequenceDisplay()
    {
        Sequance = string.Join(" > ", droppedNumbers);
        if (MessageText != null)
        {
            MessageText.text = $"Current sequence:\n {Sequance}";
        }
    }

    private void CheckSequence()
    {
        bool isCorrect = true;
        for (int i = 0; i < droppedNumbers.Count - 1; i++)
        {
            if (droppedNumbers[i] <= droppedNumbers[i + 1])
            {
                isCorrect = false;
                break;
            }
        }

        if (isCorrect)
        {
            HandleCorrectAnswer();
        }
        else
        {
            HandleWrongAnswer();
        }

        round++;
        if (round >= maxRounds)
        {
            HandleVictory();
        }
        else
        {
            StartCoroutine(DelayedReset(2f));
        }
    }

    private void ResetForNextRound()
    {
        foreach (var button in usedButtons.Values)
        {
            if (button != null)
            {
                button.gameObject.SetActive(true);
            }
        }
        
        usedButtons.Clear();
        droppedNumbers.Clear();
        Sequance = "";
        if (slotText != null)
        {
            slotText.text = "";
        }
        ShowMessage($"Drop {numbersRequired} numbers to order them from largest to smallest");
        GenerateUniqueSymbols();
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
    }

    private void HandleWrongAnswer()
    {
        PlayParticleEffect(heartBreakParticle);
        ShowMessage("Wrong answer!");
    }

    private IEnumerator DelayedReset(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        ResetForNextRound();
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

    private void HandleTimeUp()
    {
        timerIsRunning = false;
        TimeCompleted = timeRemaining;
        if (MessageText != null)
        {
            MessageText.text = "Time's Up!";
        }

        // Save remaining time (0 since time is up)
        GameSessionData.completionTime = 0;
        
        GameManager.Instance.MiniGameComplete(score, correctAnswers, maxRounds - correctAnswers);
    }

     private void HandleVictory()
    {
        timerIsRunning = false;
        TimeCompleted = timeRemaining;
        
        int finalStars = CalculateStars();
        
        if (MessageText != null)
        {
            MessageText.text = $"Congratulations! You Won!\nYou earned {finalStars} stars!";
        }

        // Save remaining time in minutes
        GameSessionData.completionTime = timeRemaining / 60f;
        
        GameManager.Instance.MiniGameComplete(score, correctAnswers, maxRounds - correctAnswers);
    }

    private void ShowMessage(string feedback)
    {
        if (MessageText != null)
        {
            MessageText.text = $"Round {round + 1} of {maxRounds}\n{feedback}";
        }
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