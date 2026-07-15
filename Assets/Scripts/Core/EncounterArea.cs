using UnityEngine;
using System.Collections.Generic;

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
        if (encounterPool == null || encounterPool.Length == 0) return null;

        int playerLevel = SessionManager.Instance != null ? SessionManager.Instance.playerLevel : 1;
        List<EncounterPoolItem> validEnemies = new List<EncounterPoolItem>();
        float totalWeight = 0f;

        foreach (EncounterPoolItem item in encounterPool)
        {
            if (item.enemyData != null && Mathf.Abs(item.enemyData.level - playerLevel) <= 1)
            {
                validEnemies.Add(item);
                totalWeight += item.encounterPercentage;
            }
        }

        if (validEnemies.Count == 0) return null;

        float randomValue = Random.Range(0f, totalWeight);
        float cumulativeWeight = 0f;

        foreach (EncounterPoolItem item in validEnemies)
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