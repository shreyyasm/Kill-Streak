using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet;
using FishNet.Object;
using TMPro;

public class WeaponManager : NetworkBehaviour
{
    [Tooltip("Projectile Prefab. This is the prefab spawned when the weapon shoots.")]
    [SerializeField]
    private GameObject prefabProjectile;

    
    GameObject spawnedObject;
    //Gun stats
    public int damage;
    public float timeBetweenShooting, spread, range, reloadTime, timeBetweenShots;
    public int magazineSize, bulletsPerTap;
    public bool allowButtonHold;
    int bulletsLeft, bulletsShot;

    //bools 
    bool shooting, readyToShoot, reloading;

    //Reference
    public Camera mainCamera;
    public Transform attackPoint;
    public RaycastHit rayHit;
    public LayerMask whatIsEnemy;

    //Graphics
    public GameObject muzzleFlash, bulletHoleGraphic;
    //public CamShake camShake;
    public float camShakeMagnitude, camShakeDuration;
    public TextMeshProUGUI text;
    Vector3 mouseWorldPosition;
    Vector3 aimDir;
    Quaternion rotation;
    private void Awake()
    {
        bulletsLeft = magazineSize;
        readyToShoot = true;
    }
    private void Update()
    {
        //MyInput();
        mainCamera = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
        //SetText
        //text.SetText(bulletsLeft + " / " + magazineSize);
    }
    public void MyInput(WeaponManager script)
    {
        if (allowButtonHold) shooting = Input.GetMouseButton(0);
        else shooting = Input.GetMouseButtonDown(0);

        if (Input.GetKeyDown(KeyCode.R) && bulletsLeft < magazineSize && !reloading) Reload();

        //Shoot
        if (readyToShoot && shooting && !reloading && bulletsLeft > 0)
        {
            bulletsShot = bulletsPerTap;
            Fire(script);
            
        }

    }
    private void ResetShot()
    {
        readyToShoot = true;
    }
    private void Reload()
    {
        reloading = true;
        Invoke("ReloadFinished", reloadTime);
    }
    private void ReloadFinished()
    {
        bulletsLeft = magazineSize;
        reloading = false;
    }
    [ServerRpc]
    public void Fire(WeaponManager script)
    {
        Vector2 screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Ray ray = Camera.main.ScreenPointToRay(screenCenterPoint);

        if (Physics.Raycast(ray, out RaycastHit raycastHit, 999f))
        {
            //debugTransform.position = raycastHit.point;
            mouseWorldPosition = raycastHit.point;
        }
        //mouseWorldPosition = Vector3.zero;
        Vector3 worldAimTarget = mouseWorldPosition;
        aimDir = (mouseWorldPosition - attackPoint.position).normalized;
        rotation = Quaternion.LookRotation(aimDir, Vector3.up);

        bulletsShot = bulletsPerTap;
            Debug.Log("Work");
            //Spawn projectile from the projectile spawn point.
            GameObject projectile = Instantiate(prefabProjectile, attackPoint.position, rotation);
            ServerManager.Spawn(projectile, base.Owner);
            SetSpawnBullet(projectile, script);

            readyToShoot = false;

            //Spread
            float x = Random.Range(-spread, spread);
            float y = Random.Range(-spread, spread);

            //Calculate Direction with Spread
            Vector3 direction = mainCamera.transform.forward + new Vector3(x, y, 0);

            //RayCast
            if (Physics.Raycast(mainCamera.transform.position, direction, out rayHit, range, whatIsEnemy))
            {
                Debug.Log(rayHit.collider.name);

                //if (rayHit.collider.CompareTag("Enemy"))
                //rayHit.collider.GetComponent<ShootingAi>().TakeDamage(damage);
            }

            //ShakeCamera
            //camShake.Shake(camShakeDuration, camShakeMagnitude);

            //Graphics
           // Instantiate(bulletHoleGraphic, rayHit.point, Quaternion.Euler(0, 180, 0));
            //Instantiate(muzzleFlash, attackPoint.position, Quaternion.identity);

            bulletsLeft--;
            bulletsShot--;

            Invoke("ResetShot", timeBetweenShooting);

            if (bulletsShot > 0 && bulletsLeft > 0)
                Invoke("Fire", timeBetweenShots);
            

    }
    [ObserversRpc]
    public void SetSpawnBullet(GameObject spawned, WeaponManager script)
    {
        script.spawnedObject = spawned;
    }
}
