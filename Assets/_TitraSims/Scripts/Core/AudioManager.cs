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

    public SFXChannel SFX { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        SFX = new SFXChannel(sfxSource, soundBank, sfxVolume);
        narrationSource.volume = narrationVolume;
    }

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