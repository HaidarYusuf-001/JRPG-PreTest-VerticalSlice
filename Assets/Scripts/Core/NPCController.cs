using UnityEngine;
using Fungus;

public class NPCController : MonoBehaviour
{
    public string npcID;
    public Flowchart dialogFlowchart;
    public string targetBlockName;
    public CanvasGroup promptCanvasGroup;
    public UnitData npcUnitData;

    public Transform standMark;
    public GameObject vcamShot1Wide;
    public GameObject vcamShot2NPC;
    public GameObject vcamShot3Player;

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
                GameFlowManager.Instance.StartNPCInteraction(this);
            }
        }
    }
}