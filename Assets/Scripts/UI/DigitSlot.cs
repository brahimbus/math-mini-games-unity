using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DigitSlot : MonoBehaviour, IDropHandler
{
    public Text slotText;
    public string symbol;

    void Start()
    {
        if (slotText == null)
            Debug.LogWarning("DigitSlot: slotText is not assigned!", this);
        else
            slotText.text = "";
    }

    public virtual void OnDrop(PointerEventData eventData)
    {
        Debug.Log("Drop detected on: " + gameObject.name);

        symbol = GhostButtonController.Instance.CurrentSymbol;
        if (!string.IsNullOrEmpty(symbol))
        {
            Debug.Log("Symbol Set: " + symbol);
        }
    }

}