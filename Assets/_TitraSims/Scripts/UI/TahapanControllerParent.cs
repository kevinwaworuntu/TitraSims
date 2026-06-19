using UnityEngine;

namespace UI
{
    public class TahapanControllerParent : MonoBehaviour
    {
        protected void OnEnable()
        {
            UIAnimator.DrawList(transform);
        }
    }
}