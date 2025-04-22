using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DigitSlot : MonoBehaviour, IDropHandler
{
    public Text slotText;

    void Start()
    {
        if (slotText == null)
            Debug.LogWarning("DigitSlot: slotText is not assigned!", this);
        else
            slotText.text = "";
    }

    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log("Drop detected on: " + gameObject.name);

        if (eventData.pointerDrag != null)
        {
            var payload = eventData.pointerDrag.GetComponent<KeyboardButtonDragPayload>();
            if (payload != null)
            {
                slotText.text = payload.symbol;
                Debug.Log("Symbol Set: " + payload.symbol);
            }

            Destroy(eventData.pointerDrag);
        }
    }
}
