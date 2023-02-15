using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class WeaponSwitching : MonoBehaviour
{
    //Index of selected weapon
    public int selectedWeapon;
    [SerializeField] ShooterController shooterController;
    bool gunChanged = false;
    public bool gunChanging = false;
    [SerializeField] private Rig pistolRig;
    [SerializeField] private Rig rifleRig;
    public GameObject animator;
    // Start is called before the first frame update
    void Start()
    {        
        SelectedWeapon();
    }

    // Update is called once per frame
    void Update()
    {
        if (animator.GetComponent<Animator>().GetCurrentAnimatorStateInfo(4).IsName("Rifle To Pistol Locomotions") && animator.GetComponent<Animator>().GetCurrentAnimatorStateInfo(4).normalizedTime > 1f)
        {
            gunChanging = false;          
        }
        if(gunChanging)
        {
           if(selectedWeapon == 1)
           {
                animator.GetComponent<Animator>().SetLayerWeight(4, 1);
                animator.GetComponent<Animator>().SetBool("Gun Changing", true);
                rifleRig.weight = 0f;
                pistolRig.weight = 0f;
           }
           else
           {
                animator.GetComponent<Animator>().SetLayerWeight(5, 1);
                animator.GetComponent<Animator>().SetBool("Gun Changing", true);
                rifleRig.weight = 0f;
                pistolRig.weight = 0f;
           }
            
        }
        else
        {
            if(selectedWeapon == 1)
            {
                animator.GetComponent<Animator>().SetLayerWeight(4, 0);
                animator.GetComponent<Animator>().SetBool("Gun Changing", false);
                if (selectedWeapon == 1)
                {
                    pistolRig.weight = 1f;
                    rifleRig.weight = 0f;
                }
                else
                {
                    pistolRig.weight = 0f;
                    rifleRig.weight = 1f;
                }

            }
            else
            {
                animator.GetComponent<Animator>().SetLayerWeight(5, 0);
                animator.GetComponent<Animator>().SetBool("Gun Changing", false);
                if (selectedWeapon == 1)
                {
                    pistolRig.weight = 1f;
                    rifleRig.weight = 0f;
                }
                else
                {
                    pistolRig.weight = 0f;
                    rifleRig.weight = 1f;
                }
            }


        }
        GunSwaping();
    }
    void SelectedWeapon()
    {
        int i = 0;
        foreach (Transform weapon in transform)
        {
            if (i == selectedWeapon)
            {
                weapon.gameObject.SetActive(true);
            }
            else
            {
                weapon.gameObject.SetActive(false);
            }

            i++;
        }
    }
    public void ChangeGunIndex()
    {
        gunChanging = true;       
        int previousSelectedWeapon = selectedWeapon;
        if (!gunChanged)
        {
            gunChanged = true;
            if (selectedWeapon >= transform.childCount - 1)
            {
                selectedWeapon = 0;
            }
            else
            {
                selectedWeapon++;
            }
        }
        else
        {
            gunChanged = false;
            if (selectedWeapon <= transform.childCount - 1)
            {
                selectedWeapon = 0;
            }
            else
            {
                selectedWeapon--;
            }
        }
        if (previousSelectedWeapon != selectedWeapon)
        {
            SelectedWeapon();
        }
        shooterController.Equip(selectedWeapon);
        shooterController.GunChanged();
       
    }
    public bool GunSwaping()
    {
        return gunChanging;
    }
}
