using UnityEngine;
using System.Collections;
using UnityEngine.Playables;
using System;

public class CombatManager : MonoBehaviour
{
    public event Action OnCombatCompleted;

    public BattleUIManager battleUIManager;
    public PlayableDirector battleEntryDirector;

    public Transform playerSpawnPoint;
    public Transform enemySpawnPoint;

    public GameObject vcamBattleMain;
    public GameObject vcamAction;

    public float attackDistanceOffset = 1.5f;

    private UnitData activePlayerUnitData;
    private UnitData activeEnemyUnitData;
    private int currentPlayerHealth;
    private int currentEnemyHealth;

    private GameObject activePlayerInstance;
    private GameObject activeEnemyInstance;
    private PlayerController spawnedPlayerController;
    private EnemyController spawnedEnemyController;

    public void InitializeDynamicCombatSequence(UnitData playerData, UnitData enemyData)
    {
        activePlayerUnitData = playerData;
        activeEnemyUnitData = enemyData;

        currentPlayerHealth = activePlayerUnitData.maxHealth;
        currentEnemyHealth = activeEnemyUnitData.maxHealth;

        SpawnCombatants();
        ConfigureCameras();

        battleUIManager.InitializeUI(this, currentPlayerHealth, currentEnemyHealth);
        battleEntryDirector.Play();
        Invoke("StartFirstTurn", (float)battleEntryDirector.duration);
    }

    private void SpawnCombatants()
    {
        activePlayerInstance = Instantiate(activePlayerUnitData.unitPrefab, playerSpawnPoint.position, playerSpawnPoint.rotation);
        spawnedPlayerController = activePlayerInstance.GetComponent<PlayerController>();
        if (spawnedPlayerController != null)
        {
            spawnedPlayerController.SetMovementState(false);
        }

        activeEnemyInstance = Instantiate(activeEnemyUnitData.unitPrefab, enemySpawnPoint.position, enemySpawnPoint.rotation);
        spawnedEnemyController = activeEnemyInstance.GetComponent<EnemyController>();
    }

    private void ConfigureCameras()
    {
        vcamBattleMain.SetActive(true);
        vcamAction.SetActive(false);
    }

    private void StartFirstTurn()
    {
        battleUIManager.EnableAttackButton();
    }

    public void ProcessPlayerTurn()
    {
        vcamBattleMain.SetActive(false);
        vcamAction.SetActive(true);

        spawnedPlayerController.ExecuteMeleeAttack(
            activeEnemyInstance.transform,
            attackDistanceOffset,
            8f,
            OnPlayerHitTarget,
            OnPlayerReturnToSpawn
        );
    }

    private void OnPlayerHitTarget()
    {
        currentEnemyHealth -= activePlayerUnitData.attackPower;
        battleUIManager.UpdateEnemyHealth(currentEnemyHealth);
    }

    private void OnPlayerReturnToSpawn()
    {
        vcamAction.SetActive(false);
        vcamBattleMain.SetActive(true);
        CheckCombatState(true);
    }

    private IEnumerator ProcessEnemyTurn()
    {
        yield return new WaitForSeconds(0.5f);

        vcamBattleMain.SetActive(false);
        vcamAction.SetActive(true);

        spawnedEnemyController.ExecuteMeleeAttack(
            activePlayerInstance.transform,
            attackDistanceOffset,
            OnEnemyHitTarget,
            OnEnemyReturnToSpawn
        );
    }

    private void OnEnemyHitTarget()
    {
        currentPlayerHealth -= activeEnemyUnitData.attackPower;
        battleUIManager.UpdatePlayerHealth(currentPlayerHealth);
    }

    private void OnEnemyReturnToSpawn()
    {
        vcamAction.SetActive(false);
        vcamBattleMain.SetActive(true);
        CheckCombatState(false);
    }

    private void CheckCombatState(bool wasPlayerTurn)
    {
        if (wasPlayerTurn)
        {
            if (currentEnemyHealth <= 0)
            {
                TerminateCombatSequence();
            }
            else
            {
                StartCoroutine(ProcessEnemyTurn());
            }
        }
        else
        {
            if (currentPlayerHealth <= 0)
            {
                TerminateCombatSequence();
            }
            else
            {
                battleUIManager.EnableAttackButton();
            }
        }
    }

    private void TerminateCombatSequence()
    {
        Destroy(activePlayerInstance);
        Destroy(activeEnemyInstance);
        OnCombatCompleted?.Invoke();
    }
}