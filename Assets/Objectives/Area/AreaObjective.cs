using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Objectives.Area
{
    public class AreaObjective : Objective
    {
        private float _currentStayTime;
        [SerializeField] private float totalStayTime;

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
            _currentStayTime += Time.fixedDeltaTime;
            if (_currentStayTime >= totalStayTime) Complete();
        }

        public override float GetCompletenessRatio()
        {
            return _currentStayTime / totalStayTime;
        }
    }
}
