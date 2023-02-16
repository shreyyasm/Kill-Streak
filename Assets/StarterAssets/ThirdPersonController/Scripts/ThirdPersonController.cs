using UnityEngine;
//using Unity.Netcode;
//using Unity.Netcode.Components;
using FishNet.Object;
using UnityEngine.InputSystem;
using UnityEngine.Animations.Rigging;
using System.Collections;
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
#endif

/* Note: animations are called via the controller for both the character and capsule using animator null checks
 */

namespace StarterAssets
{
    [RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
    [RequireComponent(typeof(PlayerInput))]
#endif
    public class ThirdPersonController :NetworkBehaviour
    {

        [Header("Player")]
        [Tooltip("Move speed of the character in m/s")]
        public float MoveSpeed = 2.0f;

        [Tooltip("Sprint speed of the character in m/s")]
        public float SprintSpeed = 5.335f;

        [Tooltip("How fast the character turns to face movement direction")]
        [Range(0.0f, 0.3f)]
        public float RotationSmoothTime = 0.12f;

        [Tooltip("Acceleration and deceleration")]
        public float SpeedChangeRate = 10.0f;

        public AudioClip LandingAudioClip;
        public AudioClip[] FootstepAudioClips;
        [Range(0, 1)] public float FootstepAudioVolume = 0.5f;

        [Space(10)]
        [Tooltip("The height the player can jump")]
        public float JumpHeight = 1.2f;

        [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
        public float Gravity = -15.0f;

        [Space(10)]
        [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
        public float JumpTimeout = 0.50f;

        [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
        public float FallTimeout = 0.15f;

        [Header("Player Grounded")]
        [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
        public bool Grounded = true;

        [Tooltip("Useful for rough ground")]
        public float GroundedOffset = -0.14f;

        [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
        public float GroundedRadius = 0.28f;

        [Tooltip("What layers the character uses as ground")]
        public LayerMask GroundLayers;

        [Header("Cinemachine")]
        [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
        public GameObject CinemachineCameraTarget;

        [Tooltip("How far in degrees can you move the camera up")]
        public float TopClamp = 70.0f;

        [Tooltip("How far in degrees can you move the camera down")]
        public float BottomClamp = -30.0f;

        [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
        public float CameraAngleOverride = 0.0f;

        [Tooltip("For locking the camera position on all axis")]
        public bool LockCameraPosition = false;

        public GameObject fPSController;
        public FixedTouchField fixedTouchField;
        public ScreenTouch screenTouch;
        [SerializeField] private Rig pistolRig;
        [SerializeField] private Rig rifleRig;
        // cinemachine
        private float _cinemachineTargetYaw;
        private float _cinemachineTargetPitch;

        // player
        private float _speed;
        private float _animationBlend;
        private float _targetRotation = 0.0f;
        private float _rotationVelocity;
        private float _verticalVelocity;
        private float _terminalVelocity = 53.0f;

        // timeout deltatime
        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;

        // animation IDs
        private int _animIDSpeed;
        private int _animIDGrounded;
        private int _animIDIdleJump;
        private int _animIDWalkJump;
        private int _animIDFreeFall;
        private int _animIDMotionSpeed;


#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
        private PlayerInput _playerInput;
#endif
        private Animator _animator;
        //NetworkAnimator _networkAnimator;
        private CharacterController _controller;
        private StarterAssetsInputs _input;
        private GameObject _mainCamera;
        public PlayerInput playerInput;
        public InputActionAsset myActionAsset;
        [SerializeField] GameObject cameraRoot;

        bool isAiming = false;
        bool isAimWalking = false;
        bool inFPSMode = false;
        public bool firedBullet = false;
        public bool firing = false;
        public bool running;
        public int gunType;
        public bool changingGun;
        float fireBulletTime = 0f;

        public float sensitivity = 100f;

        private const float _threshold = 0.01f;

        private bool _hasAnimator;
        public Vector3 direction;
        Vector3 move;
        float mouseX , mouseY;
        [SerializeField] ShooterController shooterController;
        [SerializeField] WeaponSwitching weaponSwitching;
        [SerializeField] float smoothSpeed = 80f;
        private bool IsCurrentDeviceMouse
        {
            get
            {
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
                 return _playerInput.currentControlScheme == "Any";
#else
				return false;
#endif
            }
        }

        //public override void OnNetworkSpawn()
        //{
        //    if (IsOwner)
        //    {
        //        cameraRoot.AddComponent<CameraFollow>();
        //    }

        //}
        public override void OnStartNetwork()
        {
            base.OnStartNetwork();
            /* If you wish to check for ownership inside
            * this method do not use base.IsOwner, use
            * the code below instead. This difference exist
            * to support a clientHost condition. */
            if (base.Owner.IsLocalClient)
                cameraRoot.AddComponent<CameraFollow>();
        }
        private void OnEnable()
        {
            //myActionAsset.bindingMask = new InputBinding { groups = "KeyboardMouse" };
           // playerInput.SwitchCurrentControlScheme(Keyboard.current, Mouse.current);
        }
        private void Awake()
        {
            // get a reference to our main camera
            if (_mainCamera == null)
            {
                _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            }
           
            
        }

        private void Start()
        {
            _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;

            _hasAnimator = TryGetComponent(out _animator);
            
            //_networkAnimator = gameObject.GetComponent<NetworkAnimator>();
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<StarterAssetsInputs>();
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
           _playerInput = GetComponent<PlayerInput>();
#else
			Debug.LogError( "Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
#endif

            AssignAnimationIDs();

            // reset our timeouts on start
            _jumpTimeoutDelta = JumpTimeout;
            _fallTimeoutDelta = FallTimeout;           
        }

        private void Update()
        {
            if (!base.IsOwner)
                return;

           
            if(!changingGun)
                JumpAndGravity();
            GroundedCheck();
            
    
           

            if (firedBullet && fireBulletTime >= 0)
            {
                if(!firing)
                    fireBulletTime -= Time.deltaTime;
                if (fireBulletTime <= 0)
                {
                    firedBullet = false;                   
                }
            }
            
            Move();
            if (shooterController.ReturnGuntype() == 0)
            {
                if (!running)
                {
                    pistolRig.weight = 1f;
                    rifleRig.weight = 0f;
                }
            }

            else
            {
                if (!running)
                {
                    rifleRig.weight = 1f;
                    pistolRig.weight = 0f;
                }

            }
            //MoveBasic();
            //if(screenTouch.rightFingerID != -1)
            if (shooterController.ReturnGuntype() == 0)
            {
                
                _animator.SetLayerWeight(4, Mathf.Lerp(_animator.GetLayerWeight(4), 1f, Time.deltaTime * smoothSpeed));
                _animator.SetLayerWeight(2, Mathf.Lerp(_animator.GetLayerWeight(2), 0f, Time.deltaTime * smoothSpeed));
                //_animator.SetLayerWeight(0, 1);
                //_animator.SetLayerWeight(2, 0);
            }
            else
            {
                _animator.SetLayerWeight(2, Mathf.Lerp(_animator.GetLayerWeight(2), 1f, Time.deltaTime * smoothSpeed));
                _animator.SetLayerWeight(0, Mathf.Lerp(_animator.GetLayerWeight(0), 0f, Time.deltaTime * smoothSpeed));
                //_animator.SetLayerWeight(2, 1);
                //_animator.SetLayerWeight(0, 0);
            }
            changingGun = weaponSwitching.GunSwaping();
            weaponSwitching.CheckRunning(running);
            //pistolRig.weight = Mathf.Lerp(pistolRig.weight, pistolRig.weight, Time.deltaTime * 100f);
            //rifleRig.weight = Mathf.Lerp(rifleRig.weight, rifleRig.weight, Time.deltaTime * 100f);
        }

        private void LateUpdate()
        {
            if (!base.IsOwner)
                return;
            if(!inFPSMode)
                CameraRotation();
 
        }
        public void OnMove(InputValue value)
        {
            MoveInput(value.Get<Vector2>());
        }
        public void MoveInput(Vector2 newMoveDirection)
        {
            move = newMoveDirection;
        }
        private void AssignAnimationIDs()
        {
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDGrounded = Animator.StringToHash("Grounded");
            _animIDIdleJump = Animator.StringToHash("Idle Jump");
            _animIDWalkJump = Animator.StringToHash("Walk Jump");
            _animIDFreeFall = Animator.StringToHash("FreeFall");
            _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
        }

        private void GroundedCheck()
        {
            // set sphere position, with offset
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
                transform.position.z);
            Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
                QueryTriggerInteraction.Ignore);

            // update animator if using character
            if (_hasAnimator)
            {
                _animator.SetBool(_animIDGrounded, Grounded);
            }
        }

        public void CameraRotation()
        {
            mouseX = screenTouch.lookInput.x;
            mouseY = screenTouch.lookInput.y;
            //float h = UltimateTouchpad.GetHorizontalAxis("Look");
            //float v = UltimateTouchpad.GetVerticalAxis("Look");
            //Vector3 direction = new Vector3(h, v, 0f).normalized;
            //Debug.Log(direction.x);
            // if there is an input and camera position is not fixed
            if (screenTouch.lookInput.sqrMagnitude >= _threshold && !LockCameraPosition)
            {
                //Don't multiply mouse input by Time.deltaTime;
                float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

                //_cinemachineTargetYaw += _input.look.x * deltaTimeMultiplier * sensitivity;
                //_cinemachineTargetPitch += _input.look.y * deltaTimeMultiplier * sensitivity;
                _cinemachineTargetYaw += mouseX * Time.deltaTime * 100;
                _cinemachineTargetPitch -= mouseY * Time.deltaTime * 100;
            }

            // clamp our rotations so our values are limited 360 degrees
            _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

            // Cinemachine will follow this target
            CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride,
                _cinemachineTargetYaw, 0.0f);
            

        }

        public void Move()
        {
            float x = UltimateJoystick.GetHorizontalAxis("Movement");
            float z = UltimateJoystick.GetVerticalAxis("Movement");
            direction = new Vector3(x, 0f, z).normalized;
            float neutralize = 1f;
            //Debug.Log(direction.x);
            move.x = x;
            move.z = z;
            
            _animator.SetFloat("MoveX", move.x);
            _animator.SetFloat("MoveZ", move.z);
            // set target speed based on move speed, sprint speed and if sprint is pressed
            //float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;
           
            if (direction.z > 0.2f && !isAiming && !firedBullet && !changingGun)
            {
           
                    MoveSpeed = 7f;               
            }
            else
                MoveSpeed = 5;

            if (shooterController.ReturnGuntype() == 0)
            {
                if (!isAiming)
                {
                    if(!firedBullet)
                    {
                        _animator.SetLayerWeight(1, Mathf.Lerp(_animator.GetLayerWeight(1), 0f, Time.deltaTime * smoothSpeed));
                        _animator.SetLayerWeight(3, Mathf.Lerp(_animator.GetLayerWeight(3), 0f, Time.deltaTime * smoothSpeed));
                        //_animator.SetLayerWeight(1, 0);
                       // _animator.SetLayerWeight(3, 0);
                    }
                    
                }
                else
                {
                    _animator.SetLayerWeight(1, Mathf.Lerp(_animator.GetLayerWeight(1), 1f, Time.deltaTime * smoothSpeed));
                    _animator.SetLayerWeight(3, Mathf.Lerp(_animator.GetLayerWeight(3), 0f, Time.deltaTime * smoothSpeed));
                    //_animator.SetLayerWeight(1, 1);
                    //_animator.SetLayerWeight(3, 0);
                }
                    
            }
            else
            {
                if (!isAiming)
                {
                    if (!firedBullet)
                    {
                        _animator.SetLayerWeight(1, Mathf.Lerp(_animator.GetLayerWeight(1), 0f, Time.deltaTime * smoothSpeed));
                        _animator.SetLayerWeight(3, Mathf.Lerp(_animator.GetLayerWeight(3), 0f, Time.deltaTime * smoothSpeed));
                        //_animator.SetLayerWeight(1, 0);
                        //_animator.SetLayerWeight(3, 0);
                    }
                       
                }
                else
                {
                    _animator.SetLayerWeight(3, Mathf.Lerp(_animator.GetLayerWeight(3), 1f, Time.deltaTime * smoothSpeed));
                    _animator.SetLayerWeight(1, Mathf.Lerp(_animator.GetLayerWeight(1), 0f, Time.deltaTime * smoothSpeed));
                    //_animator.SetLayerWeight(3, 1);
                    //_animator.SetLayerWeight(1, 0);
                    
                }

            }


            float targetSpeed = MoveSpeed;

            // a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

            // note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is no input, set the target speed to 0
            //if (_input.move == Vector2.zero) targetSpeed = 0.0f;
 
            if(direction == Vector3.zero)
            {
                targetSpeed = 0.0f;               
                neutralize = 0f;
                //transform.position = Vector3.zero;
            }
                
            // a reference to the players current horizontal velocity
            float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

            float speedOffset = 0.1f;
            float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

            // accelerate or decelerate to target speed
            if (currentHorizontalSpeed < targetSpeed - speedOffset ||
                currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                // creates curved result rather than a linear one giving a more organic speed change
                // note T in Lerp is clamped, so we don't need to clamp our speed
                //_speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude,
                //    Time.deltaTime * SpeedChangeRate);
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * 1,
                    Time.deltaTime * SpeedChangeRate);

                // round speed to 3 decimal places
                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
               // _speed = targetSpeed;
            }

            _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
            if (_animationBlend < 0.01f) _animationBlend = 0f;

            // normalise input direction
            Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

            // note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is a move input rotate player when the player is moving
            if (direction != Vector3.zero)
            {
                //_targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                //                  _mainCamera.transform.eulerAngles.y;
                _targetRotation = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg +
                                  _mainCamera.transform.eulerAngles.y;
                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                    RotationSmoothTime);

                // rotate to face input direction relative to camera position
                //transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            }

            Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

                // move the player
                _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) * neutralize +
                                 new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
            

            // update animator if using character
            if (_hasAnimator)
            {
                //Animations State
                _animator.SetFloat(_animIDSpeed, _animationBlend);
                _animator.SetBool("Aim Walk", isAimWalking);
                if(targetSpeed > 5f && !isAiming)
                {
                    running = true;
                    pistolRig.weight = 0f;
                   // rifleRig.weight = 0f;
                   
                }
                else
                {
                    running = false;

                    if (shooterController.ReturnGuntype() == 0)
                        pistolRig.weight = 1f;

                    else
                        rifleRig.weight = 1f;

                }

                fPSController.GetComponent<FPSController>().SetMovementSpeed(_animationBlend);            
            }
           
            //Aim and Walking
            if (isAiming && _animationBlend > 1)
            {
                isAimWalking = true;                          
            }
            else
            {
                isAimWalking = false;
            }                    
        }
        private void MoveBasic()
        {
          
            // set target speed based on move speed, sprint speed and if sprint is pressed
            float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;

            // a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

            // note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is no input, set the target speed to 0
            if (_input.move == Vector2.zero) targetSpeed = 0.0f;

            // a reference to the players current horizontal velocity
            float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

            float speedOffset = 0.1f;
            float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

            // accelerate or decelerate to target speed
            if (currentHorizontalSpeed < targetSpeed - speedOffset ||
                currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                // creates curved result rather than a linear one giving a more organic speed change
                // note T in Lerp is clamped, so we don't need to clamp our speed
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude,
                    Time.deltaTime * SpeedChangeRate);

                // round speed to 3 decimal places
                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }

            _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
            if (_animationBlend < 0.01f) _animationBlend = 0f;

            // normalise input direction
            Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

            // note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is a move input rotate player when the player is moving
            if (_input.move != Vector2.zero)
            {
                _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                                  _mainCamera.transform.eulerAngles.y;
                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                    RotationSmoothTime);

                // rotate to face input direction relative to camera position
                transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            }

            move.x = inputDirection.x;
            move.z = inputDirection.z;

            _animator.SetFloat("MoveX", move.x);
            _animator.SetFloat("MoveZ", move.z);
            Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

            // move the player
            _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) +
                             new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

            // update animator if using character
            if (_hasAnimator)
            {
                //Animations State
                _animator.SetFloat(_animIDSpeed, _animationBlend);
                _animator.SetBool("Aim Walk", isAimWalking);
                if (_animationBlend > 5.5f)
                {
                    pistolRig.weight = 0f;
                }
                else
                    pistolRig.weight = 1f;
                fPSController.GetComponent<FPSController>().SetMovementSpeed(_animationBlend);
            }

            //Aim and Walking
            if (isAiming && _animationBlend > 1)
            {
                isAimWalking = true;
            }
            else
            {
                isAimWalking = false;
            }

            //Walk Backwards
            if (move.y == -1)
            {
                _animator.SetFloat("Walk Back", -1);
            }
            else
            {
                _animator.SetFloat("Walk Back", 1);
            }
        }
        public void JumpAndGravity()
        {            
            if (Grounded)
            {
                // reset the fall timeout timer
                _fallTimeoutDelta = FallTimeout;

                // update animator if using character

                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDIdleJump, false);
                    _animator.SetBool(_animIDWalkJump, false);
                }
                // stop our velocity dropping infinitely when grounded
                if (_verticalVelocity < 0.0f)
                {
                    _verticalVelocity = -2f;
                }

                // Jump
                if (_input.jump && _jumpTimeoutDelta <= 0.0f)
                {
                    // the square root of H * -2 * G = how much velocity needed to reach desired height
                    _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

                    // update animator if using character

                    if (_hasAnimator)
                    {
                        if(_animationBlend > 1)
                        {
                            _animator.SetBool(_animIDWalkJump, Grounded);
                        }
                        else
                            _animator.SetBool(_animIDIdleJump, Grounded);
                    }


                }

                // jump timeout
                if (_jumpTimeoutDelta >= 0.0f)
                {
                    _jumpTimeoutDelta -= Time.deltaTime;
                }
            }
            else
            {
                // reset the jump timeout timer
                _jumpTimeoutDelta = JumpTimeout;

                // fall timeout
                if (_fallTimeoutDelta >= 0.0f)
                {
                    _fallTimeoutDelta -= Time.deltaTime;
                }
                else
                {
                    // update animator if using character
                    if (_hasAnimator)
                    {
                        // _animator.SetBool(_animIDFreeFall, true);
                    }
                }

                // if we are not grounded, do not jump
                _input.jump = false;
            }

            // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
            if (_verticalVelocity < _terminalVelocity)
            {
                _verticalVelocity += Gravity * Time.deltaTime;
            }
        }

        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }

        private void OnDrawGizmosSelected()
        {
            Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

            if (Grounded) Gizmos.color = transparentGreen;
            else Gizmos.color = transparentRed;

            // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
            Gizmos.DrawSphere(
                new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z),
                GroundedRadius);
        }

        private void OnFootstep(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                if (FootstepAudioClips.Length > 0)
                {
                    var index = Random.Range(0, FootstepAudioClips.Length);
                    AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(_controller.center), FootstepAudioVolume);
                }
            }
        }

        private void OnLand(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(_controller.center), FootstepAudioVolume);
            }
        }
        public void SetSensitivity(float newSensitivity)
        {
            sensitivity = newSensitivity;
        }  
        public void Aiming(bool state)
        {
            isAiming = state;
        }
        public void FPSMode(bool state)
        {
            inFPSMode = state;
        }
        public void ShotFired(bool state)
        {
            fireBulletTime = 1.3f;
            firedBullet = state;
            if (shooterController.ReturnGuntype() == 0)
            {
                _animator.SetLayerWeight(1, Mathf.Lerp(_animator.GetLayerWeight(1), 1f, Time.deltaTime * smoothSpeed));
                _animator.SetLayerWeight(3, Mathf.Lerp(_animator.GetLayerWeight(3), 0f, Time.deltaTime * smoothSpeed));
                //_animator.SetLayerWeight(1, 1);
                //_animator.SetLayerWeight(3, 0);
            }
            else
            {
                _animator.SetLayerWeight(3, Mathf.Lerp(_animator.GetLayerWeight(3), 1f, Time.deltaTime * smoothSpeed));
                _animator.SetLayerWeight(1, Mathf.Lerp(_animator.GetLayerWeight(1), 0f, Time.deltaTime * smoothSpeed));
                //_animator.SetLayerWeight(3, 1); 
                //_animator.SetLayerWeight(1, 0);
            }
                
        }
        public void FiringContinous(bool state)
        {
            firing = state;
            //if(_animationBlend <= 0)
            //_animator.SetBool("Rifle Idle Firing", state);
        }
       public void GunSwapingGunChangeIn()
       {
            weaponSwitching.GunSwapVisualTakeIn();
       }
        public void GunSwapingGunChangeOut()
        {
            weaponSwitching.GunSwapVisualTakeOut();
        }
    }
}
