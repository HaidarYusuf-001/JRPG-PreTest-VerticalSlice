using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using Unity.Cinemachine;

public class CombatManager : MonoBehaviour
{
    public BattleUIManager battleUIManager;

    public Transform playerSpawnPoint;
    public Transform enemySpawnPoint;

    [Header("Cinematic Action Cameras")]
    public GameObject vcamBattleMain;
    public GameObject vcamActionMelee;
    public GameObject vcamActionSelfPlayer;
    public GameObject vcamActionSelfEnemy;
    public GameObject vcamActionOpponentEnemy;
    public GameObject vcamActionOpponentPlayer;

    private UnitData activePlayerUnitData;
    private UnitData activeEnemyUnitData;
    private int currentEnemyHealth;

    private GameObject activePlayerInstance;
    private GameObject activeEnemyInstance;
    private BattleUnit playerBattleUnit;
    private BattleUnit enemyBattleUnit;

    private ActionTarget currentTargetType;
    private EffectCategory currentEffectCategory;
    private int currentEffectValue;
    private CombatAction currentActionExecution;

    private CinemachineBrain cmBrain;
    private CinemachineBlendDefinition originalBlend;

    private void Start()
    {
        cmBrain = Object.FindAnyObjectByType<CinemachineBrain>();
        if (cmBrain != null)
        {
            originalBlend = cmBrain.DefaultBlend;
        }

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
        SwitchCamera(vcamBattleMain, false);

        RefreshUIHUD();
        battleUIManager.InitializeUI(this);
        Invoke("StartFirstTurn", 0.5f);
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

        AssignCinemachineTargets();
    }

    private void AssignCinemachineTargets()
    {
        void SetCameraTarget(GameObject vcamObj, Transform target)
        {
            if (vcamObj != null && target != null)
            {
                CinemachineCamera cam = vcamObj.GetComponent<CinemachineCamera>();
                if (cam != null)
                {
                    cam.LookAt = target;
                    cam.Follow = target;
                }
            }
        }

        SetCameraTarget(vcamActionSelfPlayer, playerBattleUnit.cameraTrackTarget);
        SetCameraTarget(vcamActionSelfEnemy, enemyBattleUnit.cameraTrackTarget);
        SetCameraTarget(vcamActionOpponentEnemy, enemyBattleUnit.cameraTrackTarget);
        SetCameraTarget(vcamActionOpponentPlayer, playerBattleUnit.cameraTrackTarget);
    }

    private void ResetAllCameras()
    {
        vcamBattleMain.SetActive(false);
        vcamActionMelee.SetActive(false);
        vcamActionSelfPlayer.SetActive(false);
        vcamActionSelfEnemy.SetActive(false);
        vcamActionOpponentEnemy.SetActive(false);
        vcamActionOpponentPlayer.SetActive(false);
    }

    private void SwitchCamera(GameObject targetCam, bool forceCut)
    {
        ResetAllCameras();

        if (forceCut && cmBrain != null)
        {
            cmBrain.DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.Cut, 0f);
            StartCoroutine(RestoreCameraBlendCoroutine());
        }

        targetCam.SetActive(true);
    }

    private IEnumerator RestoreCameraBlendCoroutine()
    {
        yield return null;
        if (cmBrain != null)
        {
            cmBrain.DefaultBlend = originalBlend;
        }
    }

    private void RefreshUIHUD()
    {
        battleUIManager.UpdateHUD(
            SessionManager.Instance.playerCurrentHP,
            SessionManager.Instance.playerMaxHP,
            SessionManager.Instance.playerCurrentMP,
            SessionManager.Instance.playerMaxMP,
            currentEnemyHealth,
            activeEnemyUnitData.maxHealth,
            activeEnemyUnitData.maxMana
        );
    }

    private void StartFirstTurn()
    {
        battleUIManager.ShowMessage("Player Turn!");
        battleUIManager.EnableInput();
    }

    public void ExecutePlayerAttack()
    {
        battleUIManager.DisableAllInput();
        QueueEffectExecution(activePlayerUnitData.basicAttack.targetType, activePlayerUnitData.basicAttack.effectCategory, activePlayerUnitData.basicAttack.effectValue, activePlayerUnitData.basicAttack.actionExecution);
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

        QueueEffectExecution(skill.targetType, skill.effectCategory, skill.effectValue, skill.actionExecution);
        ExecuteTurnSequence(true);
    }

    public void ExecuteItem(InventorySlot slot)
    {
        battleUIManager.DisableAllInput();
        battleUIManager.ShowMainPanel();

        QueueEffectExecution(slot.item.targetType, slot.item.effectCategory, slot.item.effectValue, slot.item.actionExecution);
        InventoryManager.Instance.ConsumeItem(slot);

        ExecuteTurnSequence(true);
    }

    public void ExecuteFlee()
    {
        battleUIManager.DisableAllInput();
        if (UnityEngine.Random.value > 0.5f)
        {
            battleUIManager.ShowMessage("Escaped successfully!");
            Invoke("EndCombatInstance", 1f);
        }
        else
        {
            battleUIManager.ShowMessage("Escape failed!");
            StartCoroutine(ProcessEnemyTurn());
        }
    }

    private void QueueEffectExecution(ActionTarget target, EffectCategory category, int value, CombatAction action)
    {
        currentTargetType = target;
        currentEffectCategory = category;
        currentEffectValue = value;
        currentActionExecution = action;
    }

    private void ExecuteTurnSequence(bool isPlayer)
    {
        bool useCut = (currentEffectCategory != EffectCategory.PhysicalDamage);
        GameObject targetCam = vcamBattleMain;

        if (currentEffectCategory == EffectCategory.PhysicalDamage)
        {
            targetCam = vcamActionMelee;
        }
        else if (currentTargetType == ActionTarget.Self)
        {
            targetCam = isPlayer ? vcamActionSelfPlayer : vcamActionSelfEnemy;
        }
        else if (currentTargetType == ActionTarget.Opponent)
        {
            targetCam = isPlayer ? vcamActionOpponentEnemy : vcamActionOpponentPlayer;
        }

        SwitchCamera(targetCam, useCut);

        if (isPlayer)
        {
            Transform targetTransform = (currentTargetType == ActionTarget.Self) ? activePlayerInstance.transform : activeEnemyInstance.transform;
            playerBattleUnit.PerformAction(currentActionExecution, targetTransform, OnPlayerHitTarget, OnPlayerReturnToSpawn);
        }
        else
        {
            Transform targetTransform = (currentTargetType == ActionTarget.Self) ? activeEnemyInstance.transform : activePlayerInstance.transform;
            enemyBattleUnit.PerformAction(currentActionExecution, targetTransform, OnEnemyHitTarget, OnEnemyReturnToSpawn);
        }
    }

    private void OnPlayerHitTarget()
    {
        ProcessEffectResult(true);
    }

    private void OnPlayerReturnToSpawn()
    {
        bool useCut = (currentEffectCategory != EffectCategory.PhysicalDamage);
        SwitchCamera(vcamBattleMain, useCut);
        CheckCombatState(true);
    }

    private IEnumerator ProcessEnemyTurn()
    {
        battleUIManager.ShowMessage("Enemy Turn!");
        yield return new WaitForSeconds(1f);

        QueueEffectExecution(activeEnemyUnitData.basicAttack.targetType, activeEnemyUnitData.basicAttack.effectCategory, activeEnemyUnitData.basicAttack.effectValue, activeEnemyUnitData.basicAttack.actionExecution);
        ExecuteTurnSequence(false);
    }

    private void OnEnemyHitTarget()
    {
        ProcessEffectResult(false);
    }

    private void OnEnemyReturnToSpawn()
    {
        bool useCut = (currentEffectCategory != EffectCategory.PhysicalDamage);
        SwitchCamera(vcamBattleMain, useCut);
        CheckCombatState(false);
    }

    private void ProcessEffectResult(bool isPlayerAttacking)
    {
        if (currentEffectCategory == EffectCategory.PhysicalDamage || currentEffectCategory == EffectCategory.MagicalDamage)
        {
            int baseAtk = isPlayerAttacking ? SessionManager.Instance.playerBaseAttack : activeEnemyUnitData.attackPower;
            int finalDamage = baseAtk + currentEffectValue;

            if (isPlayerAttacking) currentEnemyHealth -= finalDamage;
            else SessionManager.Instance.playerCurrentHP -= finalDamage;
        }
        else if (currentEffectCategory == EffectCategory.HealHP)
        {
            if (isPlayerAttacking) SessionManager.Instance.playerCurrentHP = Mathf.Min(SessionManager.Instance.playerMaxHP, SessionManager.Instance.playerCurrentHP + currentEffectValue);
            else currentEnemyHealth = Mathf.Min(activeEnemyUnitData.maxHealth, currentEnemyHealth + currentEffectValue);
        }
        else if (currentEffectCategory == EffectCategory.RestoreMP)
        {
            if (isPlayerAttacking) SessionManager.Instance.playerCurrentMP = Mathf.Min(SessionManager.Instance.playerMaxMP, SessionManager.Instance.playerCurrentMP + currentEffectValue);
        }

        RefreshUIHUD();
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
        Invoke("EndCombatInstance", 1f);
    }

    private void HandleDefeat()
    {
        battleUIManager.ShowMessage("Defeated...");
        SessionManager.Instance.playerCurrentHP = SessionManager.Instance.playerMaxHP;
        Invoke("EndCombatInstance", 1.5f);
    }

    private void EndCombatInstance()
    {
        SceneTransitionManager.Instance.LoadSceneWithFade("OpenWorldScene");
    }

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
            if (i < InventoryManager.Instance.playerInventory.Count)
            {
                InventorySlot slot = InventoryManager.Instance.playerInventory[i];
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
}