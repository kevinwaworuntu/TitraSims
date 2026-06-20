using UnityEngine;

public class SpriteBlinkingAnim : MonoBehaviour
{
    [SerializeField] private float blinkSpeed = 1f;

    private SpriteRenderer spriteRenderer;
    private bool isAscending = true;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (!spriteRenderer) return;

        float target = isAscending ? 1f : 0f;
        float alpha = Mathf.MoveTowards(spriteRenderer.color.a, target, blinkSpeed * Time.deltaTime);

        spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, alpha);

        if (Mathf.Approximately(alpha, target))
            isAscending = !isAscending;
    }
}
