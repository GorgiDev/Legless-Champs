using UnityEngine;
using UnityEngine.Rendering.Universal;

public class RouterLightController : MonoBehaviour
{
    [SerializeField] private Light2D greenLight1;
    [SerializeField] private Light2D greenLight2;
    [SerializeField] private Light2D greenLight3;

    #region Green Light 1
    public void EnableGreenLight1()
    {
        greenLight1.enabled = true;
    }
    
    public void DisableGreenLight1()
    {
        greenLight1.enabled = false;
    }
    #endregion
    
    #region Green Light 2
    public void EnableGreenLight2()
    {
        greenLight2.enabled = true;
    }
    
    public void DisableGreenLight2()
    {
        greenLight1.enabled = false;
    }
    #endregion
    
    #region Green Light 3
    public void EnableGreenLight3()
    {
        greenLight3.enabled = true;
    }
    
    public void DisableGreenLight3()
    {
        greenLight3.enabled = false;
    }
    #endregion
}
