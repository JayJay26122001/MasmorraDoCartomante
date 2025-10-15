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
    public CinemachineCamera highlightCardCamera, angledTopCamera, enemyDamagedCamera, shopCamera, zoomedCamera;
    bool inputActive;
    bool blockHighlight;
    public ControlUI upControl, downControl;
    private void Awake()
    {
        //controller = this.GetComponent<CinemachineBrain>();
        instance = this;
    }

    private void Start()
    {
        ActivateAngledTopCamera();
        inputActive = false;
        blockHighlight = false;
    }
    public void ChangeCamera(int Index)
    {
        Index = math.clamp(Index, 0, cameras.Count - 1);
        if (inputActive)
        {
            if (activeCamIndex == 0 && Index != 0)
            {
                GameManager.instance.uiController.ShowCommandPopup(downControl);
            }
            if (activeCamIndex == cameras.Count - 1 && Index != cameras.Count - 1)
            {
                GameManager.instance.uiController.ShowCommandPopup(upControl);
            }
            if (Index == 0)
            {
                GameManager.instance.uiController.HideCommandPopup(downControl);
            }
            if (Index == cameras.Count - 1)
            {
                GameManager.instance.uiController.HideCommandPopup(upControl);
            }
        }
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
        if(inputActive && context.phase == InputActionPhase.Started && activeCamIndex < cameras.Count - 1)
        {
            if (activeCamIndex == 0)
            {
                GameManager.instance.uiController.ShowCommandPopup(downControl);
            }
            activeCamIndex++;
            ChangeActiveCamera();
            if(activeCamIndex == 3)
            {
                GameManager.instance.uiController.ShowEnemyDescription();
                GameManager.instance.uiController.HideCommandPopup(upControl);
                //GameManager.instance.uiController.ActivateEnemyDesc();
            }
        }
    }
    public void SwitchDownInput(InputAction.CallbackContext context)
    {
        if(inputActive && context.phase == InputActionPhase.Started && activeCamIndex > 0)
        {
            if (activeCamIndex == cameras.Count - 1)
            {
                GameManager.instance.uiController.ShowCommandPopup(upControl);
            }
            activeCamIndex--;
            ChangeActiveCamera();
            GameManager.instance.uiController.HideEnemyDesc();
            if (activeCamIndex == 0)
            {
                GameManager.instance.uiController.HideCommandPopup(downControl);
            }
        }
    }

    public void HighlightCard(Vector3 pos)
    {
        if(!blockHighlight)
        {
            if(highlightCardCamera.Priority == 0 )
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

    public void ActivateDamageEnemyCam()
    {
        enemyDamagedCamera.Priority = 2;
    }
    
    public void DeactivateDamageEnemyCam()
    {
        enemyDamagedCamera.Priority = 0;
    }

    public void PositionEnemyDamagedCam(Enemy.EnemySize size)
    {
        switch(size)
        {
            case Enemy.EnemySize.Small:
                enemyDamagedCamera.transform.position = new Vector3(0, 15, -12.5f);
                break;
            case Enemy.EnemySize.Medium:
                enemyDamagedCamera.transform.position = new Vector3(0, 19, -14.5f);
                break;
            case Enemy.EnemySize.Large:
                enemyDamagedCamera.transform.position = new Vector3(0, 23, -16.5f);
                break;
        }
    }

    public void DisableCameraInputs()
    {
        inputActive = false;
        GameManager.instance.uiController.HideCommandPopup(upControl);
        GameManager.instance.uiController.HideCommandPopup(downControl);
    }
    public void EnableCameraInputs()
    {
        inputActive = true;
        if (activeCamIndex != 0)
        {
            GameManager.instance.uiController.ShowCommandPopup(downControl);
        }
        if (activeCamIndex != cameras.Count - 1)
        {
            GameManager.instance.uiController.ShowCommandPopup(upControl);
        }
    }

    public void ActivateAngledTopCamera()
    {
        angledTopCamera.Priority = 2;
    }

    public void DeActivateAngledTopCamera()
    {
        angledTopCamera.Priority = 0;
    }

    public void ActivateZoomedCamera()
    {
        zoomedCamera.Priority = 2;
    }

    public void DeActivateZoomedCamera()
    {
        zoomedCamera.Priority = 0;
    }

    public void ActivateShopCamera()
    {
        shopCamera.Priority = 2;
    }

    public void DeActivateShopCamera()
    {
        shopCamera.Priority = 0;
    }

    public void RemoveHighlight(InputAction.CallbackContext context)
    {
        if(context.phase == InputActionPhase.Canceled)
        {
            if(highlightCardCamera.Priority == 2)
            {
                blockHighlight = true;
                highlightCardCamera.Priority = 0;
                Invoke("UnblockHighlight", 0.1f);
            }
        }
    }

    public void UnblockHighlight()
    {
        blockHighlight = false;
    }
}
