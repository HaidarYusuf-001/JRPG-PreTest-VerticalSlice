using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Data/Item")]
public class ItemConsumableData : ScriptableObject
{
    public string itemName;
    public int healAmount;
    public int manaRestoreAmount;
}