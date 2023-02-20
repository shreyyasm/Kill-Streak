using UnityEngine;
using UnityEngine.InputSystem;

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
    private bool IsReloading;

    private void Awake()
    {
        
    }
    private void Update()
    {
        if(GunSelector.ActiveGun != null)
        {
            GunSelector.ActiveGun.Tick(Mouse.current.leftButton.isPressed);
        }
        //GunSelector.ActiveGun.Tick(
        //    !IsReloading
        //    && Application.isFocused && Mouse.current.leftButton.isPressed
        //    && GunSelector.ActiveGun != null
        //);

        if (ShouldManualReload() || ShouldAutoReload())
        {
            GunSelector.ActiveGun.StartReloading();
            IsReloading = true;
            PlayerAnimator.SetTrigger("Reload");
            //InverseKinematics.HandIKAmount = 0.25f;
            //InverseKinematics.ElbowIKAmount = 0.25f;
        }
    }

    private bool ShouldManualReload()
    {
        return !IsReloading
            && Keyboard.current.rKey.wasReleasedThisFrame
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
}