using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Utility
{
    public class UnityEventRecieveDamage : UnityEvent<Damage> { }

    public class DamageableObject : MonoBehaviour
    {
        private readonly static float TIME_BEFORE_PARTICLE_DESTRUCTION = 10;
        private readonly static float DAMAGE_TO_FORCE_PROPORTION = 20;

        private bool _isDestroyed;

        private Object _lastDamageDealer;

        [SerializeField] private float _health;
        [SerializeField] private float maxHealth = 10;
        public UnityEventRecieveDamage getHit;
        public UnityEvent OnDamageTaken;

        private Collider[] _colliders;

        private Vector3 initialScale;

        private Rigidbody possibleRigidbody;
        private CharacterController possibleCharacterController;

        //--------------------------to do on destruction
        [SerializeField] private List<string> destructionParticlePrefabsPoolNames;
        private ObjectPool[] destructionParticlePrefabsPool;
        public UnityEvent destructionCalls;

        [SerializeField] private bool shrinkOnDeath;
        [SerializeField] private bool disableCollidersOnDeath = true;

        [SerializeField] private bool destroyChildrenOnDeath;
        private List<DamageableObject> _damageableChildren;


        private void Awake()
        {
            _health = maxHealth;

            getHit = new UnityEventRecieveDamage();
            getHit.AddListener(TakeDamage);

            _colliders = GetComponents<Collider>();

            initialScale = transform.localScale;

            destructionParticlePrefabsPool = destructionParticlePrefabsPoolNames.Select(dpp => GameObject.Find(dpp).GetComponent<ObjectPool>()).ToArray();

            _damageableChildren = new List<DamageableObject>();

            possibleRigidbody = GetComponent<Rigidbody>();
            possibleCharacterController = GetComponent<CharacterController>();

            for (int i = 0; i < transform.childCount; i++)
            {
                _damageableChildren.Add(transform.GetChild(i).GetComponent<DamageableObject>());
            }
        }

        public void Revive()
        {
            _health = maxHealth;
            transform.localScale = initialScale;

            if (disableCollidersOnDeath)
            {
                foreach (Collider col in _colliders)
                {
                    col.enabled = true;
                }
            }

            if (destroyChildrenOnDeath)
            {
                foreach (var dc in _damageableChildren)
                {
                    dc.Revive();
                }
            }
            
            _isDestroyed = false;
        }

        private void TakeDamage(Damage damage)
        {
            OnDamageTaken.Invoke();

            _lastDamageDealer = damage.GetDamageDealer();

            _health -= damage.GetDamageDone();

            if (_health <= 0)
            {
                Destroy(damage);
            }
        }

        //---------------------------------------------------------getters setters

        public float GetHealthRatio()
        {
            return _health / maxHealth;
        }

        public Object GetLastDamageDealer()
        {
            return _lastDamageDealer;
        }

        /// <summary>
        /// A rigidbody that the object might have
        /// </summary>
        /// <returns>the component if it has it or null</returns>
        public Rigidbody GetPossibleRigidbody()
        {
            if (possibleRigidbody != null) { return possibleRigidbody; }
            else { return GetComponent<Rigidbody>(); }
        }

        /// <summary>
        /// A CharacterController that the object might have
        /// </summary>
        /// <returns>the component if it has it or null</returns>
        public CharacterController GetPossibleCharacterController()
        {
            if (possibleCharacterController != null) { return possibleCharacterController; }
            else { return GetComponent<CharacterController>(); }
        }

        // dsetroying commands
        private void Destroy(Damage damage)
        {
            if (_isDestroyed) { return; }

            if (destroyChildrenOnDeath) { DestroyChildren(damage); }

            foreach (ObjectPool op in destructionParticlePrefabsPool)
            {
                Transform dpg = op.Pull(false).transform;
                dpg.position += transform.position;
                dpg.Rotate(transform.rotation.eulerAngles);

                if (dpg.TryGetComponent(out Rigidbody rigidbody))
                {
                    rigidbody.AddForce(
                        damage.GetDamageDone() * DAMAGE_TO_FORCE_PROPORTION * damage.GetDamageDirection());
                }

                dpg.gameObject.SetActive(true);
                Destroy(dpg.gameObject, TIME_BEFORE_PARTICLE_DESTRUCTION);
            }

            destructionCalls.Invoke();

            if (disableCollidersOnDeath)
            {
                foreach (Collider col in _colliders)
                {
                    col.enabled = false;
                }
            }

            _isDestroyed = true;

            //this needs to be done last
            if (shrinkOnDeath) {
                transform.localScale = Vector3.zero;
            }
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
}
