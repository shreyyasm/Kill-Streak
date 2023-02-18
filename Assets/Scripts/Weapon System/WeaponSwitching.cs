using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class WeaponSwitching : MonoBehaviour
{
    //Index of selected weapon
    public int selectedWeapon;
    [SerializeField] ShooterController shooterController;
    [SerializeField] WeaponInventory weaponInventory;
    bool gunChanged = false;
    public bool gunChanging = false;
    [SerializeField] private Rig pistolRig;
    [SerializeField] private Rig rifleRig;
    public GameObject animator;

    [SerializeField] GameObject realPistol;
    [SerializeField] GameObject fakePistol;
    [SerializeField] GameObject realRifle;
    [SerializeField] GameObject fakeRifle;

    bool running = false;
    bool gunInHand = true;
    [SerializeField] float smoothSpeed = 80f;
    // Start is called before the first frame update
    void Start()
    {        
        SelectedWeapon();
    }

    // Update is called once per frame
    void Update()
    {
        Animator anim = animator.GetComponent<Animator>();
        //ChangeGunIndexNew();
        GunSwaping();
        if (anim.GetCurrentAnimatorStateInfo(4).IsName("Rifle To Pistol Locomotions") && anim.GetCurrentAnimatorStateInfo(4).normalizedTime > 1f)
        {
            gunChanging = false;          
        }
        if(gunChanging)
        {
           if(selectedWeapon == 1)
           {
                //anim.SetLayerWeight(4,1);
                anim.SetLayerWeight(4, 1);
                anim.SetLayerWeight(5, 0);
                anim.SetBool("Gun Changing", true);

                //rifleRig.weight = Mathf.Lerp(1, 0,  Time.deltaTime * smoothSpeed);
                //pistolRig.weight = Mathf.Lerp(1, 0, Time.deltaTime * smoothSpeed);
                //rifleRig.weight = 0f;
                //pistolRig.weight = 0f;
           }
           else
           {
                anim.SetLayerWeight(5, 1);
                anim.SetLayerWeight(4, 0);
                anim.SetBool("Gun Changing", true);
                //rifleRig.weight = Mathf.Lerp(1, 0, Time.deltaTime * smoothSpeed);
                //pistolRig.weight = Mathf.Lerp(1, 0, Time.deltaTime * smoothSpeed);
                //rifleRig.weight = 0f;
                //pistolRig.weight = 0f;
           }
            
        }
        else
        {
            if(selectedWeapon == 1)
            {
                anim.SetLayerWeight(4, 0);
                anim.SetBool("Gun Changing", false);
                //if (selectedWeapon == 1)
                //{
                //    if(!running)
                //        pistolRig.weight = 1f;
                //    else
                //        pistolRig.weight = 0f;
                //    rifleRig.weight = 0f;
                //}

            }
            else
            {
                anim.SetLayerWeight(5,0);
                anim.SetBool("Gun Changing", false);
                //if (selectedWeapon == 1)
                //{
                //    pistolRig.weight = 1f;
                //    rifleRig.weight = 0f;
                //}
                //else
                //{
                //    pistolRig.weight = 0f;
                //    rifleRig.weight = 1f;
                //}
            }


        }
        
        weaponInventory.GunCheck(gunInHand);
    }
    void SelectedWeapon()
    {
        int i = 0;
        foreach (Transform weapon in transform)
        {
            if (i == selectedWeapon)
            {
                //weapon.gameObject.SetActive(true);
            }
            else
            {
                //weapon.gameObject.SetActive(false);
            }

            i++;
        }
    }
    public void ChangeGunIndex()
    {       
        if(!gunChanging)
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
    }
    public void ChangeGunIndexNew()
    {
        if (!gunChanging)
        {
            if (Input.GetMouseButtonDown(1))
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

        }
    }
    public bool GunSwaping()
    {
        return gunChanging;
    }
    public void GunSwapVisualTakeIn()
    {

        if (animator.GetComponent<Animator>().GetLayerWeight(4) == 1)
        {
            gunInHand = false;
            if (selectedWeapon == 1)
            {
                realRifle.SetActive(false);
                fakeRifle.SetActive(true);

            }
            else
            {
                realPistol.SetActive(false);
                fakePistol.SetActive(true);
            }
        }
        if (animator.GetComponent<Animator>().GetLayerWeight(5) == 1)
        {
            gunInHand = false;
            fakeRifle.SetActive(false);
            realRifle.SetActive(true);
        }
        
    }
    public void GunSwapVisualTakeOut()
    {
        if (animator.GetComponent<Animator>().GetLayerWeight(4) == 1)
        {
            gunInHand = true;
            if (selectedWeapon == 1)
            {
                fakePistol.SetActive(false);
                realPistol.SetActive(true);
            }
            else
            {
                fakeRifle.SetActive(false);
                realRifle.SetActive(true);
            }
        }
        if(animator.GetComponent<Animator>().GetLayerWeight(5) == 1)
        {
            
            gunInHand = false;          
            realPistol.SetActive(false);
            fakePistol.SetActive(true);
        }
            
        

    }
    public void CheckRunning(bool state)
    {
        running = state;
    }
   
   
}
