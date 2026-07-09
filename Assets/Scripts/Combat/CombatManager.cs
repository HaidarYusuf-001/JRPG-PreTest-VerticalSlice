// this code has reference to that script file code
using UnityEngine;
using System.Collections;

public class CombatManager : MonoBehaviour
{
    public UnitData playerStats;
    public UnitData enemyStats;
    private int currentPlayerHealth;
    private int currentEnemyHealth;
    private GameFlowManager flowManager;

    public void StartCombat(GameFlowManager manager)
    {
        flowManager = manager;
        currentPlayerHealth = playerStats.maxHealth;
        currentEnemyHealth = enemyStats.maxHealth;
        StartCoroutine(CombatRoutine());
    }

    private IEnumerator CombatRoutine()
    {
        yield return new WaitForSeconds(1f);
        currentEnemyHealth -= playerStats.attackPower;
        yield return new WaitForSeconds(1f);

        if (currentEnemyHealth <= 0)
        {
            EndCombat();
        }
    }

    private void EndCombat()
    {
        flowManager.OnBattleEnded();
    }
}