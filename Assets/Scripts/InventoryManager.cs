using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

[System.Serializable]
public class InventoryItem
{
    public ItemSO itemSO;
    public int quantity;
    public GameObject thisObject;

    public InventoryItem(ItemSO itemSO, int quantity)
    {
        this.itemSO = itemSO;
        this.quantity = quantity;
    }
}

public class InventoryManager : MonoBehaviour
{
    public ItemDatabase itemDatabase;
    public int maxInventoryWeight = 100;
    public TextMeshProUGUI maxWeightText;
    public TextMeshProUGUI currentWeightText;
    public Transform inventoryGrid;
    public GameObject inventoryItemPrefab;
    public TMP_Text tooltipText;
    public Button buyButton;
    public Button sellButton;



    public ShopUI shopUI; // Reference to the ShopUI component

    private List<InventoryItem> inventoryItems = new List<InventoryItem>();
    private float currentInventoryWeight = 0;
    private InventoryItem selectedItem;

    public TMP_InputField sellQuantityInput;
    public GameObject sellPanel;
    public TMP_Text currencyText;
    public int currency = 0;

    private void Start()
    {
        UpdateWeightTexts();
    }

    public void OnGatherButtonClick()
    {
        if (currentInventoryWeight < maxInventoryWeight)
        {
            GatherItems();
            buyButton.gameObject.SetActive(false);
            sellButton.gameObject.SetActive(false);
        }
        else
        {
            Debug.Log("Inventory weight limit reached!");
        }
    }

    private void GatherItems()
    {
        int cumulativeValue = CalculateCumulativeValue();

        foreach (ItemSO itemSO in itemDatabase.items)
        {
            if (currentInventoryWeight < maxInventoryWeight && inventoryItems.Count < 25)
            {
                float probability = CalculateProbability(itemSO.rarity, cumulativeValue);
                int minQuantity = GetMinQuantity(itemSO.rarity);
                int maxQuantity = GetMaxQuantity(itemSO.rarity);
                int quantity = Random.Range(minQuantity, maxQuantity + 1);

                InventoryItem existingItem = inventoryItems.Find(i => i.itemSO == itemSO);
                if (existingItem != null)
                {
                    existingItem.quantity += quantity;
                }
                else
                {
                    InventoryItem newItem = new InventoryItem(itemSO, quantity);
                    inventoryItems.Add(newItem);
                    DisplayInventoryItem(newItem);
                }

                currentInventoryWeight += itemSO.weight * quantity;
            }
            else
            {
                Debug.Log("Inventory is full or weight limit reached!");
                break;
            }
        }

        UpdateWeightTexts();
        tooltipText.text = "";
        sellButton.gameObject.SetActive(true);
        buyButton.gameObject.SetActive(false);
    }

    // Method to remove the sold item from the inventory
    private void RemoveSoldItem(InventoryItem itemToRemove)
    {
        // Check if the quantity of the sold item becomes zero
        if (itemToRemove.quantity <= 0)
        {
            // Find the index of the item to remove
            int indexToRemove = inventoryItems.FindIndex(item => item.itemSO == itemToRemove.itemSO);

            // Remove the item if found
            if (indexToRemove != -1)
            {
                inventoryItems.RemoveAt(indexToRemove);
                Destroy(itemToRemove.thisObject); // Assuming you have a reference to the instantiated GameObject of the inventory item
            }
        }
    }



    private int CalculateCumulativeValue()
    {
        int cumulativeValue = 0;
        foreach (InventoryItem i in inventoryItems)
        {
            cumulativeValue += i.itemSO.sellingPrice;
        }
        return cumulativeValue;
    }

    private float CalculateProbability(Rarity rarity, int cumulativeValue)
    {
        float baseProbability = 0.5f;
        float rarityMultiplier = GetRarityMultiplier(rarity);
        float valueMultiplier = GetCumulativeValueMultiplier(cumulativeValue);
        return baseProbability * rarityMultiplier * valueMultiplier;
    }

    private float GetRarityMultiplier(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.VeryCommon:
                return 1.0f;
            case Rarity.Common:
                return 1.2f;
            case Rarity.Rare:
                return 1.5f;
            case Rarity.Epic:
                return 2.0f;
            case Rarity.Legendary:
                return 3.0f;
            default:
                return 1.0f;
        }
    }

    private float GetCumulativeValueMultiplier(int cumulativeValue)
    {
        if (cumulativeValue > 1000)
        {
            return 2.0f;
        }
        else if (cumulativeValue > 500)
        {
            return 1.5f;
        }
        else
        {
            return 1.0f;
        }
    }

    private int GetMinQuantity(Rarity rarity)
    {
        return 1;
    }

    private int GetMaxQuantity(Rarity rarity)
    {
        return 10;
    }

    private void ClearInventory()
    {
        foreach (Transform child in inventoryGrid)
        {
            Destroy(child.gameObject);
        }
        inventoryItems.Clear();
        currentInventoryWeight = 0;
    }

    // Method to display an inventory item in the grid
    private void DisplayInventoryItem(InventoryItem inventoryItem)
    {
        if (inventoryItem.quantity <= 0)
        {
            if(inventoryItem.thisObject != null)
            {
                Destroy(inventoryItem.thisObject.gameObject);
            }
            
            return;
        }
        // Don't display items with quantity <= 0



            GameObject inventoryItemGO = Instantiate(inventoryItemPrefab, inventoryGrid);
        inventoryItem.thisObject = inventoryItemGO;
        Button itemButton = inventoryItemGO.GetComponent<Button>();
        Image itemImage = inventoryItemGO.GetComponent<Image>();

        itemImage.sprite = inventoryItem.itemSO.icon;

        itemButton.onClick.AddListener(() => OnInventoryItemClick(inventoryItem));
    }


    private void DisplayItemDetails(InventoryItem inventoryItem)
    {
        tooltipText.text = $"Name: {inventoryItem.itemSO.itemName}\nDescription: {inventoryItem.itemSO.description}\nPrice: {inventoryItem.itemSO.buyingPrice}\nWeight: {inventoryItem.itemSO.weight}\nQuantity: {inventoryItem.quantity}";
    }

    public void OnInventoryItemClick(InventoryItem inventoryItem)
    {
        DisplayItemDetails(inventoryItem);
        sellButton.gameObject.SetActive(true);
        buyButton.gameObject.SetActive(false);
        selectedItem = inventoryItem;
    }

    private void UpdateWeightTexts()
    {
        maxWeightText.text = $"Max Weight: {maxInventoryWeight}";
        currentWeightText.text = $"Current Weight: {currentInventoryWeight}";
    }

    public void OnSellButtonClick()
    {
        sellPanel.gameObject.SetActive(true);
    }

    public void OnConfirmSellButtonClick()
    {
        int quantityToSell;
        if (!int.TryParse(sellQuantityInput.text, out quantityToSell))
        {
            Debug.Log("Invalid quantity to sell!");
            sellPanel.SetActive(false);
            return;
        }

        if (quantityToSell < 1 || quantityToSell > selectedItem.quantity)
        {
            Debug.Log("Quantity to sell not in range!");
            sellPanel.SetActive(false);
            return;
        }

        int valueSold = quantityToSell * selectedItem.itemSO.sellingPrice;
        currency += valueSold;
        UpdateCurrencyText();

        selectedItem.quantity -= quantityToSell;

        currentInventoryWeight -= selectedItem.itemSO.weight * quantityToSell;
        UpdateWeightTexts();

        shopUI.UpdateShopQuantity(selectedItem.itemSO, quantityToSell); // Update shop quantity

        if (selectedItem.quantity <= 0)
        {
            RemoveSoldItem(selectedItem);
        }

        // Update the tooltip to reflect the quantity change
        DisplayItemDetails(selectedItem);

        sellPanel.gameObject.SetActive(false);
    }


    public void UpdateCurrencyText()
    {
        currencyText.text = $"Currency: {currency}";
    }


    public void UpdateInventoryQuantity(ItemSO itemSO, int quantityChange)
    {
        InventoryItem existingItem = inventoryItems.Find(i => i.itemSO == itemSO);
        if (existingItem != null)
        {
            existingItem.quantity += quantityChange;
        }
        else
        {
            // If the item doesn't exist in the inventory, add it
            InventoryItem newItem = new InventoryItem(itemSO, quantityChange);
            inventoryItems.Add(newItem);
            DisplayInventoryItem(newItem);
        }

        // Update the weight and other UI elements as needed
        UpdateWeightTexts();
    }
}
