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
    public HealAction defaultItemAction;

    private UnitData activePlayerUnitData;
    private UnitData activeEnemyUnitData;
    private int currentEnemyHealth;

    private GameObject activePlayerInstance;
    private GameObject activeEnemyInstance;
    private BattleUnit playerBattleUnit;
    private BattleUnit enemyBattleUnit;

    private SkillData currentQueuedSkill;
    private ItemConsumableData currentQueuedItem;

    private void Start()
    {
        if (SessionManager.Instance != null)
        {
            InitializeDynamicCombatSequence(SessionManager.Instance.playerUnitData, SessionManager.Instance.pendingEnemyData);
        }
    }

    private void InitializeDynamicCombatSequence(UnitData playerData, UnitData enemyData)
    {
        activePlayerUnitData = playerData;
        activeEnemyUnitData = enemyData;
        currentEnemyHealth = activeEnemyUnitData.maxHealth;

        SpawnCombatants();
        ConfigureCameras();

        RefreshUIHUD();
        battleUIManager.InitializeUI(this);
        battleEntryDirector.Play();
        Invoke("StartFirstTurn", (float)battleEntryDirector.duration);
    }

    private void SpawnCombatants()
    {
        activePlayerInstance = Instantiate(activePlayerUnitData.unitPrefab, playerSpawnPoint.position, playerSpawnPoint.rotation);
        Destroy(activePlayerInstance.GetComponent<PlayerController>());
        playerBattleUnit = activePlayerInstance.GetComponent<BattleUnit>();
        playerBattleUnit?.SetupForCombat();

        activeEnemyInstance = Instantiate(activeEnemyUnitData.unitPrefab, enemySpawnPoint.position, enemySpawnPoint.rotation);
        enemyBattleUnit = activeEnemyInstance.GetComponent<BattleUnit>();
        enemyBattleUnit?.SetupForCombat();
    }

    private void ConfigureCameras()
    {
        vcamBattleMain.SetActive(true);
        vcamAction.SetActive(false);
    }

    private void RefreshUIHUD()
    {
        battleUIManager.UpdateHUD(
            SessionManager.Instance.playerCurrentHP,
            SessionManager.Instance.playerMaxHP,
            SessionManager.Instance.playerCurrentMP,
            SessionManager.Instance.playerMaxMP,
            currentEnemyHealth,
            activeEnemyUnitData.maxHealth
        );
    }

    private void StartFirstTurn()
    {
        battleUIManager.ShowMessage("Player Turn!");
        battleUIManager.EnableInput();
    }

    // --- BUTTON CALLBACKS ---
    public void ExecutePlayerAttack()
    {
        battleUIManager.DisableAllInput();
        currentQueuedSkill = activePlayerUnitData.basicAttack;
        ExecuteTurnSequence(true);
    }

    public void ExecuteSkill(SkillData skill)
    {
        if (SessionManager.Instance.playerCurrentMP < skill.manaCost)
        {
            battleUIManager.ShowMessage("Not enough MP!");
            return;
        }

        SessionManager.Instance.playerCurrentMP -= skill.manaCost;
        RefreshUIHUD();

        battleUIManager.DisableAllInput();
        battleUIManager.ShowMainPanel();
        currentQueuedSkill = skill;
        ExecuteTurnSequence(true);
    }

    public void ExecuteItem(InventorySlot slot)
    {
        slot.quantity--;
        if (slot.quantity <= 0) SessionManager.Instance.playerInventory.Remove(slot);

        battleUIManager.DisableAllInput();
        battleUIManager.ShowMainPanel();
        currentQueuedItem = slot.item;

        vcamBattleMain.SetActive(false);
        vcamAction.SetActive(true);
        playerBattleUnit.PerformAction(defaultItemAction, activePlayerInstance.transform, OnItemApplied, OnPlayerReturnToSpawn);
    }

    public void ExecuteFlee()
    {
        battleUIManager.DisableAllInput();
        bool success = UnityEngine.Random.value > 0.5f;

        if (success)
        {
            battleUIManager.ShowMessage("Escaped successfully!");
            Invoke("EndCombatInstance", 1.5f);
        }
        else
        {
            battleUIManager.ShowMessage("Escape failed!");
            StartCoroutine(ProcessEnemyTurn());
        }
    }

    // --- UI POPULATION ---
    public void PopulateSkillUI()
    {
        for (int i = 0; i < battleUIManager.skillButtons.Length; i++)
        {
            if (i < activePlayerUnitData.availableSkills.Length)
            {
                SkillData skill = activePlayerUnitData.availableSkills[i];
                battleUIManager.skillButtons[i].gameObject.SetActive(true);
                battleUIManager.skillButtons[i].GetComponentInChildren<TMPro.TextMeshProUGUI>().text = $"{skill.skillName} ({skill.manaCost}MP)";
                battleUIManager.skillButtons[i].onClick.RemoveAllListeners();
                battleUIManager.skillButtons[i].onClick.AddListener(() => ExecuteSkill(skill));
            }
            else
            {
                battleUIManager.skillButtons[i].gameObject.SetActive(false);
            }
        }
    }

    public void PopulateItemUI()
    {
        for (int i = 0; i < battleUIManager.itemButtons.Length; i++)
        {
            if (i < SessionManager.Instance.playerInventory.Count)
            {
                InventorySlot slot = SessionManager.Instance.playerInventory[i];
                battleUIManager.itemButtons[i].gameObject.SetActive(true);
                battleUIManager.itemButtons[i].GetComponentInChildren<TMPro.TextMeshProUGUI>().text = $"{slot.item.itemName} (x{slot.quantity})";
                battleUIManager.itemButtons[i].onClick.RemoveAllListeners();
                battleUIManager.itemButtons[i].onClick.AddListener(() => ExecuteItem(slot));
            }
            else
            {
                battleUIManager.itemButtons[i].gameObject.SetActive(false);
            }
        }
    }

    // --- COMBAT LOGIC ---
    private void ExecuteTurnSequence(bool isPlayer)
    {
        vcamBattleMain.SetActive(false);
        vcamAction.SetActive(true);

        if (isPlayer)
        {
            Transform target = currentQueuedSkill.isHealing ? activePlayerInstance.transform : activeEnemyInstance.transform;
            playerBattleUnit.PerformAction(currentQueuedSkill.actionExecution, target, OnPlayerHitTarget, OnPlayerReturnToSpawn);
        }
        else
        {
            currentQueuedSkill = activeEnemyUnitData.basicAttack;
            enemyBattleUnit.PerformAction(currentQueuedSkill.actionExecution, activePlayerInstance.transform, OnEnemyHitTarget, OnEnemyReturnToSpawn);
        }
    }

    private void OnPlayerHitTarget()
    {
        if (currentQueuedSkill.isHealing)
        {
            SessionManager.Instance.playerCurrentHP = Mathf.Min(SessionManager.Instance.playerMaxHP, SessionManager.Instance.playerCurrentHP + currentQueuedSkill.power);
        }
        else
        {
            int damage = SessionManager.Instance.playerBaseAttack + currentQueuedSkill.power;
            currentEnemyHealth -= damage;
        }
        RefreshUIHUD();
    }

    private void OnItemApplied()
    {
        SessionManager.Instance.playerCurrentHP = Mathf.Min(SessionManager.Instance.playerMaxHP, SessionManager.Instance.playerCurrentHP + currentQueuedItem.healAmount);
        SessionManager.Instance.playerCurrentMP = Mathf.Min(SessionManager.Instance.playerMaxMP, SessionManager.Instance.playerCurrentMP + currentQueuedItem.manaRestoreAmount);
        RefreshUIHUD();
    }

    private void OnPlayerReturnToSpawn()
    {
        vcamAction.SetActive(false);
        vcamBattleMain.SetActive(true);
        CheckCombatState(true);
    }

    private IEnumerator ProcessEnemyTurn()
    {
        battleUIManager.ShowMessage("Enemy Turn!");
        yield return new WaitForSeconds(1f);
        ExecuteTurnSequence(false);
    }

    private void OnEnemyHitTarget()
    {
        int damage = activeEnemyUnitData.attackPower + currentQueuedSkill.power;
        SessionManager.Instance.playerCurrentHP -= damage;
        RefreshUIHUD();
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
            if (currentEnemyHealth <= 0) HandleVictory();
            else StartCoroutine(ProcessEnemyTurn());
        }
        else
        {
            if (SessionManager.Instance.playerCurrentHP <= 0) HandleDefeat();
            else StartFirstTurn();
        }
    }

    private void HandleVictory()
    {
        int gainedExp = activeEnemyUnitData.baseExpYield;
        battleUIManager.ShowMessage($"Victory! Gained {gainedExp} EXP.");
        SessionManager.Instance.GainExperience(gainedExp);
        Invoke("EndCombatInstance", 2f);
    }

    private void HandleDefeat()
    {
        battleUIManager.ShowMessage("Defeated...");
        // Add Game Over logic here later
    }

    private void EndCombatInstance()
    {
        SceneManager.LoadScene("OpenWorldScene", LoadSceneMode.Single);
    }
}