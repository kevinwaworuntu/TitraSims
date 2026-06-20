using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace TitraSims.Utility
{
    public class TouchDebugCursor : MonoBehaviour
    {
        [SerializeField] private GameObject cursorPrefab;
        [SerializeField] private bool enableInEditor = true;

        private readonly Dictionary<int, RectTransform> _cursors = new();

        void OnEnable()
        {
            EnhancedTouchSupport.Enable();
        }

        void OnDisable()
        {
            EnhancedTouchSupport.Disable();
            foreach (var rt in _cursors.Values)
                if (rt != null) Destroy(rt.gameObject);
            _cursors.Clear();
        }

        void Update()
        {
            HandleTouches();

#if UNITY_EDITOR
            if (enableInEditor)
                HandleMouseAsTouch();
#endif
        }

        void HandleTouches()
        {
            foreach (var touch in Touch.activeTouches)
            {
                int id = touch.finger.index;
                var phase = touch.phase;
                Vector2 pos = touch.screenPosition;

                if (phase == UnityEngine.InputSystem.TouchPhase.Began)
                    SpawnCursor(id, pos);

                if (_cursors.TryGetValue(id, out var rt))
                    rt.position = pos;

                if (phase == UnityEngine.InputSystem.TouchPhase.Ended ||
                    phase == UnityEngine.InputSystem.TouchPhase.Canceled)
                    RemoveCursor(id);
            }
        }

#if UNITY_EDITOR
        void HandleMouseAsTouch()
        {
            const int mouseId = -1;
            var mouse = Mouse.current;
            if (mouse == null) return;

            Vector2 pos = mouse.position.ReadValue();

            if (mouse.leftButton.wasPressedThisFrame)
                SpawnCursor(mouseId, pos);

            if (mouse.leftButton.isPressed && _cursors.TryGetValue(mouseId, out var rt))
                rt.position = pos;

            if (mouse.leftButton.wasReleasedThisFrame)
                RemoveCursor(mouseId);
        }
#endif

        void SpawnCursor(int id, Vector2 position)
        {
            if (_cursors.ContainsKey(id)) return;
            var go = Instantiate(cursorPrefab, transform);
            var rt = go.GetComponent<RectTransform>();
            rt.position = position;
            _cursors[id] = rt;
        }

        void RemoveCursor(int id)
        {
            if (_cursors.TryGetValue(id, out var rt))
            {
                Destroy(rt.gameObject);
                _cursors.Remove(id);
            }
        }
    }
}