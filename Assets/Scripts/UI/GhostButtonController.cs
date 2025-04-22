using UnityEngine;
using UnityEngine.UI;

public class GhostButtonController : MonoBehaviour
{
    public static GhostButtonController Instance;

    public Text text;
    private RectTransform rect;
    private CanvasGroup canvasGroup;

    void Awake()
    {
        Instance = this;

        rect = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

        if (text == null)
            text = GetComponentInChildren<Text>();

        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        Hide();
    }

    public void Show(string symbol, Vector2 position)
    {
        text.text = symbol;
        rect.position = position;
        canvasGroup.alpha = 0.95f;
    }

    public void Move(Vector2 position)
    {
        rect.position = position;
    }

    public void Hide()
    {
        canvasGroup.alpha = 0f;
    }
}
