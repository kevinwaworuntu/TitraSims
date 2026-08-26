using UnityEngine;

namespace InteractionLogic
{
    public class DetachAttachObject : MonoBehaviour
    {
        [SerializeField] private GameObject parent;

        private void OnEnable()
        {
            Detach();
        }

        private void OnDisable()
        {
            Attach();
        }

        public void Attach()
        {
            transform.parent = parent.transform;
        }
        public void Detach()
        {
            transform.parent = null;   
        }
    }
}