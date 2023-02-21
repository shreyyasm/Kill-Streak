using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

[CreateAssetMenu(fileName = "Gun", menuName = "Guns/Gun", order = 0)]
public class GunScriptableObject : ScriptableObject
{
    public ImpactType ImpactType;
    public GunType Type;
    public string Name;
    public GameObject ModelPrefab;
    public Vector3 SpawnPoint;
    public Vector3 SpawnRotation;

    public DamageConfigScriptableObject DamageConfig;
    public AmmoConfigScriptableObject AmmoConfig;
    public ShootConfigurationScriptableObject ShootConfig;
    public TrailConfigScriptableObject TrailConfig;
    public AudioConfigScriptableObject AudioConfig;

    private MonoBehaviour ActionMonoBehaviour;
    private GameObject Model;
    private AudioSource ShootingAudioSource;
    private float LastShootTime;
    private float InitialClickTime;
    private float StopShootingTime;
    private bool LastFrameWantedToShoot;
    private ParticleSystem ShootSystem;
    private ObjectPool<TrailRenderer> TrailPool;
    [SerializeField] GameObject bloodImpactPrefabs;

    public void Spawn(Transform Parent, MonoBehaviour ActiveMonoBehaviour)
    {
        this.ActionMonoBehaviour = ActiveMonoBehaviour;
        LastShootTime = 0;
        TrailPool = new ObjectPool<TrailRenderer>(CreateTrail);

        AmmoConfig.CurrentClipAmmo = AmmoConfig.ClipSize;
        AmmoConfig.CurrentAmmo = AmmoConfig.MaxAmmo;

        Model = Instantiate(ModelPrefab);
        Model.transform.SetParent(Parent, false);
        Model.transform.localPosition = SpawnPoint;
        Model.transform.localRotation = Quaternion.Euler(SpawnRotation);

        ShootSystem = Model.GetComponentInChildren<ParticleSystem>();
        ShootingAudioSource = Model.GetComponent<AudioSource>();
    }
    Vector3 aimDir;
    Vector3 mouseWorldPosition;
    public void TryToShoot()
    {
        if(Time.time - LastShootTime - ShootConfig.FireRate > Time.deltaTime)
        {
            InitialClickTime = Time.time;
            float lastDuration = Mathf.Clamp(
                (StopShootingTime - InitialClickTime),
                0,
                ShootConfig.MaxSpeedTime);
            float lerpTime = (ShootConfig.RecoilRecoverySpeed - (Time.time - StopShootingTime))
                / ShootConfig.RecoilRecoverySpeed;

            InitialClickTime = Time.time - Mathf.Lerp(0,lastDuration, Mathf.Clamp01(lerpTime));

        }
        if(Time.time > ShootConfig.FireRate + LastShootTime)
        {      
            LastShootTime = Time.time;

            if (AmmoConfig.CurrentClipAmmo == 0)
            {
                AudioConfig.PlayOutOfAmmoClip(ShootingAudioSource);
                return;
            }

            ShootSystem.Play();
            AudioConfig.PlayShootingClip(ShootingAudioSource, AmmoConfig.CurrentClipAmmo == 1);

            Vector3 spreadAmount = ShootConfig.GetSpread(Time.time - InitialClickTime);
            Model.transform.forward += Model.transform.TransformDirection(spreadAmount);
            

            //Vector2 screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);
            //Ray ray = Camera.main.ScreenPointToRay(screenCenterPoint);

            //if (Physics.Raycast(ray, out RaycastHit raycastHit, float.MaxValue, ShootConfig.Hitmask))
            //{
            //    //debugTransform.position = raycastHit.point;
            //    Model.transform.LookAt(raycastHit.point);
            //}
            Vector3 screenCenterPoint = new Vector3(Screen.width / 2f, Screen.height / 2f);
            Camera.main.transform.forward += Camera.main.transform.TransformDirection(spreadAmount);
            Ray ray = Camera.main.ScreenPointToRay(screenCenterPoint) ;

            //if (Physics.Raycast(ray, out RaycastHit raycastHit, float.MaxValue, ShootConfig.Hitmask))
            //{
            //    //debugTransform.position = raycastHit.point;
            //    mouseWorldPosition = raycastHit.point;
            //    aimDir = (mouseWorldPosition - Model.transform.position) + Model.transform.forward;
            //}
            Vector3 ShootDirection = Model.transform.forward;
            //mouseWorldPosition = Vector3.zero;



            AmmoConfig.CurrentClipAmmo--;

            if (Physics.Raycast(ray, out RaycastHit raycastHit, float.MaxValue, ShootConfig.Hitmask))
            {
                //Model.transform.rotation = hit.transform.rotation;
                ActionMonoBehaviour.StartCoroutine(
                    PlayTrail(
                        ShootSystem.transform.position,
                         raycastHit.point,
                         raycastHit
                    ));
            }
            else
            {
                ActionMonoBehaviour.StartCoroutine(
                    PlayTrail(
                        ShootSystem.transform.position,
                        ShootSystem.transform.position + (ShootDirection * TrailConfig.MissDistance),
                        new RaycastHit()
                    ));
            }
        }
    }
    public bool CanReload()
    {
        return AmmoConfig.CanReload();
    }
    public void StartReloading()
    {
        AudioConfig.PlayReloadClip(ShootingAudioSource);
    }
    public void EndReload()
    {
        AmmoConfig.Reload();
    }
    public void Tick(bool WantsToShoot)
    {
        Model.transform.localRotation = Quaternion.Lerp(
            Model.transform.localRotation,
            Quaternion.Euler(SpawnRotation),
            Time.deltaTime * ShootConfig.RecoilRecoverySpeed);
        if(WantsToShoot)
        {
            LastFrameWantedToShoot = true;
            TryToShoot();
           
        }
        else if(!WantsToShoot && LastFrameWantedToShoot)
        {
            StopShootingTime = Time.time;
            LastFrameWantedToShoot = false;
        }
    }
    IEnumerator PlayTrail(Vector3 StartPoint, Vector3 EndPoint, RaycastHit Hit)
    {
        TrailRenderer instance = TrailPool.Get();
        instance.gameObject.SetActive(true);
        instance.transform.position = StartPoint;
        yield return null;

        instance.emitting = true;
        float distance = Vector3.Distance(StartPoint, EndPoint);
        float remainingDistance = distance;
        while(remainingDistance > 0)
        {
            instance.transform.position = Vector3.Lerp(
                StartPoint,
                EndPoint,
                Mathf.Clamp01(1 - (remainingDistance / distance))
            );
            remainingDistance -= TrailConfig.SimulationSpeed * Time.deltaTime;
            yield return null;
        }
        instance.transform.position = EndPoint;

        if (Hit.collider != null)
        {
            SurfaceManager.Instance.HandleImpact(Hit.transform.gameObject, EndPoint, Hit.normal, ImpactType,0);
            //SpawnVFX(VFX.gameObject, this);
           //DespawnBullet();
            //imapct system
            if (Hit.collider.TryGetComponent(out IDamageable damageable))
            {
                damageable.TakeDamage(DamageConfig.GetDamage(distance));
                Debug.Log("Hit");
            }
        }
        yield return new WaitForSeconds(TrailConfig.Duration);
        yield return null;
        instance.emitting = false;
        instance.gameObject.SetActive(false);
        TrailPool.Release(instance);

    }
    private TrailRenderer CreateTrail()
    {
        GameObject instance = new GameObject("Bullet Trail");
        TrailRenderer trail = instance.AddComponent<TrailRenderer>();
        trail.colorGradient = TrailConfig.Color;
        trail.material = TrailConfig.Material;
        trail.widthCurve = TrailConfig.WidthCurve;
        trail.time = TrailConfig.Duration;
        trail.minVertexDistance = TrailConfig.MinVertexDistance;

        trail.emitting = false;
        trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        return trail;
    }
}
