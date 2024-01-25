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

        private bool isDead;

        public bool IsDead
        {
            get
            {
                return isDead;
            }
        }

        private Object lastDamageDealer;
        private Damage lastDamageRecieved;

        [SerializeField] private float _health;
        [SerializeField] private float maxHealth = 10;
        public UnityEventRecieveDamage getHit;
        public UnityEvent OnDamageTaken;

        private Collider[] _colliders;

        private Vector3 initialScale;

        private Rigidbody possibleRigidbody;
        private CharacterController possibleCharacterController;

        //--------------------------to do on destruction
        [SerializeField] private List<string> destructionParticlePoolNames;
        private ObjectPool[] destructionParticlePool;
        public UnityEvent<DamageableObject> deathCalls;

        [SerializeField] private bool shrinkOnDeath;
        [SerializeField] private bool disableCollidersOnDeath = true;

        [SerializeField] private bool killChildrenOnDeath = true;
        private List<DamageableObject> _damageableChildren;


        private void Awake()
        {
            _health = maxHealth;

            getHit = new UnityEventRecieveDamage();
            getHit.AddListener(TakeDamage);

            _colliders = GetComponents<Collider>();

            initialScale = transform.localScale;

            destructionParticlePool =
                destructionParticlePoolNames.Select(dpp =>
                {
                    return ObjectPool.GetPool(dpp);
                }).Where(dpp => dpp != null).ToArray();

            _damageableChildren = new List<DamageableObject>();

            for (int i = 0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i).TryGetComponent(out DamageableObject damageableObject)) _damageableChildren.Add(damageableObject);
            }

            possibleRigidbody = GetComponent<Rigidbody>();
            possibleCharacterController = GetComponent<CharacterController>();

        }

        public void Revive()
        {
            _health = maxHealth;
            transform.localScale = initialScale;

            if (disableCollidersOnDeath)
            {
                _colliders.Select(c => c.enabled = true);
            }

            if (killChildrenOnDeath)
            {
                _damageableChildren?.ForEach(d => d.Revive());
            }

            isDead = false;
        }

        private void TakeDamage(Damage damage)
        {
            OnDamageTaken.Invoke();

            lastDamageDealer = damage.GetDamageDealer();
            lastDamageRecieved = damage;

            _health -= damage.GetDamageDone();

            if (_health <= 0)
            {
                Die(damage);
            }
        }



        //---------------------------------------------------------getters setters

        public float GetHealthRatio()
        {
            return _health / maxHealth;
        }

        public Object GetLastDamageDealer()
        {
            return lastDamageDealer;
        }

        public Damage GetLastDamageDone()
        {
            return lastDamageRecieved;
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

        // -----------------------------------------------------------destroying commands
        private void Die(Damage damage)
        {
            if (isDead) { return; }

            if (killChildrenOnDeath) { KillChildren(damage); }

            foreach (ObjectPool op in destructionParticlePool)
            {
                Transform dpg = op.Pull(false).transform;
                dpg.position = transform.position;
                dpg.Rotate(transform.rotation.eulerAngles);

                if (dpg.TryGetComponent(out Rigidbody rigidbody))
                {
                    rigidbody.AddForce(
                        damage.GetDamageDone() * DAMAGE_TO_FORCE_PROPORTION * damage.GetDamageDirection());
                }

                dpg.gameObject.SetActive(true);

                StartCoroutine(nameof(DisableParticle), dpg.gameObject);
            }

            deathCalls.Invoke(this);

            if (disableCollidersOnDeath)
            {
                _colliders.Select(c => c.enabled = false);
            }

            isDead = true;

            //this needs to be done last
            if (shrinkOnDeath) {
                transform.localScale = Vector3.zero;
            }
        }

        private void KillChildren(Damage damage)
        {
            _damageableChildren?.ForEach(d => d.Die(damage));
        }

        private IEnumerator DisableParticle(GameObject particle)
        {
            yield return new WaitForSeconds(TIME_BEFORE_PARTICLE_DESTRUCTION);

            particle.SetActive(false);
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
