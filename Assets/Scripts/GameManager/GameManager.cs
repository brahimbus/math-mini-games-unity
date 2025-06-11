using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Threading.Tasks;
using DG.Tweening;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Loading UI References")]
    public CanvasGroup loadingCanvasGroup;    // First canvas with slider
    public Slider loadingSlider;
    public CanvasGroup transitionCanvasGroup; // Second canvas with background
    public float fadeDuration = 0.5f;
    public float loadingSliderDuration = 1f;

    // Test session data
    private string currentTestId;
    private string currentStudentId;
    private string currentStudentName;
    private TestData testData;

    // Result tracking
    public int Score { get; private set; }
    public int completeTime;
    public int correctAnswers;
    public int wrongAnswers;

    // Stars awarded at the end of the test
    public int stars;

    // Current Game Settings
    public int maxNumberRange;
    public int maxNumbersInSequence;
    public int numQuestions;
    public int requiredCorrectAnswersMinimumPercent;
    public float testDuration;

    // Internal state
    private int currentMiniGameIndex = 0;
    private bool waitingForMiniGameComplete = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            DOTween.SetTweensCapacity(200, 125); // Initialize DOTween
            InitializeLoadingUI();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeLoadingUI()
    {
        // Validate UI references
        if (loadingCanvasGroup == null)
        {
            Debug.LogError("Loading Canvas Group is not assigned!");
            return;
        }

        if (loadingSlider == null)
        {
            Debug.LogError("Loading Slider is not assigned!");
            return;
        }

        if (transitionCanvasGroup == null)
        {
            Debug.LogError("Transition Canvas Group is not assigned!");
            return;
        }

        // Initialize canvas groups
        loadingCanvasGroup.alpha = 0f;
        transitionCanvasGroup.alpha = 0f;
        loadingSlider.value = 0f;

        // Make sure canvases are in the DontDestroyOnLoad scene
        DontDestroyOnLoad(loadingCanvasGroup.gameObject);
        DontDestroyOnLoad(transitionCanvasGroup.gameObject);
    }

    private bool ValidateLoadingUI()
    {
        bool isValid = true;

        if (loadingCanvasGroup == null)
        {
            Debug.LogError("Loading Canvas Group is missing!");
            isValid = false;
        }

        if (loadingSlider == null)
        {
            Debug.LogError("Loading Slider is missing!");
            isValid = false;
        }

        if (transitionCanvasGroup == null)
        {
            Debug.LogError("Transition Canvas Group is missing!");
            isValid = false;
        }

        return isValid;
    }

    public async void InitializeTest(string testId, string studentId, string studentName)
    {
        currentTestId = testId;
        currentStudentId = studentId;
        currentStudentName = studentName;

        try
        {
            testData = await FirebaseService.Instance.GetTestData(testId);
            testDuration = testData.testDuration;
            GameSessionData.completionTime = testDuration;
            StartTestFlow();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to initialize test: {e.Message}");
        }
    }

    private void StartTestFlow()
    {
        Score = 0;
        correctAnswers = 0;
        wrongAnswers = 0;
        completeTime = 0;
        currentMiniGameIndex = 0;
        
        Debug.Log($"Starting test: {testData.testName}");
        LoadNextMiniGame();
    }

    private async void LoadNextMiniGame()
    {
        if (currentMiniGameIndex < testData.miniGameOrder.Count)
        {
            string miniGameName = testData.miniGameOrder[currentMiniGameIndex];
            Debug.Log($"Loading mini-game: {miniGameName}");

            // Validate UI references before proceeding
            if (!ValidateLoadingUI())
            {
                Debug.LogError("Cannot proceed with loading - UI references missing!");
                return;
            }

            // Show transition background
            await FadeTransition(true);

            ApplyMiniGameConfig(miniGameName);
            
            // Show loading slider
            await ShowLoadingScreen();

            // Load the scene asynchronously
            var asyncOperation = SceneManager.LoadSceneAsync(miniGameName);
            asyncOperation.allowSceneActivation = false;

            while (asyncOperation.progress < 0.9f)
            {
                float progress = Mathf.Clamp01(asyncOperation.progress / 0.9f);
                try
                {
                    var tweener = loadingSlider.DOValue(progress, 0.1f);
                    await tweener.AsyncWaitForCompletion();
                }
                
                catch (System.Exception e)
                {
                    Debug.LogError($"Error updating loading slider: {e.Message}");
                }
                await Task.Delay(10);
            }

            // Final loading progress
            try
            {
                var finalTween = loadingSlider.DOValue(1f, 0.1f);
                await finalTween.AsyncWaitForCompletion();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error setting final loading progress: {e.Message}");
            }

            await Task.Delay(100);

            // Hide loading screen
            await HideLoadingScreen();

            asyncOperation.allowSceneActivation = true;
            SceneManager.sceneLoaded += OnSceneLoaded;
            waitingForMiniGameComplete = true;

            // Fade out transition after scene is loaded
            await FadeTransition(false);
        }
        else
        {
            CompleteTest();
        }
    }

    private async Task ShowLoadingScreen()
    {
        if (loadingCanvasGroup != null && loadingSlider != null)
        {
            loadingSlider.value = 0f;
            try
            {
                
                await loadingCanvasGroup.DOFade(1f, fadeDuration).AsyncWaitForCompletion();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error showing loading screen: {e.Message}");
            }
        }
        else
        {
            Debug.LogError("Loading UI references are missing!");
        }
    }

    private async Task HideLoadingScreen()
    {
        if (loadingCanvasGroup)
        {
            await loadingCanvasGroup.DOFade(0f, fadeDuration).AsyncWaitForCompletion();
        }
    }

    private async Task FadeTransition(bool fadeIn)
    {
        if (transitionCanvasGroup)
        {
            float targetAlpha = fadeIn ? 1f : 0f;
            await transitionCanvasGroup.DOFade(targetAlpha, fadeDuration).AsyncWaitForCompletion();
        }
    }

    private async void CompleteTest()
    {
        // Show transition before loading end scene
        await FadeTransition(true);

        float completionTime = testDuration - GameSessionData.completionTime;

        Debug.Log($"Test complete! Final Score: {Score}, Correct: {correctAnswers}, Wrong: {wrongAnswers}, Complete Time: {completionTime}");

        int totalQuestions = correctAnswers + wrongAnswers;
        stars = CalculateStars(correctAnswers, totalQuestions);

        TestResult result = new TestResult
        {
            score = Score.ToString(),
            studentId = currentStudentId,
            studentName = currentStudentName,
            teacherId = testData.teacherId,
            testId = currentTestId,
            testName = testData.testName,
            correctAnswers = correctAnswers,
            wrongAnswers = wrongAnswers,
            completionTime = completionTime
        };        try
        {
            await FirebaseService.Instance.SaveTestResult(result);
            Debug.Log("Test result saved successfully!");
            
            // Load the End Scene after saving results
            var asyncOperation = SceneManager.LoadSceneAsync("End Scene");
            asyncOperation.allowSceneActivation = false;

            // Wait until the scene is fully loaded
            while (asyncOperation.progress < 0.9f)
            {
                await Task.Delay(10);
            }

            // Activate the scene
            asyncOperation.allowSceneActivation = true;

            // Wait a moment for the scene to initialize
            await Task.Delay(500);
            
            // Fade out transition after end scene is loaded
            await FadeTransition(false);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to save test result: {e.Message}");
        }
    }

    private int CalculateStars(int correctAnswers, int totalQuestions)
    {
        if (totalQuestions == 0) return 0; // Prevent division by zero

        float percentage = (float)correctAnswers / totalQuestions * 100f;

        // Example thresholds — adjust as you like
        if (percentage >= 90f)
        {
            return 3; 
        }
        else if (percentage >= 75f)
        {
            return 2; 
        }
        else if (percentage >= 50f)
        {
            return 1; 
        }
        else
        {
            return 0; // No stars
        }
    }


    private void ApplyMiniGameConfig(string miniGameName)
    {
        if (testData.miniGameConfigs.TryGetValue(miniGameName, out MiniGameConfig config))
        {
            maxNumberRange = config.maxNumberRange;
            numQuestions = config.numQuestions;
            maxNumbersInSequence = config.maxNumbersInSequence;
            requiredCorrectAnswersMinimumPercent = config.requiredCorrectAnswersMinimumPercent;

            Debug.Log($"Applied config for {miniGameName}: " +
                     $"maxNumberRange={maxNumberRange}, " +
                     $"numQuestions={numQuestions}, " +
                     $"maxNumbersInSequence={maxNumbersInSequence}, " +
                     $"requiredCorrectAnswersMinimumPercent={requiredCorrectAnswersMinimumPercent}");
        }
        else
        {
            Debug.LogError($"Mini-game config not found for: {miniGameName}");
        }
    }

    public void MiniGameComplete(int scoreDelta, int correctDelta, int wrongDelta)
    {
        Score += scoreDelta;
        correctAnswers += correctDelta;
        wrongAnswers += wrongDelta;

        Debug.Log($"Mini-game complete. Score delta: {scoreDelta}, correct: {correctDelta}, wrong: {wrongDelta}");

        if (waitingForMiniGameComplete)
        {
            waitingForMiniGameComplete = false;
            StartCoroutine(ProceedToNextMiniGame());
        }
    }

    private IEnumerator ProceedToNextMiniGame()
    {
        yield return new WaitForSeconds(0.5f);
        currentMiniGameIndex++;
        LoadNextMiniGame();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        AudioListener[] listeners = FindObjectsOfType<AudioListener>();
        if (listeners.Length > 1)
        {
            Debug.LogWarning($"Found {listeners.Length} AudioListeners - disabling extras.");
            for (int i = 1; i < listeners.Length; i++)
            {
                listeners[i].enabled = false;
            }
        }

        UnityEngine.EventSystems.EventSystem[] eventSystems = 
            FindObjectsOfType<UnityEngine.EventSystems.EventSystem>();
        if (eventSystems.Length > 1)
        {
            Debug.LogWarning($"Found {eventSystems.Length} EventSystems - destroying extras.");
            for (int i = 1; i < eventSystems.Length; i++)
            {
                Destroy(eventSystems[i].gameObject);
            }
        }
    }
}