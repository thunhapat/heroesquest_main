using System.Collections.Generic;
using UnityEngine;

public class VFXMoveSuggestion : MonoBehaviour
{
    public ParticleSystem Arrow_Top;
    public ParticleSystem Arrow_Right;
    public ParticleSystem Arrow_Bottom;
    public ParticleSystem Arrow_Left;

    [SerializeField] float lifeTime = 1f;

    public void PlayArrowSuggestion(List<Vector2Int> arrowList)
    {
        gameObject.SetActive(true);

        PlayParticle(Arrow_Top, arrowList.Contains(Vector2Int.up));
        PlayParticle(Arrow_Right, arrowList.Contains(Vector2Int.right));
        PlayParticle(Arrow_Bottom, arrowList.Contains(Vector2Int.down));
        PlayParticle(Arrow_Left, arrowList.Contains(Vector2Int.left));

        CancelInvoke();
        Invoke("GoDeactive", lifeTime);
    }

    void PlayParticle(ParticleSystem particle, bool isPlayable)
    {
        particle.gameObject.SetActive(isPlayable);
        if(isPlayable)
        {
            particle.Play();
        }
    }

    void GoDeactive()
    {
        gameObject.SetActive(false);
    }
}
