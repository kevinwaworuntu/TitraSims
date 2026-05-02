using System;
using UnityEngine;

namespace Gameplay
{
    public class AnimNotify : MonoBehaviour
    {
        public Action OnNotifyHit;

        public void OnNotifyHitHandler()
        {
            OnNotifyHit?.Invoke();
        }
    }
}