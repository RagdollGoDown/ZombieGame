using System.Collections;
using UnityEngine.VFX;
using UnityEngine;
using UnityEngine.Events;
using Utility;
using Utility.Observable;
using System.Linq;

namespace Weapons
{
    public class RayCastGunBehaviour : WeaponBehaviour, CrossHaired
    {
        protected static readonly int BULLETHOLE_RECIPIENTS_LAYERMASK = 0;

        private static readonly string POOL_HEADER_OBJECT_NAME = "Pools";

        private static readonly float BULLETHOLE_LIFETIME = 10;
        private static readonly float HITMARKER_LIFETIME = .1f;

        protected static readonly int SHOOTABLE_LAYERMASK_VALUE = 65;

        [Header("General Stats")]
        [SerializeField] private float fireRateForShots;
        private float _lastTimeShot;
        [SerializeField] protected float damage;
        [SerializeField] protected bool destroyOnKill;
        [SerializeField] protected int pelletsPerShot = 1;
        [SerializeField] protected int penetration = 1;
        [SerializeField] protected float range = 100;

        [Header("Spread Stats")]
        //the spread origin is the player camera that we keep to get it's forward vector
        protected Transform raycastOrigin;

        protected ObservableFloat spread;
        private ReadOnlyObservableFloat Spread;
        private float _spreadPositionInLerp;
        [SerializeField] private float _maxSpread;
        [SerializeField] private float _restingSpread;
        [SerializeField] private float _spreadDecreaseSpeed;
        [SerializeField] private float _spreadDecreaseSpeedWhenAiming;
        [SerializeField] private float _spreadIncreasePerShot;

        [Header("Visual Effects")]
        [SerializeField] private VisualEffect muzzleFlash;
        [SerializeField] private GameObject hitMarker;
        //[SerializeField] private GameObject bulletHole;
        [SerializeField] private Mesh bulletHoleMesh;
        [SerializeField] private Material bulletHoleMaterial;
        [SerializeField] private ObjectPool bulletTrailPool;
        [SerializeField] private float bulletLerpSpeed = 100;
        [SerializeField] private Transform barrelExit;

        [Header("Ammo and Reloading")]
        protected int _ammoRemainingInMag;
        [SerializeField] protected int maxBulletsInMag;
        [SerializeField] protected int maxBulletsOnPlayer;
        protected int _bulletsOnPlayer;
        [SerializeField] private int shotsPerTriggerPress = 100;
        protected int _remainingShots;


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

            bulletTrailPool.transform.parent = GameObject.Find(POOL_HEADER_OBJECT_NAME).transform;

            if (!barrelExit) throw new System.ArgumentNullException("Gun barrel is null");
            if (bulletTrailPool != null && bulletLerpSpeed <= 0) throw new System.ArgumentException("Speed isn't strictly positiv");
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
                    if (_remainingShots == 0 || _ammoRemainingInMag == 0)
                    {
                        //we need to stop it next frame or else the animation for the shot won't start
                        StopUsing();

                        if (_ammoRemainingInMag == 0)
                        {
                            StartReload();
                        }
                    }
                    else{
                        UseWeapon();
                    }

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

            TakeShot();

            _lastTimeShot = Time.time;

            //ammo handling
            _ammoRemainingInMag--;
            _remainingShots--;
            UpdateAmmoText();
        }

        protected virtual void TakeShot(DamageableObject DO = null)
        {
            for (int i = 0; i < pelletsPerShot; i++)
            {
                RaycastHit[] pointsShot = GetRaycastHitsFromOrigin();

                for (int j = 0; j < pointsShot.Length; j++)
                {
                    HandleRayCastShot(pointsShot[j], raycastOrigin.position);
                }
            }
            if (muzzleFlash) muzzleFlash.Play();
        }
        
        /// <summary>
        /// Shoots the ray and gets all the hits by this ray then ordering them and return the first of number penetration
        /// </summary>
        /// <returns>an ordered array of the hits of number penetration</returns>
        protected RaycastHit[] GetRaycastHitsFromOrigin()
        {
            float randomAngle = Random.Range(0, 2 * Mathf.PI);

            Vector3 spreadDiff = raycastOrigin.up * Mathf.Cos(randomAngle) + raycastOrigin.right * Mathf.Sin(randomAngle);
            spreadDiff *= spread.GetValue() * Random.value;
            
            return Physics.RaycastAll(raycastOrigin.position + spreadDiff, raycastOrigin.forward + spreadDiff, range, SHOOTABLE_LAYERMASK_VALUE)
                .OrderBy(p => p.distance).Take(penetration).ToArray();
        }

        //-----------------------------------------------------reacting to shot

        protected IEnumerator BulletTrailHandler(Vector3 target)
        {
            if (bulletTrailPool == null) yield break;

            Vector3 barrelPosition = barrelExit.position;

            TrailRenderer bulletTrail = bulletTrailPool.Pull(false).GetComponent<TrailRenderer>();
            bulletTrail.Clear();

            bulletTrail.gameObject.SetActive(true);

            float adaptedSpeed = bulletLerpSpeed / Vector3.Distance(barrelPosition, target);

            float lerpPosition = 0;

            while (lerpPosition < 1)
            {
                bulletTrail.transform.position = Vector3.Lerp(barrelPosition, target, lerpPosition);

                lerpPosition += Time.deltaTime * adaptedSpeed;

                yield return null;
            }

            bulletTrail.gameObject.SetActive(false);
        }

        private void HandleRayCastShot(RaycastHit pointShot, Vector3 cameraPosition)
        {
            if (pointShot.transform && pointShot.distance <= range)
            {
                if (bulletTrailPool != null)
                {
                    StartCoroutine(nameof(BulletTrailHandler), pointShot.point);
                }

                if (pointShot.transform.TryGetComponent(out DamageableObject DO))
                {
                    Damage damageToDO = new(damage, pointShot.point - transform.position, this,
                        destroyOnKill);
                    DO.GetHitEvent().Invoke(damageToDO);

                    //--------------------------hitmarker
                    Transform HM = Instantiate(hitMarker,
                        pointShot.point + pointShot.normal * 0.03f,
                        Quaternion.FromToRotation(Vector3.forward, pointShot.normal)).transform;
                    Destroy(HM.gameObject, HITMARKER_LIFETIME);
                }

                //-----------------------------------------------bullet hole
                else if (pointShot.transform.gameObject.layer == BULLETHOLE_RECIPIENTS_LAYERMASK)
                {
                    GPUInstanceManager.AddInstance(new GPUInstanceStatic(
                        pointShot.point + pointShot.normal * 0.03f,
                        Quaternion.FromToRotation(Vector3.forward, pointShot.normal),
                        bulletHoleMesh, bulletHoleMaterial,
                        BULLETHOLE_RECIPIENTS_LAYERMASK,
                        BULLETHOLE_LIFETIME));
                }
            }
        }

        //-----------------------------------------------------------using gun
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

        public override int GetAmmoInMag()
        {
            return _ammoRemainingInMag;
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
