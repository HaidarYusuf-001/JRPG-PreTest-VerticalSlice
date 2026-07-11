// this code has reference to that script file code
using UnityEngine;
using Fungus;

public class NPCController : MonoBehaviour
{
    public Flowchart dialogFlowchart;
    public string targetBlockName;
    public UnitData npcUnitData;
    public GameFlowManager activeFlowManager;

    public GameObject interactionPrompt;
    public Transform dialogCameraPoint;
    public Transform dialogLookAtPoint;

    private void Start()
    {
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
    }

    public void SetPromptVisibility(bool isVisible)
    {
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(isVisible);
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