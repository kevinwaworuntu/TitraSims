using System;
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
        private Action currentActionCached;

        private void Awake()
        {
            button = GetComponent<Button>();
            text = GetComponentInChildren<TextMeshProUGUI>();
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
            currentActionCached?.Invoke();
        }

        public void SetEnabled(bool enabled)
        {
            button.interactable = enabled;
        }
    }
}