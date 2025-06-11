using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class KeyboardButton : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Text buttonText;
    public string symbol;

    [SerializeField] private Canvas dragCanvas;

    void Start()
    {
        if (buttonText != null)
            buttonText.text = symbol;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        GhostButtonController.Instance.Show(symbol, eventData.position);
    }

    public void OnDrag(PointerEventData eventData)
    {
        GhostButtonController.Instance.Move(eventData.position);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        
           
            
            // Check if the pointer is over a valid drop target
        GameObject dropTarget = eventData.pointerEnter;
        bool isValidTarget = dropTarget != null && dropTarget.GetComponent<IDropHandler>() != null;

        if (isValidTarget)
        {
            GhostButtonController.Instance.Hide();
            
            
        }
        else
        {
            
            GhostButtonController.Instance.Hide();
        }
            

        
        
    }
}
