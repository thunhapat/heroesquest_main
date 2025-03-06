using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class InputManager : MonoBehaviour
{
    public static UnityAction<float,float> OnInputDirectionalKey;

    private void Awake()
    {
        InitInputManager();
    }

    private void Update()
    {
        if(Input.anyKeyDown)
        {
            Debug.Log($"inputString is {Input.inputString}");
        }
    }

    public void InitInputManager()
    {
        //Clear all events and subscribes.
        OnInputDirectionalKey = null;
    }

}
