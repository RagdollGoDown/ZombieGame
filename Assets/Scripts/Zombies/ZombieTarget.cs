using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(DamageableObject))]
[RequireComponent(typeof(Collider))]
public class ZombieTarget : MonoBehaviour
{
    private DamageableObject damageableObject;

    [SerializeField] private Vector3 targetOffsetFromPosition;

    private void Awake()
    {
        damageableObject = GetComponent<DamageableObject>();
    }

    public void HitTarget(Damage damage)
    {
        damageableObject.getHit.Invoke(damage);
    }

    public Vector3 GetTargetPosition()
    {
        return targetOffsetFromPosition + transform.position;
    }
}