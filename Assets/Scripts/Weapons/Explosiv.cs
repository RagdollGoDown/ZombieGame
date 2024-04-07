using System.Collections;
using System.Linq;
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
    [SerializeField] private List<string> onExplosionParticlePoolNames;
    private ObjectPool[] onExplosionParticlePool;

    public void Explode()
    {
        if (exploded) { return; }
        exploded = true;
        Vector3 position = transform.position + offset;

        Collider[] cols = Physics.OverlapSphere(position, radius);

        HandleExplosionParticles();
        
        foreach (Collider col in cols)
        {
            if (col.TryGetComponent(out DamageableObject d) && col.gameObject != gameObject)
            {
                Damage damageDone = new(damage, col.transform.position - position, position, Vector3.zero, this);

                d.GetHitEvent().Invoke(damageDone);
            }
        }

        onExplosion.Invoke();
    }

    private void HandleExplosionParticles()
    {
        onExplosionParticlePool ??=
        onExplosionParticlePoolNames.Select(dpp =>
        {
            return ObjectPool.GetPool(dpp);
        }).Where(dpp => dpp != null).ToArray();

        if (onExplosionParticlePool == null) return;

        foreach (ObjectPool op in onExplosionParticlePool)
        {
            Debug.Log("s");
            Transform particle = op.Pull(false).transform;
            particle.position = transform.position;
            particle.Rotate(transform.rotation.eulerAngles);
            particle.gameObject.SetActive(true);
        }
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
