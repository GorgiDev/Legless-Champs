using UnityEngine;
using UnityEngine.Rendering.Universal;

public class TurretLightController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Light2D idleLight;
    [SerializeField] private Light2D lockedInLight;

    public void EnableIdleLight()
    {
        idleLight.enabled = true;
    }
    public void DisableIdleLight()
    {
        idleLight.enabled = false;
    }
    public void EnableLockedInLight()
    {
        lockedInLight.enabled = true;
    }
    public void DisableLockedInLight()
    {
        lockedInLight.enabled = false;
    }
}
