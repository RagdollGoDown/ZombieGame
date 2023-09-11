using System;
using System.Collections;
using UnityEngine;
using Utility.Observable;
using UnityEngine.InputSystem;

namespace Weapons
{
    public abstract class WeaponBehaviour : MonoBehaviour
    {
        private Animator _gunAnimator;

        protected ObservableObject<string> ammoText;
        public ReadOnlyObservableObject<string> AmmoText;
 
        [Header("General")]
        [SerializeField] private float reloadDuration = 1;

        //---------------------------------state bools
        private bool _isReloading;

        private bool _isShooting;
        private bool _isAiming;

        private bool _isHolstered = true;

        [Header("Animations and effects")]
        [SerializeField] private int shotsPerShootingAnimation = 1;
        [SerializeField] private float shootingAnimationLength = 1;
        [SerializeField] private float reloadAnimationTime = 1;

        protected virtual void AwakeWeapon()
        {
            if (reloadDuration <= 0) throw new ArgumentException("duration not strictly positiv");

            _gunAnimator = GetComponent<Animator>();
            ReadyAnimationLengths(_gunAnimator);

            ammoText = new("");
            AmmoText = new(ammoText);
        }

        protected virtual void ReadyAnimationLengths(Animator animator)
        {
            animator.SetFloat("reloadSpeed", reloadAnimationTime / reloadDuration);
            animator.SetFloat("shootingSpeed", shootingAnimationLength / shotsPerShootingAnimation);
        }

        protected abstract void UpdateWeapon();

        //-------------------------------inputs
        public virtual void UseWeaponInput(InputAction.CallbackContext context)
        {
            if (context.started && !_isReloading && !_isHolstered)
            {
                StartUsing();
            }
            else if (context.canceled)
            {
                StopUsing();
            }
        }

        public virtual void ReloadInput(InputAction.CallbackContext context)
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

        //---------------------------------------unity events

        private void OnEnable()
        {
            _isHolstered = false;
            ReadyAnimationLengths(_gunAnimator);
        }

        private void OnDisable()
        {
            _isShooting = false;
            _isReloading = false;

            _isHolstered= true;
        }

        //---------------------------------------shooting
        protected abstract void UseWeapon();

        protected virtual void StartUsing()
        {
            _isShooting = true;
            if (_gunAnimator) _gunAnimator.SetBool("IsShooting", true);
        }

        protected virtual void StopUsing()
        {
            _isShooting = false;
            if (_gunAnimator) _gunAnimator.SetBool("IsShooting", false);
        }

        protected virtual void ValidateKill() { }

        //----------------------------------------reloading
        protected abstract bool ReloadConditions();

        protected virtual void StartReload()
        {
            if (!ReloadConditions()) return;

            _isReloading = true;
            if (_gunAnimator) _gunAnimator.SetBool("IsReloading", true);
            Invoke("StopAndAccomplishReload", reloadDuration);
        }

        protected virtual void StopAndAccomplishReload()
        {
            _isReloading = false;
            if (_gunAnimator) _gunAnimator.SetBool("IsReloading", false);
        }

        protected virtual void StopReload()
        {
            CancelInvoke("StopAndAccomplishReload");
            _isReloading = false;
            if (_gunAnimator) _gunAnimator.SetBool("IsReloading", false);
        }

        public abstract void RefillWeaponAmmo();

        //----------------------------------------------ui

        /// <summary>
        /// will update the value of the ammoText, making it available to AmmoText
        /// </summary>
        protected virtual void UpdateAmmoText() {}

        protected virtual void UpdateSpread() { }

        //-------------------------------------------------getters
        protected Animator GetAnimator()
        {
            return _gunAnimator;
        }

        //-------------------------------------------------getters/setters_state

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

        /*
         * Give the current bullets left on player divided by the max bullets on player
         */
        public abstract float GetAmmoFillRatio();
    }
}
