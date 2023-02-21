using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerGunSelector : MonoBehaviour
{
    [SerializeField]
    private GunType PrimaryGun;

    [SerializeField]
    private GunType SecondaryGun;

    [SerializeField]
    private Transform GunParent;

    public List<GunScriptableObject> Guns;

    [SerializeField]
    private PlayerIK InverseKinematics;

    [Space]
    [Header("Runtime Filled")]
    public GunScriptableObject ActiveGun;

    [SerializeField]
    private WeaponSwitching weaponSwitching;

    int gunSelected;
    GunScriptableObject gun1;
    GunScriptableObject gun2;

    private void Start()
    {
        gun1 = Guns.Find(gun => gun.Type == PrimaryGun);
        gun2 = Guns.Find(gun => gun.Type == SecondaryGun);
        if (gun1 == null)
        {
            Debug.Log($"No GunscriptableObject found for GunType: {gun1}");
            return;
        }

       
        gun1.Spawn(GunParent, this);
        gun2.Spawn(GunParent, this);
        

        // some magic for IK
        Transform[] allChildren = GunParent.GetComponentsInChildren<Transform>();
        InverseKinematics.LeftElbowIKTarget = allChildren.FirstOrDefault(child => child.name == "LeftElbow");
        InverseKinematics.RightElbowIKTarget = allChildren.FirstOrDefault(child => child.name == "RightElbow");
        InverseKinematics.LeftHandIKTarget = allChildren.FirstOrDefault(child => child.name == "LeftHand");
        InverseKinematics.RightHandIKTarget = allChildren.FirstOrDefault(child => child.name == "RightHand");
    }
    private void Update()
    {
        gunSelected = weaponSwitching.selectedWeapon;
        if (gunSelected == 0)
        {
            if(!weaponSwitching.gunChanging)
                ActiveGun = gun1;
        }
        else
        {
            if (!weaponSwitching.gunChanging)
                ActiveGun = gun2;
        }
    }
}
