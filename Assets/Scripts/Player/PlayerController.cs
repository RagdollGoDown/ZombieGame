using UnityEngine;
using Utility;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine.InputSystem;
using Weapons;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(DamageableObject))]
public class PlayerController : MonoBehaviour,Interactor
{
    private static List<PlayerController> PLAYERS;

    private static int KICK_TRIGGER_PARAM_ID = Animator.StringToHash("Kick");
    private static int KICK_SPEED_PARAM_ID = Animator.StringToHash("Speed");

    private static float ACCELERATION_DEGRADATION_SPEED = 5;

    private static readonly int SHOOTABLE_LAYERMASK_VALUE = 65;
    private static Vector3[] KICK_RAYCAST_FORWARD_RIGHT_UP_SCALE = new Vector3[]
    {
        new Vector3(1,0,0), new Vector3(0.9f,0.1f,0), new Vector3(0.9f,0,0.1f), 
        new Vector3(0.9f,-0.1f,0), new Vector3(0.9f,0,-0.1f), new Vector3(0.9f,-0.05f,.05f),
        new Vector3(0.9f,0.05f, .05f), new Vector3(0.9f,-0.05f,-0.05f), new Vector3(0.9f,0.05f,-0.05f)
    };

    public enum PlayerState
    {
        Normal,
        Dead
    }

    private PlayerState _playerState;

    private CharacterController _characterController;
    private PlayerInput _playerInput;
    [Header("Camera mouvement")]
    private Transform _cameraTransform;
    private Transform _cameraHolderTransform;
    [SerializeField] private float maximumHeadSway = 0.07f;
    [SerializeField] private float headSwaySpeed = 5;
    [SerializeField] private float maximumHeadBob = 0.1f;
    [SerializeField] private float headBobSpeed = 1;
    private float _headBobTime;
    private Vector3 _tempHeadBobDirection;

    private DamageableObject _damageablePlayer;

    //--------------------movement
    [Header("Mouvement")]
    private Vector2 _movementDirection;
    private Vector3 _movementVector;
    private Vector3 _movementSpeedAffectedByAcceleration;
    private Vector3 _movementSpeedGravity;
    private float _currentMovementSpeed;
    [SerializeField] private float movementSpeed;
    [SerializeField] private float movementSpeedWhenAiming;
    
    private Vector2 _mouseDelta;
    private Vector2 _headRotation;
    [SerializeField] float mouseSensitivity;

    //----------------------------ui
    private PlayerUI _playerUI;

    //----------------------------shooting
    //temporarily a serialize field to check the guns
    private int _currentWeaponIndex;
    [SerializeField] private WeaponBehaviour[] _weaponsHeld;

    private Dictionary<string,WeaponBehaviour> _onPlayerWeaponsToName;

    [SerializeField] private float timeBetweenWeaponSwitches;
    private float lastTimeSwitched;

    //---------------------------interaction
    private Interaction _currentInteract;
    private List<Interaction> _interactions;

    //---------------------------kicking
    private Animator _kickAnimator;
    [SerializeField] private float kickDamage;
    [SerializeField] private float kickRange;
    [SerializeField] private float kickAnimationLength;
    [SerializeField] private float kickDuration;
    private float _lastTimeKicked;

    //--------------------------------------------------general
    private void GunSetup()
    {
        _onPlayerWeaponsToName = new Dictionary<string, WeaponBehaviour>();
        _currentWeaponIndex = 0;

        foreach (Transform t in transform.Find("CameraAndGunHolder/GunHolder"))
        {
            if (t.TryGetComponent(out WeaponBehaviour wpb))
            {
                _onPlayerWeaponsToName.Add(t.name, wpb);

                if (wpb is RayCastGunBehaviour) { ((RayCastGunBehaviour)wpb).SetSpreadOrigin(_cameraTransform); }

                //t.gameObject.SetActive(_weaponsHeld[0] == wpb || _weaponsHeld[1] == wpb);
                t.gameObject.SetActive(_weaponsHeld[_currentWeaponIndex] == wpb);
            }
        }
    }

    private void PlayerMovement(float deltaTime)
    {
        MovePlayer(deltaTime);
        PlayerCameraMouvement(deltaTime);
        CalculateSway(_movementDirection, deltaTime);
    }

    private void PlayerAcceleration(float deltaTime)
    {
        _movementSpeedAffectedByAcceleration -= 
            ACCELERATION_DEGRADATION_SPEED * _movementSpeedAffectedByAcceleration * deltaTime;

        //-----------------acceleration
        if (!_characterController.isGrounded)
        {
            //add gravity
            _movementSpeedGravity.y -= deltaTime * 9.81f;
        }
        else
        {
            //make sure when the player is grounded he doesn't go through the floor
            if (_movementSpeedAffectedByAcceleration.y < 0)
            {
                _movementSpeedAffectedByAcceleration.y = 0;
            }

            if (_movementSpeedGravity.y < 0)
            {
                _movementSpeedGravity.y = 0;
            }
        }
    }

    private void MovePlayer(float deltaTime)
    {
        PlayerAcceleration(deltaTime);

        //---------------normal movement
        _movementVector = transform.right * _movementDirection.x + transform.forward * _movementDirection.y;
        _movementVector *= deltaTime *_currentMovementSpeed;

        _movementVector += (_movementSpeedAffectedByAcceleration+_movementSpeedGravity) * deltaTime;

        _characterController.Move(_movementVector);
    }

    private void PlayerCameraMouvement(float deltaTime)
    {
        //--------------head turning
        _mouseDelta *= Time.deltaTime * mouseSensitivity;
        _headRotation += _mouseDelta;
        _headRotation.y = Mathf.Clamp(_headRotation.y, -90f, 90f);

        _cameraHolderTransform.localEulerAngles = Vector3.right * -_headRotation.y;
        transform.localEulerAngles = Vector3.up * _headRotation.x;

        CalculateSway(new Vector3(_movementDirection.x, 0, _movementDirection.y), deltaTime);
        CalculateBob(deltaTime);
    }

    private void HandleKickDamageAndRayCast() 
    {
        RaycastHit hit;

        foreach (Vector3 v in KICK_RAYCAST_FORWARD_RIGHT_UP_SCALE)
        {
            Physics.Raycast(_cameraTransform.position, 
                _cameraTransform.forward * v.x + _cameraTransform.right * v.y + _cameraTransform.up * v.z,
                out hit, kickRange, SHOOTABLE_LAYERMASK_VALUE);

            if (hit.transform && hit.transform.TryGetComponent(out DamageableObject damObj))
            {
                damObj.getHit.Invoke(new Damage(kickDamage, _cameraTransform.forward, this));
            }
        }
    }

    //--------------------------------------------------unity events
    private void Awake()
    {
        if (PLAYERS == null) PLAYERS = new List<PlayerController>();

        PLAYERS.Add(this);

        _playerState = PlayerState.Normal;

        _characterController = GetComponent<CharacterController>();
   
        _playerInput = GetComponent<PlayerInput>();
        PlayerScore.ResetScore();

        _cameraHolderTransform = transform.Find("CameraAndGunHolder").transform;

        _cameraTransform = _cameraHolderTransform.Find("PlayerCamera").transform;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        _movementDirection = new Vector2();
        _currentMovementSpeed = movementSpeed;

        _playerUI = _cameraTransform.Find("UI").GetComponent<PlayerUI>();

        _damageablePlayer = GetComponent<DamageableObject>();
        _damageablePlayer.getHit.AddListener(_playerUI.UpdateHealthBar);
        
        GunSetup();

        _interactions = new List<Interaction>();

        _kickAnimator = transform.Find("CameraAndGunHolder/PlayerCamera/GunCamera/KickAnimations").GetComponent<Animator>();
        _kickAnimator.SetFloat(KICK_SPEED_PARAM_ID, kickAnimationLength / kickDuration);
    }

    private void FixedUpdate()
    {
        if (_playerState == PlayerState.Normal)
        {
            PlayerMovement(Time.fixedDeltaTime);
            PlayerScore.AddDeltaTimeToTimeSurvived(Time.fixedDeltaTime);
        }
    }

    private void OnDisable()
    {
        PLAYERS.Remove(this);
    }

    //---------------------------------------------------------------------ui
    private void CalculateSway(Vector3 direction, float deltaTime)
    {
        direction = direction.normalized;

        _cameraTransform.localPosition += (direction * maximumHeadSway - _cameraTransform.localPosition) * headSwaySpeed * deltaTime;
    }

    private void CalculateBob(float deltaTime)
    {
        _headBobTime += deltaTime * headBobSpeed * _characterController.velocity.magnitude;
        if (_headBobTime >= 2*Mathf.PI) { _headBobTime = 0; }

        _tempHeadBobDirection.x = Mathf.Cos(_headBobTime);
        _tempHeadBobDirection.y = Mathf.Sin(2 * _headBobTime);

        _cameraTransform.localPosition += maximumHeadBob*_tempHeadBobDirection;
    }
    
    //------------------------------------------------------------------weapons
    public RaycastHit GetRaycastHitInFrontOfCamera(float spread)
    {
        float randomAngle = Random.Range(0, 2 * Mathf.PI);

        Vector3 spreadDiff = _cameraTransform.up * Mathf.Cos(randomAngle) + _cameraTransform.right * Mathf.Sin(randomAngle);
        spreadDiff *= spread * Random.value;

        Physics.Raycast(_cameraTransform.position + spreadDiff, _cameraTransform.forward + spreadDiff, out RaycastHit hit, 1000,SHOOTABLE_LAYERMASK_VALUE);
        return hit;
    }

    //---------------------------------------------------------------interactions
    public void OnInteractableEntered(Interaction interaction)
    {
        if (!_interactions.Contains(interaction))
        {
            _interactions.Add(interaction);
        }

        UpdateCurrentInteraction();
    }

    public void OnInteractableExit(Interaction interaction)
    {
        _interactions.Remove(interaction);

        UpdateCurrentInteraction();
    }

    private void UpdateCurrentInteraction()
    {
        if (_interactions.Count == 0) 
        {
            _playerUI.SetInteractionText("");
            _currentInteract = null;
        }
        else
        {
            _playerUI.SetInteractionText("Press [E] to " + _interactions[0].GetInteractionText());
            _currentInteract = _interactions[0];
        }
    }

    /*
     * returns false if the player didn't pick up the weapon
     * 
     * replaces the weapon currently equipped with the weapon given, identified by it's name
     * if the weapon isn't identified then it throws illegal argument Exception
     */
    public bool PickUpWeapon(string weaponName)
    {
        if (_onPlayerWeaponsToName.TryGetValue(weaponName, out WeaponBehaviour newWeapon))
        {
            _weaponsHeld[_currentWeaponIndex].gameObject.SetActive(false);

            newWeapon.gameObject.SetActive(true);

            //if he already has the weapon just refill it and switch to it
            if (_weaponsHeld[0] == newWeapon)
            {
                _currentWeaponIndex = 0;
            }
            else if (_weaponsHeld[1] == newWeapon)
            {
                _currentWeaponIndex = 1;
            }
            else
            {
                _weaponsHeld[_currentWeaponIndex] = newWeapon;
            }

            _weaponsHeld[_currentWeaponIndex].RefillWeaponAmmo();

            return true;
        }
        else
        {
            throw new System.ArgumentException("No such weapon");
        }
    }

    /*
     * used to see if the player needs more ammo or a new weapon
     */
    public float GetPlayerAmmoFillRatio()
    {
        return (_weaponsHeld[0].GetAmmoFillRatio() + _weaponsHeld[1].GetAmmoFillRatio()) / 2;
    }

    //-----------------------------------------player state
    public void Die()
    {
        _playerState = PlayerState.Dead;
    }

    //----------------------------input events
    public void SetMovementDirection(InputAction.CallbackContext context)
    {
        _movementDirection = context.ReadValue<Vector2>();
    }
    
    public void SetMouseDelta(InputAction.CallbackContext context)
    {
        _mouseDelta = context.ReadValue<Vector2>();
    }

    public void UseWeaponInput(InputAction.CallbackContext context)
    {
        if (_playerState == PlayerState.Dead) return;

        _weaponsHeld[_currentWeaponIndex].UseWeaponInput(context);
    }
    
    public void ReloadCurrentGun(InputAction.CallbackContext context)
    {
        if (_playerState == PlayerState.Dead) return;

        _weaponsHeld[_currentWeaponIndex].ReloadInput(context);
    }

    public void AimCurrentGun(InputAction.CallbackContext context)
    {
        if (_playerState == PlayerState.Dead) return;
        _weaponsHeld[_currentWeaponIndex].AimInputAction(context);

        if (context.started) { _currentMovementSpeed = movementSpeedWhenAiming; }
        if (context.canceled) { _currentMovementSpeed = movementSpeed; }
    }

    public void SwitchWeaponsInput(InputAction.CallbackContext context)
    {
        if (_playerState == PlayerState.Dead) return;

        if (context.started)
        {
            _weaponsHeld[_currentWeaponIndex].gameObject.SetActive(false);
            _weaponsHeld[_currentWeaponIndex].AmmoText.RemoveActionToOnValueChange(_playerUI.SetAmmoText);

            if (_currentWeaponIndex == 0)
            {
                _currentWeaponIndex = 1;
            }
            else
            {
                _currentWeaponIndex = 0;
            }

            _weaponsHeld[_currentWeaponIndex].gameObject.SetActive(true);
            _weaponsHeld[_currentWeaponIndex].AmmoText.AddActionToOnValueChange(_playerUI.SetAmmoText);
            _playerUI.SetWeaponName(_weaponsHeld[_currentWeaponIndex].name);
        }
    }

    public void InteractInput(InputAction.CallbackContext context)
    {
        if (context.started && _currentInteract != null) { 
            _currentInteract.GetAction().Invoke();
            OnInteractableExit(_currentInteract);
        }
    }

    public void KickInput(InputAction.CallbackContext context)
    {
        if (_playerState == PlayerState.Dead) return;

        if (context.started && Time.time - _lastTimeKicked > kickDuration)
        {
            _kickAnimator.SetTrigger(KICK_TRIGGER_PARAM_ID);

            _lastTimeKicked = Time.time;

            HandleKickDamageAndRayCast();
        }
    }

    //--------------------------------------------------------getters

    public DamageableObject GetPlayerTargetComponent() { return _damageablePlayer; }

    public float GetPlayerHealthRatio() { return _damageablePlayer.GetHealthRatio(); }

    public PlayerState GetPlayerState() { return _playerState; }

    public static ReadOnlyCollection<PlayerController> GetPlayers()
    {
        return PLAYERS.AsReadOnly();
    }

    //--------------------------------------------------------setters

    /*
     * makes the round text show which is the current one
     */
    public void SetRoundText(int round)
    {
        _playerUI.SetRoundText(round);
    }

    public void SetObjectiveText(string text)
    {
        _playerUI.SetObjectiveText(text);
    }
}

