using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using StarterAssets;
using UnityEngine.InputSystem;
using FishNet.Object;
using UnityEngine.Animations.Rigging;
using FishNet;

[DisallowMultipleComponent]
public class PlayerAction : MonoBehaviour
{
    [SerializeField]
    private PlayerGunSelector GunSelector;
    [SerializeField]
    private bool AutoReload = true;
    [SerializeField]
    private PlayerIK InverseKinematics;
    [SerializeField]
    private Animator PlayerAnimator;
    [SerializeField]
    private ThirdPersonController thirdPersonController;
    [SerializeField]
    private ShooterController shooterController;
    [SerializeField]
    private WeaponSwitching weaponSwitch;
    public bool IsReloading;
    private bool IsShooting;

    //Camera's Reference
    public GameObject fpsVirtualCamera;
    GameObject aimVirtualCamera;
    GameObject followVirtualCamera;

    private void Awake()
    {
        aimVirtualCamera = GameObject.FindWithTag("Aim Camera");
        followVirtualCamera = GameObject.FindWithTag("Follow Camera");
    }
    private void Update()
    {
        if (GunSelector.ActiveGun != null)
        {
            GunSelector.ActiveGun.Tick(IsShooting);
        }
        //GunSelector.ActiveGun.Tick(
        //    !IsReloading
        //    && Application.isFocused && Mouse.current.leftButton.isPressed
        //    && GunSelector.ActiveGun != null
        //);
        if(!IsReloading)
            PlayerAnimator.SetLayerWeight(6, 0);

        if (weaponSwitch.gunChanging)
            IsReloading = false;

        if (ShouldAutoReload())
        {
            if (weaponSwitch.gunChanging)
                return;

            PlayerAnimator.SetLayerWeight(6, 1);
            GunSelector.ActiveGun.StartReloading();
            IsReloading = true;
            PlayerAnimator.SetTrigger("Reload");
            //InverseKinematics.HandIKAmount = 0.25f;
            //InverseKinematics.ElbowIKAmount = 0.25f;
        }
        thirdPersonController.ReloadCheck(IsReloading);
       
    }
    public void ManualReload()
    {
        if(ShouldManualReload())
        {
            
            if (weaponSwitch.gunChanging)
                return;
            PlayerAnimator.SetLayerWeight(6, 1);
            GunSelector.ActiveGun.StartReloading();
            IsReloading = true;
            PlayerAnimator.SetTrigger("Reload");
        }
        
    }
    public bool ShouldManualReload()
    {
        return !IsReloading            
            && GunSelector.ActiveGun.CanReload();
    }

    private bool ShouldAutoReload()
    {
        return !IsReloading
            && AutoReload
            && GunSelector.ActiveGun.AmmoConfig.CurrentClipAmmo == 0
            && GunSelector.ActiveGun.CanReload();
    }

    private void EndReload()
    {
        GunSelector.ActiveGun.EndReload();
        //InverseKinematics.HandIKAmount = 1f;
        //InverseKinematics.ElbowIKAmount = 1f;
        IsReloading = false;
    }
    public void Shoot(float input)
    {
        if (GunSelector.ActiveGun.AmmoConfig.CurrentClipAmmo > 0)
            GunSelector.ActiveGun.FireCheck();
        if (!shooterController.changingGun)
        {
            if (input == 1)
            {
                IsShooting = true;
                thirdPersonController.ShotFired(true);
                thirdPersonController.FiringContinous(true);
                //InvokeRepeating("CameraShake", 0, GunSelector.ActiveGun.ShootConfig.FireRate);
            }
            else
            {
                IsShooting = false;
                thirdPersonController.FiringContinous(false);
                //CancelInvoke("CameraShake");
            }
        }
    }
    public void CameraShake()
    {
        if (GunSelector.ActiveGun.AmmoConfig.CurrentClipAmmo == 0)
            return;
        
            followVirtualCamera.GetComponent<CinemachineShake>().ShakeCamera(1f, 0.1f);
            aimVirtualCamera.GetComponent<CinemachineShake>().ShakeCamera(1f, 0.1f);
            fpsVirtualCamera.GetComponent<CinemachineShake>().ShakeCamera(1f, 0.1f);
     
    }

}