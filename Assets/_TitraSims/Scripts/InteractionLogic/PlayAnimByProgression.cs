using Config;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class PlayAnimByProgression : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator        _animator;
    [SerializeField] private AnimationConfig _animationConfig;
    [SerializeField] private AnimationClip   _animationClip;

    [Header("Progression")]
    [SerializeField] private float progressionIncrement = 1f;
    [SerializeField] private int loopCountNeeded = 4;
    [SerializeField] private float lerpStep = 1;
    [SerializeField] UnityEvent OnComplete;
    
    private AnimatorOverrideController _runtimeOverride;
    private float _targetProgression = 0f;
    private float currentProgression = 0;
    private int _loopCount;

    private void OnEnable()
    {
        if (_animator == null || _animationConfig?.GenericAnimController == null)
        {
            return;
        }
        
        _runtimeOverride = new AnimatorOverrideController(_animationConfig.GenericAnimController);
        _animator.runtimeAnimatorController = _runtimeOverride;
        
        if (_animationClip != null)
        {
            _runtimeOverride[_animationConfig.GetCustomAnimGenericClipEntryName()] = _animationClip;
        }
    }

    private void Update()
    {
        PlayAnimation();
    }

    private void PlayAnimation()
    {
        if (_animator == null)
        {
            return;
        }
        if(_targetProgression - currentProgression <= 0.01f)
        {
            return;
        }

        var targetStateName = _animationConfig.GetCustomAnimEntryStateName();
        if (_runtimeOverride[targetStateName] == null)
        {
            _runtimeOverride[targetStateName] = _animationClip;
        }
        currentProgression = Mathf.Lerp(currentProgression, _targetProgression, lerpStep * Time.deltaTime);
        _animator.Play(targetStateName, 0, (currentProgression % 100) / 100f);
        
        
        if (currentProgression >= 100f)
        {
            _loopCount++;
            if (_loopCount >= loopCountNeeded)
            {
                enabled = false;
            }
            // _targetProgression = 0;
            // currentProgression = 0;
        }
    }

    public void DriveAnimator()
    {
        _targetProgression += progressionIncrement;
    }

    private void OnDisable()
    {
        _animator.runtimeAnimatorController = null;
    }
}