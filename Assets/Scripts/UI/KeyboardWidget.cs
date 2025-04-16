using UnityEngine;
using UnityEngine.UI;

public class KeyboardWidget : MonoBehaviour
{
    public GridLayoutGroup keyboardGrid;
    public GameObject buttonPrefab;
    public string[] symbols;

    // Start is called before the first frame update
    void Start()
    {
        PopulateKeyboard();
    }

    // Populate the grid with buttons
    void PopulateKeyboard()
    {
        if (keyboardGrid == null || buttonPrefab == null) return;

        symbols = new string[] {
            "9", "8", "7",
            "6", "5", "4",
            "3", "2", "1",
            "0", ".", "÷",
            "×", "-", "+"
        };

        foreach (string symbol in symbols)
        {
            // Instantiate a new button
            GameObject newButton = Instantiate(buttonPrefab, keyboardGrid.transform);
            KeyboardButton keyboardButton = newButton.GetComponent<KeyboardButton>();
            if (keyboardButton != null)
            {
                keyboardButton.symbol = symbol; // Set the button symbol
            }
        }
    }
}
