using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Utility;

public class Explosiv : MonoBehaviour
{
    private bool exploded;

    [SerializeField] private float radius;
    [SerializeField] private float damage;
    [SerializeField] private Vector3 offset;

    [SerializeField] private UnityEvent onTimerStart;
    [SerializeField] private UnityEvent onExplosion;

    public void Explode()
    {
        if (exploded) { return; }
        exploded = true;
        Vector3 position = transform.position + offset;

        Collider[] cols = Physics.OverlapSphere(position, radius);

        foreach (Collider col in cols)
        {
            if (col.TryGetComponent(out DamageableObject d) && col.gameObject != gameObject)
            {
                Damage damageDone = new(damage, col.transform.position - position, this);

                d.GetHitEvent().Invoke(damageDone);
            }
        }

        Debug.Log("explosion");

        onExplosion.Invoke();
    }

    public void StartExplosionTimer(float time)
    {
        onTimerStart.Invoke();
        Debug.Log("start timer");
        Invoke(nameof(Explode), time);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position + offset, radius);
    }
}
