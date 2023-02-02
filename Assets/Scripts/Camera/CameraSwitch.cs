using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
//using Unity.Netcode;
using FishNet.Object;
using UnityEngine.Animations.Rigging;
public class CameraSwitch : NetworkBehaviour
{

    [SerializeField] CinemachineVirtualCamera fpsCamera;
    [SerializeField] GameObject FPSplayer;
    [SerializeField] Transform[] playerGears;

    private GameObject m_FollowCamera;
    private GameObject m_AimCamera;

    public bool cameraSwitched;
    bool inFPSMode = false;

    Camera MainCamera;
    ThirdPersonController thirdPersonController;
    ShooterController shooterController;


    private void Awake()
    {
        m_FollowCamera = GameObject.FindWithTag("Follow Camera");
        m_AimCamera = GameObject.FindWithTag("Aim Camera");
        MainCamera = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();       
        thirdPersonController = GetComponent<ThirdPersonController>();
        shooterController = GetComponent<ShooterController>();
        cameraSwitched = true;
        
    }
    void Update()
    {  
        if (!base.IsOwner)
            return;

        if (!inFPSMode)
            fpsCamera.transform.rotation = MainCamera.transform.rotation;

        foreach (Transform gears in playerGears)
        {
            var childGameObjects = gears.GetComponentsInChildren<Transform>();
            foreach (Transform allObjects in childGameObjects)
            {
                allObjects.gameObject.layer = LayerMask.NameToLayer("HideItself");
            }          
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            if (cameraSwitched)
            {
                //SwitchMode
                inFPSMode = true;
                shooterController.FPSModeCheck(cameraSwitched);

                MainCamera.cullingMask = LayerMask.GetMask("Default", "TransparentFX", "Ignore Raycast", "PostProcessing", "Water", "UI", "Player", "Ground and Walls", "PhysicalAmmo", "FirstPersonWeapon", "Projectile", "OtherPlayers");               
                fpsCamera.enabled = true;
                FPSplayer.SetActive(true);
                m_FollowCamera.SetActive(false);
                m_AimCamera.SetActive(false);

                cameraSwitched = false;
            }
            else
            {
                //SwitchMode
                inFPSMode = false;
                shooterController.FPSModeCheck(cameraSwitched);

                MainCamera.cullingMask = LayerMask.GetMask("Default", "TransparentFX", "Ignore Raycast", "HideItself", "PostProcessing", "Water", "UI", "Player", "Ground and Walls", "PhysicalAmmo", "FirstPersonWeapon", "Projectile", "OtherPlayers");

                fpsCamera.enabled = false;
                FPSplayer.SetActive(false);
                m_FollowCamera.SetActive(true);
                m_AimCamera.SetActive(true);

                cameraSwitched = true;
            }
        }
              
    }

}
