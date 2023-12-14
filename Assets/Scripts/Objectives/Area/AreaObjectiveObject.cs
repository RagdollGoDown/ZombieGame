using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Objectives.Area
{
    [RequireComponent(typeof(Collider))]
    public class AreaObjectiveObject : ObjectiveObject
    {
        private void OnTriggerStay(Collider other)
        {
            if (!other.tag.Equals("Player") || !getIsOn()) return;

            GetObjectEvent().Invoke();
        }
    }
}
