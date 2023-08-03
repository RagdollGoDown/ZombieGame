
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using TMPro.EditorUtilities;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Animations.Rigging;
using Debug = UnityEngine.Debug;

public enum ZombieState
{
    Chasing,
    Idle,
    Jump,
    Dead
}

[RequireComponent(typeof(NavMeshAgent))]
//the boxcollider used so that it can get detected by other objects
[RequireComponent(typeof(BoxCollider))]
public class ZombieBehaviour : MonoBehaviour
{
    private static readonly int DETECTION_LAYERMASK = 9;

    private static readonly float TIME_BEFORE_BODY_DISINTEGRATES = 20;

    private static readonly float ATTACK_ANIMATION_STUNT_TIME = 0.1f;

    private static readonly int SPEED_ANIMATOR_PARAMETER_ID = Animator.StringToHash("Speed");
    private static readonly int JUMP_ANIMATOR_PARAMETER_ID = Animator.StringToHash("Jump");

    private ZombieState _currentState;
    
    //TODO unserialize
    [SerializeField]private Transform _headTransform;
    private float _cosOfAngleOfVision;

    //used as a collider the other objects can collide with to
    //get the behavior script without passing by the children
    private BoxCollider _detectionBox;
    
    //-------------------navigation
    private NavMeshAgent _navMeshAgent;
    [SerializeField] private float minRunningSpeed;
    [SerializeField] private float maxRunningSpeed;
    private float runningSpeed;
    [SerializeField] private float crawlingSpeed;
    private ZombieTarget _zombieTarget;

    //--------------------animation
    private MultiAimConstraint _headConstraint;
    private RigBuilder _rigBuilder;
    private Animator _zombieAnimator;
    
    //--------------------body state
    private bool _leftArmBroken;
    private bool _rightArmBroken;
    private bool _leftLegBroken;
    private bool _rightLegBroken;

    //--------------------attacking
    [SerializeField]private float attackDamage;
    [SerializeField]private float attackDamageWithHeads;
    [SerializeField]private float timeBetweenAttacks;
    private float _timeBetweenAttackCounter;
    [SerializeField]private float distanceBeforeAttack;
    [SerializeField]private float MaxNavMeshSpeedRequieredAttack= 0.2f;

    //--------------------mesh
    //we enable these transforms to choose which skinned skeleton is rendered 
    [SerializeField] List<GameObject> differentZombieMeshsGameObject;
    [SerializeField] List<Material> differentZombieMaterialsForSkinnedMesh;
    [SerializeField] GameObject _selectedZombieMesh;
    //[SerializeField] private GameObject defaultZombieMeshTransform;

    private void Awake()
    {
        runningSpeed = UnityEngine.Random.Range(minRunningSpeed, maxRunningSpeed);
        _navMeshAgent = gameObject.GetComponent<NavMeshAgent>();
        _navMeshAgent.speed = runningSpeed;
        _navMeshAgent.stoppingDistance = distanceBeforeAttack;
        
        _cosOfAngleOfVision = 0.2f;

        _currentState = ZombieState.Idle;

        _detectionBox = GetComponent<BoxCollider>();
        
        _zombieAnimator = GetComponent<Animator>();
        _headConstraint = transform.Find("HeadRig/HeadAimConstraint").GetComponent<MultiAimConstraint>();
        _rigBuilder = GetComponent<RigBuilder>();

        ChooseRandomMesh();
    }

    private void FixedUpdate()
    {
        _zombieAnimator.SetFloat(SPEED_ANIMATOR_PARAMETER_ID, _navMeshAgent.velocity.magnitude);

        _zombieAnimator.SetBool(JUMP_ANIMATOR_PARAMETER_ID, _navMeshAgent.isOnOffMeshLink);
        
        switch (_currentState)
        {
            case ZombieState.Chasing:
                Chase();
                break;
        }
    }

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
            
            if (!zombieSkinnedMesh.IsUnityNull())
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
    
    private bool CanSeeTarget(ZombieTarget target)
    {
        Vector3 position = _headTransform.position;
        Vector3 direction = target.GetTargetPosition() - position;

        if (IsInFieldOfView(target.GetTargetPosition()))
        {
            RaycastHit hit;
            //only sees default layer
            Physics.Raycast(position, direction, out hit, 50,DETECTION_LAYERMASK);
            Debug.DrawRay(position,direction);
            return !hit.transform.IsUnityNull() && hit.transform.name.Equals(target.name);
        }
        else
        {
            return false;
        }
    }

    private bool IsInFieldOfView(Vector3 point)
    {
        Vector3 position = _headTransform.position;
        Vector3 direction = point - position;
        float cosinBetweenVectors = Vector3.Dot(_headTransform.forward, direction) / direction.magnitude;
        return cosinBetweenVectors > _cosOfAngleOfVision;
    }

    //------------------------------chasing
    public bool CheckChase(ZombieTarget Target)
    {
        if (_currentState == ZombieState.Idle && CanSeeTarget(Target))
        {
            StartChase(Target);

            return true;
        }

        return false;
    }

    public void StartChase(ZombieTarget target)
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
    }

    private void Chase()
    {
        _navMeshAgent.SetDestination(_zombieTarget.GetTargetPosition());

        //-------------------------------------attack
        float distanceFromPLayer = Vector3.Distance(_zombieTarget.GetTargetPosition(),transform.position);

        if (distanceFromPLayer <= distanceBeforeAttack 
            && _navMeshAgent.velocity.magnitude <= MaxNavMeshSpeedRequieredAttack
            && _timeBetweenAttackCounter <= 0 
            && CanSeeTarget(_zombieTarget)) { 
            StartCoroutine(Attack());
        }

        _timeBetweenAttackCounter -= (_timeBetweenAttackCounter > 0) ? Time.fixedDeltaTime : 0;
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

        _zombieTarget.HitTarget(new Damage(attackDamage,transform.position - _zombieTarget.GetTargetPosition(), this));
        _timeBetweenAttackCounter = timeBetweenAttacks;
        
        yield return new WaitForSeconds(TIME_BEFORE_BODY_DISINTEGRATES);
        
        //when the attack type is 0 then it isn't attacking
        _zombieAnimator.SetInteger("AttackType", 0);
    }

    private void ReadyRagdoll()
    {
        _detectionBox.enabled = false;
        Rigidbody[] rbs = GetComponentsInChildren<Rigidbody>();

        foreach (Rigidbody rb in rbs)
        {
            rb.useGravity = true;
        }
    }

    //--------------------------get setters

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

    public void ScoreHeadShot()
    {

    }

    public void Die()
    {
        if (_currentState == ZombieState.Dead) { return; }

        PlayerScore.AddKill();

        _currentState = ZombieState.Dead;
        _zombieAnimator.enabled = false;

        ReadyRagdoll();

        ZombieSpawnerManager.RemoveZombie();
        Destroy(gameObject, TIME_BEFORE_BODY_DISINTEGRATES);
    }
}