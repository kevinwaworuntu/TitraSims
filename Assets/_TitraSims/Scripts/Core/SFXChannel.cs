using System;
using Data;
using UnityEngine;
using Random = UnityEngine.Random;

public class SFXChannel
{
    private readonly AudioSource _source;
    private readonly SoundBank _bank;
    private float _volume;

    public float Volume => _volume;

    public SFXChannel(AudioSource source, SoundBank bank, float volume = 1f)
    {
        _source = source;
        _bank = bank;
        _volume = Mathf.Clamp01(volume);
        _source.volume = _volume;
    }

    public void Play(AudioClip clip)
    {
        if (clip == null) return;
        _source.PlayOneShot(clip, _volume);
    }

    public void Play(string key)     => Play(_bank?.GetClip(key));
    public void Play(SoundType type) => Play(_bank?.GetClip(type));
    public bool PlayNonInterrupt(SoundType type)
    {
        if (_source.isPlaying)
        {
            return false;
        }
        Play(_bank?.GetClip(type));
        return true;
    }

    public AudioClip GetRandomClipInType(SoundType type)
    {
        var clips = _bank?.GetClips(type);
        if (clips == null || clips.Length == 0)
        {
            return null;
        }

        return clips[Random.Range(0, clips.Length)];
    }
    
    public void Stop() => _source.Stop();

    public void SetVolume(float volume)
    {
        _volume = Mathf.Clamp01(volume);
        _source.volume = _volume;
    }
}