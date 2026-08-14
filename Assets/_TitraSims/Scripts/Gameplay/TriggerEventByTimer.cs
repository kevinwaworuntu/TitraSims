using System;
using UnityEngine;
using UnityEngine.Events;

namespace Gameplay
{
    public class TriggerEventByTimer : MonoBehaviour
    {
        [SerializeField] private float timer = 5;
        [SerializeField] private UnityEvent OnTimerEndAction;
        
        private float _timer = 10;
        private bool _isStartCounting;

        private void Update()
        {
            if( !_isStartCounting) return;
            
            _timer += Time.deltaTime;
            if (_timer >= timer)
            {
                _isStartCounting = false;
                OnTimerEndAction?.Invoke();
            }
        }
        
        public void StartCounting()
        {
            _isStartCounting = true;
            _timer = 0;
        }
    }
}