using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using Fungus;
using System.Collections;
using Unity.Cinemachine;

public enum GameState
{
    Exploration,
    Dialog,
    Combat,
    Cutscene
}

public class GameFlowManager : MonoBehaviour
{
    public static GameFlowManager Instance { get; private set; }

    public PlayerController mainPlayerController;
    public PlayableDirector postBattleCutsceneDirector;
    public Transform postBattleMovementTarget;
    public Camera explorationMainCamera;
    public UnitData playerUnitData;

    public GameObject vcamExplorationFollow;
    public GameObject vcamExplorationDialog;

    private CinemachineCamera cinemachineDialogCamera;
    private GameState currentGameState;
    private UnitData pendingEnemyData;
    private bool isRandomEncounter = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        cinemachineDialogCamera = vcamExplorationDialog.GetComponent<CinemachineCamera>();
        ChangeGameState(GameState.Exploration);
    }

    public void ChangeGameState(GameState newState)
    {
        currentGameState = newState;
        ProcessStateChange();
    }

    private void ProcessStateChange()
    {
        switch (currentGameState)
        {
            case GameState.Exploration:
                mainPlayerController.SetMovementState(true);
                vcamExplorationFollow.SetActive(true);
                vcamExplorationDialog.SetActive(false);

                NPCController activeNPC = mainPlayerController.GetCurrentNPC();
                if (activeNPC != null)
                {
                    activeNPC.SetPromptVisibility(true);
                }
                break;
            case GameState.Dialog:
            case GameState.Combat:
            case GameState.Cutscene:
                mainPlayerController.SetMovementState(false);

                NPCController currentNPC = mainPlayerController.GetCurrentNPC();
                if (currentNPC != null)
                {
                    currentNPC.SetPromptVisibility(false);
                }
                break;
        }
    }

    public void SetCurrentEnemyData(UnitData enemyData)
    {
        pendingEnemyData = enemyData;
    }

    public void TriggerDialogSequence(NPCController activeNPC)
    {
        ChangeGameState(GameState.Dialog);
        vcamExplorationFollow.SetActive(false);

        if (cinemachineDialogCamera != null)
        {
            cinemachineDialogCamera.LookAt = activeNPC.dialogLookAtPoint;
        }

        vcamExplorationDialog.SetActive(true);
    }

    public void TriggerNPCBattle()
    {
        isRandomEncounter = false;
        ChangeGameState(GameState.Combat);
        StartCoroutine(LoadBattleSceneAdditive());
    }

    public void TriggerRandomEncounter(UnitData enemyData)
    {
        isRandomEncounter = true;
        pendingEnemyData = enemyData;
        ChangeGameState(GameState.Combat);
        StartCoroutine(LoadBattleSceneAdditive());
    }

    private IEnumerator LoadBattleSceneAdditive()
    {
        explorationMainCamera.gameObject.SetActive(false);
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("BattleScene", LoadSceneMode.Additive);

        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        CombatManager activeCombatManager = Object.FindAnyObjectByType<CombatManager>();
        activeCombatManager.OnCombatCompleted += HandleCombatCompletion;
        activeCombatManager.InitializeDynamicCombatSequence(playerUnitData, pendingEnemyData);
    }

    private void HandleCombatCompletion()
    {
        StartCoroutine(UnloadBattleScene());
    }

    private IEnumerator UnloadBattleScene()
    {
        AsyncOperation asyncUnload = SceneManager.UnloadSceneAsync("BattleScene");

        while (!asyncUnload.isDone)
        {
            yield return null;
        }

        explorationMainCamera.gameObject.SetActive(true);

        if (isRandomEncounter)
        {
            ChangeGameState(GameState.Exploration);
        }
        else
        {
            ChangeGameState(GameState.Dialog);
            Flowchart.BroadcastFungusMessage("BattleEnded");
        }
    }

    public void TriggerPostBattleCutscene()
    {
        ChangeGameState(GameState.Cutscene);
        vcamExplorationDialog.SetActive(false);
        vcamExplorationFollow.SetActive(true);

        if (postBattleCutsceneDirector != null)
        {
            postBattleCutsceneDirector.Play();
        }

        mainPlayerController.ExecuteHappyAnimation();
        Invoke("TriggerPlayerAutoMovement", 0.5f);
    }

    private void TriggerPlayerAutoMovement()
    {
        mainPlayerController.TriggerAutoMovement(postBattleMovementTarget);
    }

    public void RestoreExplorationState()
    {
        ChangeGameState(GameState.Exploration);
    }
}