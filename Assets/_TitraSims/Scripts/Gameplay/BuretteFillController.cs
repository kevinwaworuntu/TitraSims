using Data;
using UnityEngine;

namespace Gameplay
{
    public class BuretteFillController : MonoBehaviour
    {
        [Header("Stage Config")]
        [SerializeField] private BuretteStageConfig config;

        [Header("Burette Fill")]
        [SerializeField] private Transform buretteFillObject;
        [SerializeField] private Transform buretteMeniskusObject;

        private float currentValue;

        /// <summary>Total ml applied since the last <see cref="ResetFill"/>.</summary>
        public float CurrentValue => currentValue;

        public BuretteStageConfig Config => config;

        private bool IsReady => config && buretteFillObject && buretteMeniskusObject;

        private void Awake()
        {
            ResetFill();
        }

        /// <summary>Sets the config at runtime, then snaps the visual back to the full state.</summary>
        public void SetConfig(BuretteStageConfig value)
        {
            config = value;
            ResetFill();
        }

        /// <summary>
        /// Drains <paramref name="value"/> ml from the burette, relative to the current level.
        /// Negative values refill.
        /// </summary>
        public void ApplyFillValue(float value)
        {
            if (!IsReady)
                return;

            currentValue += value;
            ApplyVisual(currentValue);
        }

        /// <summary>Sets the drained amount to an absolute <paramref name="value"/> ml from full.</summary>
        public void SetFillValue(float value)
        {
            if (!IsReady)
                return;

            currentValue = value;
            ApplyVisual(currentValue);
        }

        /// <summary>Restores the burette to its initial (full) visual state.</summary>
        public void ResetFill()
        {
            if (!IsReady)
                return;

            currentValue = 0f;
            ApplyVisual(0f);
        }

        private void ApplyVisual(float value)
        {
            if (buretteFillObject)
            {
                var scale = buretteFillObject.localScale;
                scale.z = config.FillObjectInitialScale - config.FillObjectScaleModifier * value;
                buretteFillObject.localScale = scale;
            }

            if (buretteMeniskusObject)
            {
                var pos = buretteMeniskusObject.localPosition;
                pos.y = config.MeniskusInitPos - config.MeniskusPositionModifier * value;
                buretteMeniskusObject.localPosition = pos;
            }
        }

        [ContextMenu("TestSetFill")]
        void TestSetFill()
        {
            SetFillValue(30);
        }
    }
}