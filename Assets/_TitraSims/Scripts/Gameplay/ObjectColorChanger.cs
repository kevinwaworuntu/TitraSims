using System.Diagnostics;
using UnityEngine;

namespace Gameplay
{
    public class ObjectColorChanger : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Renderer targetRenderer;

        [Header("Colors")]
        [SerializeField] private Color colorBefore = Color.white;
        [SerializeField] private Color colorAfter = Color.red;

        [Header("Transition")]
        [SerializeField] private float transitionDuration = 0.5f;
        [SerializeField] private GameObject snapArea;

        private Coroutine colorCoroutine;

        private void Awake()
        {
            if (targetRenderer)
                targetRenderer.material.color = colorBefore;
        }

        public void ChangeColor()
        {
            if (!targetRenderer || !snapArea || !snapArea.activeInHierarchy)
                return;

            if (colorCoroutine != null)
                StopCoroutine(colorCoroutine);

            colorCoroutine = StartCoroutine(LerpColor(targetRenderer.material.color, colorAfter, transitionDuration));
            UnityEngine.Debug.Log("WARNA BERUBAH");
        }

        public void ResetColor()
        {
            if (!targetRenderer)
                return;

            if (colorCoroutine != null)
                StopCoroutine(colorCoroutine);

            colorCoroutine = StartCoroutine(LerpColor(targetRenderer.material.color, colorBefore, transitionDuration));
        }

        private System.Collections.IEnumerator LerpColor(Color from, Color to, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                targetRenderer.material.color = Color.Lerp(from, to, elapsed / duration);
                yield return null;
            }
            targetRenderer.material.color = to;
        }
    }
}