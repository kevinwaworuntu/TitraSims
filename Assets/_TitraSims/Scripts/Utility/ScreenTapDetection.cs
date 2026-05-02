using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Utility
{
    public class ScreenTapDetection : MonoBehaviour
    {
        public static ScreenTapDetection Instance { get; private set; }
        public event Action OnScreenTappedDelegate;

        private InputAction tapAction;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            tapAction = new InputAction("Tap", binding: "<Pointer>/press");  // Unified mouse + touch press
        }

        private void OnEnable()
        {
            tapAction.Enable();
            tapAction.performed += HandleTap;
        }

        private void OnDisable()
        {
            tapAction.performed -= HandleTap;
            tapAction.Disable();
        }

        private void HandleTap(InputAction.CallbackContext ctx)
        {
            Pointer pointer = Pointer.current;
            if (pointer == null)
            {
                return;
                
            }
            int pointerId = pointer.deviceId;
            if (EventSystem.current != null &&  EventSystem.current.IsPointerOverGameObject(pointerId))
            {
                return;
            }
            OnScreenTappedDelegate?.Invoke();
        }
    }
}