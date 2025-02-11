using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFXObject : MonoBehaviour
{
    [SerializeField] float lifTime = 1f;
    ParticleSystem _particle;

    Coroutine _countdown;

    private void Awake()
    {
        if(_particle == null)
        {
            _particle = GetComponent<ParticleSystem>();
        }
        _particle.Play(true);
        StartCountdown();
    }

    private void OnEnable()
    {
        if (_particle != null)
        {
            _particle.Play(true);
        }
        StartCountdown();
    }

    void StartCountdown()
    {
        if(_countdown != null)
        {
            StopCoroutine(_countdown);
        }
        _countdown = StartCoroutine(LifeCountdown());
    }

    IEnumerator LifeCountdown()
    {
        yield return new WaitForSeconds(lifTime);

        gameObject.ReturnToPool();
    }
}
