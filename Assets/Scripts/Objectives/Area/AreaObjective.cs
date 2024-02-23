using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Objectives
{
    public class AreaObjective : Objective
    {
        private float _currentStayTime;
        [SerializeField] private float totalStayTime;

        private Action stayinArea;

        protected override void Awake()
        {
            base.Awake();

            foreach (ObjectiveObject areaObjective in GetObjectiveObjects())
            { 
                areaObjective.GetObjectEvent().AddListener(stayInObjective);
            }

        }

        public override void Begin()
        {
            base.Begin();

            _currentStayTime = 0;
        }

        private void stayInObjective()
        {
            stayinArea?.Invoke();
            _currentStayTime += Time.fixedDeltaTime * Time.timeScale;
            if (_currentStayTime >= totalStayTime) Complete();
        }

        public override float GetCompletenessRatio()
        {
            return _currentStayTime / totalStayTime;
        }

        public void ObserveStayInArea(Action action)
        {
            stayinArea += action;
        }

        public void StopObservingStayInArea(Action action)
        {
            stayinArea -= action;
        }
    }
}
