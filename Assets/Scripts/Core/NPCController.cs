using UnityEngine;
using Fungus;

public class NPCController : MonoBehaviour
{
    public Flowchart dialogFlowchart;
    public string targetBlockName;

    public void InitiateDialog()
    {
        if (dialogFlowchart != null && !dialogFlowchart.HasExecutingBlocks())
        {
            dialogFlowchart.ExecuteBlock(targetBlockName);
        }
    }
}