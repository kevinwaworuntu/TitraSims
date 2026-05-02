using UnityEngine;

namespace Gameplay
{
    public class TimbanganAnimNotify : MonoBehaviour
    {
        [SerializeField] private GameObject objectToActivate;

        private void OnDisable() =>  objectToActivate.SetActive(false);
        public void SetObjectToActivate() => objectToActivate.SetActive(true);
    }
}