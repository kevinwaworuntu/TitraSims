using Data;
using UnityEngine;

public class SetCairanErlenmeyer : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Renderer targetRenderer;

    [Header("Colors")]
    [SerializeField] private Color colorAfter;

    private Material titrasiMatInstance;
    private static readonly int SideColorID = Shader.PropertyToID("_Side_Color");
    private static readonly int TopColorID = Shader.PropertyToID("_TopColor");
    private static readonly int FillID = Shader.PropertyToID("_Fill");
    private static readonly int AlphaID = Shader.PropertyToID("_Alpha");

    private void Awake()
    {
        if (!targetRenderer)
            return;

        titrasiMatInstance = targetRenderer.material;
    }

    public void SetCairanErlen(Color colorInput)
    {
        titrasiMatInstance.SetColor(SideColorID, colorInput);
        titrasiMatInstance.SetColor(TopColorID, colorInput);
    }

    public void SetFill(float fillValue)
    {
        titrasiMatInstance.SetFloat(FillID, fillValue);
    }

    public void SetAlpha(float alphaValue)
    {
        titrasiMatInstance.SetFloat(AlphaID, alphaValue);
    }

    public void SetCairanErlenFromConfig(CairanErlenmeyerConfig config)
    {
        if (!config) return;
        SetCairanErlen(config.ColorValue);
        //SetFill(config.FillValue);
    }
}
