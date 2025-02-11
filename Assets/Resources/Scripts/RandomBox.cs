using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RandomBoxChance 
{
    public float ChanceWeight;
    public ScriptableObject Reward;
}

public class RandomBox
{
    public static ScriptableObject GetRandomReward(List<RandomBoxChance> pool)
    {
        float totalWeight = 0;
        foreach(var item in pool)
        {
            totalWeight += item.ChanceWeight;
        }

        float randomedWeight = Random.Range(0, totalWeight);
        foreach(var item in pool)
        {
            if(randomedWeight < item.ChanceWeight)
            {
                return item.Reward;
            }
            randomedWeight -= item.ChanceWeight;
        }

        return null;
    }
}
