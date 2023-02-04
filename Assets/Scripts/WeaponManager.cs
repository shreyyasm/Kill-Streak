using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
   

    [Header("Firing")]

    [Tooltip("Is this weapon automatic? If yes, then holding down the firing button will continuously fire.")]
    [SerializeField]
    private bool automatic;

    [Tooltip("How fast the projectiles are.")]
    [SerializeField]
    private float projectileImpulse = 400.0f;

    [Tooltip("Amount of shots this weapon can shoot in a minute. It determines how fast the weapon shoots.")]
    [SerializeField]
    private int roundsPerMinutes = 200;

    [Tooltip("Mask of things recognized when firing.")]
    [SerializeField]
    private LayerMask mask;

    [Tooltip("Maximum distance at which this weapon can fire accurately. Shots beyond this distance will not use linetracing for accuracy.")]
    [SerializeField]
    private float maximumDistance = 500.0f;

    [Header("Animation")]

    [Tooltip("Transform that represents the weapon's ejection port, meaning the part of the weapon that casings shoot from.")]
    [SerializeField]
    private Transform socketEjection;

    [Header("Resources")]

    [Tooltip("Casing Prefab.")]
    [SerializeField]
    private GameObject prefabCasing;

    [Tooltip("Projectile Prefab. This is the prefab spawned when the weapon shoots.")]
    [SerializeField]
    private GameObject prefabProjectile;

    [Tooltip("Weapon Body Texture.")]
    [SerializeField]
    private Sprite spriteBody;

    [Header("Audio Clips Holster")]

    [Tooltip("Holster Audio Clip.")]
    [SerializeField]
    private AudioClip audioClipHolster;

    [Tooltip("Unholster Audio Clip.")]
    [SerializeField]
    private AudioClip audioClipUnholster;

    [Header("Audio Clips Reloads")]

    [Tooltip("Reload Audio Clip.")]
    [SerializeField]
    private AudioClip audioClipReload;

    [Tooltip("Reload Empty Audio Clip.")]
    [SerializeField]
    private AudioClip audioClipReloadEmpty;

    [Header("Audio Clips Other")]

    [Tooltip("AudioClip played when this weapon is fired without any ammunition.")]
    [SerializeField]
    private AudioClip audioClipFireEmpty;




    [Header("Settings")]

    [Tooltip("Total Ammunition.")]
    [SerializeField]
    private int ammunitionTotal = 10;

    [Header("Interface")]

    [Tooltip("Interface Sprite.")]
    [SerializeField]
    private Sprite sprite;

    [Tooltip("Socket at the tip of the Muzzle. Commonly used as a firing point.")]
    [SerializeField]
    private Transform socket;

    #region FIELDS

    /// <summary>
    /// Weapon Animator.
    /// </summary>
    private Animator animator;
    /// <summary>
    /// Attachment Manager.
    /// </summary>
   // private WeaponAttachmentManagerBehaviour attachmentManager;

    /// <summary>
    /// Amount of ammunition left.
    /// </summary>
    private int ammunitionCurrent;

    #region Attachment Behaviours

    /// <summary>
    /// Equipped Magazine Reference.
    /// </summary>
    //private MagazineBehaviour magazineBehaviour;
    /// <summary>
    /// Equipped Muzzle Reference.
    /// </summary>
    //private MuzzleBehaviour muzzleBehaviour;

    #endregion

    /// <summary>
    /// The GameModeService used in this game!
    /// </summary>
    //private IGameModeService gameModeService;
    /// <summary>
    /// The main player character behaviour component.
    /// </summary>
    //private CharacterBehaviour characterBehaviour;

    /// <summary>
    /// The player character's camera.
    /// </summary>
    private Transform playerCamera;

    #endregion

    #region UNITY

    void Awake()
    {
        playerCamera = GameObject.FindWithTag("MainCamera").GetComponent<Transform>();
        //Get Animator.
        animator = GetComponent<Animator>();
        //Get Attachment Manager.
        //attachmentManager = GetComponent<WeaponAttachmentManagerBehaviour>();

        //Cache the game mode service. We only need this right here, but we'll cache it in case we ever need it again.
        //gameModeService = ServiceLocator.Current.Get<IGameModeService>();
        //Cache the player character.
        //characterBehaviour = gameModeService.GetPlayerCharacter();
        //Cache the world camera. We use this in line traces.
        //playerCamera = characterBehaviour.GetCameraWorld().transform;
    }
    void Start()
    {
        #region Cache Attachment References

        //Get Magazine.
        //magazineBehaviour = attachmentManager.GetEquippedMagazine();
        //Get Muzzle.
        //muzzleBehaviour = attachmentManager.GetEquippedMuzzle();

        #endregion

        //Max Out Ammo.
        ammunitionCurrent = ammunitionTotal;
    }

    #endregion

    #region GETTERS

    public  Animator GetAnimator() => animator;

    public Sprite GetSpriteBody() => spriteBody;

    public AudioClip GetAudioClipHolster() => audioClipHolster;
    public AudioClip GetAudioClipUnholster() => audioClipUnholster;

    public AudioClip GetAudioClipReload() => audioClipReload;
    public AudioClip GetAudioClipReloadEmpty() => audioClipReloadEmpty;

    public AudioClip GetAudioClipFireEmpty() => audioClipFireEmpty;

    //public AudioClip GetAudioClipFire() => muzzleBehaviour.GetAudioClipFire();

    public int GetAmmunitionCurrent() => ammunitionCurrent;

    public int GetAmmunitionTotal() => ammunitionTotal;

    public bool IsAutomatic() => automatic;
    public float GetRateOfFire() => roundsPerMinutes;

    public bool IsFull() => ammunitionCurrent == ammunitionTotal;
    public bool HasAmmunition() => ammunitionCurrent > 0;

   // public RuntimeAnimatorController GetAnimatorController() => controller;
   // public WeaponAttachmentManagerBehaviour GetAttachmentManager() => attachmentManager;

    #endregion

    #region METHODS

    public void Reload()
    {
        //Play Reload Animation.
        animator.Play(HasAmmunition() ? "Reload" : "Reload Empty", 0, 0.0f);
    }
    public void Fire(float spreadMultiplier = 1.0f)
    {

        //We need a muzzle in order to fire this weapon!
        //if (muzzleBehaviour == null)
           // return;

        //Make sure that we have a camera cached, otherwise we don't really have the ability to perform traces.
        if (playerCamera == null)
            return;

        //Get Muzzle Socket. This is the point we fire from.
        Transform muzzleSocket = socket;

        //Play the firing animation.
       // const string stateName = "Fire";
        //animator.Play(stateName, 0, 0.0f);

        //Reduce ammunition! We just shot, so we need to get rid of one!
        ammunitionCurrent = Mathf.Clamp(ammunitionCurrent - 1, 0, ammunitionTotal);

        //Play all muzzle effects.
        //muzzleBehaviour.Effect();

        //Determine the rotation that we want to shoot our projectile in.
        Quaternion rotation = Quaternion.LookRotation(playerCamera.forward * 1000.0f - muzzleSocket.position);

        //If there's something blocking, then we can aim directly at that thing, which will result in more accurate shooting.
        if (Physics.Raycast(new Ray(playerCamera.position, playerCamera.forward),
            out RaycastHit hit, maximumDistance, mask))
            rotation = Quaternion.LookRotation(hit.point - muzzleSocket.position);

        //Spawn projectile from the projectile spawn point.
        GameObject projectile = Instantiate(prefabProjectile, muzzleSocket.position, rotation);
        //Add velocity to the projectile.
        //projectile.GetComponent<Rigidbody>().velocity = projectile.transform.forward * projectileImpulse;
    }

    public void FillAmmunition(int amount)
    {
        //Update the value by a certain amount.
        ammunitionCurrent = amount != 0 ? Mathf.Clamp(ammunitionCurrent + amount,
            0, ammunitionTotal) : ammunitionTotal;
    }

    public void EjectCasing()
    {
        //Spawn casing prefab at spawn point.
        if (prefabCasing != null && socketEjection != null)
            Instantiate(prefabCasing, socketEjection.position, socketEjection.rotation);
    }

    
    #endregion
}
