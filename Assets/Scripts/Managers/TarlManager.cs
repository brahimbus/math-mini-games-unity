using System.Collections.Generic;
using UnityEngine;

public class TarlManager : MonoBehaviour
{
   /* public int FinalGradeLevel;
    public Dictionary<GradeLevel, MiniGameList> MiniGames = new Dictionary<GradeLevel, MiniGameList>();

    public int CurrentMiniGameIndex = -1;
    public GameObject CurrentMiniGame;

    public static TarlManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); 
    }

    void Start()
    {
        Initialize();
    }

    void Initialize()
    {
        FinalGradeLevel = 2;
        Debug.Log("Profile successfully initialized. " + FinalGradeLevel);

        InitializeMiniGames();
    }

    void InitializeMiniGames()
    {
        // Grade 1 Mini Games
        MiniGameCustomization gradeOneCustomization = new MiniGameCustomization
        {
            numberOfQuestions = 4,
            numberRange = 10,
            numberOfOptions = 3,
            duration = 30.0f,
            timeLimit = 10.0f
        };

        MiniGameSuccessCriteria gradeOneSuccessCriteria = new MiniGameSuccessCriteria
        { 
            requiredCorrectAnswers = 3,
            completed = false 
        };

        MiniGameList gradeOneMiniGames = new MiniGameList
        {
            miniGamesWidgets = new List<MathoriaMiniGameWidget>
            {
                MiniGameFactory.Get().CreateMiniGame(MiniGameName.WhatNumberDoYouHear, gradeOneCustomization, gradeOneSuccessCriteria)
            }
        };

        MiniGames[GradeLevel.One] = gradeOneMiniGames;

        // Grade 2 Mini Games
        MiniGameCustomization gradeTwoCustomization = new MiniGameCustomization
        {
            numberOfQuestions = 7,
            numberRange = 100,
            numberOfOptions = 4,
            duration = 45.0f,
            timeLimit = 15.0f
        };

        MiniGameSuccessCriteria gradeTwoSuccessCriteria = new MiniGameSuccessCriteria
        {
            requiredCorrectAnswers = 5,
            completed = false
        };

        MiniGameList gradeTwoMiniGames = new MiniGameList
        {
            miniGamesWidgets = new List<MathoriaMiniGameWidget>
            {
                MiniGameFactory.Get().CreateMiniGame(MiniGameName.WhatNumberDoYouHear, gradeTwoCustomization, gradeTwoSuccessCriteria)
            }
        };

        MiniGames[GradeLevel.Two] = gradeTwoMiniGames;

        // Grade 3 Mini Games
        MiniGameCustomization gradeThreeCustomization = new MiniGameCustomization
        {
            numberOfQuestions = 10,
            numberRange = 1000,
            numberOfOptions = 5,
            duration = 60.0f,
            timeLimit = 20.0f
        };

        MiniGameSuccessCriteria gradeThreeSuccessCriteria = new MiniGameSuccessCriteria
        {
            requiredCorrectAnswers = 7,
            completed = false
        };

        MiniGameList gradeThreeMiniGames = new MiniGameList
        {
            miniGamesWidgets = new List<MathoriaMiniGameWidget>
            {
                MiniGameFactory.Get().CreateMiniGame(MiniGameName.WhatNumberDoYouHear, gradeThreeCustomization, gradeThreeSuccessCriteria)
            }
        };

        MiniGames[GradeLevel.Three] = gradeThreeMiniGames;
    }
}


    public void StartTest()
    {
        if (Profile == null)
        {
            Debug.LogError("StartTest() failed: Profile is null!");
            return;
        }

        // Test starting logic here, use FinalGradeLevel to decide mini-game progression
        // Example: StartMiniGame();
    }

    public bool CanStartTest()
    {
        return Profile != null && Profile.playerProfile != null;
    }

    public bool EvaluateMiniGameSuccess(GameObject miniGame)
    {
        // Logic to evaluate if the mini-game was successful
        // Based on the current mini-game and its logic
        return true; // Placeholder return
    }

    public void NotifyMiniGameCompleted()
    {
        // Logic to notify that the mini-game was completed, then proceed to the next
        StartNextMiniGame();
    }

    public void SetCurrentMiniGame(GameObject currentGame)
    {
        CurrentMiniGame = currentGame;
    }

    public void SetPlayerMathLevel()
    {
        // Logic to set the player's math level based on performance, for example
    }

    public GameObject StartMiniGame(GameObject miniGame)
    {
        // Logic to start a mini-game
        return miniGame; // Placeholder return
    }

    public GameObject StartNextMiniGame()
    {
        // Logic to start the next mini-game based on current index or progression
        return null; // Placeholder return
    }*/
}