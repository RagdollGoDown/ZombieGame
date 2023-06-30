using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.Scripts.Objectives
{
    public class ObjectivesManager : MonoBehaviour
    {
        private UnityEvent onObjectiveComplete;
        private List<PlayerController> players;

        [SerializeField] private AreaObjectiveObject test;
        [SerializeField] private AreaObjectiveObject test2;
        [SerializeField] private ButtonsObjectiveObject test3;

        private Objective objective1;
        private Objective objective2;
        private Objective objective3;

        private Objective currentObjective;
        private List<Objective> objs;

        [Header("Time")]
        [SerializeField] private float timeBetweenObjectives;
        [SerializeField] private float timeBetweenChecks;

        private void Awake()
        {
            onObjectiveComplete = new UnityEvent();

            objective1 = new AreaObjective(onObjectiveComplete, 5, test);
            objective2 = new AreaObjective(onObjectiveComplete, 5, test2);
            objective3 = new ButtonsObjective(onObjectiveComplete, new ButtonsObjectiveObject[] { test3 });

            objs = new List<Objective>();
            objs.Add(objective1);
            objs.Add(objective2);

            //BeginObjective();

            onObjectiveComplete.AddListener(CompleteObjective);
        }

        private void CompleteObjective()
        {
            Debug.Log("complete");
            Invoke(nameof(CheckIfShouldStartObjective), timeBetweenObjectives);
        }
        private void BeginObjective()
        {
            Debug.Log("begin");
            currentObjective = objective3;
            currentObjective.Begin();
        }

        private void CheckIfShouldStartObjective()
        {
            if (ZombieSpawnerManager.GetCurrentSpawnerState() == ZombieSpawnerManager.SpawnerState.BREAK) return;

            

            Invoke(nameof(CheckIfShouldStartObjective), timeBetweenChecks);
        }
    }
}