using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public static class GameObjectExtension
{
    private static Dictionary<string, List<GameObject>> _poolDict = new Dictionary<string, List<GameObject>>();
    private static Dictionary<GameObject, string> _poolMapDict = new Dictionary<GameObject, string>();

    public static GameObject GetFromPool(this GameObject prefab, Transform parent = null)
    {
        string prefabKey = prefab.GetInstanceID().ToString();

        if (!_poolDict.ContainsKey(prefabKey))
        {
            _poolDict.Add(prefabKey, new List<GameObject>());
        }

        GameObject poolObj = null;
        if(_poolDict.ContainsKey(prefabKey))
        {
            List<GameObject> selectedPool = _poolDict[prefabKey];
            if (selectedPool.Count > 0)
            {
                poolObj = selectedPool.First();
                selectedPool.Remove(poolObj);
                poolObj.transform.SetParent(parent);
            }
        }

        if(poolObj == null)
        {
            poolObj = GameObject.Instantiate(prefab, parent);
        }

        _poolMapDict.Add(poolObj, prefabKey);
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
        if (_poolMapDict.ContainsKey(poolObj))
        {
            string poolKey = _poolMapDict[poolObj];

            _poolMapDict.Remove(poolObj);

            if (_poolDict.ContainsKey(poolKey))
            {
                //Return to selected pool.
                List<GameObject> selectedPool = _poolDict[poolKey];
                selectedPool.Add(poolObj);

                //Hide poolObj
                poolObj.transform.SetParent(null);
                poolObj.transform.localScale = Vector3.one;
                poolObj.SetActive(false);

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
