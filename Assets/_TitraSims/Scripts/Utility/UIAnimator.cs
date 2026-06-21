using DG.Tweening;
using UnityEngine;

public static class UIAnimator
{
    public enum SlideDirection { Left, Right, Up, Down }

    // ── Scale ──────────────────────────────────────────────────────────────────

    public static Tween PunchScale(RectTransform rt, float punch = 0.25f, float duration = 0.35f, int vibrato = 5, float elasticity = 0.5f)
        => rt.DOPunchScale(Vector3.one * punch, duration, vibrato, elasticity).SetUpdate(true);

    public static Tween PopIn(RectTransform rt, float duration = 0.4f, Ease ease = Ease.OutBack)
    {
        rt.localScale = Vector3.zero;
        return rt.DOScale(1f, duration).SetEase(ease).SetUpdate(true);
    }

    public static Tween PopOut(RectTransform rt, float duration = 0.25f, Ease ease = Ease.InBack)
        => rt.DOScale(0f, duration).SetEase(ease).SetUpdate(true);

    public static Sequence SquashAndStretch(RectTransform rt, float duration = 0.45f)
        => DOTween.Sequence().SetUpdate(true)
            .Append(rt.DOScale(new Vector3(1.25f, 0.75f, 1f), duration * 0.25f).SetEase(Ease.OutQuad))
            .Append(rt.DOScale(new Vector3(0.85f, 1.25f, 1f), duration * 0.3f).SetEase(Ease.OutQuad))
            .Append(rt.DOScale(Vector3.one, duration * 0.45f).SetEase(Ease.OutElastic));

    public static Sequence Pulse(RectTransform rt, float scale = 1.08f, float halfDuration = 0.4f)
        => DOTween.Sequence().SetUpdate(true)
            .Append(rt.DOScale(scale, halfDuration).SetEase(Ease.InOutSine))
            .Append(rt.DOScale(1f, halfDuration).SetEase(Ease.InOutSine))
            .SetLoops(-1);

    public static Sequence ButtonPulse(RectTransform rt, int loopCount = -1, float scale = 1.06f, float inhaleDuration = 1.2f, float exhaleDuration = 1.5f)
    {
        var originalScale = rt.localScale;
        return DOTween.Sequence().SetUpdate(true)
            .Append(rt.DOScale(originalScale * scale, inhaleDuration).SetEase(Ease.InOutSine))
            .Append(rt.DOScale(originalScale, exhaleDuration).SetEase(Ease.InOutSine))
            .SetLoops(loopCount);
    }
        

    public static Tween StopPulse(RectTransform rt, float duration = 0.12f)
    {
        rt.DOKill();
        return rt.DOScale(1f, duration).SetEase(Ease.OutQuad).SetUpdate(true);
    }

    // ── Fade ───────────────────────────────────────────────────────────────────

    public static Tween FadeIn(CanvasGroup cg, float duration = 0.3f, Ease ease = Ease.OutQuad)
    {
        cg.alpha = 0f;
        return cg.DOFade(1f, duration).SetEase(ease).SetUpdate(true);
    }

    public static Tween FadeOut(CanvasGroup cg, float duration = 0.25f, Ease ease = Ease.InQuad)
        => cg.DOFade(0f, duration).SetEase(ease).SetUpdate(true);

    // ── Panel ──────────────────────────────────────────────────────────────────

    public static Sequence ShowPanel(RectTransform rt, CanvasGroup cg, SlideDirection from = SlideDirection.Down, float slideDist = 40f, float duration = 0.4f)
    {
        Vector2 origin = rt.anchoredPosition;
        rt.anchoredPosition = origin + ToVector(from) * slideDist;
        cg.alpha = 0f;
        return DOTween.Sequence().SetUpdate(true)
            .Join(rt.DOAnchorPos(origin, duration).SetEase(Ease.OutCubic))
            .Join(cg.DOFade(1f, duration * 0.7f).SetEase(Ease.OutQuad));
    }

    public static Sequence HidePanel(RectTransform rt, CanvasGroup cg, SlideDirection to = SlideDirection.Down, float slideDist = 40f, float duration = 0.3f)
        => DOTween.Sequence().SetUpdate(true)
            .Join(rt.DOAnchorPos(rt.anchoredPosition + ToVector(to) * slideDist, duration).SetEase(Ease.InCubic))
            .Join(cg.DOFade(0f, duration * 0.7f).SetEase(Ease.InQuad));

    // ── Feedback ───────────────────────────────────────────────────────────────

    public static Tween Shake(RectTransform rt, float strength = 25f, float duration = 0.4f, int vibrato = 20)
        => rt.DOShakeAnchorPos(duration, strength, vibrato).SetUpdate(true);

    public static Sequence ButtonPress(RectTransform rt, float pressDuration = 0.08f, float releaseDuration = 0.18f)
    {
        Vector3 originalScale = rt.localScale;
        return DOTween.Sequence().SetUpdate(true)
            .Append(rt.DOScale(originalScale * 0.88f, pressDuration).SetEase(Ease.InQuad))
            .Append(rt.DOScale(originalScale, releaseDuration).SetEase(Ease.OutElastic));
    }

    public static Sequence Wobble(RectTransform rt, int loopCount = -1, float interval = 0.5f, float angle = 8f, float duration = 0.5f)
        => DOTween.Sequence().SetUpdate(true)
            .Append(rt.DORotate(new Vector3(0f, 0f, angle), duration * 0.25f).SetEase(Ease.OutQuad))
            .Append(rt.DORotate(new Vector3(0f, 0f, -angle), duration * 0.5f).SetEase(Ease.InOutQuad))
            .Append(rt.DORotate(Vector3.zero, duration * 0.25f).SetEase(Ease.InQuad))
            .AppendInterval(interval)
            .SetLoops(loopCount);

    public static Tween StopWobble(RectTransform rt, float duration = 0.15f)
    {
        rt.DOKill();
        return rt.DORotate(Vector3.zero, duration).SetEase(Ease.OutQuad).SetUpdate(true);
    }

    // ── List ───────────────────────────────────────────────────────────────────

    public static Sequence DrawList(Transform container, float stagger = 0.06f, float perItemDuration = 0.3f, System.Action onItemAppear = null)
    {
        var seq = DOTween.Sequence().SetUpdate(true);
        int index = 0;
        for (int i = 0; i < container.childCount; i++)
        {
            var child = container.GetChild(i) as RectTransform;
            if (child == null || !child.gameObject.activeSelf) continue;
            child.localScale = Vector3.zero;
            float t = index++ * stagger;
            seq.Insert(t, child.DOScale(Vector3.one, perItemDuration).SetEase(Ease.OutBack));
            if (onItemAppear != null) seq.InsertCallback(t, () => onItemAppear());
        }
        return seq;
    }

    // ── Helpers ────────────────────────────────────────────────────────────────

    private static Vector2 ToVector(SlideDirection dir) => dir switch
    {
        SlideDirection.Left  => Vector2.left,
        SlideDirection.Right => Vector2.right,
        SlideDirection.Up    => Vector2.up,
        SlideDirection.Down  => Vector2.down,
        _                    => Vector2.zero,
    };
}
