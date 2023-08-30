using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Utility.Observable;

namespace Objectives
{
    public class ObjectivesManager : MonoBehaviour
    {
        //this unity event is used by the objectives
        //while the delegate is used for other objects
        private UnityEvent _onObjectiveComplete;

        public ObservableObject<Objective> currentObjective;
        private Objective[] _objs;
        private List<Objective> _lastObjs;

        [Header("Objective Times")]
        [SerializeField] private float necessaryTimeBetweenObjectives;
        [SerializeField] private float maximumTimeBetweenObjectives;
        [SerializeField] private float timeBetweenChecks;
        private float timeSinceLastObjective;

        public ObjectivesManager(Objective[] _objs, UnityEvent _onObjectiveComplete)
        {
            this._objs = _objs;
        }

        private void Awake()
        {
            if (necessaryTimeBetweenObjectives >= maximumTimeBetweenObjectives) 
                throw new System.ArgumentException("Necessary time needs to be strictly smaller than the maximum time");

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
            timeSinceLastObjective = 0;

            Invoke(nameof(CheckIfShouldStartObjective), timeBetweenChecks);
        }
        private void BeginObjective()
        {
            Debug.Log("begin");
            FindNextRandomObjective();
            currentObjective.GetValue().Begin();
        }

        private void CheckIfShouldStartObjective()
        {
            Debug.Log("check");
            timeSinceLastObjective += timeBetweenChecks;

            if (maximumTimeBetweenObjectives <= timeSinceLastObjective && 
                ZombieSpawnerManager.GetCurrentSpawnerState() != ZombieSpawnerManager.SpawnerState.BREAK &&
               timeSinceLastObjective > necessaryTimeBetweenObjectives) BeginObjective();
            else Invoke(nameof(CheckIfShouldStartObjective), timeBetweenChecks);
        }

        private void FindNextRandomObjective()
        {
            int selectedObj = Random.Range(0, _objs.Length);

            while (_lastObjs.Contains(_objs[selectedObj]))
            {
                selectedObj = Random.Range(0, _objs.Length);
            }

            currentObjective.SetValue(_objs[selectedObj]);
            _lastObjs.Add(currentObjective.GetValue());

            if (_lastObjs.Count == _objs.Length) { _lastObjs = new(); }
        }
    }
}