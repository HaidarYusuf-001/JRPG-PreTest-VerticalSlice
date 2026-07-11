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

    public float actionMovementSpeed = 8f;
    public float attackDistanceOffset = 1.5f;

    private UnitData activePlayerUnitData;
    private UnitData activeEnemyUnitData;
    private int currentPlayerHealth;
    private int currentEnemyHealth;

    private GameObject activePlayerInstance;
    private GameObject activeEnemyInstance;

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
        activeEnemyInstance = Instantiate(activeEnemyUnitData.unitPrefab, enemySpawnPoint.position, enemySpawnPoint.rotation);
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
        StartCoroutine(ExecuteActionSequence(activePlayerInstance, activeEnemyInstance, true));
    }

    private IEnumerator ExecuteEnemyTurn()
    {
        yield return new WaitForSeconds(0.5f);
        StartCoroutine(ExecuteActionSequence(activeEnemyInstance, activePlayerInstance, false));
    }

    private IEnumerator ExecuteActionSequence(GameObject attacker, GameObject target, bool isPlayerTurn)
    {
        Vector3 startPosition = attacker.transform.position;
        Vector3 directionToTarget = (target.transform.position - attacker.transform.position).normalized;
        Vector3 targetPosition = target.transform.position - (directionToTarget * attackDistanceOffset);

        vcamBattleMain.SetActive(false);
        vcamAction.SetActive(true);

        Animator attackerAnimator = attacker.GetComponent<Animator>();
        if (attackerAnimator != null) attackerAnimator.SetBool("isWalking", true);

        while (Vector3.Distance(attacker.transform.position, targetPosition) > 0.1f)
        {
            attacker.transform.position = Vector3.MoveTowards(attacker.transform.position, targetPosition, actionMovementSpeed * Time.deltaTime);
            yield return null;
        }

        if (attackerAnimator != null) attackerAnimator.SetBool("isWalking", false);
        yield return new WaitForSeconds(0.5f);

        if (isPlayerTurn)
        {
            currentEnemyHealth -= activePlayerUnitData.attackPower;
            battleUIManager.UpdateEnemyHealth(currentEnemyHealth);
        }
        else
        {
            currentPlayerHealth -= activeEnemyUnitData.attackPower;
            battleUIManager.UpdatePlayerHealth(currentPlayerHealth);
        }

        yield return new WaitForSeconds(0.5f);

        vcamAction.SetActive(false);
        vcamBattleMain.SetActive(true);

        if (attackerAnimator != null) attackerAnimator.SetBool("isWalking", true);

        Vector3 directionBack = (startPosition - attacker.transform.position).normalized;
        if (directionBack != Vector3.zero)
        {
            attacker.transform.rotation = Quaternion.LookRotation(directionBack);
        }

        while (Vector3.Distance(attacker.transform.position, startPosition) > 0.1f)
        {
            attacker.transform.position = Vector3.MoveTowards(attacker.transform.position, startPosition, actionMovementSpeed * Time.deltaTime);
            yield return null;
        }

        if (attackerAnimator != null) attackerAnimator.SetBool("isWalking", false);
        attacker.transform.rotation = isPlayerTurn ? playerSpawnPoint.rotation : enemySpawnPoint.rotation;

        yield return new WaitForSeconds(0.5f);
        CheckCombatState(isPlayerTurn);
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
                StartCoroutine(ExecuteEnemyTurn());
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