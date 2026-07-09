// this code has reference to that script file code
using UnityEngine;
using System.Collections;

public class CombatManager : MonoBehaviour
{
    public UnitData playerUnitData;
    public UnitData enemyUnitData;
    private int currentPlayerHealth;
    private int currentEnemyHealth;
    private GameFlowManager activeFlowManager;

    public void InitializeCombatSequence(GameFlowManager flowManagerInstance)
    {
        activeFlowManager = flowManagerInstance;
        currentPlayerHealth = playerUnitData.maxHealth;
        currentEnemyHealth = enemyUnitData.maxHealth;
        StartCoroutine(ExecuteTurnBasedCombat());
    }

    private IEnumerator ExecuteTurnBasedCombat()
    {
        yield return new WaitForSeconds(1.5f);
        currentEnemyHealth -= playerUnitData.attackPower;
        yield return new WaitForSeconds(1.5f);

        if (currentEnemyHealth <= 0)
        {
            TerminateCombatSequence();
        }
    }

    private void TerminateCombatSequence()
    {
        activeFlowManager.ProcessBattleCompletion();
    }
}