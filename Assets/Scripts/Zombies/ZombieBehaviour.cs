
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Animations.Rigging;
using Utility;

public enum ZombieState
{
    Chasing,
    Idle,
    Jump,
    Dead
}

[RequireComponent(typeof(NavMeshAgent))]
public class ZombieBehaviour : MonoBehaviour
{
    private static readonly float TIME_BEFORE_BODY_DISINTEGRATES = 20;

    private static readonly int SPEED_ANIMATOR_PARAMETER_ID = Animator.StringToHash("Speed");
    private static readonly int JUMP_ANIMATOR_PARAMETER_ID = Animator.StringToHash("Jump");

    //we don't want the zombie to look at the damageable object but at the head
    private static readonly Vector3 OFFSET_FOR_HEAD_CONSTRAINT = new(0,1.7f,0);

    //used for the chase direction
    private static readonly float DISTANCE_TO_TIME_BETWEEN_NAVMESH_UPDATES_PROPORTION = 0.01f;
    private static readonly float DISTANCE_BEFORE_VELOCITY_IS_CLAMPED = 1;

    private ZombieState _currentState;
    
    //TODO unserialize
    [SerializeField]private Transform _headTransform;

    [Header("Navigation")]
    //-------------------navigation
    private NavMeshAgent _navMeshAgent;
    [SerializeField] private float minRunningSpeed;
    [SerializeField] private float maxRunningSpeed;
    private float runningSpeed;
    [SerializeField] private float crawlingSpeed;
    [SerializeField] private float targetVelocityBias = 1;
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

    [Header("Attacking")]
    //--------------------attacking
    [SerializeField]private float attackDamage;
    [SerializeField]private float attackDamageWithHeads;
    [SerializeField]private float timeBetweenAttacks;
    private float _timeBetweenAttackCounter;
    [SerializeField]private float distanceBeforeAttack;
    [SerializeField]private float MaxNavMeshSpeedRequieredAttack= 0.2f;

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
        if (!_headTransform) throw new System.NullReferenceException("Head transform is null");
        
        runningSpeed = UnityEngine.Random.Range(minRunningSpeed, maxRunningSpeed);
        _navMeshAgent = gameObject.GetComponent<NavMeshAgent>();
        _navMeshAgent.speed = runningSpeed;
        _navMeshAgent.stoppingDistance = distanceBeforeAttack;

        _currentState = ZombieState.Idle;
        rigidBodys = GetComponentsInChildren<Rigidbody>();
        mainDamageableObject = GetComponentInChildren<DamageableObject>();

        _zombieAnimator = GetComponent<Animator>();
        _headConstraint = transform.Find("HeadRig/HeadAimConstraint").GetComponent<MultiAimConstraint>();
        _rigBuilder = GetComponent<RigBuilder>();

        ChooseRandomMesh();
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

        Invoke(nameof(UnReadyRagdoll),Time.fixedDeltaTime);
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

    //------------------------------chasing

    public void StartChase(DamageableObject target)
    {
        if (target == null) { return; }

        _currentState = ZombieState.Chasing;
        _zombieTarget = target;

        _timeBetweenAttackCounter = timeBetweenAttacks;

        WeightedTransformArray sources = new WeightedTransformArray();
        sources.Add(new WeightedTransform(_zombieTarget.transform,1));
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

        while(_currentState == ZombieState.Chasing)
        {
            distanceFromPlayer = Vector3.Distance(_zombieTarget.transform.position, transform.position);

            lookingPosition = _zombieTarget.transform.position + OFFSET_FOR_HEAD_CONSTRAINT;
                //+ MathF.Min(distanceFromPlayer, DISTANCE_BEFORE_VELOCITY_IS_CLAMPED) * targetVelocityBias
                //* _zombieTarget.GetPossibleCharacterController().velocity;


            _navMeshAgent.SetDestination(lookingPosition);

            if (distanceFromPlayer <= distanceBeforeAttack
                && _navMeshAgent.velocity.magnitude <= MaxNavMeshSpeedRequieredAttack
                && _timeBetweenAttackCounter <= 0)
            {
                StartCoroutine(Attack());
            }

            _timeBetweenAttackCounter -= (_timeBetweenAttackCounter > 0) ? Time.fixedDeltaTime : 0;

            yield return new WaitForSeconds(distanceFromPlayer * DISTANCE_TO_TIME_BETWEEN_NAVMESH_UPDATES_PROPORTION);
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

        ZombieSpawnerManager.RemoveZombie();
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
}