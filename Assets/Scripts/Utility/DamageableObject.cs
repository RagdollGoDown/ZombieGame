using System.Threading.Tasks;
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
        private readonly static float DAMAGE_TO_VELOCITY_PROPORTION = .3f;

        private bool isDead;
        private bool isDestroyed;

        public bool IsDead
        {
            get
            {
                return isDead;
            }
        }

        public bool IsDestroyed
        {
            get
            {
                return isDestroyed;
            }
        }

        private Object lastDamageDealer;
        private Damage lastDamageRecieved;

        private float currentHealth;
        [SerializeField] private float health = 10;
        private float currentExtraHealthBeforeDestruction;
        [SerializeField] private float extraHealthBeforeDestruction = 20;
        private UnityEventRecieveDamage getHit;
        public UnityEvent<DamageableObject> OnDamageTaken;
        [SerializeField] private List<string> onDamageTakenParticlePoolNames;
        private ObjectPool[] onDamageTakenParticlePool;

        private Collider[] _colliders;

        private Vector3 initialScale;

        private Rigidbody possibleRigidbody;
        private CharacterController possibleCharacterController;

        [Header("Death")]
        [SerializeField] private List<string> deathParticlePoolNames;
        [SerializeField] private List<string> deathParticlePoolNamesWhenChild;
        public UnityEvent<DamageableObject> deathCalls;
        private ObjectPool[] deathParticlePool;
        private ObjectPool[] deathParticlePoolWhenChild;

        [Header("Destruction")]
        [SerializeField] private List<string> destructionParticlePoolNames;
        [SerializeField] private List<string> destructionParticlePoolNamesWhenChild;
        public UnityEvent<DamageableObject> destructionCalls;
        private ObjectPool[] destructionParticlePool;
        private ObjectPool[] destructionParticlePoolWhenChild;

        [SerializeField] private bool shrinkOnDestruction = true;
        [SerializeField] private bool disableCollidersOnDestruction = true;

        [SerializeField] private bool killChildrenOnDeath = true;
        [SerializeField] private bool destroyGameObjectOnDestruction = false;
        [SerializeField] private bool destroyChildrenOnDestruction = true;
        private List<DamageableObject> _damageableChildren;


        private void Awake()
        {
            currentHealth = health;
            currentExtraHealthBeforeDestruction = extraHealthBeforeDestruction;

            getHit ??= new UnityEventRecieveDamage();
            getHit.AddListener(TakeDamage);

            _colliders = GetComponents<Collider>();

            initialScale = transform.localScale;

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
            currentHealth = health;
            currentExtraHealthBeforeDestruction = extraHealthBeforeDestruction;
            transform.localScale = initialScale;

            if (disableCollidersOnDestruction)
            {
                _colliders.Select(c => c.enabled = true);
            }

            if (killChildrenOnDeath || destroyChildrenOnDestruction)
            {
                _damageableChildren?.ForEach(d => d.Revive());
            }

            isDead = false;
            isDestroyed = false;
        }

        private void TakeDamage(Damage damage)
        {
            lastDamageDealer = damage.GetDamageDealer();
            lastDamageRecieved = damage;

            currentHealth -= damage.GetDamageDone();

            if (onDamageTakenParticlePoolNames.Count != 0 && damage.GetDamageNormal() != Vector3.zero){
                HandleOnDamageEffect(damage);
            }

            //Should be called before death or destruction
            OnDamageTaken.Invoke(this);

            if (currentHealth <= 0)
            {
                Die(damage);

                extraHealthBeforeDestruction -= damage.GetDamageDone();

                if (extraHealthBeforeDestruction <= 0 || damage.WillDestroyOnKill())
                {
                    Destroy(damage);
                }
            }
        }

        private void HandleOnDamageEffect(Damage damage)
        {
            onDamageTakenParticlePool ??=
                onDamageTakenParticlePoolNames.Select(dpp =>
                {
                    return ObjectPool.GetPool(dpp);
                }).Where(dpp => dpp != null).ToArray();

            Transform effect;

            foreach(ObjectPool op in onDamageTakenParticlePool)
            {
                effect = op.Pull(false).transform;
                effect.position = damage.GetDamagePosition();
                effect.rotation = Quaternion.FromToRotation(Vector3.forward, damage.GetDamageNormal());
                effect.gameObject.SetActive(true);
            }
        }



        //---------------------------------------------------------getters setters

        public float GetHealthRatio()
        {
            return currentHealth / health;
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

        public UnityEvent<Damage> GetHitEvent()
        {
            if (getHit == null) { getHit = new UnityEventRecieveDamage(); }

            return getHit;
        }

        // -----------------------------------------------------------Death commands
        private void Die(Damage damage, bool asChild = false)
        {
            //if the object is dead the there is no need to kill it
            if (isDead) { return; }

            if (asChild)
            {
                deathParticlePoolWhenChild ??=
                deathParticlePoolNamesWhenChild.Select(dpp =>
                {
                    return ObjectPool.GetPool(dpp);
                }).Where(dpp => dpp != null).ToArray();

                PullParticles(deathParticlePoolWhenChild, damage);
            }
            else
            {
                deathParticlePool ??=
                deathParticlePoolNames.Select(dpp =>
                {
                    return ObjectPool.GetPool(dpp);
                }).Where(dpp => dpp != null).ToArray();

                PullParticles(deathParticlePool, damage);
            }

            if (killChildrenOnDeath) { KillChildren(damage); }

            deathCalls.Invoke(this);

            isDead = true;
        }

        private void KillChildren(Damage damage)
        {
            _damageableChildren?.ForEach(d => d.Die(damage, true));
        }

        private void DestroyChildren(Damage damage)
        {
            _damageableChildren?.ForEach(d => d.Destroy(damage, true));
        }

        private void Destroy(Damage damage, bool asChild = false)
        {
            if (isDestroyed) return;

            if (asChild)
            {
                destructionParticlePoolWhenChild ??=
                destructionParticlePoolNamesWhenChild.Select(dpp =>
                {
                    return ObjectPool.GetPool(dpp);
                }).Where(dpp => dpp != null).ToArray();

                PullParticles(destructionParticlePoolWhenChild, damage);
            }
            else
            {
                destructionParticlePool ??=
                destructionParticlePoolNames.Select(dpp =>
                {
                    return ObjectPool.GetPool(dpp);
                }).Where(dpp => dpp != null).ToArray();

                PullParticles(destructionParticlePool, damage);
            }

            if (destroyChildrenOnDestruction) DestroyChildren(damage);

            //this needs to be done last
            if (shrinkOnDestruction)
            {
                transform.localScale = Vector3.zero;
            }

            if (disableCollidersOnDestruction && _colliders != null)
            {
                _colliders.Select(c => {
                    if (c == null) Debug.Log("null");
                    return c.enabled = false;
                });
            }

            destructionCalls.Invoke(this);
            isDestroyed = true;

            if (destroyGameObjectOnDestruction) Destroy(gameObject);
        }

        private void PullParticles(ObjectPool[] particlesPool, Damage damage)
        {
            if (particlesPool == null) return;

            foreach (ObjectPool op in particlesPool)
            {
                Transform dpg = op.Pull(false).transform;
                dpg.position = transform.position;
                dpg.Rotate(transform.rotation.eulerAngles);

                if (dpg.TryGetComponent(out Rigidbody rigidbody))
                {
                    rigidbody.velocity =
                        damage.GetDamageDone() * DAMAGE_TO_VELOCITY_PROPORTION * damage.GetDamageDirection();
                }

                dpg.gameObject.SetActive(true);
            }
        }
    }

    public class Damage
    {
        private readonly Object damageDealer;
        private readonly Vector3 damageDirection;
        private readonly Vector3 damagePosition;
        private readonly Vector3 damageNormal;
        private readonly float damageDone;
        private readonly bool destroyOnKill = false;

        public Damage(float damageDone, 
            Vector3 damageDirection,
            Vector3 damagePosition,
            Vector3 damageNormal,
            Object damageDealer,
            bool destroyOnKill = false)
        {
            this.damageDone = damageDone;
            this.damageDirection = damageDirection.normalized;
            this.damagePosition = damagePosition;
            this.damageNormal = damageNormal;
            this.damageDealer = damageDealer;

            this.destroyOnKill = destroyOnKill;
        }

        public float GetDamageDone()
        {
            return damageDone;
        }

        public Vector3 GetDamageDirection()
        {
            return damageDirection;
        }

        public Vector3 GetDamagePosition()
        {
            return damagePosition;
        }

        public Vector3 GetDamageNormal()
        {
            return damageNormal;
        }

        public Object GetDamageDealer()
        {
            return damageDealer;
        }

        public bool WillDestroyOnKill()
        {
            return destroyOnKill;
        }
    }
}
