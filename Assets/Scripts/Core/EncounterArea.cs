using UnityEngine;

[System.Serializable]
public class EncounterPoolItem
{
    public UnitData enemyData;
    [Range(0f, 100f)]
    public float encounterPercentage;
}

public class EncounterArea : MonoBehaviour
{
    public EncounterPoolItem[] encounterPool;

    public UnitData GetRandomEnemy()
    {
        if (encounterPool == null || encounterPool.Length == 0)
        {
            return null;
        }

        float totalWeight = 0f;
        foreach (EncounterPoolItem item in encounterPool)
        {
            totalWeight += item.encounterPercentage;
        }

        float randomValue = Random.Range(0f, totalWeight);
        float cumulativeWeight = 0f;

        foreach (EncounterPoolItem item in encounterPool)
        {
            cumulativeWeight += item.encounterPercentage;
            if (randomValue <= cumulativeWeight)
            {
                return item.enemyData;
            }
        }

        return null;
    }
}