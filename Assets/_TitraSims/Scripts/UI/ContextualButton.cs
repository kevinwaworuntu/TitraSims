using System;
using TMPro;
using UnityEngine;

namespace UI
{
    public class ContextualButton : TitraSims_Button
    {
        private TextMeshProUGUI _text;
        private Action _currentAction;

        protected override void Awake()
        {
            base.Awake();
            _text = GetComponentInChildren<TextMeshProUGUI>();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            ClearAction();
            ClearText();
        }

        protected override void OnClick() => _currentAction?.Invoke();

        public void RegisterText(string value)  => _text?.SetText(value);
        public void ClearText()                 => _text?.SetText(String.Empty);
        public void RegisterAction(Action action) => _currentAction = action;
        public void ClearAction()               => _currentAction = null;
        public void SetEnabled(bool enabled)    => interactable = enabled;
    }
}
