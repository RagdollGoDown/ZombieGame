using System.Collections;
using UnityEngine.VFX;
using UnityEngine;
using UnityEngine.Events;
using Utility;
using Utility.Observable;

namespace Weapons
{
    public class RayCastGunBehaviour : WeaponBehaviour, CrossHaired
    {
        private static readonly float BULLETHOLE_LIFETIME = 10;
        private static readonly float HITMARKER_LIFETIME = .1f;

        private static readonly int SHOOTABLE_LAYERMASK_VALUE = 65;

        [Header("General Stats")]
        [SerializeField] private float fireRateForShots;
        private float _lastTimeShot;
        [SerializeField] private float damage;
        [SerializeField] private int pelletsPerShot = 1;
        [SerializeField] private float range = 100;

        [Header("Spread Stats")]
        //the spread origin is the player camera that we keep to get it's forward vector
        private Transform raycastOrigin;

        private ObservableFloat spread;
        private ReadOnlyObservableFloat Spread;
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
        [SerializeField] private GameObject bulletObject;
        [SerializeField] private float bulletLerpSpeed = 100;
        [SerializeField] private Transform barrelExit;

        [Header("Ammo and Reloading")]
        protected int _ammoRemainingInMag;
        [SerializeField] protected int maxBulletsInMag;
        [SerializeField] protected int maxBulletsOnPlayer;
        protected int _bulletsOnPlayer;
        [SerializeField] private int shotsPerTriggerPress = 100;
        private int _remainingShots;


        private void Awake()
        {
            AwakeWeapon();
        }

        protected override void AwakeWeapon()
        {
            base.AwakeWeapon();

            _bulletsOnPlayer = maxBulletsOnPlayer;

            _ammoRemainingInMag = maxBulletsInMag;
            UpdateAmmoText();

            _spreadPositionInLerp = 0;
            spread = new();
            Spread = new(spread);
            UpdateSpread();


            if (!barrelExit) throw new System.ArgumentNullException("Gun barrel is null");
            if (bulletObject && bulletLerpSpeed <= 0) throw new System.ArgumentException("Speed isn't strictly positiv");
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
                    UseWeapon();
                }

                if (_ammoRemainingInMag == 0 || _remainingShots == 0)
                {
                    //we need to stop it next frame or else the animation for the shot won't start
                    Invoke(nameof(StopUsing), Time.deltaTime);
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

            if (!GetIsHolstered()) { UpdateSpread(); }
        }

        //---------------------------------------------shooting
        protected override void UseWeapon()
        {
            //add spread when shooting
            _spreadPositionInLerp = Mathf.Clamp01(_spreadPositionInLerp + _spreadIncreasePerShot);

            //raycast shot
            Vector3 cameraPosition = raycastOrigin.position;

            for (int i = 0; i < pelletsPerShot; i++)
            {
                RaycastHit pointShot = GetRaycastHitFromOrigin();

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
                StartCoroutine(nameof(BulletTrailHandler), pointShot.point);

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

        protected RaycastHit GetRaycastHitFromOrigin()
        {
            float randomAngle = Random.Range(0, 2 * Mathf.PI);

            Vector3 spreadDiff = raycastOrigin.up * Mathf.Cos(randomAngle) + raycastOrigin.right * Mathf.Sin(randomAngle);
            spreadDiff *= spread.GetValue() * Random.value;

            Physics.Raycast(raycastOrigin.position + spreadDiff, raycastOrigin.forward + spreadDiff, out RaycastHit hit, 1000, SHOOTABLE_LAYERMASK_VALUE);
            return hit;
        }

        protected IEnumerator BulletTrailHandler(Vector3 target)
        {
            Vector3 barrelPosition = barrelExit.position;

            Transform bullet = Instantiate(bulletObject, barrelPosition, barrelExit.rotation).transform;

            float adaptedSpeed = bulletLerpSpeed / Vector3.Distance(barrelPosition, target);

            float lerpPosition = 0;

            while (lerpPosition < 1)
            {
                bullet.position = Vector3.Lerp(barrelPosition, target, lerpPosition);

                lerpPosition += Time.deltaTime * adaptedSpeed;

                yield return null;
            }

            Destroy(bullet.gameObject);
        }

        protected override void StartUsing()
        {
            if (_lastTimeShot + fireRateForShots <= Time.time && _ammoRemainingInMag != 0)
            {
                base.StartUsing();

                _remainingShots = shotsPerTriggerPress;
            }
            //reload if you don't have ammo
            else if (_lastTimeShot + fireRateForShots <= Time.time && _ammoRemainingInMag == 0)
            {
                StartReload();
            }
        }

        //--------------------------------reload
        protected override bool ReloadConditions()
        {
            //we also check if the gun past it's shot so that it doesn't reload while shooting
            return _ammoRemainingInMag != maxBulletsInMag && _bulletsOnPlayer > 0 && _lastTimeShot + fireRateForShots <= Time.time;
        }

        protected override void StopAndAccomplishReload()
        {
            base.StopAndAccomplishReload();

            if (_bulletsOnPlayer >= maxBulletsInMag - _ammoRemainingInMag)
            {
                _bulletsOnPlayer -= maxBulletsInMag - _ammoRemainingInMag;
                _ammoRemainingInMag = maxBulletsInMag;
            }
            else
            {
                _ammoRemainingInMag += _bulletsOnPlayer;
                _bulletsOnPlayer = 0;
            }

            UpdateAmmoText();
        }

        public override void RefillWeaponAmmo()
        {
            _bulletsOnPlayer = maxBulletsOnPlayer;
        }

        //-------------------------------------------get/setters

        public void SetSpreadOrigin(Transform raycastOrigin)
        {
            this.raycastOrigin = raycastOrigin;
        }

        public override float GetAmmoFillRatio()
        {
            return _bulletsOnPlayer / maxBulletsOnPlayer;
        }

        protected override void UpdateAmmoText()
        {
            ammoText.SetValue(_ammoRemainingInMag + " / " + _bulletsOnPlayer);
        }

        protected override void UpdateSpread()
        {
            spread.SetValue(Mathf.Lerp(_restingSpread, _maxSpread, _spreadPositionInLerp));
        }

        public ReadOnlyObservableFloat GetSpread()
        {
            return Spread;
        }
    }
}
