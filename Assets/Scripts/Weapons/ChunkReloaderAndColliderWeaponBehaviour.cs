﻿using System.Collections;
using UnityEngine.InputSystem;
using UnityEngine;

namespace Weapons
{
    public class ChunkReloaderAndColliderWeaponBehaviour : ColliderWeaponBehaviour
    {
        [Header("Chunk Reloading")]
        [SerializeField] private float reloadEnterAnimationTime = 1;
        [SerializeField] private float reloadChunkAnimationTime = 1;
        [SerializeField] private float reloadExitAnimationTime = 1;

        private WaitForSeconds _reloadEnterWait;
        private WaitForSeconds _reloadChunkWait;
        private WaitForSeconds _reloadExitWait;

        [SerializeField] private float reloadSpeed = 1;
        [SerializeField] private int chunk = 1;
        private bool _wantsToReload;

        private Animator animator;

        private void Awake()
        {
            AwakeWeapon();
        }

        protected override void AwakeWeapon()
        {
            base.AwakeWeapon();

            if (reloadSpeed *
                reloadChunkAnimationTime *
                reloadExitAnimationTime *
                reloadEnterAnimationTime == 0) throw new System.ArgumentException("One of the vars required to not be zero is zero");

            _reloadEnterWait = new WaitForSeconds(reloadEnterAnimationTime / reloadSpeed);
            _reloadChunkWait = new WaitForSeconds(reloadChunkAnimationTime / reloadSpeed);
            _reloadExitWait = new WaitForSeconds(reloadExitAnimationTime / reloadSpeed);
        }

        protected override void ReadyAnimationLengths(Animator animator)
        {
            base.ReadyAnimationLengths(animator);

            animator.SetFloat("reloadSpeed", reloadSpeed);
        }

        public override void UseWeaponInput(InputAction.CallbackContext context)
        {
            _wantsToReload = false;

            base.UseWeaponInput(context);
        }

        public override void ReloadInput(InputAction.CallbackContext context)
        {
            if (context.started) _wantsToReload = true;

            base.ReloadInput(context);
        }

        protected override void StartReload()
        {
            if (!ReloadConditions()) return;

            if (!GetIsReloading()) StartCoroutine(nameof(Reload));
        }

        private IEnumerator Reload()
        {
            SetIsReloading(true);

            GetAnimator().SetBool("IsReloading", true);

            yield return _reloadEnterWait;

            int nextChunk;

            do
            {
                yield return _reloadChunkWait;

                nextChunk = Mathf.Min(chunk, maxBulletsInMag - _ammoRemainingInMag);

                _ammoRemainingInMag += nextChunk;

                _bulletsOnPlayer -= nextChunk;

                UpdateAmmoText();
            } while (ReloadConditions());

            GetAnimator().SetBool("IsReloading", false);

            yield return _reloadExitWait;

            SetIsReloading(false);
        }

        protected override bool ReloadConditions()
        {
            return base.ReloadConditions() && _wantsToReload;
        }
    }
}