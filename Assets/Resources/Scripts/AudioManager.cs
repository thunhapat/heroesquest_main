using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] AudioSource _mainAudioSource;

    #region Singleton
    private static AudioManager _instance;

    public static AudioManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<AudioManager>();
            }
            return _instance;
        }
    }
    #endregion

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
