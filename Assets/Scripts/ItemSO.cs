using UnityEngine;


[CreateAssetMenu(fileName = "New Item", menuName = "ScriptableObjects/Item")]
public class ItemSO : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public string description;
    public ItemType type;
    public int buyingPrice;
    public int sellingPrice;
    public float weight;
    public Rarity rarity;
}


public enum ItemType { Materials, Weapons, Consumables, Treasure }
public enum Rarity { VeryCommon, Common, Rare, Epic, Legendary }

