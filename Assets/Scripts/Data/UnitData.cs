using UnityEngine;

[CreateAssetMenu(fileName = "NewUnitData", menuName = "Data/UnitData")]
public class UnitData : ScriptableObject
{
    public string unitName;
    public int level = 1;
    public int maxHealth;
    public int maxMana;
    public int attackPower;
    public int baseExpYield;
    public GameObject unitPrefab;

    [Header("Combat Actions")]
    public SkillData basicAttack;
    public SkillData[] availableSkills;
}