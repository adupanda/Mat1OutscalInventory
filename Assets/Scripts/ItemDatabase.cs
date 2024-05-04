using UnityEngine;

[CreateAssetMenu(fileName = "New Item Database", menuName = "ScriptableObjects/Items/Item Database")]
public class ItemDatabase : ScriptableObject
{
    public ItemSO[] items;
}
