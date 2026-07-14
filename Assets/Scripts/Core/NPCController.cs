using UnityEngine;
using Fungus;

public class NPCController : MonoBehaviour
{
    public Flowchart dialogFlowchart;
    public string targetBlockName;
    public Transform dialogLookAtPoint;
    public CanvasGroup promptCanvasGroup;
    public UnitData npcUnitData;

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

            if (GameFlowManager.Instance != null)
            {
                GameFlowManager.Instance.SetCurrentEnemyData(npcUnitData);
                GameFlowManager.Instance.TriggerDialogSequence(this);
            }

            dialogFlowchart.ExecuteBlock(targetBlockName);
        }
    }
}