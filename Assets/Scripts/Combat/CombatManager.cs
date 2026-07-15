using UnityEngine;
using System.Collections;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using System;

public class CombatManager : MonoBehaviour
{
    public BattleUIManager battleUIManager;
    public PlayableDirector battleEntryDirector;

    public Transform playerSpawnPoint;
    public Transform enemySpawnPoint;

    public GameObject vcamBattleMain;
    public GameObject vcamAction;

    private UnitData activePlayerUnitData;
    private UnitData activeEnemyUnitData;
    private int currentPlayerHealth;
    private int currentEnemyHealth;

    private GameObject activePlayerInstance;
    private GameObject activeEnemyInstance;

    private BattleUnit playerBattleUnit;
    private BattleUnit enemyBattleUnit;

    private void Start()
    {
        if (SessionManager.Instance != null)
        {
            InitializeDynamicCombatSequence(SessionManager.Instance.playerUnitData, SessionManager.Instance.pendingEnemyData);
        }
        else
        {
            Debug.LogError("SessionManager not found! Combat requires persistent data.");
        }
    }

    private void InitializeDynamicCombatSequence(UnitData playerData, UnitData enemyData)
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

        PlayerController cloneController = activePlayerInstance.GetComponent<PlayerController>();
        if (cloneController != null)
        {
            Destroy(cloneController);
        }

        playerBattleUnit = activePlayerInstance.GetComponent<BattleUnit>();
        if (playerBattleUnit != null)
        {
            playerBattleUnit.SetupForCombat();
        }

        activeEnemyInstance = Instantiate(activeEnemyUnitData.unitPrefab, enemySpawnPoint.position, enemySpawnPoint.rotation);

        enemyBattleUnit = activeEnemyInstance.GetComponent<BattleUnit>();
        if (enemyBattleUnit != null)
        {
            enemyBattleUnit.SetupForCombat();
        }
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

        playerBattleUnit.PerformAction(activePlayerUnitData.defaultAttack, activeEnemyInstance.transform, OnPlayerHitTarget, OnPlayerReturnToSpawn);
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

        enemyBattleUnit.PerformAction(activeEnemyUnitData.defaultAttack, activePlayerInstance.transform, OnEnemyHitTarget, OnEnemyReturnToSpawn);
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
        SceneManager.LoadScene("OpenWorldScene", LoadSceneMode.Single);
    }
}