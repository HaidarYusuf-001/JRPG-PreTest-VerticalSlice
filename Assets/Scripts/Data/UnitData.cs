using UnityEngine;

[CreateAssetMenu(fileName = "NewUnitData", menuName = "Data/UnitData")]
public class UnitData : ScriptableObject
{
    public string unitName;
    public int maxHealth;
    public int attackPower;
    public GameObject unitPrefab;

    [Header("Combat Actions")]
    public CombatAction defaultAttack; // Bisa diisi MeleeAction atau JumpAction
}