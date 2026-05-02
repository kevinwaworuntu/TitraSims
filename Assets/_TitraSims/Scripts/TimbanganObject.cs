using System;
using System.Collections;
using UnityEngine;
using TMPro;

public class TimbanganObject : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI displayText;
    private float currentWeight;

    private void OnEnable()
    {
        SetWeight(0);
    }

    public void SetWeight(float weight)
    {
        currentWeight = weight;
        displayText.SetText(currentWeight.ToString());
    }
    
    public float GetCurrentWeight()
    {
        return currentWeight;
    }

    public void IncreaseWeight(float weight)
    {
        if (displayText == null)
        {
            return;
        }
        float start = currentWeight;
        float target = currentWeight + weight;
        StartCoroutine(LerpValue(start, target, 0.1f, value =>
        {
            currentWeight = value;
            displayText.SetText($"{currentWeight:F1}");
        })); // ToDo : Make duration listen to anim length
    }

    public void DecreaseWeight(float weight)
    {
        if (displayText == null)
        {
            return;
        }
        float start = currentWeight;
        float target = currentWeight - weight >= 0 ? currentWeight - weight : 0;

        StartCoroutine(LerpValue(start, target, 0.1f, value =>
        {
            currentWeight = value;
            displayText.SetText($"{currentWeight:F1}");
        }));  // ToDo : Make duration listen to anim length
    }
    
    private IEnumerator LerpValue(float start, float end, float duration, Action<float> onValueChanged)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float interpValue = elapsed / duration;
            float value = Mathf.Lerp(start, end, interpValue);

            onValueChanged?.Invoke(value);
            yield return null;
        }
        onValueChanged?.Invoke(end);
    }
}
