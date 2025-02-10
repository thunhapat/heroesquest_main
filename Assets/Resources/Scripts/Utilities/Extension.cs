using System.Collections.Generic;
using UnityEngine;

public static class GameObjectPool
{
    private static Dictionary<string, Queue<GameObject>> poolDict = new Dictionary<string, Queue<GameObject>>();
    private static Dictionary<string, string> poolMapDict = new Dictionary<string, string>();

    public static GameObject GetFromPool(this GameObject prefab, Transform parent = null)
    {
        string prefabKey = prefab.GetInstanceID().ToString();

        if (!poolDict.ContainsKey(prefabKey))
        {
            poolDict.Add(prefabKey, new Queue<GameObject>());
        }

        GameObject poolObj = null;
        if(poolDict.ContainsKey(prefabKey) && poolDict[prefabKey].Count > 0)
        {
            Queue<GameObject> pool = poolDict[prefabKey];
            poolObj = pool.Dequeue();
            poolObj.transform.SetParent(parent);
        }

        if(poolObj == null)
        {
            poolObj = GameObject.Instantiate(prefab, parent);
        }

        string poolInstanceID = poolObj.GetInstanceID().ToString();
        poolMapDict.Add(poolInstanceID, prefabKey);
        poolObj.SetActive(true);

        return poolObj;
    }

    public static GameObject GetFromPool(this GameObject prefab, Vector3 position)
    {
        GameObject poolObj = prefab.GetFromPool();
        poolObj.transform.position = position;
        return poolObj;
    }

    public static void ReturnToPool(this GameObject poolObj)
    {
        string key = poolObj.GetInstanceID().ToString();

        if (poolMapDict.ContainsKey(key))
        {
            string poolKey = poolMapDict[key];

            poolMapDict.Remove(key);

            if (poolDict.ContainsKey(poolKey))
            {
                Queue<GameObject> pool = poolDict[poolKey];
                poolObj.transform.SetParent(null);
                poolObj.transform.localScale = Vector3.one;
                poolObj.SetActive(false);
                pool.Enqueue(poolObj);

                return;
            }
        }
    }
}

public static class AudioExtension
{
    public static void PlaySFXOneShot(this AudioClip audioClip)
    {
        AudioManager.Instance.PlaySFXOneShot(audioClip);
    }

    public static void PlayAsBackgroundMusic(this AudioClip audioClip)
    {
        AudioManager.Instance.PlayBGM(audioClip);
    }
}
