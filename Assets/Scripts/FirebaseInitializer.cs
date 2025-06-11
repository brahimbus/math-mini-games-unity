using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine;
using System;
using System.Threading.Tasks;

public class FirebaseInitializer : MonoBehaviour
{
    private static FirebaseInitializer _instance;
    public static FirebaseInitializer Instance => _instance;

    public DatabaseReference DbReference { get; private set; }
    public bool IsFirebaseInitialized { get; private set; }

    private Task<DependencyStatus> initTask;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        // Removed DontDestroyOnLoad to make it destroyable
    }

    public void InitializeFirebase(Action onInitialized)
    {
        if (IsFirebaseInitialized)
        {
            onInitialized?.Invoke();
            return;
        }

        if (initTask == null)
        {
            initTask = FirebaseApp.CheckAndFixDependenciesAsync();
        }

        initTask.ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                DbReference = FirebaseDatabase.DefaultInstance.RootReference;
                IsFirebaseInitialized = true;
                Debug.Log("Firebase initialized.");
                onInitialized?.Invoke();
            }
            else
            {
                Debug.LogError("Firebase dependencies not available: " + task.Result);
            }
        });
    }
}
