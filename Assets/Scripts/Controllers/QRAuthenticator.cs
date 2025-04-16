using Firebase.Auth;
using Firebase.Database;
using UnityEngine.UI;
using Firebase.Extensions;
using System.Collections;
using UnityEngine;
using ZXing;
using Firebase;
using System.Linq;

public class QRAuthenticator : MonoBehaviour
{
    public GameObject authenticationPanel;
    public Text statusText;
    public RawImage cameraFeed;  // UI element to display the camera feed

    private DatabaseReference dbReference;
    private FirebaseAuth auth;

    private WebCamTexture backCameraTexture;
    private IBarcodeReader barcodeReader;

    void Start()
    {
        FirebaseInitializer.Instance.InitializeFirebase(() =>
        {
            if (FirebaseInitializer.Instance.IsFirebaseInitialized)
            {
                dbReference = FirebaseInitializer.Instance.DbReference;
                auth = FirebaseInitializer.Instance.Auth;
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

        string backCamName = WebCamTexture.devices.FirstOrDefault(device => !device.isFrontFacing).name;

        if (string.IsNullOrEmpty(backCamName))
        {
            statusText.text = "No back camera found.";
            return;
        }

        if (WebCamTexture.devices.Length == 0)
        {
            statusText.text = "No cameras found on device.";
            return;
        }

        backCameraTexture = new WebCamTexture(backCamName, 960, 960);
        
        backCameraTexture.requestedFPS = 30;
        backCameraTexture.filterMode = FilterMode.Bilinear;

        cameraFeed.texture = null; // Clear first
        cameraFeed.texture = backCameraTexture;
        cameraFeed.material = null;
        cameraFeed.color = Color.white;

        backCameraTexture.Play();

        // Wait until the camera starts updating
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

        Debug.Log("Camera playing: " + backCameraTexture.isPlaying);
        Debug.Log("Camera frame size: " + backCameraTexture.width + "x" + backCameraTexture.height);


        StartCoroutine(ScanQRCode());
    }

    private IEnumerator AdjustCameraOrientation()
    {
        yield return new WaitUntil(() => backCameraTexture.width > 16); // Wait for valid dimensions

        bool isVerticallyFlipped = backCameraTexture.videoVerticallyMirrored;
        int rotationAngle = backCameraTexture.videoRotationAngle;

        Debug.Log($"Camera flip: {isVerticallyFlipped}, Rotation: {rotationAngle}");

        cameraFeed.rectTransform.localEulerAngles = new Vector3(0, 0, -rotationAngle);
        cameraFeed.uvRect = isVerticallyFlipped
            ? new Rect(0, 1, 1, -1)
            : new Rect(0, 0, 1, 1);
    }


    private IEnumerator ScanQRCode()
    {
        while (true)
        {
            if (backCameraTexture.didUpdateThisFrame)
            {
                // Convert the camera frame to a color array
                Color32[] colors = backCameraTexture.GetPixels32();
                var barcodeResult = barcodeReader.Decode(colors, backCameraTexture.width, backCameraTexture.height);

                if (barcodeResult != null)
                {
                    string uid = ExtractUIDFromURL(barcodeResult.Text);
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

    private string ExtractUIDFromURL(string url)
    {
        // Assuming the URL contains "uid=<uid>"
        if (url.Contains("uid="))
        {
            return url.Substring(url.IndexOf("uid=") + 4);
        }
        return "";
    }

    private void AuthenticateUser(string uid)
    {
        if (string.IsNullOrEmpty(uid))
        {
            statusText.text = "Authentication failed: Uid is empty.";
            return;
        }

        statusText.text = "Checking user in database...";

        DatabaseReference emailRef = dbReference.Child("players").Child(uid).Child("email");

        emailRef.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && task.Result.Exists)
            {
                string email = task.Result.Value.ToString();
                statusText.text = $"Email found: {email}\nSigning in...";

                SignInWithEmail(email, "password");
            }
            else
            {
                statusText.text = "User not found in database.";
            }
        });
    }

    private void SignInWithEmail(string email, string password)
    {
        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                statusText.text = "Failed to sign in.";
                Debug.LogError("Authentication error: " + task.Exception);
                return;
            }

            FirebaseUser user = task.Result.User;
            statusText.text = $"Signed in as: {user.UserId}";

            LoadPlayerData(user.UserId);
        });
    }

    private void LoadPlayerData(string uid)
    {
        statusText.text = "Welcome back! Loading your profile...";

        DatabaseReference playerRef = dbReference.Child("players").Child(uid);
        playerRef.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && task.Result.Exists)
            {
                var playerData = task.Result;
                Debug.Log("Player data loaded.");

                // Proceed to the main game scene
                UnityEngine.SceneManagement.SceneManager.LoadScene("TestScene");
            }
            else
            {
                statusText.text = "Failed to load player data.";
            }
        });
    }
}
