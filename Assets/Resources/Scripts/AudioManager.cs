using UnityEngine;

public class AudioManager : MonoSingleton<AudioManager>
{
    [SerializeField] AudioSource _mainAudioSource;

    void Awake()
    {
        if (_mainAudioSource == null)
        {
            _mainAudioSource = FindFirstObjectByType<AudioSource>();
        }
    }

    public void PlaySFXOneShot(AudioClip audio)
    {
        _mainAudioSource.PlayOneShot(audio);
    }
    public void PlayBGM(AudioClip audio)
    {
        _mainAudioSource.clip = audio;
        _mainAudioSource.Play();
    }
}
