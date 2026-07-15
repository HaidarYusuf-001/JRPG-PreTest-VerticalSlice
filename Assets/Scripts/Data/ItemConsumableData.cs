using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Data/Item")]
public class ItemConsumableData : ScriptableObject
{
    public string itemName;

    [Header("Effect Settings")]
    public ActionTarget targetType;
    public EffectCategory effectCategory;
    public int effectValue;

    [Header("Animation")]
    public CombatAction actionExecution;
}