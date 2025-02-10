using UnityEngine;

public class UICanvas : MonoBehaviour
{

    #region Singleton
    private static UICanvas _instance;

    public static UICanvas Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<UICanvas>();
                _instance.InitUICanvas();
            }
            return _instance;
        }
    }
    #endregion

    public RectTransform Rect;

    public void InitUICanvas()
    {
        Rect = GetComponent<RectTransform>();
    }

}
