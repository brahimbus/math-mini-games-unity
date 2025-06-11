using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine;

public class FirebasePlayerDataManager : MonoBehaviour
{
    private DatabaseReference playerRef;

    private void Start()
    {
    }

    private void InitializeDatabaseReference(string firebasePlayerId)
    {
        playerRef = FirebaseDatabase.DefaultInstance.GetReference("users").Child(firebasePlayerId);
        Debug.Log($"Initialized player reference for: {firebasePlayerId}");
    }

    public void SavePlayerData(string firebasePlayerId, PlayerProfile profile)
    {
        if (FirebaseInitializer.Instance.IsFirebaseInitialized)
        {
            InitializeDatabaseReference(firebasePlayerId);

            string json = JsonUtility.ToJson(profile);
            playerRef.Child("playerProfile").SetRawJsonValueAsync(json).ContinueWithOnMainThread(task =>
            {
                if (task.IsCompletedSuccessfully)
                {
                    Debug.Log("Player data saved successfully!");
                }
                else
                {
                    Debug.LogError("Failed to save player data: " + task.Exception);
                }
            });
        }
        else
        {
            Debug.LogError("Firebase not initialized yet.");
        }
    }

    public void LoadTestForPlayerGrade(string playerGrade, System.Action<DataSnapshot> onTestFound)
    {
        DatabaseReference testsRef = FirebaseDatabase.DefaultInstance.GetReference("tests");

        testsRef.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully && task.Result.Exists)
            {
                foreach (var childSnapshot in task.Result.Children)
                {
                    if (childSnapshot.Child("grade").Value != null && childSnapshot.Child("grade").Value.ToString() == playerGrade)
                    {
                        Debug.Log($"Found test matching grade {playerGrade}: {childSnapshot.Key}");
                        onTestFound?.Invoke(childSnapshot);
                        return; // Stop after finding the first match
                    }
                }

                Debug.Log($"No test found for grade {playerGrade}.");
                onTestFound?.Invoke(null);
            }
            else
            {
                Debug.LogError("Failed to load tests: " + task.Exception);
                onTestFound?.Invoke(null);
            }
        });
    }

    public void LoadPlayerData(string firebasePlayerId, System.Action<PlayerProfile> onComplete)
    {
        if (FirebaseInitializer.Instance.IsFirebaseInitialized)
        {
            InitializeDatabaseReference(firebasePlayerId);

            playerRef.GetValueAsync().ContinueWithOnMainThread(task => {
                if (task.IsCompletedSuccessfully && task.Result.Exists)
                {
                    string json = task.Result.GetRawJsonValue();
                    UserData user = JsonUtility.FromJson<UserData>(json);
                    Debug.Log("Player data loaded.");
                    onComplete?.Invoke(user.playerProfile);
                }
                else
                {
                    Debug.LogError("Failed to load player data: " + task.Exception);
                    onComplete?.Invoke(null);
                }
            });
        }
        else
        {
            Debug.LogError("Firebase not initialized yet.");
        }
    }
/*
    // NEW METHOD: Load player data AND test data
    public void LoadPlayerDataAndTest(string firebasePlayerId, System.Action<PlayerProfile, TestData> onComplete)
    {
        LoadPlayerData(firebasePlayerId, playerProfile =>
        {
            if (playerProfile != null)
            {
                // Now load test data for this player's grade
                LoadTestForPlayerGrade(playerProfile.schoolGrade, testSnapshot =>
                {
                    TestData testData = null;

                    if (testSnapshot != null)
                    {
                        // Parse snapshot to TestData
                        testData = JsonUtility.FromJson<TestData>(testSnapshot.GetRawJsonValue());
                        Debug.Log("Test data loaded.");
                    }
                    else
                    {
                        Debug.LogWarning("No test data found for this grade.");
                    }

                    // Finally call the combined callback
                    onComplete?.Invoke(playerProfile, testData);
                });
            }
            else
            {
                // If playerProfile is null, return nulls
                onComplete?.Invoke(null, null);
            }
        });
    }*/
}
