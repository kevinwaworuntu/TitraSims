using System.Diagnostics;
using UnityEngine;

namespace Gameplay
{
    public class ObjectColorChanger : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Renderer targetRenderer;

        [Header("Colors")]
        [SerializeField] private Color colorBefore;
        [SerializeField] private Color colorAfter;
        [SerializeField] private GameObject cairanPink;

        [Header("Transition")]
        [SerializeField] private float transitionDuration = 0.5f;
        [SerializeField] private GameObject snapArea;
        private Material titrasiMatInstance;
        private static readonly int SideColorID = Shader.PropertyToID("_Side_Color");
        private static readonly int TopColorID = Shader.PropertyToID("_TopColor");

        private Coroutine colorCoroutine;

        private void Awake()
        {
            UnityEngine.Debug.Log($"Awake dipanggil di {gameObject.name}. targetRenderer: {targetRenderer}");

            if (!targetRenderer)
            {
                UnityEngine.Debug.LogWarning($"targetRenderer NULL di {gameObject.name}!");
                return;
            }

            titrasiMatInstance = targetRenderer.material;
            UnityEngine.Debug.Log($"Material: {titrasiMatInstance.name}, Shader: {titrasiMatInstance.shader.name}");
            UnityEngine.Debug.Log($"HasProperty _Side_Color: {titrasiMatInstance.HasProperty(SideColorID)}, HasProperty _TopColor: {titrasiMatInstance.HasProperty(TopColorID)}");

            titrasiMatInstance.SetColor(SideColorID, colorBefore);
            titrasiMatInstance.SetColor(TopColorID, colorBefore);
            UnityEngine.Debug.Log("WARNA AWAL");
        }

        public void SetCairanPink(GameObject cairanPink)
        { 
            this.cairanPink = cairanPink;   
        }
        
        public void SetCairanPinkVisibility(bool isVisible)
        {
            cairanPink.SetActive(isVisible);
        }
        
        public void ChangeColor()
        {
            if (!targetRenderer || !snapArea || !snapArea.activeInHierarchy)
                return;

            if (colorCoroutine != null)
                StopCoroutine(colorCoroutine);

            colorCoroutine = StartCoroutine(LerpColor(titrasiMatInstance.GetColor(SideColorID), colorAfter, transitionDuration));
            colorCoroutine = StartCoroutine(LerpColor(titrasiMatInstance.GetColor(TopColorID), colorAfter, transitionDuration));
            cairanPink.SetActive(true);
            UnityEngine.Debug.Log("WARNA BERUBAH");
        }

        public void ResetColor()
        {
            if (!targetRenderer)
                return;

            if (colorCoroutine != null)
                StopCoroutine(colorCoroutine);

            colorCoroutine = StartCoroutine(LerpColor(titrasiMatInstance.GetColor(SideColorID), colorBefore, transitionDuration));
            colorCoroutine = StartCoroutine(LerpColor(titrasiMatInstance.GetColor(TopColorID), colorBefore, transitionDuration));
        }

        private System.Collections.IEnumerator LerpColor(Color from, Color to, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                Color current = Color.Lerp(from, to, elapsed / duration);
                titrasiMatInstance.SetColor(SideColorID, current);
                titrasiMatInstance.SetColor(TopColorID, current);
                yield return null;
            }
            titrasiMatInstance.SetColor(SideColorID, to);
            titrasiMatInstance.SetColor(TopColorID, to);
        }
    }
}