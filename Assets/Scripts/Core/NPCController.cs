using UnityEngine;
using Fungus;

public class NPCController : MonoBehaviour
{
    public Flowchart dialogFlowchart;
    public string targetBlockName;
    public UnitData npcUnitData;
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
            GameFlowManager.Instance.SetCurrentEnemyData(npcUnitData);
            GameFlowManager.Instance.TriggerDialogSequence(this);
            dialogFlowchart.ExecuteBlock(targetBlockName);
        }
    }
}