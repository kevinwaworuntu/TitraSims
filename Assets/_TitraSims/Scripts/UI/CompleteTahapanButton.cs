using DG.Tweening;
using UnityEngine;

public class CompleteTahapanButton : MonoBehaviour
{
    private RectTransform rt;
    private Sequence wobble;

    [SerializeField] private float interval = 0.5f;
    [SerializeField] private float angle = 1;
    [SerializeField] private float duration = 0.5f;
    
    private void Awake()
    {
        rt = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        wobble = UIAnimator.Wobble(rt, -1, interval, angle, duration);
    }

    private void OnDisable()
    {
        wobble?.Kill();
        UIAnimator.StopWobble(rt, 0);
    }
}
