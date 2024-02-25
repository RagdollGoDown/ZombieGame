using UnityEngine;
using UnityEngine.Events;
using Utility;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine.InputSystem;
using Weapons;
using Utility.Observable;
using Objectives;
using System;

namespace Player
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(DamageableObject))]
    public class PlayerController : MonoBehaviour,Interactor
    {
        private static List<PlayerController> PLAYERS;

        private static int KICK_TRIGGER_PARAM_ID = Animator.StringToHash("Kick");
        private static int KICK_SPEED_PARAM_ID = Animator.StringToHash("Speed");

        private static float ACCELERATION_DEGRADATION_SPEED = 5;

        private static float TIMESCALE_IN_SLOWMO = 0.2f;

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
            InMenu,
            Dead
        }

        private PlayerState playerState;

        private CharacterController _characterController;
        private PlayerInput _playerInput;

        private int moneyOnPlayer;

        [Header("Camera mouvement")]
        private Transform _cameraTransform;
        private Transform _cameraHolderTransform;
        [SerializeField] private float maximumHeadSway = 0.07f;
        [SerializeField] private float headSwaySpeed = 5;
        [SerializeField] private float maximumHeadBob = 0.1f;
        [SerializeField] private float headBobSpeed = 1;
        private float _headBobTime;
        private Vector3 _headSwayMotion;
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
        private PlayerUI playerUI;

        [Header("Weapons")]
        //----------------------------shooting
        //temporarily a serialize field to check the guns
        private int _currentWeaponIndex;
        [SerializeField] private int maxWeaponsHeld = 2;
        [SerializeField] private List<WeaponBehaviour> _weaponsHeld;

        private Dictionary<string,WeaponBehaviour> _onPlayerWeaponsToName;

        [SerializeField] private float timeBetweenWeaponSwitches;
        private float lastTimeSwitched;

        [Header("Slow Mo")]
        //--------------------------slow mo
        private bool isInSlowMo;
        private float slowMoCharge;
        [SerializeField] private float slowMoMaxCharge = 1;
        [SerializeField] private float slowMoDecreaseSpeed = 0.2f;
        [SerializeField] private float slowMoRegenSpeed = 0.2f;

        //---------------------------interaction
        private Interaction _currentInteract;
        private List<Interaction> _interactions;

        //---------------------------kicking
        [Header("Kicking")]
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

            if (_weaponsHeld.Count > maxWeaponsHeld) throw new System.ArgumentException("Too many weapons held on the player!");

            foreach (Transform t in transform.Find("CameraAndGunHolder/GunHolder"))
            {
                if (t.TryGetComponent(out WeaponBehaviour wpb))
                {
                    _onPlayerWeaponsToName.Add(t.name, wpb);

                    if (wpb is CrossHaired) { ((CrossHaired)wpb).SetSpreadOrigin(_cameraTransform); }

                    //t.gameObject.SetActive(_weaponsHeld[0] == wpb || _weaponsHeld[1] == wpb);
                    t.gameObject.SetActive(_weaponsHeld[_currentWeaponIndex] == wpb);
                }
            }

            Invoke(nameof(EquipCurrentWeapon), Time.deltaTime);
        }

        private void PlayerMovement(float deltaTime)
        {
            MovePlayer(deltaTime);
            PlayerCameraMouvement(deltaTime);
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
            _mouseDelta *= deltaTime * mouseSensitivity;
            _headRotation += _mouseDelta;
            _headRotation.y = Mathf.Clamp(_headRotation.y, -90f, 90f);

            _cameraHolderTransform.localEulerAngles = Vector3.right * -_headRotation.y;
            transform.localEulerAngles = Vector3.up * _headRotation.x;

            ApplyBobandSway(deltaTime);
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
                    damObj.GetHitEvent().Invoke(new Damage(kickDamage, _cameraTransform.forward, this));
                }
            }
        }

        

        
        private void ApplyBobandSway(float deltaTime)
        {
            //--------------head bob
            _headBobTime += deltaTime * headBobSpeed * _characterController.velocity.magnitude;
            if (_headBobTime >= 2*Mathf.PI) { _headBobTime = 0; }

            _tempHeadBobDirection.x = Mathf.Cos(_headBobTime);
            _tempHeadBobDirection.y = Mathf.Sin(2 * _headBobTime);

            //--------------head sway
            Vector3 swayDirection = new Vector3(_movementDirection.x, 0, _movementDirection.y).normalized;

            _headSwayMotion  += (swayDirection * maximumHeadSway - _cameraTransform.localPosition) * headSwaySpeed * deltaTime;
            
            _cameraTransform.localPosition = maximumHeadBob*_tempHeadBobDirection + _headSwayMotion;
        }

        //------------------------------------------SlowMo

        private void UpdateSlowMo(float unscaledDeltaTime)
        {
            if (isInSlowMo)
            {
                if (slowMoCharge >= 0)
                {
                    slowMoCharge -= unscaledDeltaTime * slowMoDecreaseSpeed;
                    playerUI.UpdateSlowMoChargeSliders(slowMoCharge);
                }
                else
                {
                    ExitSlowMo();
                }
            }
            else
            {
                if (slowMoCharge < slowMoMaxCharge)
                {
                    slowMoCharge += unscaledDeltaTime * slowMoRegenSpeed;
                    playerUI.UpdateSlowMoChargeSliders(slowMoCharge);
                }
            }
        }

        private void EnterSlowMo()
        {
            if (slowMoCharge <= 0 || isInSlowMo) return;

            isInSlowMo = true;
            Time.timeScale = TIMESCALE_IN_SLOWMO;
        }

        private void ExitSlowMo()
        {
            if (!isInSlowMo) return;

            isInSlowMo = false;
            Time.timeScale = 1;
        }

        //--------------------------------------------------unity events
        private void Awake()
        {
            if (PLAYERS == null) PLAYERS = new List<PlayerController>();

            PLAYERS.Add(this);

            playerState = PlayerState.Normal;

            _characterController = GetComponent<CharacterController>();
    
            _playerInput = GetComponent<PlayerInput>();
            PlayerScore.ResetScore();

            _cameraHolderTransform = transform.Find("CameraAndGunHolder").transform;

            _cameraTransform = _cameraHolderTransform.Find("PlayerCamera").transform;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            _movementDirection = new Vector2();
            _currentMovementSpeed = movementSpeed;

            playerUI = _cameraTransform.Find("UI").GetComponent<PlayerUI>();

            _damageablePlayer = GetComponent<DamageableObject>();
            _damageablePlayer.GetHitEvent().AddListener(playerUI.UpdateHealthBar);
            
            GunSetup();

            _interactions = new List<Interaction>();

            _kickAnimator = transform.Find("CameraAndGunHolder/PlayerCamera/GunCamera/KickAnimations").GetComponent<Animator>();
            _kickAnimator.SetFloat(KICK_SPEED_PARAM_ID, kickAnimationLength / kickDuration);
        }

        private void Update()
        {
            if (playerState == PlayerState.Normal)
            {
                float deltaTime = Time.unscaledDeltaTime * Time.timeScale;

                PlayerMovement(deltaTime);
                PlayerScore.AddDeltaTimeToTimeSurvived(deltaTime);
                UpdateSlowMo(Time.unscaledDeltaTime);
            }
        }

        private void OnDisable()
        {
            PLAYERS.Remove(this);
        }

        //------------------------------------------------------------------weapons
        public RaycastHit GetRaycastHitInFrontOfCamera(float spread)
        {
            float randomAngle = UnityEngine.Random.Range(0, 2 * Mathf.PI);

            Vector3 spreadDiff = _cameraTransform.up * Mathf.Cos(randomAngle) + _cameraTransform.right * Mathf.Sin(randomAngle);
            spreadDiff *= spread * UnityEngine.Random.value;

            Physics.Raycast(_cameraTransform.position + spreadDiff, _cameraTransform.forward + spreadDiff, out RaycastHit hit, 1000,SHOOTABLE_LAYERMASK_VALUE);
            return hit;
        }

        private void Switch()
        {
            UnequipCurrentWeapon();
            _currentWeaponIndex = (_currentWeaponIndex + 1) % _weaponsHeld.Count;

            EquipCurrentWeapon();
        }

        private void EquipCurrentWeapon()
        {
            if (_weaponsHeld.Count == 0) return;

            _weaponsHeld[_currentWeaponIndex].gameObject.SetActive(true);
            _weaponsHeld[_currentWeaponIndex].AmmoText.onValueChange += playerUI.SetAmmoText;
            if (_weaponsHeld[_currentWeaponIndex] is CrossHaired c)
            {
                c.GetSpread().onValueChange += playerUI.SetCrosshairScale;
            }
            playerUI.SetWeaponName(_weaponsHeld[_currentWeaponIndex].name);
        }

        private void UnequipCurrentWeapon()
        {
            if (_weaponsHeld.Count == 0) return;

            _weaponsHeld[_currentWeaponIndex].gameObject.SetActive(false);
            _weaponsHeld[_currentWeaponIndex].AmmoText.onValueChange -= playerUI.SetAmmoText;
            if (_weaponsHeld[_currentWeaponIndex] is CrossHaired)
            {
                CrossHaired c = (CrossHaired)_weaponsHeld[_currentWeaponIndex];

                c.GetSpread().onValueChange -= playerUI.SetCrosshairScale;
            }
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
                playerUI.SetInteractionText("");
                _currentInteract = null;
            }
            else
            {
                playerUI.SetInteractionText("Press [E] to " + _interactions[0].GetInteractionText());
                _currentInteract = _interactions[0];
            }
        }

        /* DEPRECATED does not support for when there are more than 2 weapons on the player
        * 
        * returns false if the player didn't pick up the weapon
        * 
        * replaces the weapon currently equipped with the weapon given, identified by it's name
        */
        public bool PickUpWeapon(string weaponName)
        {
            if (_onPlayerWeaponsToName.TryGetValue(weaponName, out WeaponBehaviour newWeapon))
            {
                UnequipCurrentWeapon();
               

                if (_weaponsHeld.Count < maxWeaponsHeld)
                {
                    _currentWeaponIndex = _weaponsHeld.Count - 1;
                    _weaponsHeld.Add(newWeapon);
                }
                else
                {
                    _weaponsHeld[_currentWeaponIndex] = newWeapon;
                }

                EquipCurrentWeapon();

                return true;
            }
            else
            {
                return false;
            }
        }

        public void OpenMenu(PlayerUI.Menu menu)
        {
            playerUI.OpenMenu(menu);

            playerState = PlayerState.InMenu;

            Time.timeScale = 0;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void CloseMenu()
        {
            playerUI.CloseMenu();

            playerState = PlayerState.Normal;

            Time.timeScale = 1;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        //-----------------------------------------player state
        public void Die()
        {
            playerState = PlayerState.Dead;
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
            if (playerState != PlayerState.Normal) return;

            _weaponsHeld[_currentWeaponIndex].UseWeaponInput(context);
        }
        
        public void ReloadCurrentGun(InputAction.CallbackContext context)
        {
            if (playerState != PlayerState.Normal) return;

            _weaponsHeld[_currentWeaponIndex].ReloadInput(context);
        }

        public void AimCurrentGun(InputAction.CallbackContext context)
        {
            if (playerState != PlayerState.Normal) return;
            _weaponsHeld[_currentWeaponIndex].AimInputAction(context);

            if (context.started) { EnterSlowMo(); }
            if (context.canceled) { ExitSlowMo(); }
        }

        public void SwitchWeaponsInput(InputAction.CallbackContext context)
        {
            if (playerState != PlayerState.Normal) return;

            if (context.started && lastTimeSwitched < Time.time - timeBetweenWeaponSwitches)
            {
                Switch();

                lastTimeSwitched = Time.time;
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
            if (playerState != PlayerState.Normal) return;

            if (context.started && Time.time - _lastTimeKicked > kickDuration)
            {
                _kickAnimator.SetTrigger(KICK_TRIGGER_PARAM_ID);

                _lastTimeKicked = Time.time;

                HandleKickDamageAndRayCast();
            }
        }

        public void Escape(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                if (playerState == PlayerState.Normal)
                {
                    OpenMenu(PlayerUI.Menu.Pause);
                }
                else if (playerState == PlayerState.InMenu)
                {
                    CloseMenu();
                }
            }
        }

        //--------------------------------------------------------getters

        public DamageableObject GetPlayerTargetComponent() { return _damageablePlayer; }

        public float GetPlayerHealthRatio() { return _damageablePlayer.GetHealthRatio(); }

        public PlayerState GetPlayerState() { return playerState; }

        public static ReadOnlyCollection<PlayerController> GetPlayers()
        {
            return PLAYERS.AsReadOnly();
        }

        public PlayerSaveData GetSaveData()
        {
            PlayerSaveData data = new();

            data.money = GetMoney();

            data.Weapons = new List<string>();

            foreach (WeaponBehaviour wpb in _weaponsHeld)
            {
                data.Weapons.Add(wpb.name);
            }

            data.currentWeaponIndex = _currentWeaponIndex;

            return data;
        }

        public int GetMoney() { return moneyOnPlayer; }

        //--------------------------------------------------------setters
        public void SetMission(Mission mission)
        {
            playerUI.SetMission(mission);
        }

        public void SetMoney(int money)
        {
            moneyOnPlayer = money;

            playerUI.SetMoneyText(money);
        }

        public void AddMoney(int money)
        {
            moneyOnPlayer += money;

            playerUI.SetMoneyText(moneyOnPlayer);
        }

        public void SetPlayerData(PlayerSaveData data)
        {
            moneyOnPlayer = data.money;

            UnequipCurrentWeapon();

            _weaponsHeld.Clear();

            foreach (string wpnName in data.Weapons)
            {
                if (_onPlayerWeaponsToName.TryGetValue(wpnName, out WeaponBehaviour newWeapon)){
                    _weaponsHeld.Add(newWeapon);
                }
                else{
                    Debug.LogError("Weapon not found: " + wpnName);
                }
            }

            if (data.currentWeaponIndex < _weaponsHeld.Count)
            {
                _currentWeaponIndex = data.currentWeaponIndex;
            }
            else
            {
                _currentWeaponIndex = 0;
            }
            
            EquipCurrentWeapon();
        }
    }    

    [Serializable]
    public struct PlayerSaveData
    {
        public int money;

        public List<string> Weapons;

        public int currentWeaponIndex;
    }
}