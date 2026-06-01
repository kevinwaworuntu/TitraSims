using System;
using UnityEngine;

namespace Animation
{
    public class AnimNotify : MonoBehaviour
    {
        public event Action<string> OnNotify;

        public void Notify(string notifyName)
        {
            OnNotify?.Invoke(notifyName);
        }
    }
}