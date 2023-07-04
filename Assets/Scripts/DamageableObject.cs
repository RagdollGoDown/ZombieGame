using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class UnityEventRecieveDamage : UnityEvent<Damage> {}

public class DamageableObject : MonoBehaviour
{
    private static float _timeBeforeDestructionOfParticles = 10;

    private bool _isDestroyed;

    private Object _lastDamageDealer;

    [SerializeField]private float _health;
    [SerializeField]private float maxHealth = 10;
    public UnityEventRecieveDamage getHit;
    public UnityEvent OnDamageTaken;

    private Collider[] _colliders;

    //--------------------------to do on destruction
    [SerializeField] private List<GameObject> destructionParticleGameObjectPrefabs;
    public UnityEvent destructionCalls;

    [SerializeField] private bool shrinkOnDeath;
    
    [SerializeField] private bool destroyChildrenOnDeath;
    private List<DamageableObject> _damageableChildren;
    

    private void Awake()
    {
        _health = maxHealth;

        getHit = new UnityEventRecieveDamage();
        getHit.AddListener(TakeDamage);

        _colliders = GetComponents<Collider>();

        _damageableChildren = new List<DamageableObject>();
        
        for (int i = 0; i < transform.childCount; i++)
        {
            _damageableChildren.Add(transform.GetChild(i).GetComponent<DamageableObject>());
        }
    }
    
    private void TakeDamage(Damage damage)
    {
        OnDamageTaken.Invoke();

        _health -= damage.GetDamageDone();

        if (shrinkOnDeath && _health <= 0)
        {
            Destroy(damage);
        }
    }

    public float GetHealthRatio()
    {
        return _health / maxHealth;
    }

    public Object GetLastDamageDealer()
    {
        return _lastDamageDealer;
    }

    private void Destroy(Damage damage)
    {
        if (_isDestroyed){return;}
        
        if (destroyChildrenOnDeath){DestroyChildren(damage);}
        
        foreach (GameObject destructionParticleGamObjectPrefab in destructionParticleGameObjectPrefabs)
        {
            Transform dpg = Instantiate(destructionParticleGamObjectPrefab).transform;
            dpg.position += transform.position;
            dpg.Rotate(transform.rotation.eulerAngles);

            if (dpg.TryGetComponent(out Rigidbody rigidbody))
            {
                rigidbody.AddForce(damage.GetDamageDirection() * 10 * damage.GetDamageDone());
            }

            Destroy(dpg.gameObject, _timeBeforeDestructionOfParticles);
        }
        
        destructionCalls.Invoke();

        foreach (Collider col in _colliders)
        {
            col.enabled = false;
        }

        _isDestroyed = true;
        //this needs to be done last
        transform.localScale = Vector3.zero;
    }

    private void DestroyChildren(Damage damage)
    {
        foreach (var dc in _damageableChildren)
        {
            dc.Destroy(damage);
        }
    }
}

public class Damage
{
    private readonly Object damageDealer;
    private readonly Vector3 damageDirection;
    private readonly float damageDone;

    public Damage(float damageDone, Vector3 damageDirection, Object damageDealer)
    {
        this.damageDone = damageDone;
        this.damageDirection = damageDirection.normalized;
        this.damageDealer = damageDealer;
    }

    public float GetDamageDone()
    {
        return damageDone;
    }

    public Vector3 GetDamageDirection()
    {
        return damageDirection;
    }

    public Object GetDamageDealer()
    {
        return damageDealer;
    }
}
