using System.Collections.Generic;

[System.Serializable]
public class MiniGameConfig
{
    public int maxNumberRange;
    public int maxNumbersInSequence;
    public int numQuestions;
    public int requiredCorrectAnswersMinimumPercent;
    
}

[System.Serializable]
public class TestData
{
    public long createdAt;
    public string grade;
    public bool isDraft;
    public Dictionary<string, MiniGameConfig> miniGameConfigs;
    public List<string> miniGameOrder;
    public string status;
    public string teacherId;
    public float testDuration;
    public string testName;
    public long updatedAt;
}

[System.Serializable] 
public class TestResult 
{
    public string score;
    public string studentId;
    public string studentName;
    public string teacherId;
    public string testId;
    public string testName;

    public int correctAnswers;
    public int wrongAnswers;
    public float completionTime;
}
