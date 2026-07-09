// this code has reference to that script file code
using UnityEngine;
using UnityEngine.Playables;
using Fungus;

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
    public CombatManager mainCombatManager;
    public PlayableDirector battleCameraDirector;
    public PlayableDirector postBattleCutsceneDirector;
    public Transform postBattleMovementTarget;

    private GameState currentGameState;

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
                break;
            case GameState.Dialog:
            case GameState.Combat:
            case GameState.Cutscene:
                mainPlayerController.canMove = false;
                break;
        }
    }

    public void TriggerBattleSequence()
    {
        ChangeGameState(GameState.Combat);
        battleCameraDirector.Play();
        Invoke("StartCombatLogic", (float)battleCameraDirector.duration);
    }

    private void StartCombatLogic()
    {
        mainCombatManager.InitializeCombatSequence(this);
    }

    public void ProcessBattleCompletion()
    {
        ChangeGameState(GameState.Dialog);
        Flowchart.BroadcastFungusMessage("BattleEnded");
    }

    public void TriggerPostBattleCutscene()
    {
        ChangeGameState(GameState.Cutscene);
        postBattleCutsceneDirector.Play();
        mainPlayerController.ExecuteHappyAnimation();
        Invoke("TriggerPlayerAutoMovement", 3f);
    }

    private void TriggerPlayerAutoMovement()
    {
        mainPlayerController.TriggerAutoMovement(postBattleMovementTarget);
    }
}