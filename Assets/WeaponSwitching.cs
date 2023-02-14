using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSwitching : MonoBehaviour
{
    //Index of selected weapon
    public int selectedWeapon;
    [SerializeField] ShooterController shooterController;
    bool gunChanged = false;
    // Start is called before the first frame update
    void Start()
    {
        SelectedWeapon();
    }

    // Update is called once per frame
    void Update()
    {
       
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
