using UnityEngine;

public class AudioPlayer : MonoBehaviour
{
    public void PlaySFX(AudioClip sfx)
    {
        if(sfx != null)
        {
            sfx.PlaySFXOneShot();
        }
    }
}
