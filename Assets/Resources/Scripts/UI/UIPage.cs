using UnityEngine;

public class UIPage<T> : MonoBehaviour where T : MonoBehaviour
{
    #region Singleton
    protected static T _instance;

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<T>();
            }
            return _instance;
        }
    }
    #endregion
}
