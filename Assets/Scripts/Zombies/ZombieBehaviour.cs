using System;
using System.Threading.Tasks;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Animations.Rigging;
using Utility;
using UnityEngine.Events;
using UnityEngine.Animations;

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
    private static readonly int DISTANCE_TO_TIME_BETWEEN_NAVMESH_UPDATES_PROPORTION_MILLISEC = 10;
    private static readonly float SPEED_TO_ATTACK_DISTANCE_PROPORTION = .1f;

    private static readonly float IDLING_MAX_DISTANCE = 10;
    private static readonly float IDLING_MAX_WAIT_BETWEEN_WANDERS = 20;
    private static readonly float IDLING_MIN_WAIT_BETWEEN_WANDERS = 5;

    private ZombieState _currentState;

    private CancellationTokenSource chasingCancellationTokenSource;
    private CancellationToken chasingCancellationToken;

    [Header("Navigation")]
    //-------------------navigation
    private NavMeshAgent _navMeshAgent;
    [SerializeField] private float walkingSpeed = 2;
    [SerializeField] private float runningSpeedWhenClosest = 9;
    [SerializeField] private float runningSpeedWhenFurthest = 11;
    [SerializeField] private float distanceForMaxSpeed = 5;
    private float runningSpeed;
    [SerializeField] private float crawlingSpeed;

    private DamageableObject target;

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
    private DamageableObject rootDamageableObject;
    private DamageableObject lastDamagedBodyPart;

    public UnityEvent<ZombieBehaviour> OnDeath;

    [Header("Attacking")]
    //--------------------attacking
    private bool isAttacking;

    private bool isSpeedingUp;

    [SerializeField] private float attackDamage;
    [SerializeField] private float attackDamageWithHead;
    [SerializeField] private float timeBetweenAttacks;
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
        runningSpeed = UnityEngine.Random.Range(runningSpeedWhenClosest, runningSpeedWhenFurthest);
        _navMeshAgent.stoppingDistance = distanceBeforeAttack;

        float angle = UnityEngine.Random.Range(0, 2 * MathF.PI);

        DamagingZone hand_l = transform.Find("Armature/Root/Hips/Spine_01/Spine_02/Spine_03/Clavicle_L/Shoulder_L/Elbow_L/Hand_L").GetComponent<DamagingZone>();
        DamagingZone hand_r = transform.Find("Armature/Root/Hips/Spine_01/Spine_02/Spine_03/Clavicle_R/Shoulder_R/Elbow_R/Hand_R").GetComponent<DamagingZone>();
        DamagingZone head = transform.Find("Armature/Root/Hips/Spine_01/Spine_02/Spine_03/Neck/Head").GetComponent<DamagingZone>();

        hand_l.SetDamage(attackDamage);
        hand_r.SetDamage(attackDamage);
        head.SetDamage(attackDamageWithHead);

        _currentState = ZombieState.Idle;
        rigidBodys = GetComponentsInChildren<Rigidbody>();
        rootDamageableObject = GetComponentInChildren<DamageableObject>();

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

    private void OnDisable()
    {
        chasingCancellationTokenSource?.Cancel();
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
        target = null;

        _navMeshAgent.speed = walkingSpeed;

        _headConstraint.data.sourceObjects.Clear();

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
        this.target = target;

        _navMeshAgent.speed = runningSpeed;

        _headConstraint.data.sourceObjects.Add(new WeightedTransform(target.transform, 1));

        _zombieAnimator.enabled = false;
        _rigBuilder.Build();
        _zombieAnimator.enabled = true;

        chasingCancellationTokenSource = new();
        chasingCancellationToken = chasingCancellationTokenSource.Token;

        Chase();
    }

    private async void Chase()
    {
        Vector3 lookingPosition;

        float distanceFromPlayer;

        while (_currentState == ZombieState.Chasing && !chasingCancellationToken.IsCancellationRequested)
        {
            distanceFromPlayer = Vector3.Distance(target.transform.position, transform.position);

            lookingPosition = target.transform.position + OFFSET_FOR_HEAD_CONSTRAINT;

            //the speed gets linearly smaller when the zombie gets closer to the player
            _navMeshAgent.speed = distanceFromPlayer >= distanceForMaxSpeed ? runningSpeedWhenFurthest :
                runningSpeedWhenClosest + (runningSpeedWhenFurthest - runningSpeedWhenClosest) * distanceFromPlayer / distanceForMaxSpeed;

            _navMeshAgent.SetDestination(lookingPosition);

            _headConstraint.transform.LookAt(lookingPosition);

            if (distanceFromPlayer <= distanceBeforeAttack + SPEED_TO_ATTACK_DISTANCE_PROPORTION * _navMeshAgent.speed
                && _navMeshAgent.velocity.magnitude <= MaxNavMeshSpeedRequieredAttack
                && !isAttacking)
            {
                Attack();
            }

            await Task.Delay((int)(distanceFromPlayer * DISTANCE_TO_TIME_BETWEEN_NAVMESH_UPDATES_PROPORTION_MILLISEC) + 100);

            if ((target == null || target.IsDead) && !chasingCancellationToken.IsCancellationRequested)
            {
                StartIdle();
            }
        }
    }

    private async void Attack()
    {
        isAttacking = true;
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

        //await Task.Delay(1000);
        await Task.Delay((int)(timeBetweenAttacks * 1000));

        //when the attack type is 0 then it isn't attacking
        _zombieAnimator.SetInteger("AttackType", 0);
        isAttacking = false;
    }

    private async void SpeedUp(){
        isSpeedingUp = true;
        await Task.Delay(1000);
        isSpeedingUp = false;
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

        rootDamageableObject.Revive();
    }

    public void Die()
    {
        if (_currentState == ZombieState.Dead) { return; }

        _currentState = ZombieState.Dead;
        _zombieAnimator.enabled = false;

        if (lastDamagedBodyPart == null) lastDamagedBodyPart = rootDamageableObject;

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
    public void SetLastDamagedBodyPart(DamageableObject damageableObject)
    {
        lastDamagedBodyPart = damageableObject;

        _zombieAnimator.SetTrigger("Damage");
    }

    /// <summary>
    /// This is the last part that was damaged, we need this for the reaper
    /// </summary>
    /// <returns>The last damageable object that was damaged</returns>
    public DamageableObject GetLastDamagedBodyPart()
    {
        return lastDamagedBodyPart;
    }
}