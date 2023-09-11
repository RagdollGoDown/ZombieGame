using System.Collections.Generic;
using UnityEngine;

namespace Weapons
{
    public class MeleeWeaponBehaviour : WeaponBehaviour
    {
        private float chargeTime;
        private float cooldownTime;

        private void Awake()
        {
            AwakeWeapon();
        }

        protected override void AwakeWeapon()
        {
            base.AwakeWeapon();

        }

        protected override void UpdateWeapon()
        {
            throw new System.NotImplementedException();
        }

        protected override void UseWeapon()
        {
            throw new System.NotImplementedException();
        }

        protected override bool ReloadConditions()
        {
            throw new System.NotImplementedException();
        }

        public override void RefillWeaponAmmo()
        {
            throw new System.NotImplementedException();
        }

        public override float GetAmmoFillRatio()
        {
            throw new System.NotImplementedException();
        }
    }
}