using UnityEngine;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine.InputSystem;
public class CameraController : MonoBehaviour
{
    //CinemachineBrain controller;
    public List<CinemachineCamera> cameras = new List<CinemachineCamera>();
    [SerializeField]int activeCamIndex;

    private void Awake()
    {
        //controller = this.GetComponent<CinemachineBrain>();
    }

    private void Start()
    {
        ChangeActiveCamera();
    }

    void ChangeActiveCamera()
    {
        for(int i = 0; i < cameras.Count; i++)
        {
            if(i == activeCamIndex)
            {
                cameras[i].Priority = 1;
            }
            else
            {
                cameras[i].Priority = 0;
            }
        }
    }

    public void SwitchUpInput(InputAction.CallbackContext context)
    {
        if(context.phase == InputActionPhase.Started && activeCamIndex < cameras.Count - 1)
        {
            activeCamIndex++;
            ChangeActiveCamera();
        }
    }
    public void SwitchDownInput(InputAction.CallbackContext context)
    {
        if(context.phase == InputActionPhase.Started && activeCamIndex > 0)
        {
            activeCamIndex--;
            ChangeActiveCamera();
        }
    }
}
