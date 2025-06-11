using Firebase.Database;
using UnityEngine.UI;
using Firebase.Extensions;
using System.Collections;
using UnityEngine;
using ZXing;
using System.Linq;
using DG.Tweening;

public class QRAuthenticator : MonoBehaviour
{
    public GameObject authenticationPanel;
    public Text statusText;
    public RawImage cameraFeed;
    public CanvasGroup transitionPanel;  // Add this UI element
    
    private DatabaseReference dbReference;
    private WebCamTexture backCameraTexture;
    private IBarcodeReader barcodeReader;
    private readonly Rect uvRectFlipped = new(1f, 0f, -1f, 1f);
    private readonly Rect uvRectNormal = new(0f, 0f, 1f, 1f);
    private float transitionDuration = 1f;

    void Start()
    {
        // Ensure transition panel starts invisible
        if (transitionPanel != null)
        {
            transitionPanel.alpha = 0f;
            transitionPanel.gameObject.SetActive(false);
        }

        FirebaseInitializer.Instance.InitializeFirebase(() =>
        {
            if (FirebaseInitializer.Instance.IsFirebaseInitialized)
            {
                dbReference = FirebaseInitializer.Instance.DbReference;
                statusText.text = "Checking camera permissions...";

                if (!Application.HasUserAuthorization(UserAuthorization.WebCam))
                {
                    StartCoroutine(RequestCameraPermission());
                }
                else
                {
                    SetupCamera();
                }
            }
        });
    }

    private IEnumerator RequestCameraPermission()
    {
        yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);

        if (Application.HasUserAuthorization(UserAuthorization.WebCam))
        {
            SetupCamera();
        }
        else
        {
            statusText.text = "Camera permission denied.";
        }
    }

    private void SetupCamera()
    {
        statusText.text = "Initializing camera...";

        if (WebCamTexture.devices.Length == 0)
        {
            statusText.text = "No cameras found on device.";
            return;
        }

        string frontCamName = WebCamTexture.devices.FirstOrDefault(device => device.isFrontFacing).name;

        if (string.IsNullOrEmpty(frontCamName))
        {
            statusText.text = "No front camera found.";
            return;
        }

        backCameraTexture = new WebCamTexture(frontCamName, 960, 960);
        backCameraTexture.requestedFPS = 30;
        backCameraTexture.filterMode = FilterMode.Bilinear;

        cameraFeed.texture = null;
        cameraFeed.texture = backCameraTexture;
        cameraFeed.material = null;
        cameraFeed.color = Color.white;

        backCameraTexture.Play();

        StartCoroutine(AdjustCameraOrientation());

        barcodeReader = new BarcodeReader
        {
            AutoRotate = false,
            Options = new ZXing.Common.DecodingOptions
            {
                TryHarder = true,
                TryInverted = true,
                PossibleFormats = new[] { BarcodeFormat.QR_CODE }
            }
        };

        StartCoroutine(ScanQRCode());
    }

    private IEnumerator AdjustCameraOrientation()
    {
        yield return new WaitUntil(() => backCameraTexture.width > 100);

        float angle = backCameraTexture.videoRotationAngle;
        if (!WebCamTexture.devices[0].isFrontFacing)
        {
            angle = -angle;
        }
        if (backCameraTexture.videoVerticallyMirrored)
        {
            angle += 180f;
        }

        cameraFeed.rectTransform.localEulerAngles = new Vector3(0, 0, angle);

        bool needsFlip = (backCameraTexture.videoVerticallyMirrored && !WebCamTexture.devices[0].isFrontFacing)
                      || (!backCameraTexture.videoVerticallyMirrored && WebCamTexture.devices[0].isFrontFacing);

        cameraFeed.uvRect = needsFlip ? uvRectFlipped : uvRectNormal;
    }

    private IEnumerator ScanQRCode()
    {
        while (true)
        {
            if (backCameraTexture.didUpdateThisFrame)
            {
                Color32[] colors = backCameraTexture.GetPixels32();
                var barcodeResult = barcodeReader.Decode(colors, backCameraTexture.width, backCameraTexture.height);

                if (barcodeResult != null)
                {
                    string uid = barcodeResult.Text;
                    if (!string.IsNullOrEmpty(uid))
                    {
                        AuthenticateUser(uid);
                        break;
                    }
                }
            }
            yield return null;
        }
    }

    private void AuthenticateUser(string rawData)
    {
        statusText.text = "Verifying QR Code...";

        try
        {
            var qrPayload = JsonUtility.FromJson<PlayerQRPayload>(rawData);
            if (qrPayload == null || string.IsNullOrEmpty(qrPayload.uid) || string.IsNullOrEmpty(qrPayload.pin))
            {
                statusText.text = "Invalid QR Code format.";
                return;
            }

            string uid = qrPayload.uid;
            string enteredPin = qrPayload.pin;

            var pinRef = dbReference.Child("users").Child(uid).Child("password");
            pinRef.GetValueAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted && task.Result.Exists)
                {
                    string storedPin = task.Result.Value.ToString();

                    if (enteredPin == storedPin)
                    {
                        statusText.text = "Authentication successful!";
                        LoadPlayerData(uid);
                    }
                    else
                    {
                        statusText.text = "Invalid PIN.";
                    }
                }
                else
                {
                    statusText.text = "Player not found.";
                }
            });
        }
        catch
        {
            statusText.text = "QR Code data unreadable.";
        }
    }

    private void LoadPlayerData(string uid)
    {
        statusText.text = $"UID: {uid}\nLoading student data...";

        DatabaseReference userRef = dbReference.Child("users").Child(uid);
        userRef.GetValueAsync().ContinueWithOnMainThread(async userTask =>
        {
            if (!userTask.IsCompleted || !userTask.Result.Exists)
            {
                statusText.text = "Could not load student data.";
                return;
            }

            string studentGrade = userTask.Result.Child("schoolGrade").Value.ToString();
            string studentName = $"{userTask.Result.Child("firstName").Value} {userTask.Result.Child("lastName").Value}";

            DatabaseReference testsRef = dbReference.Child("tests");
            testsRef.GetValueAsync().ContinueWithOnMainThread(testsTask =>
            {
                if (!testsTask.IsCompleted || !testsTask.Result.Exists)
                {
                    statusText.text = "Could not load tests.";
                    return;
                }

                foreach (var testSnapshot in testsTask.Result.Children)
                {
                    if (testSnapshot.Child("grade").Value?.ToString() == studentGrade
                        && testSnapshot.Child("status").Value?.ToString() == "PUBLISHED")
                    {
                        string testId = testSnapshot.Key;

                        GameSessionData.TestId = testId;
                        GameSessionData.StudentId = uid;
                        GameSessionData.StudentName = studentName;

                        statusText.text = "Authentication successful! Loading test scene...";
                        Debug.Log($"Test ID: {testId}, Student ID: {uid}, Student Name: {studentName}");

                        if (backCameraTexture != null && backCameraTexture.isPlaying)
                        {
                            backCameraTexture.Stop();
                            backCameraTexture = null;
                        }

                        StartCoroutine(TransitionToNewScene("GameManager"));
                        return;
                    }
                }

                statusText.text = $"No available test found for grade {studentGrade}";
            });
        });
    }

    private IEnumerator TransitionToNewScene(string sceneName)
{
    transitionPanel.alpha = 0f;
    transitionPanel.gameObject.SetActive(true);

    // Create a tween and wait for completion using coroutine instead of await
    yield return transitionPanel.DOFade(1f, transitionDuration)
        .SetEase(Ease.InOutQuad)
        .WaitForCompletion();
    
    UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
}

    private void OnDestroy()
    {
        if (backCameraTexture != null && backCameraTexture.isPlaying)
        {
            backCameraTexture.Stop();
        }
    }
}