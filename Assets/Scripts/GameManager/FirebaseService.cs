using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;

public class FirebaseService : MonoBehaviour
{
    private static FirebaseService instance;
    private DatabaseReference dbReference;
    private bool isInitialized = false;

    public static FirebaseService Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("FirebaseService");
                instance = go.AddComponent<FirebaseService>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeFirebase();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private async void InitializeFirebase()
    {
        var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
        if (dependencyStatus == DependencyStatus.Available)
        {
            FirebaseApp app = FirebaseApp.DefaultInstance;
            dbReference = FirebaseDatabase.DefaultInstance.RootReference;
            isInitialized = true;
            Debug.Log("Firebase initialized successfully");
        }
        else
        {
            Debug.LogError($"Could not resolve Firebase dependencies: {dependencyStatus}");
        }
    }

    private async Task EnsureInitialized()
    {
        int maxAttempts = 10;
        int attempts = 0;
        while (!isInitialized && attempts < maxAttempts)
        {
            await Task.Delay(500); // Wait 500ms between checks
            attempts++;
        }

        if (!isInitialized)
        {
            throw new System.Exception("Firebase failed to initialize");
        }
    }

    public async Task<TestData> GetTestData(string testId)
    {
        try
        {
            await EnsureInitialized();

            var snapshot = await dbReference
                .Child("tests")
                .Child(testId)
                .GetValueAsync();

            if (snapshot.Exists)
            {
                string json = snapshot.GetRawJsonValue();
                return JsonConvert.DeserializeObject<TestData>(json);
            }
            
            throw new System.Exception($"Test {testId} not found!");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error loading test data: {e.Message}");
            throw;
        }
    }

    public async Task SaveTestResult(TestResult result)
    {
        try
        {
            await EnsureInitialized();

            string resultKey = $"result_{System.DateTime.Now.Ticks}";
            await dbReference
                .Child("testResults")
                .Child(resultKey)
                .SetRawJsonValueAsync(JsonConvert.SerializeObject(result));
            
            Debug.Log($"Test result saved successfully with key: {resultKey}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error saving test result: {e.Message}");
            throw;
        }
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
}