using Data;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Sources")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource narrationSource;

    [Header("Sound Bank")]
    [SerializeField] private SoundBank soundBank;

    [Header("Volume")]
    [Range(0f, 1f)] [SerializeField] private float sfxVolume = 1f;
    [Range(0f, 1f)] [SerializeField] private float narrationVolume = 1f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        sfxSource.volume = sfxVolume;
        narrationSource.volume = narrationVolume;
    }

    #region SFX
    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;
        sfxSource.PlayOneShot(clip, sfxVolume);
    }

    public void PlaySFX(string key) => PlaySFX(soundBank?.Get(key));

    public void StopSFX() => sfxSource.Stop();

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        sfxSource.volume = sfxVolume;
    }
    #endregion
    
    #region Narration
    public void PlayNarration(AudioClip clip)
    {
        if (clip == null) return;
        narrationSource.Stop();
        narrationSource.clip = clip;
        narrationSource.Play();
    }

    public void StopNarration() => narrationSource.Stop();

    public bool IsNarrationPlaying => narrationSource.isPlaying;

    public void SetNarrationVolume(float volume)
    {
        narrationVolume = Mathf.Clamp01(volume);
        narrationSource.volume = narrationVolume;
    }
    #endregion
}