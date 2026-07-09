// this code has reference to that script file code
using UnityEngine;
using UnityEngine.Playables;

public class GameFlowManager : MonoBehaviour
{
    public PlayerController playerController;
    public CombatManager combatManager;
    public PlayableDirector battleCameraSequence;
    public PlayableDirector cutsceneSequence;
    public Transform postBattleWalkTarget;

    public void StartBattleSequence()
    {
        playerController.canMove = false;
        battleCameraSequence.Play();
        Invoke("InitializeCombat", (float)battleCameraSequence.duration);
    }

    private void InitializeCombat()
    {
        combatManager.StartCombat(this);
    }

    public void OnBattleEnded()
    {
        Fungus.Flowchart.BroadcastFungusMessage("BattleEnded");
    }

    public void StartPostBattleCutscene()
    {
        cutsceneSequence.Play();
        playerController.PlayHappyAnimation();
        Invoke("StartAutoWalkOut", 3f);
    }

    private void StartAutoWalkOut()
    {
        playerController.SetAutoMove(postBattleWalkTarget);
    }
}