// this code has reference to that script file code
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using Fungus;
using System.Collections;

public enum GameState
{
    Exploration,
    Dialog,
    Combat,
    Cutscene
}

public class GameFlowManager : MonoBehaviour
{
    public PlayerController mainPlayerController;
    public PlayableDirector postBattleCutsceneDirector;
    public Transform postBattleMovementTarget;
    public Camera explorationMainCamera;
    public UnitData playerUnitData;

    public GameObject vcamExplorationFollow;
    public GameObject vcamExplorationDialog;

    private GameState currentGameState;
    private CombatManager activeCombatManager;
    private UnitData currentTargetEnemyData;

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
                mainPlayerController.canMove = true;
                vcamExplorationFollow.SetActive(true);
                vcamExplorationDialog.SetActive(false);
                break;
            case GameState.Dialog:
            case GameState.Combat:
            case GameState.Cutscene:
                mainPlayerController.canMove = false;
                break;
        }
    }

    public void SetCurrentEnemyData(UnitData enemyData)
    {
        currentTargetEnemyData = enemyData;
    }

    public void TriggerDialogSequence(NPCController activeNPC)
    {
        ChangeGameState(GameState.Dialog);
        vcamExplorationFollow.SetActive(false);

        vcamExplorationDialog.transform.position = activeNPC.dialogCameraPoint.position;
        vcamExplorationDialog.transform.LookAt(activeNPC.dialogLookAtPoint);
        vcamExplorationDialog.SetActive(true);
    }

    public void TriggerBattleSequence()
    {
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

        activeCombatManager = Object.FindAnyObjectByType<CombatManager>();
        activeCombatManager.InitializeDynamicCombatSequence(this, playerUnitData, currentTargetEnemyData);
    }

    public void ProcessBattleCompletion()
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
        ChangeGameState(GameState.Dialog);
        Flowchart.BroadcastFungusMessage("BattleEnded");
    }

    public void TriggerPostBattleCutscene()
    {
        ChangeGameState(GameState.Cutscene);
        vcamExplorationDialog.SetActive(false);
        vcamExplorationFollow.SetActive(true);

        postBattleCutsceneDirector.Play();
        mainPlayerController.ExecuteHappyAnimation();
        Invoke("TriggerPlayerAutoMovement", 3f);
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