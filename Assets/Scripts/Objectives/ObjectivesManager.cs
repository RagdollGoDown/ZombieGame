using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.Scripts.Objectives
{
    public class ObjectivesManager : MonoBehaviour
    {
        private UnityEvent onObjectiveComplete;

        [SerializeField] private AreaObjectiveObject test;
        [SerializeField] private AreaObjectiveObject test2;
        [SerializeField] private ButtonsObjectiveObject test3;
        [SerializeField] private float timeBetweenObjectives;

        private Objective objective1;
        private Objective objective2;
        private Objective objective3;

        private Objective currentObjective;
        private List<Objective> objs;

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
            Invoke("BeginObjective", timeBetweenObjectives);
        }
        private void BeginObjective()
        {
            Debug.Log("begin");
            currentObjective = objective3;
            currentObjective.Begin();
        }
    }
}