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
        private List<PlayerController> _players;

        private Objective _currentObjective;
        private List<Objective> _objs;
        private Queue<Objective> _lastFewObjs;
        [SerializeField] private int _maxLastFewObjs;

        [Header("Time")]
        [SerializeField] private float timeBetweenObjectives;
        [SerializeField] private float timeBetweenChecks;

        [Header("Objective Triggers")]
        //ammo fill ratio = current ammo on player / base ammo
        [SerializeField] private float playerAmmoFillRatioForTrigger;

        private void Awake()
        {
            _onObjectiveComplete = new UnityEvent();

            _onObjectiveComplete.AddListener(CompleteObjective);

            _objs = new();

            foreach(ObjectiveHolder oh in transform.GetComponentsInChildren<ObjectiveHolder>())
            {
                _objs.Add(oh.Build(_onObjectiveComplete));
            }
            
            if (_maxLastFewObjs >= _objs.Count) throw new System.ArgumentException("smaller max last few objs must be given");

            _lastFewObjs = new();

            Invoke(nameof(CheckIfShouldStartObjective), timeBetweenChecks);
        }

        private void CompleteObjective()
        {
            Debug.Log("complete");
            Invoke(nameof(CheckIfShouldStartObjective), timeBetweenObjectives);
        }
        private void BeginObjective()
        {
            Debug.Log("begin");
            FindNextRandomObjective();
            _currentObjective.Begin();
        }

        private void CheckIfShouldStartObjective()
        {
            if (CheckIfPlayerNeedsGun() && 
                ZombieSpawnerManager.GetCurrentSpawnerState() != ZombieSpawnerManager.SpawnerState.BREAK) BeginObjective();
        }

        private bool CheckIfPlayerNeedsGun()
        {
            bool needsNewWeapon = false;

            foreach (PlayerController p in PlayerController.GetPlayers())
            {
                needsNewWeapon = p.GetPlayerAmmoFillRatio() < playerAmmoFillRatioForTrigger || needsNewWeapon;
            }

            return needsNewWeapon;
        }

        private void FindNextRandomObjective()
        {
            int selectedObj = Random.Range(0, _objs.Count);

            while (_lastFewObjs.Contains(_objs[selectedObj]))
            {
                selectedObj = Random.Range(0, _objs.Count);
            }

            _currentObjective = _objs[selectedObj];
            _lastFewObjs.Append(_currentObjective);

            if (_lastFewObjs.Count >= _maxLastFewObjs) { _lastFewObjs.Dequeue(); }
        }
    }
}