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
    public void UpdateSymbols(string[] newSymbols)
    {
        symbols = newSymbols;
        ClearKeyboard();
        PopulateKeyboard();
    }
    private void ClearKeyboard()
    {
        // Destroy all existing buttons
        foreach (Transform child in keyboardGrid.transform)
        {
            Destroy(child.gameObject);
        }
    }
    // Populate the grid with buttons
    void PopulateKeyboard()
    {
        if (keyboardGrid == null || buttonPrefab == null) return;

        if (symbols == null || symbols.Length == 0)
        {
            Debug.LogWarning("No symbols provided for the keyboard.");
            return;
        }

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
