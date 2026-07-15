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
    public GameObject vcamExplorationFollow;

    private GameState currentGameState;
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
        InitializeSceneState();
    }

    private void InitializeSceneState()
    {
        if (SessionManager.Instance != null && SessionManager.Instance.isReturningFromBattle)
        {
            mainPlayerController.transform.position = SessionManager.Instance.savedPlayerPosition;
            mainPlayerController.transform.rotation = SessionManager.Instance.savedPlayerRotation;

            CinemachineCamera followCam = vcamExplorationFollow.GetComponent<CinemachineCamera>();
            if (followCam != null)
            {
                followCam.PreviousStateIsValid = false;
            }

            if (SessionManager.Instance.isRandomEncounter)
            {
                ChangeGameState(GameState.Exploration);
            }
            else
            {
                NPCController[] allNPCs = FindObjectsByType<NPCController>(FindObjectsSortMode.None);
                foreach (NPCController npc in allNPCs)
                {
                    if (npc.npcID == SessionManager.Instance.lastInteractedNpcID)
                    {
                        activeDialogNPC = npc;
                        ChangeGameState(GameState.Dialog);
                        StartCoroutine(DelayFungusBroadcast());
                        break;
                    }
                }
            }

            SessionManager.Instance.isReturningFromBattle = false;
        }
        else
        {
            ChangeGameState(GameState.Exploration);
        }
    }

    private IEnumerator DelayFungusBroadcast()
    {
        yield return null;
        Flowchart.BroadcastFungusMessage("BattleEnded");
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

        if (SessionManager.Instance != null)
        {
            SessionManager.Instance.pendingEnemyData = npc.npcUnitData;
        }

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
        SaveExplorationState(false);
        SceneManager.LoadScene("BattleScene", LoadSceneMode.Single);
    }

    public void TriggerRandomEncounter(UnitData enemyData)
    {
        if (SessionManager.Instance != null)
        {
            SessionManager.Instance.pendingEnemyData = enemyData;
        }
        SaveExplorationState(true);
        SceneManager.LoadScene("BattleScene", LoadSceneMode.Single);
    }

    private void SaveExplorationState(bool isRandom)
    {
        if (SessionManager.Instance != null)
        {
            SessionManager.Instance.savedPlayerPosition = mainPlayerController.transform.position;
            SessionManager.Instance.savedPlayerRotation = mainPlayerController.transform.rotation;
            SessionManager.Instance.isReturningFromBattle = true;
            SessionManager.Instance.isRandomEncounter = isRandom;

            if (!isRandom && activeDialogNPC != null)
            {
                SessionManager.Instance.lastInteractedNpcID = activeDialogNPC.npcID;
            }
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