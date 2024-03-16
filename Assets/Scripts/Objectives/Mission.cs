using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Utility.Observable;

namespace Objectives
{
    public class Mission : MonoBehaviour
    {
        private enum MissionState{
            NotReady,
            Ready,
            OnGoing
        }

        private MissionState currentMissionState = MissionState.NotReady;

        private ObservableObject<Objective> currentObjective;

        private int currentObjectiveIndex;
        private List<Objective> objectives;

        public UnityEvent onStarted;
        public UnityEvent onCompleted;

        public int RewardMoney;

        public void StartMission()
        {
            if (currentMissionState == MissionState.NotReady)
            {
                Prepare();
                ReadyMission();
            }

            if (currentMissionState == MissionState.OnGoing)
            {
                Debug.LogWarning("Called StartMission() on ongoing mission");
                return;
            }

            objectives[currentObjectiveIndex].Begin();
        }

        private void StartNextObjective()
        {
            currentObjectiveIndex++;

            if (currentObjectiveIndex == objectives.Count) CompleteMission();
            else
            {
                Objective next = objectives[currentObjectiveIndex];

                next.Begin();
                currentObjective.SetValue(next);
            }
        }

        private void CompleteMission()
        {
            ReadyMission();

            onCompleted.Invoke();
        }

        private void ReadyMission()
        {
            currentMissionState = MissionState.Ready;
            currentObjectiveIndex = 0;

            if (objectives.Count > 0)
            {
                currentObjective.SetValue(objectives[0]);
            }
        }

        private void Prepare()
        {
            currentObjective = new ObservableObject<Objective>(null);

            objectives = new();

            foreach (Transform t in transform)
            {
                if (t.TryGetComponent(out Objective obj))
                {
                    objectives.Add(obj);
                    obj.GetOnObjectiveCompleteEvent().AddListener(StartNextObjective);
                }
            }
        }

        public ObjectiveObject[] ObjectiveObjects()
        {
            if (currentMissionState == MissionState.NotReady) Prepare();

            List<ObjectiveObject> objects = new();

            foreach (var item in objectives)
            {
                objects.AddRange(item.GetObjectiveObjects());
            }

            return objects.ToArray();
        }

        public ReadOnlyObservableObject<Objective> GetCurrentObjective()
        {
            if (currentMissionState == MissionState.NotReady) Prepare();

            return new ReadOnlyObservableObject<Objective>(currentObjective);
        }
    }
}