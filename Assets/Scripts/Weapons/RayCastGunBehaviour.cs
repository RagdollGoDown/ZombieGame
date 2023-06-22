using System.Collections;
using System.Collections.Generic;
using UnityEngine.VFX;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RayCastGunBehaviour : WeaponBehaviour
{
    private static readonly float BULLETHOLE_LIFETIME = 10;
    private static readonly float HITMARKER_LIFETIME = .1f;

    [Header("General Stats")]
    [SerializeField] private float fireRateForShots;
    private float _lastTimeShot;
    private int _currentEnemiesKilledByPellets;
    private int _currentEnemiesKilledByFireStream;
    [SerializeField] private float damage;
    [SerializeField] private int pelletsPerShot = 1;
    [SerializeField] private float range = 100;

    [Header("Spread Stats")]
    private float _spread;
    private float _spreadPositionInLerp;
    [SerializeField] private float _maxSpread;
    [SerializeField] private float _restingSpread;
    [SerializeField] private float _spreadDecreaseSpeed;
    [SerializeField] private float _spreadDecreaseSpeedWhenAiming;
    [SerializeField] private float _spreadIncreasePerShot;

    [Header("Visual Effects")]
    [SerializeField] private VisualEffect muzzleFlash;
    [SerializeField] private GameObject HitMarker;
    [SerializeField] private GameObject bulletHoleOnDefault;

    [Header("Ammo and Reloading")]
    protected int _ammoRemainingInMag;
    [SerializeField] protected int maxBulletsInMag;
    [SerializeField] protected int bulletsOnPlayer;
    [SerializeField] private int shotsPerTriggerPress = 100;
    private int _remainingShots;


    private void Awake()
    {
        AwakeWeapon();
    }

    protected override void AwakeWeapon()
    {
        base.AwakeWeapon();

        _ammoRemainingInMag = maxBulletsInMag;
        UpdateAmmoText();

        _spread = _restingSpread;
        UpdateUIScale();
    }

    protected override void ReadyAnimationLengths(Animator animator)
    {
        base.ReadyAnimationLengths(animator);

        animator.SetFloat("shootingSpeed", animator.GetFloat("shootingSpeed") / fireRateForShots);
    }

    private void Update()
    {
        UpdateWeapon();
    }

    protected override void UpdateWeapon()
    {
        if (GetIsShooting())
        {
            if (_lastTimeShot + fireRateForShots <= Time.time)
            {
                ShootGun();
            }

            if (_ammoRemainingInMag == 0 || _remainingShots == 0)
            {
                //we need to stop it next frame or else the animation for the shot won't start
                Invoke(nameof(StopShooting),Time.deltaTime);
            }
            else if (_ammoRemainingInMag == 0)
            {
                Invoke(nameof(StartReload), Time.deltaTime);
            }
        }
        
        //reduce spread when aiming
        if (GetIsAiming())
        {
            _spreadPositionInLerp = Mathf.Clamp01(_spreadPositionInLerp - _spreadDecreaseSpeedWhenAiming * Time.deltaTime);
        }
        //decrease spread when not shooting
        else
        {
            _spreadPositionInLerp = Mathf.Clamp01(_spreadPositionInLerp - _spreadDecreaseSpeed * Time.deltaTime);
        }

        if (!GetIsHolstered()) { UpdateCrosshair(); }
    }

    //---------------------------------------------shooting
    protected override void ShootGun()
    {
        //add spread when shooting
        _spreadPositionInLerp = Mathf.Clamp01(_spreadPositionInLerp + _spreadIncreasePerShot);

        //raycast shot
        Vector3 cameraPosition = GetPLayerCamera().transform.position;

        for (int i = 0; i < pelletsPerShot; i++)
        {
            RaycastHit pointShot = GetPlayerController().GetRaycastHitInFrontOfCamera(_spread);

            HandleRayCastShot(pointShot, cameraPosition);
        }
        if (muzzleFlash) muzzleFlash.Play();

        _lastTimeShot = Time.time;

        //ammo handling
        _ammoRemainingInMag--;
        _remainingShots--;
        UpdateAmmoText();
    }

    private void HandleRayCastShot(RaycastHit pointShot, Vector3 cameraPosition)
    {
        if (pointShot.transform && pointShot.distance <= range)
        {
            
            if (pointShot.transform.TryGetComponent(out DamageableObject DO))
            {
                DO.getHit.Invoke(new Damage(damage, pointShot.point - transform.position, this));

                //--------------------------hitmarker
                Transform HM = Instantiate(HitMarker, pointShot.point, Quaternion.identity).transform;
                HM.transform.LookAt(cameraPosition);
                HM.localScale *= pointShot.distance / 10;
                Destroy(HM.gameObject, HITMARKER_LIFETIME);
            }

            //-----------------------------------------------bullet hole
            if (pointShot.transform.gameObject.layer == 0)
            {
                Transform BHD = Instantiate(bulletHoleOnDefault, pointShot.point, Quaternion.identity).transform;
                BHD.rotation = Quaternion.FromToRotation(BHD.forward, pointShot.normal);
                BHD.position -= BHD.forward * 0.03f;
                BHD.parent = pointShot.transform;

                Destroy(BHD.gameObject, BULLETHOLE_LIFETIME);
            }
        }
    }

    protected override void StartShooting()
    {
        if (_lastTimeShot + fireRateForShots <= Time.time && _ammoRemainingInMag != 0)
        {
            base.StartShooting();

            _remainingShots = shotsPerTriggerPress;
        }
    }

    //--------------------------------reload
    protected override bool ReloadConditions()
    {
        //we also check if the gun past it's shot so that it doesn't reload while shooting
        return _ammoRemainingInMag != maxBulletsInMag && bulletsOnPlayer > 0 && _lastTimeShot + fireRateForShots <= Time.time;
    }

    protected override void StopAndAccomplishReload()
    {
        base.StopAndAccomplishReload();

        int bulletsInNextMag = bulletsOnPlayer >= maxBulletsInMag ? maxBulletsInMag : bulletsOnPlayer % maxBulletsInMag;
        //here we subtract only what needs to be added to fill the mag from the player's bullets
        bulletsOnPlayer -= bulletsInNextMag - _ammoRemainingInMag;
        _ammoRemainingInMag = bulletsInNextMag;

        UpdateAmmoText();
    }

    //-------------------------------------------ui
    protected override void UpdateAmmoText()
    {
        GetAmmoTextHolder().text = _ammoRemainingInMag + " / " + bulletsOnPlayer;
    }

    protected override void UpdateCrosshair()
    {
        _spread = Mathf.Lerp(_restingSpread, _maxSpread, _spreadPositionInLerp);
        GetCrosshairTransform().sizeDelta = new Vector2(_spread, _spread) * GetUIScale();
    }
}
