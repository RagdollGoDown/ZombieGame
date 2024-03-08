using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utility
{    
    public class DamagingZone : MonoBehaviour
    {
        [SerializeField] private float damageOnEnter = 1;

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.TryGetComponent(out DamageableObject damageableObject))
            {
                damageableObject.GetHitEvent().Invoke(new Damage(damageOnEnter,other.transform.position - transform.position,
                transform.position, Vector3.zero,
                gameObject));
            }
        }

        public void SetDamage(float damage)
        {
            damageOnEnter = damage;
        }
    }
}
