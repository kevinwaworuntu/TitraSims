using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI
{
    public class ContextualButton : MonoBehaviour
    {
        private Button button;
        private TextMeshProUGUI text;
        private RectTransform rt;
        private Action currentActionCached;

        private void Awake()
        {
            button = GetComponent<Button>();
            text = GetComponentInChildren<TextMeshProUGUI>();
            rt = GetComponent<RectTransform>();
            if (button)
            {
                button.onClick.AddListener(InvokeCurrentAction);
            }
        }

        private void OnDisable()
        {
            ClearAction();
            ClearText();
        }

        public void RegisterText(string value)
        {
            if(text) text.SetText(value);
        }
        
        public void ClearText()
        {
            if(text) text.SetText(String.Empty);
        }
        
        public void RegisterAction(Action action)
        {
            currentActionCached = action;
        }

        public void ClearAction()
        {
            currentActionCached = null;
        }
        
        private void InvokeCurrentAction()
        {
            UIAnimator.ButtonPress(rt).OnComplete(() => currentActionCached?.Invoke());
        }

        public void SetEnabled(bool enabled)
        {
            button.interactable = enabled;
        }
    }
}