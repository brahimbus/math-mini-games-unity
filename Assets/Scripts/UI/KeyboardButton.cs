using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class KeyboardButton : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    public Text buttonText;
    public string symbol;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;

    // Start is called before the first frame update
    void Start()
    {
        if (buttonText != null)
        {
            buttonText.text = symbol; // Set the button's text to its symbol
        }

        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    // Handle when dragging starts
    public void OnBeginDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 0.6f; // Make the button slightly transparent when dragging
        canvasGroup.blocksRaycasts = false; // Disable raycasting so the object doesn't block other UI elements
    }

    // Handle dragging
    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta; // Move the object with the mouse
    }

    // Handle when dragging ends
    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f; // Reset transparency when dragging ends
        canvasGroup.blocksRaycasts = true; // Re-enable raycasting
    }
}
