using UnityEngine;
//using Unity.Netcode;
using Cinemachine;
using UnityEngine.Assertions;

public class CameraFollow : MonoBehaviour
{
    private CinemachineVirtualCamera m_MainCamera;
    private CinemachineVirtualCamera m_AimCamera;


    void Start()
    {
        AttachCamera();
    }
    private void Update()
    {
        AttachCamera();
    }

    private void AttachCamera()
    {
        m_MainCamera = GameObject.FindWithTag("Follow Camera").GetComponent<CinemachineVirtualCamera>();
        m_AimCamera = GameObject.FindWithTag("Aim Camera").GetComponent<CinemachineVirtualCamera>();
        //Assert.IsNotNull(m_MainCamera, "CameraController.AttachCamera: Couldn't find gameplay freelook camera");

        if (m_MainCamera)
        {
            // camera body / aim
            m_MainCamera.Follow = transform;
            m_AimCamera.Follow = transform;
        }

    }


}