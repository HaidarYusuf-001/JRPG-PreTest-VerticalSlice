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

    private GameState currentGameState;
    private UnitData pendingEnemyData;
    private bool isRandomEncounter = false;
    private NPCController activeDialogNPC;

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

    public void StartNPCInteraction(NPCController npc)
    {
        activeDialogNPC = npc;
        pendingEnemyData = npc.npcUnitData;
        ChangeGameState(GameState.Cutscene);

        SetDialogCamera(1);

        mainPlayerController.TriggerAutoMovement(npc.standMark, false, () =>
        {
            ChangeGameState(GameState.Dialog);
            npc.dialogFlowchart.ExecuteBlock(npc.targetBlockName);
        });
    }

    public void SetDialogCamera(int shotIndex)
    {
        if (activeDialogNPC == null) return;

        activeDialogNPC.vcamShot1Wide.SetActive(shotIndex == 1);
        activeDialogNPC.vcamShot2NPC.SetActive(shotIndex == 2);
        activeDialogNPC.vcamShot3Player.SetActive(shotIndex == 3);

        vcamExplorationFollow.SetActive(shotIndex == 0);
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
        SetDialogCamera(0);

        if (postBattleCutsceneDirector != null)
        {
            postBattleCutsceneDirector.Play();
        }

        mainPlayerController.SetTalkingState(true);
        Invoke("ExecuteWalkOutLogic", 0.5f);
    }

    private void ExecuteWalkOutLogic()
    {
        mainPlayerController.SetTalkingState(false);
        mainPlayerController.TriggerAutoMovement(postBattleMovementTarget, true, null);
    }

    public void RestoreExplorationState()
    {
        ChangeGameState(GameState.Exploration);
    }
}