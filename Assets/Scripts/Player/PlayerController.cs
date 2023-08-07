using TMPro;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Collections.ObjectModel;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(DamageableObject))]
[RequireComponent(typeof(ZombieTarget))]
public class PlayerController : MonoBehaviour
{
    private static List<PlayerController> PLAYERS;

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
    private ZombieTarget _playerTarget;

    //--------------------movement
    [Header("Mouvement")]
    private Vector2 _movementDirection;
    private Vector3 _movementVector;
    private Vector3 _movementSpeedAffectedByAcceleration;
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
    public InfoForGunSetup playerInfoForGunSetup;
    private static readonly int shootableLayerMaskValue = 65;

    private Dictionary<string,WeaponBehaviour> _onPlayerWeaponsToName;

    //---------------------------interaction
    private Interaction _currentInteract;
    private List<Interaction> _interactions;

    //--------------------------------------------------general
    private void GunSetup()
    {
        playerInfoForGunSetup = new InfoForGunSetup(
           _cameraTransform.Find("UI/PlayScreen/Ammo/AmmoTextHolder").GetComponent<TextMeshProUGUI>(),
           _cameraTransform.Find("UI/PlayScreen/Ammo/WeaponNameTextHolder").GetComponent<TextMeshProUGUI>(),
           _cameraTransform.Find("UI/PlayScreen/Crosshair").GetComponent<RectTransform>(),
           _cameraTransform.Find("UI").GetComponent<CanvasScaler>(),
           _cameraTransform.Find("UI").GetComponent<Canvas>(),
           _cameraTransform.GetComponent<Camera>()
           );

        _onPlayerWeaponsToName = new Dictionary<string, WeaponBehaviour>();

        foreach (Transform t in transform.Find("CameraAndGunHolder/GunHolder"))
        {
            if (t.TryGetComponent(out WeaponBehaviour wpb))
            {
                _onPlayerWeaponsToName.Add(t.name, wpb);

                t.gameObject.SetActive(_weaponsHeld[0] == wpb || _weaponsHeld[1] == wpb);
            }
        }

        _currentWeaponIndex = 0;
        Invoke("EquipNextWeapon", Time.fixedDeltaTime);
    }

    private void PlayerMovement(float deltaTime)
    {
        MovePlayer(deltaTime);
        PlayerCameraMouvement(deltaTime);
        CalculateSway(_movementDirection, deltaTime);
    }

    private void MovePlayer(float deltaTime)
    {
        //-----------------acceleration
        if (!_characterController.isGrounded)
        {
            //add gravity
            _movementSpeedAffectedByAcceleration.y -= deltaTime * 9.81f;
        }
        else if (_movementSpeedAffectedByAcceleration.y < 0)
        {
            _movementSpeedAffectedByAcceleration.y = 0;
        }

        //---------------normal movement
        _movementVector = transform.right * _movementDirection.x + transform.forward * _movementDirection.y;
        _movementVector *= deltaTime * _currentMovementSpeed;

        _movementVector += _movementSpeedAffectedByAcceleration * deltaTime;

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
        _playerTarget = GetComponent<ZombieTarget>();

        GunSetup();

        _interactions = new List<Interaction>();
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

        float distance = Vector3.Distance(direction * maximumHeadSway, _cameraTransform.localPosition);
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
        RaycastHit hit;

        float randomAngle = UnityEngine.Random.Range(0,2*Mathf.PI);

        Vector3 spreadDiff = _cameraTransform.up * Mathf.Cos(randomAngle) + _cameraTransform.right * Mathf.Sin(randomAngle);
        spreadDiff *= spread * UnityEngine.Random.value;

        Physics.Raycast(_cameraTransform.position + spreadDiff, _cameraTransform.forward + spreadDiff, out hit, 1000,shootableLayerMaskValue);
        return hit;
    }

    public void EquipNextWeapon()
    {
        _weaponsHeld[_currentWeaponIndex].EquipOrUnequip();

        //_playerUI.SetUIWeaponModel(_weaponsHeld[_currentWeaponIndex].GetWeaponModelTransform());
    }

    //---------------------------------------------------------------interactions
    public void AddInteractListener(Interaction interaction)
    {
        if (!_interactions.Contains(interaction))
        {
            _interactions.Add(interaction);
        }

        UpdateCurrentInteraction();
    }

    public void RemoveInteractListener(Interaction interaction)
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
        if (_weaponsHeld[0].GetIsSwitching() || _weaponsHeld[1].GetIsSwitching()) return false;

        if (_onPlayerWeaponsToName.TryGetValue(weaponName, out WeaponBehaviour newWeapon))
        {
            _weaponsHeld[_currentWeaponIndex].StartCoroutine("UnequipWeapon");

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
    private void Die()
    {
        _playerState = PlayerState.Dead;

        _playerUI.Die();
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

    public void ShootCurrentGun(InputAction.CallbackContext context)
    {
        if (_playerState != PlayerState.Dead)_weaponsHeld[_currentWeaponIndex].ShootInputAction(context);
    }
    
    public void ReloadCurrentGun(InputAction.CallbackContext context)
    {
        _weaponsHeld[_currentWeaponIndex].ReloadInputAction(context);
    }
    public void AimCurrentGun(InputAction.CallbackContext context)
    {
        _weaponsHeld[_currentWeaponIndex].AimInputAction(context);

        if (context.started) { _currentMovementSpeed = movementSpeedWhenAiming; }
        if (context.canceled) { _currentMovementSpeed = movementSpeed; }
    }

    public void SwitchWeapons(InputAction.CallbackContext context)
    {
        if (_weaponsHeld[0].IsReadyForSwitch() && _weaponsHeld[1].IsReadyForSwitch() && context.started)
        {
            _weaponsHeld[_currentWeaponIndex].StartCoroutine("UnequipWeapon");

            if (_currentWeaponIndex == 0)
            {
                _currentWeaponIndex = 1;
            }
            else
            {
                _currentWeaponIndex = 0;
            }
        }
    }

    public void Interact(InputAction.CallbackContext context)
    {
        if (context.started && _currentInteract != null) { 
            _currentInteract.GetAction().Invoke();
            RemoveInteractListener(_currentInteract);
        }
    }

    //--------------------------------------------------------getters

    public ZombieTarget GetPlayerTargetComponent() { return _playerTarget; }

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

    public void SetObjectiveText(Objective objective)
    {
        _playerUI.SetObjectiveText(objective);
    }
}

