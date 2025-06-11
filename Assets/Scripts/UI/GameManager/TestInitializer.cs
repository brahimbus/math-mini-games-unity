using UnityEngine;

public class TestInitializer : MonoBehaviour
{
    void Start()
    {
       
            
            GameManager.Instance.InitializeTest(
                testId: GameSessionData.TestId,
                studentId: GameSessionData.StudentId,
                studentName: GameSessionData.StudentName
            );
           

           GameSessionData.Clear(); 
        
        
    }
}
