using UnityEngine;

[CreateAssetMenu(fileName = "NewSkill", menuName = "Data/Skill")]
public class SkillData : ScriptableObject
{
    public string skillName;
    public int manaCost;
    public int power;
    public bool isHealing;
    public CombatAction actionExecution;
}