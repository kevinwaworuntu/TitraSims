using UnityEngine;

public class WaterSplashAnimNotify : MonoBehaviour
{
    [SerializeField] private ParticleSystem particleSystem;
    public void PlayOnLoop()
    {
        if(particleSystem == null)
        {
            return;
        }
        particleSystem.loop = true;
        particleSystem.Play();
    }

    public void Stop()
    {
        if(particleSystem == null)
        {
            return;
        }
        particleSystem.Stop();
    }
}
