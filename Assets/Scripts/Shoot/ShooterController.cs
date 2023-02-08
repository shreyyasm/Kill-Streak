using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using StarterAssets;
using UnityEngine.InputSystem;
using FishNet.Object;
using UnityEngine.Animations.Rigging;
using FishNet;

public class ShooterController : NetworkBehaviour
{
    //Aim Settings  
    [SerializeField] float normalSensitivity;
    [SerializeField] float aimSensitivity;
    [SerializeField] LayerMask aimcolliderLayerMask;
    [SerializeField] Transform debugTransform;

    //Bullet Content
    [SerializeField] GameObject bulletPrefab;   
    [SerializeField] AudioClip audioClipFire;
    [SerializeField] GameObject flashPrefab;
    [SerializeField] GameObject prefabFlashLight;

    //Gun Socket
    [SerializeField] Transform socket;
    [SerializeField] Transform socketBeforePos;
    [SerializeField] Transform spawnBulletPosition;
    
    //outer References
    [SerializeField] private Rig aimRig;
    [SerializeField] private float aimRigWeight;
    [SerializeField] GameObject fPSController;

    //Offsets
    public Vector3 vfxSpawnOffset;
    public Vector3 offset;
    Vector3 aimDir;
    Quaternion rotation;
    Vector3 mouseWorldPosition;

    //Conditions
    bool FPSMode;
    bool Aiming = false;

    //Camera's Reference
    public GameObject fpsVirtualCamera;
    GameObject aimVirtualCamera;
    GameObject followVirtualCamera;

    //References
    private ParticleSystem particles;
    private Light flashLight;
    private StarterAssetsInputs starterAssetsInputs;
    private ThirdPersonController thirdPersonController;
    private Animator animator;
    private AudioSource audioSource;
    
    public GameObject spawnedObject;
    public WeaponManager equippedWeapon;

    bool cursorLocked;
    bool holdingButtonFire;
    float lastShotTime;
    private void Awake()
    {
        //References
        starterAssetsInputs = GetComponent<StarterAssetsInputs>();
        thirdPersonController = GetComponent<ThirdPersonController>();
        animator = GetComponent<Animator>();
        aimVirtualCamera = GameObject.FindWithTag("Aim Camera");
        followVirtualCamera = GameObject.FindWithTag("Follow Camera");
        

        //Instansiate Flash 
        audioSource = GetComponent<AudioSource>();
        GameObject flash = Instantiate(flashPrefab, socket);
        flash.transform.localPosition = default;
        flash.transform.localEulerAngles = default;
        flash.SetActive(true);

        //Instansiate FlashLight 
        GameObject spawnedFlashLightPrefab = Instantiate(prefabFlashLight, socket);
        spawnedFlashLightPrefab.transform.localPosition = offset;
        spawnedFlashLightPrefab.transform.localEulerAngles = default;
        flashLight = spawnedFlashLightPrefab.GetComponent<Light>();
        flashLight.enabled = false;

        particles = flash.GetComponent<ParticleSystem>();
       
    }
    public void Update()
    {
        if (!base.IsOwner)
            return;
        
            Aim();
            //Fire();

           //equippedWeapon.MyInput(equippedWeapon);
    }
   
    public void Aim()
    {
        socket.transform.position = socketBeforePos.transform.position;
        aimVirtualCamera.transform.position = followVirtualCamera.transform.position;
        mouseWorldPosition = Vector3.zero;

        Vector2 screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Ray ray = Camera.main.ScreenPointToRay(screenCenterPoint);

        if (Physics.Raycast(ray, out RaycastHit raycastHit, 999f, aimcolliderLayerMask))
        {
            //debugTransform.position = raycastHit.point;
            debugTransform.position = Vector3.Lerp(debugTransform.position,raycastHit.point,Time.deltaTime*20f);
            

            mouseWorldPosition = raycastHit.point;
        }

        if (Input.GetMouseButton(1))
        {
            Aiming = true;
            thirdPersonController.Aiming(true);
            if (!FPSMode)
            {
                //aimVirtualCamera.GetComponent<CinemachineVirtualCamera>().enabled = true;
                //thirdPersonController.SetSensitivity(aimSensitivity);

                Vector3 worldAimTarget = mouseWorldPosition;
                worldAimTarget.y = transform.position.y;
                Vector3 aimDirection = (worldAimTarget - transform.position).normalized;

                transform.forward = Vector3.Lerp(transform.forward, aimDirection, Time.deltaTime * 10f);
                //Vector3 direction = (aimDir - transform.position).normalized;
                //Quaternion rotGoal = Quaternion.LookRotation(aimDir);
                //transform.rotation = Quaternion.Slerp(transform.rotation, rotGoal, 1);
            }
            if (FPSMode)
            {
                socket.transform.localPosition = vfxSpawnOffset;
            }

            //animator.SetLayerWeight(1, Mathf.Lerp(animator.GetLayerWeight(1), 1f, Time.deltaTime * 10f));
        }
        else
        {
            Aiming = false;
            thirdPersonController.Aiming(false);
            if (!FPSMode)
            {
                //aimVirtualCamera.GetComponent<CinemachineVirtualCamera>().enabled = false;
                //thirdPersonController.SetSensitivity(normalSensitivity);

                Vector3 worldAimTarget = mouseWorldPosition;
                worldAimTarget.y = transform.position.y;
                Vector3 aimDirection = (worldAimTarget - transform.position).normalized;
                transform.forward = Vector3.Lerp(transform.forward, aimDirection, Time.deltaTime * 20f);
                //Vector3 direction = (aimDir - transform.position).normalized;
                //Quaternion rotGoal = Quaternion.LookRotation(aimDir);
                //transform.rotation = Quaternion.Slerp(transform.rotation, rotGoal, 1);
            }

            //animator.SetLayerWeight(1, Mathf.Lerp(animator.GetLayerWeight(1), 0f, Time.deltaTime * 10f));
        }

        if (FPSMode)
            fPSController.GetComponent<FPSController>().AimFPS(Aiming);
    }

    public void Fire()
    {
        if(Input.GetMouseButton(0))
        {
            if (starterAssetsInputs.shoot)
            {
                
                aimDir = (mouseWorldPosition - spawnBulletPosition.position).normalized;
                rotation = Quaternion.LookRotation(aimDir, Vector3.up);

                //SpawnBulletServerRPC(aimDir, rotation,this);
                lastShotTime = Time.time;

                //equippedWeapon.Fire(aimDir, rotation, spawnBulletPosition, equippedWeapon);
                //Show Flash
                particles.Emit(5);
                flashLight.enabled = true;
                StartCoroutine(nameof(DisableFlashLight));
                audioSource.PlayOneShot(audioClipFire, 0.5f);

                //Camera Shake
                followVirtualCamera.GetComponent<CinemachineShake>().ShakeCamera(1f, 0.1f);
                aimVirtualCamera.GetComponent<CinemachineShake>().ShakeCamera(1f, 0.1f);
                fpsVirtualCamera.GetComponent<CinemachineShake>().ShakeCamera(1f, 0.1f);

                starterAssetsInputs.shoot = false;
                flashPrefab.SetActive(false);

            }
        }
            
    }
    [ServerRpc]
    public void SpawnBulletServerRPC(Vector3 aimDir, Quaternion rotation, ShooterController script)
    {

        //Instansiate Bullet
        GameObject projectile = Instantiate(bulletPrefab, spawnBulletPosition.position, rotation);
        ServerManager.Spawn(projectile, base.Owner);
        SetSpawnBullet(projectile, script);
    }
    
    [ObserversRpc]
    public void SetSpawnBullet(GameObject spawned, ShooterController script)
    {
        script.spawnedObject = spawned;
       
    }

    [ServerRpc(RequireOwnership = false)]
    public void DespawnBullet()
    {
        ServerManager.Despawn(spawnedObject);
    }

    public void FPSModeCheck(bool state)
    {
        FPSMode = state;
    }  
    private IEnumerator DisableFlashLight()
    {
        //Wait.
        yield return new WaitForSeconds(0.1f);
        //Disable.
        flashLight.enabled = false;
    }
    
}
