using UnityEngine;

[CreateAssetMenu(fileName = "NewSkill", menuName = "Data/Skill")]
public class SkillData : ScriptableObject
{
    public string skillName;
    public int manaCost;

    [Header("Effect Settings")]
    public ActionTarget targetType;
    public EffectCategory effectCategory;
    public int effectValue; 

    [Header("Animation")]
    public CombatAction actionExecution;
}