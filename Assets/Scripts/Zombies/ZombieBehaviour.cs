using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Animations.Rigging;
using Utility;
using UnityEngine.Events;

public enum ZombieState
{
    Chasing,
    Idle,
    Dead
}

[RequireComponent(typeof(NavMeshAgent))]
public class ZombieBehaviour : MonoBehaviour
{
    private static readonly float TIME_BEFORE_BODY_DISINTEGRATES = 20;

    private static readonly int SPEED_ANIMATOR_PARAMETER_ID = Animator.StringToHash("Speed");
    private static readonly int JUMP_ANIMATOR_PARAMETER_ID = Animator.StringToHash("Jump");

    //we don't want the zombie to look at the damageable object but at the head
    private static readonly Vector3 OFFSET_FOR_HEAD_CONSTRAINT = new(0, 1.7f, 0);

    //used for the chase direction
    private static readonly float DISTANCE_TO_TIME_BETWEEN_NAVMESH_UPDATES_PROPORTION = 0.01f;

    private static readonly float DISTANCE_OF_DISPLACEMENT = 4;
    private static readonly float DISTANCE_FOR_DISPLACEMENT_ACTIVATION = 8;

    private static readonly float IDLING_MAX_DISTANCE = 10;
    private static readonly float IDLING_MAX_WAIT_BETWEEN_WANDERS = 20;
    private static readonly float IDLING_MIN_WAIT_BETWEEN_WANDERS = 5;

    private ZombieState _currentState;

    [Header("Navigation")]
    //-------------------navigation
    private NavMeshAgent _navMeshAgent;
    [SerializeField] private float walkingSpeed = 2;
    [SerializeField] private float minRunningSpeed = 9;
    [SerializeField] private float maxRunningSpeed = 11;
    [SerializeField] private float distanceForMaxSpeed = 5;
    private float runningSpeed;
    [SerializeField] private float crawlingSpeed;
    private Vector3 displacementVector;

    private DamageableObject _zombieTarget;

    //--------------------animation
    private MultiAimConstraint _headConstraint;
    private RigBuilder _rigBuilder;
    private Animator _zombieAnimator;

    //--------------------body state
    private bool _leftArmBroken;
    private bool _rightArmBroken;
    private bool _leftLegBroken;
    private bool _rightLegBroken;
    private Rigidbody[] rigidBodys;
    private DamageableObject mainDamageableObject;
    private DamageableObject lastDeadBodyPart;

    public UnityEvent<ZombieBehaviour> OnDeath;

    [Header("Attacking")]
    //--------------------attacking
    [SerializeField] private float attackDamage;
    [SerializeField] private float attackDamageWithHeads;
    [SerializeField] private float timeBetweenAttacks;
    private float _timeBetweenAttackCounter;
    [SerializeField] private float distanceBeforeAttack;
    [SerializeField] private float MaxNavMeshSpeedRequieredAttack = 0.2f;

    [Header("Mesh")]
    //--------------------mesh
    //we enable these transforms to choose which skinned skeleton is rendered 
    [SerializeField] List<GameObject> differentZombieMeshsGameObject;
    [SerializeField] List<Material> differentZombieMaterialsForSkinnedMesh;
    [SerializeField] GameObject _selectedZombieMesh;
    //[SerializeField] private GameObject defaultZombieMeshTransform;

    //------------------------------------unity events
    private void Awake()
    {
        _navMeshAgent = gameObject.GetComponent<NavMeshAgent>();
        runningSpeed = UnityEngine.Random.Range(minRunningSpeed, maxRunningSpeed);
        _navMeshAgent.stoppingDistance = distanceBeforeAttack;

        float angle = UnityEngine.Random.Range(0, 2 * MathF.PI);

        displacementVector = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * DISTANCE_OF_DISPLACEMENT;

        _currentState = ZombieState.Idle;
        rigidBodys = GetComponentsInChildren<Rigidbody>();
        mainDamageableObject = GetComponentInChildren<DamageableObject>();

        _zombieAnimator = GetComponent<Animator>();
        _headConstraint = transform.Find("ChasingRig/HeadAimConstraint").GetComponent<MultiAimConstraint>();
        _rigBuilder = GetComponent<RigBuilder>();

        ChooseRandomMesh();
        StartIdle();
    }

    private void FixedUpdate()
    {
        _zombieAnimator.SetFloat(SPEED_ANIMATOR_PARAMETER_ID, _navMeshAgent.velocity.magnitude);

        _zombieAnimator.SetBool(JUMP_ANIMATOR_PARAMETER_ID, _navMeshAgent.isOnOffMeshLink);
    }

    private void OnEnable()
    {
        _zombieAnimator.enabled = true;

        _leftArmBroken = false;
        _leftLegBroken = false;
        _rightArmBroken = false;
        _rightLegBroken = false;

        _currentState = ZombieState.Idle;

        Invoke(nameof(UnReadyRagdoll), Time.fixedDeltaTime);
    }

    //--------------------------------------general
    public void ChooseRandomMesh()
    {
        if (_selectedZombieMesh != null)
        {
            _selectedZombieMesh.SetActive(false);
            _selectedZombieMesh = null;
        }

        if (differentZombieMeshsGameObject.Count > 0)
        {
            int selectedMeshIndex = UnityEngine.Random.Range(0, differentZombieMeshsGameObject.Count);

            differentZombieMeshsGameObject[selectedMeshIndex].SetActive(true);
            _selectedZombieMesh = differentZombieMeshsGameObject[selectedMeshIndex];

            SkinnedMeshRenderer zombieSkinnedMesh = _selectedZombieMesh.GetComponent<SkinnedMeshRenderer>();

            if (zombieSkinnedMesh != null)
            {
                zombieSkinnedMesh.material =
                differentZombieMaterialsForSkinnedMesh[UnityEngine.Random.Range(0, differentZombieMaterialsForSkinnedMesh.Count)];
            }
            else
            {
                //if the gameobject itself doesn't have it then it's probably the LODs as children
                SkinnedMeshRenderer[] skinRenderers = _selectedZombieMesh.GetComponentsInChildren<SkinnedMeshRenderer>();
                int selectedMaterial = UnityEngine.Random.Range(0, differentZombieMaterialsForSkinnedMesh.Count);

                foreach (SkinnedMeshRenderer SR in skinRenderers)
                {
                    SR.material = differentZombieMaterialsForSkinnedMesh[selectedMaterial];
                }
            }
        }
    }

    //------------------------------Idle

    private void StartIdle()
    {
        _currentState = ZombieState.Idle;
        _zombieTarget = null;

        _navMeshAgent.speed = walkingSpeed;

        WeightedTransformArray sources = new();
        _headConstraint.data.sourceObjects = sources;

        _zombieAnimator.enabled = false;
        _rigBuilder.Build();
        _zombieAnimator.enabled = true;

        StartCoroutine(nameof(Idling));
    }

    private IEnumerator Idling()
    {
        while (_currentState == ZombieState.Idle)
        {
            Vector3 nextPositionToWanderTo = new Vector3(
                transform.position.x + UnityEngine.Random.Range(-IDLING_MAX_DISTANCE, IDLING_MAX_DISTANCE),
                transform.position.y,
                transform.position.z + UnityEngine.Random.Range(-IDLING_MAX_DISTANCE, IDLING_MAX_DISTANCE));

            _navMeshAgent.SetDestination(nextPositionToWanderTo);

            yield return new WaitForSeconds(UnityEngine.Random.Range(IDLING_MIN_WAIT_BETWEEN_WANDERS, IDLING_MAX_WAIT_BETWEEN_WANDERS));
        }
    }

    //------------------------------Chasing

    public void StartChase(DamageableObject target)
    {
        if (target == null) { return; }

        _currentState = ZombieState.Chasing;
        _zombieTarget = target;

        _navMeshAgent.speed = runningSpeed;

        _timeBetweenAttackCounter = timeBetweenAttacks;

        WeightedTransformArray sources = new();
        sources.Add(new WeightedTransform(_zombieTarget.transform, 1));
        _headConstraint.data.sourceObjects = sources;

        _zombieAnimator.enabled = false;
        _rigBuilder.Build();
        _zombieAnimator.enabled = true;

        StartCoroutine(nameof(Chase));
    }

    private IEnumerator Chase()
    {
        Vector3 lookingPosition;

        float distanceFromPlayer;

        while (_currentState == ZombieState.Chasing)
        {
            distanceFromPlayer = Vector3.Distance(_zombieTarget.transform.position, transform.position);

            lookingPosition = _zombieTarget.transform.position + OFFSET_FOR_HEAD_CONSTRAINT;

            if (distanceFromPlayer >= DISTANCE_FOR_DISPLACEMENT_ACTIVATION)
            {
                lookingPosition += displacementVector;
            }

            //the speed gets linearly smaller when the zombie gets closer to the player
            _navMeshAgent.speed = distanceFromPlayer >= distanceForMaxSpeed ? maxRunningSpeed :
                minRunningSpeed + (maxRunningSpeed - minRunningSpeed) * distanceFromPlayer / distanceForMaxSpeed;

            _navMeshAgent.SetDestination(lookingPosition);

            if (distanceFromPlayer <= distanceBeforeAttack
                && _navMeshAgent.velocity.magnitude <= MaxNavMeshSpeedRequieredAttack
                && _timeBetweenAttackCounter <= 0)
            {
                StartCoroutine(Attack());
            }

            _timeBetweenAttackCounter -= (_timeBetweenAttackCounter > 0) ? Time.fixedDeltaTime : 0;

            yield return new WaitForSeconds(distanceFromPlayer * DISTANCE_TO_TIME_BETWEEN_NAVMESH_UPDATES_PROPORTION);

            if (_zombieTarget == null || _zombieTarget.IsDead)
            {
                StartIdle();
            }
        }
    }

    private IEnumerator Attack()
    {
        if (!_leftArmBroken)
        {
            _zombieAnimator.SetInteger("AttackType", 1);
        }
        else if (!_rightArmBroken)
        {
            _zombieAnimator.SetInteger("AttackType", 2);
        }
        else
        {
            _zombieAnimator.SetInteger("AttackType", 3);
        }

        _zombieTarget.getHit.Invoke(new Damage(attackDamage, transform.position - _zombieTarget.transform.position + OFFSET_FOR_HEAD_CONSTRAINT, this));
        _timeBetweenAttackCounter = timeBetweenAttacks;

        yield return null;

        //when the attack type is 0 then it isn't attacking
        _zombieAnimator.SetInteger("AttackType", 0);
    }

    //---------------------------------zombie body

    public void BreakLeftArm()
    {
        _leftArmBroken = true;
        CheckBodyIntegrity();
    }

    public void BreakRightArm()
    {
        _rightArmBroken = true;
        CheckBodyIntegrity();
    }

    public void BreakLeftLeg()
    {
        _leftLegBroken = true;
        CheckBodyIntegrity();
    }

    public void BreakRightLeg()
    {
        _rightLegBroken = true;
        CheckBodyIntegrity();
    }

    private void Cripple()
    {
        _zombieAnimator.SetInteger("ChaseType", 1);
    }

    private void CheckBodyIntegrity()
    {
        if ((_leftArmBroken || _rightArmBroken) && (_leftLegBroken || _rightLegBroken))
        {
            Die();
        }
        else if (_leftLegBroken || _rightLegBroken)
        {
            Cripple();
        }
    }

    private void ReadyRagdoll()
    {
        foreach (Rigidbody rb in rigidBodys)
        {
            rb.useGravity = true;
        }
    }

    private void UnReadyRagdoll()
    {
        foreach (Rigidbody rb in rigidBodys)
        {
            rb.useGravity = false;
        }

        mainDamageableObject.Revive();
    }

    public void Die()
    {
        if (_currentState == ZombieState.Dead) { return; }

        _currentState = ZombieState.Dead;
        _zombieAnimator.enabled = false;

        OnDeath.Invoke(this);

        ReadyRagdoll();

        Invoke(nameof(DisableSelf), TIME_BEFORE_BODY_DISINTEGRATES);
    }

    private void DisableSelf()
    {
        gameObject.SetActive(false);
    }

    //--------------------------get setters

    public NavMeshAgent GetNavMeshAgent()
    {
        return _navMeshAgent;
    }


    /// <summary>
    /// This should only be used by the zombies body parts in damageable objectives
    /// </summary>
    public void SetLastDeadBodyPart(DamageableObject damageableObject)
    {
        lastDeadBodyPart = damageableObject;
    }

    /// <summary>
    /// This is the last part that was damaged, we need this for the reaper
    /// </summary>
    /// <returns>The last damageable object that was damaged</returns>
    public DamageableObject GetLastDeadBodyPart()
    {
        return lastDeadBodyPart;
    }
}