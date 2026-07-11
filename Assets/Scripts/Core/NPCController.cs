// this code has reference to that script file code
using UnityEngine;
using Fungus;

public class NPCController : MonoBehaviour
{
    public Flowchart dialogFlowchart;
    public string targetBlockName;
    public UnitData npcUnitData;
    public GameFlowManager activeFlowManager;
    public Transform dialogLookAtPoint;
    public CanvasGroup promptCanvasGroup;

    private void Start()
    {
        if (promptCanvasGroup != null)
        {
            promptCanvasGroup.alpha = 0f;
        }
    }

    public void SetPromptVisibility(bool isVisible)
    {
        if (promptCanvasGroup != null)
        {
            promptCanvasGroup.alpha = isVisible ? 1f : 0f;
        }
    }

    public void InitiateDialog()
    {
        if (dialogFlowchart != null && !dialogFlowchart.HasExecutingBlocks())
        {
            SetPromptVisibility(false);
            activeFlowManager.SetCurrentEnemyData(npcUnitData);
            activeFlowManager.TriggerDialogSequence(this);
            dialogFlowchart.ExecuteBlock(targetBlockName);
        }
    }
}