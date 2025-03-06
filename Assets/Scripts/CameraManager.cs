using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoSingleton<CameraManager>
{
    [SerializeField] float FollowSpeed;

    Camera _mainCamera;
    Transform _focusTarget;
    Coroutine _cameraFollow;
    bool _initialized = false;

    public void InitCamera()
    {
        if(_mainCamera == null)
        {
            _mainCamera = Camera.main;
        }

        _cameraFollow = StartCoroutine(CameraFollow());

        _initialized = true;
    }

    private void OnDisable()
    {
        if(_cameraFollow != null)
        {
            StopCoroutine(_cameraFollow);
            _cameraFollow = null;
        }
    }

    private void OnEnable()
    {
        if(!_initialized)
        {
            InitCamera();
        }
        else
        {
            _cameraFollow = StartCoroutine(CameraFollow());
        }
    }


    public void SetFocus(Transform target)
    {
        _focusTarget = target;
    }

    IEnumerator CameraFollow()
    {
        while(true)
        {
            if(_focusTarget != null)
            {
                Vector3 moveTo = Vector3.Lerp(transform.position, _focusTarget.position, FollowSpeed * Time.deltaTime);
                moveTo.z = transform.position.z;

                transform.position = moveTo;
            }

            yield return null;
        }

    }
}
