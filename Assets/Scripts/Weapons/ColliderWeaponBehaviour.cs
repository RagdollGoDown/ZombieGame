using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility;
using System.Linq;

namespace Weapons
{
    public class ColliderWeaponBehaviour : RayCastGunBehaviour
    {
        [SerializeField] private int pierceCount;

        [SerializeField] private float colliderRadius;

        private Collider[] tempColliders;

        private void Awake()
        {
            AwakeWeapon();
        }

        protected override void TakeShot()
        {
            for (int i = 0; i < pelletsPerShot; i++)
            {
                float randomAngle = Random.Range(0, 2 * Mathf.PI);

                Vector3 spreadDiff = raycastOrigin.up * Mathf.Cos(randomAngle) + raycastOrigin.right * Mathf.Sin(randomAngle);
                spreadDiff *= spread.GetValue() * Random.value;

                tempColliders = Physics.OverlapCapsule(raycastOrigin.position + spreadDiff, 
                    raycastOrigin.position + raycastOrigin.forward * range + spreadDiff,
                    colliderRadius, SHOOTABLE_LAYERMASK_VALUE);

                Vector3 lastShotPosition = Vector3.zero;

                tempColliders.OrderBy(p => Vector3.Distance(raycastOrigin.position, p.transform.position)).Take(pierceCount).ToList().ForEach(p =>
                {
                    Debug.Log(p.name);

                    if (p.TryGetComponent(out DamageableObject d) && p.gameObject != gameObject)
                    {
                        Damage damageDone = new(damage, p.transform.position - d.transform.position, this);

                        d.getHit.Invoke(damageDone);
                    }

                    lastShotPosition = p.transform.position;
                });

                StartCoroutine(nameof(BulletTrailHandler), lastShotPosition);

                tempColliders.Initialize();
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.white;
            if (raycastOrigin != null)
            {
                Gizmos.DrawLine(raycastOrigin.position,raycastOrigin.position + raycastOrigin.forward * range);
            }
        }
    }
}
