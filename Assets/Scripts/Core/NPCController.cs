using UnityEngine;
using Fungus;

public class NPCController : MonoBehaviour
{
    public Flowchart flowchart;
    public string interactionBlockName;

    public void StartInteraction()
    {
        if (flowchart != null && !flowchart.HasExecutingBlocks())
        {
            flowchart.ExecuteBlock(interactionBlockName);
        }
    }
}