using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using static UnityEditor.Progress;

public class ShopUI : MonoBehaviour
{
    public GameObject itemButtonPrefab; // Prefab for the item button
    public Transform contentPanel; // The panel where items will be placed
    public TMP_Text tooltipText; // TextMeshProUGUI element for displaying item details
    public Button buyButton; // Reference to the buy button
    public Button sellButton; // Reference to the sell button
    public ItemSO[] itemSOs; // Scriptable objects for items
    public InventoryManager inventoryManager; // Reference to the InventoryManager component

    private Dictionary<ItemSO, int> shopItems = new Dictionary<ItemSO, int>(); // Dictionary to store shop items and their quantities
    private Dictionary<ItemSO, int> initialShopItems = new Dictionary<ItemSO, int>(); // Dictionary to store initial shop items and their quantities
    private ItemSO selectedItemSO; // Currently selected item

    public TMP_InputField buyQuantityInput;
    public GameObject buyPanel;

    private void Start()
    {
        PopulateInitialShopItems(); // Populate initial shop items
        PopulateShop(ItemType.Materials); // Populate the shop with initial items
    }

    // Method to populate initial shop items
    private void PopulateInitialShopItems()
    {
        foreach (ItemSO itemSO in itemSOs)
        {
            initialShopItems.Add(itemSO, Random.Range(0, 11));
            
        }
    }

   

    // Method to clear existing items in the shop
    private void ClearShop()
    {
        foreach (Transform child in contentPanel)
        {
            Destroy(child.gameObject);
        }
        shopItems.Clear();
    }

    private void OnItemClick(ItemSO itemSO)
    {
        // Show item details in the tooltip including quantity
        int quantity = shopItems[itemSO];
        tooltipText.text = $"Name: {itemSO.itemName}\nDescription: {itemSO.description}\nPrice: {itemSO.buyingPrice}\nWeight: {itemSO.weight}\nQuantity: {quantity}";

        // Show buy button for shop items
        buyButton.gameObject.SetActive(true);
        // Hide sell button for shop items
        sellButton.gameObject.SetActive(false);

        // Store the selected item
        selectedItemSO = itemSO;
    }

    // Method to be called when clicking on a tab
    public void OnTabClick(int itemTypeIndex)
    {
        ItemType selectedType = (ItemType)itemTypeIndex; // Convert tab index to ItemType enum value
        PopulateShop(selectedType); // Populate the shop with items of the selected type
    }

    // Method to populate the shop with items of a specific type
    private void PopulateShop(ItemType itemType)
    {
        ClearShop(); // Clear existing items in the shop

        foreach (var initialItem in initialShopItems)
        {
            ItemSO itemSO = initialItem.Key;
            int initialQuantity = initialItem.Value;
            if (itemSO.type == itemType)
            {
                Debug.Log($"Adding item of type {itemType}: {itemSO.itemName}");

                // Add item to shop with initial quantity
                shopItems.Add(itemSO, initialQuantity);

                // Instantiate an item button prefab for each item
                GameObject itemButtonGO = Instantiate(itemButtonPrefab, contentPanel);
                Button itemButton = itemButtonGO.GetComponent<Button>();
                Image itemImage = itemButtonGO.GetComponent<Image>();

                // Set the item icon on the button
                itemImage.sprite = itemSO.icon;

                // Add click event to the button
                itemButton.onClick.AddListener(() => OnItemClick(itemSO));
            }
            else
            {
                Debug.Log($"Skipping item of type {itemSO.type}: {itemSO.itemName}");
            }
        }
    }




    // Method to handle the buy button click
    public void OnBuyButtonClick()
    {
        buyPanel.SetActive(true);
    }

    // Method to update the quantity of items in the shop
    public void UpdateShopQuantity(ItemSO itemSO, int quantityChange)
    {
        if (initialShopItems.ContainsKey(itemSO))
        {
            initialShopItems[itemSO] += quantityChange;

            // Refresh the shop UI to update the displayed quantities
            PopulateShop((ItemType)itemSO.type);

            // If the selected item matches the updated item, update the tooltip
            if (selectedItemSO == itemSO)
            {
                int updatedQuantity = initialShopItems[itemSO];
                tooltipText.text = $"Name: {itemSO.itemName}\nDescription: {itemSO.description}\nPrice: {itemSO.buyingPrice}\nWeight: {itemSO.weight}\nQuantity: {updatedQuantity}";
            }
        }
        else
        {
            Debug.LogWarning("Item not found in shop: " + itemSO.itemName);
        }

        
    }

    // Method to handle the confirm button click in the buy panel
    public void OnConfirmBuyButtonClick()
    {
        
        // Get the quantity to buy from the input field
        int quantityToBuy = int.Parse(buyQuantityInput.text);

        // Check if the quantity to buy is valid
        if (quantityToBuy <= 0)
        {
            Debug.LogWarning("Invalid quantity to buy.");
            buyPanel.SetActive(false);
            return;
        }

        // Check if the selected item is valid
        if (selectedItemSO == null)
        {
            Debug.LogWarning("No item selected.");
            buyPanel.SetActive(false);
            return;
        }

        // Check if the shop has enough quantity of the item
        if (!shopItems.ContainsKey(selectedItemSO) || shopItems[selectedItemSO] < quantityToBuy)
        {
            Debug.LogWarning("Not enough quantity available in the shop.");
            buyPanel.SetActive(false);
            return;
        }

        if (quantityToBuy * selectedItemSO.buyingPrice > inventoryManager.currency)
        {
            Debug.LogWarning("Not enough money");
            buyPanel.SetActive(false);
            return;
        }

        inventoryManager.currency -= quantityToBuy * selectedItemSO.buyingPrice;
        inventoryManager.UpdateCurrencyText();
        // Reduce the quantity of the item in the shop
        UpdateShopQuantity(selectedItemSO, -quantityToBuy);

        // Increase the quantity of the item in the inventory
        // Call the method in InventoryManager to update the inventory quantities
        inventoryManager.UpdateInventoryQuantity(selectedItemSO, quantityToBuy);
        // Implement this according to your inventory management system

        // Close the buy panel
        buyPanel.SetActive(false);
    }
}
