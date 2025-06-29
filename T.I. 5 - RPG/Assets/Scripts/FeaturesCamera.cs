using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.InputSystem;

public class FeaturesCamera : MonoBehaviour
{
    public CinemachineSplineCart cart;
    private void Start()
    {
        cart = this.GetComponent<CinemachineSplineCart>();
        cart.SplinePosition = 0;
    }

    public void SwitchRightInput(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started)
        {
            if(cart.SplinePosition < cart.Spline.Spline.Count - 2)
            {
                cart.SplinePosition++;
            }
            else
            {
                cart.SplinePosition = 0;
            }
        }
    }
    public void SwitchLeftInput(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started)
        {
            if(cart.SplinePosition > 0)
            {
                cart.SplinePosition--;
            }
            else
            {
                cart.SplinePosition = cart.Spline.Spline.Count - 2;
            }
        }
    }
}
