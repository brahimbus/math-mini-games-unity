using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class KeyboardButton : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Text buttonText;
    public string symbol;

    private GameObject draggedClone;
    private RectTransform draggedRect;
    private CanvasGroup draggedGroup;

    [SerializeField] private Canvas dragCanvas;

    void Start()
    {
        if (buttonText != null)
            buttonText.text = symbol;

        if (dragCanvas == null)
            dragCanvas = FindTopmostCanvas(); // Use correct canvas
        else if (dragCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
            Debug.LogWarning("DragCanvas is not in ScreenSpaceOverlay mode!", this);
        
    }

    private Canvas FindTopmostCanvas()
    {
        GameObject go = GameObject.Find("DragCanvas");
        return go != null ? go.GetComponent<Canvas>() : null;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        //draggedClone = Instantiate(gameObject);
        //draggedClone.transform.SetParent(dragCanvas.transform, false);
        //draggedClone.name = "Dragged_" + symbol;

        //// Clean original behavior
        //Destroy(draggedClone.GetComponent<KeyboardButton>());
        //var payload = draggedClone.AddComponent<KeyboardButtonDragPayload>();
        //payload.symbol = symbol;

        //// assign RectTransform
        //draggedRect = draggedClone.GetComponent<RectTransform>();

        //// Placement and scale
        //draggedRect.localScale = Vector3.one;
        //draggedRect.localPosition = Vector3.zero;
        //draggedRect.SetAsLastSibling();

        //// Visibility and interaction
        //draggedGroup = draggedClone.GetComponent<CanvasGroup>() ?? draggedClone.AddComponent<CanvasGroup>();
        //draggedGroup.blocksRaycasts = false;
        //draggedGroup.alpha = 0.95f;

        //// Force visuals ON
        //foreach (var image in draggedClone.GetComponentsInChildren<Image>(true))
        //    image.enabled = true;
        //foreach (var text in draggedClone.GetComponentsInChildren<Text>(true))
        //    text.enabled = true;

        //// TEMP COLOR FOR DEBUG
        //Image bg = draggedClone.GetComponent<Image>();
        //if (bg != null) bg.color = new Color(1f, 0.3f, 0.3f, 0.9f);

        //eventData.pointerDrag = draggedClone;

        GhostButtonController.Instance.Show(symbol, Mouse.current.position.ReadValue());
        eventData.pointerDrag = gameObject; // so DigitSlot can still receive the drop

    }


    public void OnDrag(PointerEventData eventData)
    {
        //if (draggedRect != null)
        //    draggedRect.position = Mouse.current.position.ReadValue();
        GhostButtonController.Instance.Move(Mouse.current.position.ReadValue());
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        //if (draggedClone != null)
        //    Destroy(draggedClone);

        GhostButtonController.Instance.Hide();
    }
}
