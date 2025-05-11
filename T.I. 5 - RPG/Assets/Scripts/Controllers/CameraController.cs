using UnityEngine;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine.InputSystem;
using Unity.Mathematics;
public class CameraController : MonoBehaviour
{
    //CinemachineBrain controller;
    public List<CinemachineCamera> cameras = new List<CinemachineCamera>();
    [SerializeField]int activeCamIndex;
    public static CameraController instance;
    public CinemachineCamera highlightCardCamera;

    private void Awake()
    {
        //controller = this.GetComponent<CinemachineBrain>();
        instance = this;
    }

    private void Start()
    {
        ChangeActiveCamera();
    }
    public void ChangeCamera(int Index)
    {
        Index = math.clamp(Index, 0, cameras.Count - 1);
        activeCamIndex = Index;
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

    public void HighlightCard(Vector3 pos)
    {
        if(highlightCardCamera.Priority == 0)
        {
            highlightCardCamera.transform.position = new Vector3(pos.x, highlightCardCamera.transform.position.y, pos.z);
            highlightCardCamera.Priority = 2;
        }
        else
        {
            highlightCardCamera.Priority = 0;
        }
    }
}
