using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DigitSlot : MonoBehaviour, IDropHandler
{
    public Text slotText;

    // Start is called before the first frame update
    void Start()
    {
        if (slotText != null)
        {
            slotText.text = "";  // Clear the slot text initially
        }
    }

    // Handle drop event when a draggable item is dropped onto this slot
    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log("Drop Event Triggered");

        // Check if the dropped object is a keyboard button
        if (eventData.pointerDrag != null)
        {
            KeyboardButton draggedButton = eventData.pointerDrag.GetComponent<KeyboardButton>();
            if (draggedButton != null)
            {
                Debug.Log("Dropped Symbol: " + draggedButton.symbol);

                // Set the dropped symbol as the text
                if (slotText != null)
                {
                    slotText.text = draggedButton.symbol;
                }
            }
        }
    }
}
