// this code has reference to that script file code
using UnityEngine;
using Fungus;

public class NPCController : MonoBehaviour
{
    public Flowchart dialogFlowchart;
    public string targetBlockName;
    public UnitData npcUnitData;
    public GameFlowManager activeFlowManager;

    public void InitiateDialog()
    {
        if (dialogFlowchart != null && !dialogFlowchart.HasExecutingBlocks())
        {
            activeFlowManager.SetCurrentEnemyData(npcUnitData);
            dialogFlowchart.ExecuteBlock(targetBlockName);
        }
    }
}