using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public abstract class WeaponBehaviour : MonoBehaviour
{
    private PlayerController _playerController;
    private TextMeshProUGUI _ammoTextHolder;
    private TextMeshProUGUI _weaponNameTextHolder;
    private RectTransform _crosshairTransform;
    private Canvas _playerCanvas;
    private CanvasScaler _canvasScaler;
    private Camera _playerCamera;

    private Animator _gunAnimator;

    [Header("General")]
    [SerializeField] private float reloadDuration = 1;
    [SerializeField] private float timeToEquip = 1;
    //[SerializeField] private Transform UIModel;

    private float _uiScale;

    //---------------------------------state bools
    private bool _isReloading;

    private bool _isShooting;
    private bool _isAiming;

    private bool _isHolstered = true;
    private bool _isSwitching;

    [Header("Animations and effects")]
    [SerializeField] private int shotsPerShootingAnimation = 1;
    [SerializeField] private float shootingAnimationLength = 1;
    [SerializeField] private float reloadAnimationTime = 1;
    [SerializeField] private float switchAnimationLength = 1;

    protected virtual void AwakeWeapon()
    {
        if (reloadDuration <= 0) throw new ArgumentException("duration not strictly positiv");
        if (timeToEquip <= 0) throw new ArgumentException("time to equip not strictly positiv");
        //if (!UIModel) throw new ArgumentException("no UI model");

        _playerController = transform.GetComponentInParent<PlayerController>();
        InfoForGunSetup IFGS = _playerController.playerInfoForGunSetup;
        _ammoTextHolder = IFGS.GetAmmoTextHolder();
        _weaponNameTextHolder = IFGS.GetWeaponNameTextHolder();
        _crosshairTransform = IFGS.GetCrosshairTransform();
        _canvasScaler = IFGS.GetCanvasScaler();
        _playerCanvas = IFGS.GetCanvas();
        _playerCamera = IFGS.GetPlayerCamera();

        _gunAnimator = GetComponent<Animator>();
        ReadyAnimationLengths(_gunAnimator);
    }

    protected virtual void ReadyAnimationLengths(Animator animator)
    {
        animator.SetFloat("reloadSpeed", reloadAnimationTime / reloadDuration);
        animator.SetFloat("shootingSpeed", shootingAnimationLength / shotsPerShootingAnimation);
        animator.SetFloat("switchSpeed", switchAnimationLength / timeToEquip);
    }

    protected abstract void UpdateWeapon();

    //-------------------------------inputs
    public virtual void ShootInputAction(InputAction.CallbackContext context)
    {
        if (context.started && !_isReloading && !_isHolstered)
        {
            StartShooting();
        }
        else if (context.canceled)
        {
            StopShooting();
        }
    }
    
    public virtual void ReloadInputAction(InputAction.CallbackContext context)
    {
        if (context.started && !_isShooting && ReloadConditions() && !_isHolstered)
        {
            StartReload();
        }
    }

    public virtual void AimInputAction(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            _isAiming = true;
        }
        else if (context.canceled)
        {
            _isAiming = false;
        }
    }

    public void SwitchInputAction(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            EquipOrUnequip();
        }
    }

    //---------------------------------------shooting
    protected abstract void ShootGun();

    protected virtual void StartShooting()
    {
        _isShooting = true;
        if (_gunAnimator)_gunAnimator.SetBool("IsShooting", true);
    }

    protected virtual void StopShooting()
    {
        _isShooting = false;
       if(_gunAnimator)_gunAnimator.SetBool("IsShooting", false);
    }

    protected virtual void ValidateKill(){}

    //----------------------------------------reloading
    protected abstract bool ReloadConditions();

    protected virtual void StartReload()
    {
        if (!ReloadConditions()) return;

        _isReloading = true;
        if(_gunAnimator)_gunAnimator.SetBool("IsReloading", true);
        Invoke("StopAndAccomplishReload", reloadDuration);
    }

    protected virtual void StopAndAccomplishReload()
    {
        _isReloading = false;
        if(_gunAnimator)_gunAnimator.SetBool("IsReloading", false);
    }

    protected virtual void StopReload()
    {
        CancelInvoke("StopAndAccomplishReload");
        _isReloading = false;
        if(_gunAnimator)_gunAnimator.SetBool("IsReloading", false);
    }

    //--------------------------------------------equiping
    public void EquipOrUnequip()
    {
        if (!_isSwitching)
        {
            if (_isHolstered)
            {
                StartCoroutine(nameof(EquipWeapon));
            }
            else
            {
                StartCoroutine(nameof(UnequipWeapon));
            }
        }
    }

    private IEnumerator UnequipWeapon()
    {
        _isSwitching = true;
        _isHolstered = true;

        _gunAnimator.SetBool("IsSwitching", true);
        StopReload();

        StopShooting();

        //the param change needs to be done earlier or it will cycle to the equip animation
        yield return new WaitForSeconds(timeToEquip/2);
        _gunAnimator.SetBool("IsSwitching", false);

        yield return new WaitForSeconds(timeToEquip / 2);
        _isSwitching = false;
        _playerController.EquipNextWeapon();
    }

    private IEnumerator EquipWeapon()
    {
        _isSwitching = true;
        _gunAnimator.SetBool("IsSwitching", true);

        //the param change needs to be done earlier or it will cycle to the unequip animation
        yield return new WaitForSeconds(timeToEquip/2);
        _gunAnimator.SetBool("IsSwitching", false);

        yield return new WaitForSeconds(timeToEquip / 2);
        _isHolstered = false;
        _isSwitching = false;

        UpdateAmmoText();
        UpdateWeaponNameText();
    }

    public abstract void RefillWeaponAmmo();

    //----------------------------------------------ui
    protected virtual void UpdateAmmoText() { }

    private void UpdateWeaponNameText() 
    {
        _weaponNameTextHolder.text = name;
    }

    protected virtual void UpdateCrosshair() { }

    //this is public because we want to access it from the options menu if we ever have one
    public void UpdateUIScale()
    {
        _uiScale = _canvasScaler.referencePixelsPerUnit * _canvasScaler.referenceResolution.x / _playerCamera.fieldOfView;
    }

    //-------------------------------------------------getters

    protected PlayerController GetPlayerController()
    {
        return _playerController;
    }

    protected Animator GetAnimator()
    {
        return _gunAnimator;
    }

    //-------------------------------------------------getters/setters_state

    public bool IsReadyForSwitch()
    {
        return !_isSwitching;
    }

    protected bool GetIsShooting()
    {
        return _isShooting;
    }

    protected bool GetIsReloading()
    {
        return _isReloading;
    }

    protected void SetIsReloading(bool val)
    {
        _isReloading = val;
    }

    protected bool GetIsHolstered()
    {
        return _isHolstered;
    }

    protected bool GetIsAiming()
    {
        return _isAiming;
    }

    //tells us if the gun is being switched or not
    public bool GetIsSwitching()
    {
        return _isSwitching;
    }

    /*
     * Give the current bullets left on player divided by the max bullets on player
     */
    public abstract float GetAmmoFillRatio();

    //---------------------------------------getters_ui
    protected Camera GetPLayerCamera()
    {
        return _playerCamera;
    }

    protected RectTransform GetCrosshairTransform()
    {
        return _crosshairTransform;
    }

    protected TextMeshProUGUI GetAmmoTextHolder()
    {
        return _ammoTextHolder;
    }

    protected float GetUIScale()
    {
        return _uiScale;
    }

    /*public Transform GetWeaponModelTransform()
    {
        return UIModel;
    }*/
}

public class InfoForGunSetup
{
    private readonly TextMeshProUGUI _ammoTextHolder;
    private readonly TextMeshProUGUI _weaponNameTextHolder;
    private readonly RectTransform _crosshairTransform;
    private readonly CanvasScaler _canvasScaler;
    private readonly Canvas _canvas;
    private readonly Camera _playerCamera;

    public InfoForGunSetup(TextMeshProUGUI ammoTextHolder, TextMeshProUGUI weaponNameTextHolder
        , RectTransform crosshairTransform, CanvasScaler canvasScaler, Canvas canvas, Camera playerCamera)
    {
        _ammoTextHolder = ammoTextHolder;
        _weaponNameTextHolder = weaponNameTextHolder;
        _crosshairTransform = crosshairTransform;
        _canvasScaler = canvasScaler;
        _canvas = canvas;
        _playerCamera = playerCamera;
    }

    public TextMeshProUGUI GetAmmoTextHolder()
    {
        return _ammoTextHolder;
    }
    public TextMeshProUGUI GetWeaponNameTextHolder()
    {
        return _weaponNameTextHolder;
    }
    public RectTransform GetCrosshairTransform()
    {
        return _crosshairTransform;
    }
    public CanvasScaler GetCanvasScaler()
    {
        return _canvasScaler;
    }
    public Canvas GetCanvas()
    {
        return _canvas;
    }
    public Camera GetPlayerCamera()
    {
        return _playerCamera;
    }
}
