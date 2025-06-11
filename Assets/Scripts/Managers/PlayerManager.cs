using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance;

    public PlayerProfile playerProfile;
    //public TestData currentTest; // Add this line to store the test

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public PlayerProfile GetPlayerState()
    {
        return playerProfile;
    }
/*
    public TestData GetCurrentTest()
    {
        return currentTest;
    }

    public void SetCurrentTest(TestData test)
    {
        currentTest = test;
    }*/
}
