using UnityEngine;
using UnityEngine.Rendering.Universal;

public class SignLightController : MonoBehaviour
{
    [SerializeField] private Light2D arrowLight;

    public void EnableArrowLight()
    {
        arrowLight.enabled = true;
    }
    public void DisableArrowLight()
    {
        arrowLight.enabled = false;
    }
}
