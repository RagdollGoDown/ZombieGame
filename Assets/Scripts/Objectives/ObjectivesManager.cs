using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.Scripts.Objectives
{
    public class ObjectivesManager : MonoBehaviour
    {
        private UnityEvent _onObjectiveComplete;
        private RewardManager _rewardManager;

        private Objective _currentObjective;
        private Objective[] _objs;
        private List<Objective> _lastObjs;

        [Header("Objective Times")]
        [SerializeField] private float _necessaryTimeBetweenObjectives;
        [SerializeField] private float _maximumTimeBetweenObjectives;
        [SerializeField] private float timeBetweenChecks;
        private float _timeDifferenceSinceLastObjective;

        [Header("Objective Triggers")]
        //ammo fill ratio = current ammo on player / base ammo
        [SerializeField] private float playerAmmoFillRatioForTrigger;

        public ObjectivesManager(Objective[] _objs, UnityEvent _onObjectiveComplete)
        {
            this._objs = _objs;
        }

        private void Awake()
        {
            if (_necessaryTimeBetweenObjectives >= _maximumTimeBetweenObjectives) 
                throw new System.ArgumentException("Necessary time needs to be strictly smaller than the maximum time");

            _rewardManager = transform.Find("/RewardManager").GetComponent<RewardManager>();
            if (_rewardManager == null) throw new System.Exception("reward manager not found");

            _onObjectiveComplete = new UnityEvent();

            _onObjectiveComplete.AddListener(CompleteObjective);

            List<Objective> _objsList = new();

            foreach(ObjectiveHolder oh in transform.GetComponentsInChildren<ObjectiveHolder>())
            {
                _objsList.Add(oh.Build(_onObjectiveComplete));
            }

            _objs = _objsList.ToArray();
            
            _lastObjs = new();

            Invoke(nameof(CheckIfShouldStartObjective), timeBetweenChecks);
        }

        private void CompleteObjective()
        {
            Debug.Log("complete");
            _timeDifferenceSinceLastObjective = 0;

            _rewardManager.GiveReward();

            Invoke(nameof(CheckIfShouldStartObjective), timeBetweenChecks);
        }
        private void BeginObjective()
        {
            Debug.Log("begin");
            FindNextRandomObjective();
            _currentObjective.Begin();
        }

        private void CheckIfShouldStartObjective()
        {
            Debug.Log("check");
            _timeDifferenceSinceLastObjective += timeBetweenChecks;

            if ((CheckIfPlayerNeedsGun() || _maximumTimeBetweenObjectives <= _timeDifferenceSinceLastObjective) && 
                ZombieSpawnerManager.GetCurrentSpawnerState() != ZombieSpawnerManager.SpawnerState.BREAK &&
               _timeDifferenceSinceLastObjective > _necessaryTimeBetweenObjectives) BeginObjective();
            else Invoke(nameof(CheckIfShouldStartObjective), timeBetweenChecks);
        }

        private bool CheckIfPlayerNeedsGun()
        {
            bool needsNewWeapon = false;

            foreach (PlayerController p in PlayerController.GetPlayers())
            {
                needsNewWeapon = p.GetPlayerAmmoFillRatio() < playerAmmoFillRatioForTrigger || needsNewWeapon;
            }
            Debug.Log("player needs ? " + needsNewWeapon);

            return needsNewWeapon;
        }

        private void FindNextRandomObjective()
        {
            Debug.Log("finding next"); 

            int selectedObj = Random.Range(0, _objs.Length);

            while (_lastObjs.Contains(_objs[selectedObj]))
            {
                selectedObj = Random.Range(0, _objs.Length);
            }

            _currentObjective = _objs[selectedObj];
            _lastObjs.Append(_currentObjective);

            if (_lastObjs.Count == _objs.Length) { _lastObjs = new(); }
        }
    }
}