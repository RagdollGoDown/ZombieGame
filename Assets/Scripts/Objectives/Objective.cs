using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Utility.Observable;

namespace Objectives
{
    public abstract class Objective : MonoBehaviour
    {
        [SerializeField] private UnityEvent onComplete;
        [SerializeField] private UnityEvent onStarted;

        [SerializeField] private List<ObjectiveObject> objectiveObjects;

        [SerializeField] private string initialObjectiveText;
        private ObservableObject<string> objectiveText;

        protected virtual void Awake()
        {
            objectiveText = new("");
            objectiveText.SetValue(initialObjectiveText);
        }

        /// <summary>
        /// calculates how complete the objective is
        /// </summary>
        /// <returns>the ratio of completeness</returns>
        public abstract float GetCompletenessRatio();

        /// <summary>
        /// add an objectiveObject to the list of the concerned objects
        /// </summary>
        /// <param name="objObj">the added object</param>
        public void AddObjectiveObject(ObjectiveObject objObj)
        {
            objectiveObjects.Add(objObj);
        }

        /// <summary>
        /// Begins the objective, making it possible to complete and activating it's objectiveObjects
        /// </summary>
        public virtual void Begin()
        {
            onStarted.Invoke();

            foreach (ObjectiveObject o in objectiveObjects)
            {
                o.turnOn();
            }
        }

        /// <summary>
        /// Completes the objective and turns off the objectiveObjects
        /// </summary>
        public virtual void Complete()
        {
            onComplete.Invoke();

            foreach (ObjectiveObject o in objectiveObjects)
            {
                o.turnOff();
            }
        }

        /// <summary>
        /// Used to modify the listeners of the onComplete event by the holder
        /// </summary>
        /// <returns>the event</returns>
        public UnityEvent GetOnObjectiveCompleteEvent()
        {
            return onComplete;
        }

        /// <summary>
        /// Used to modify the listeners of the onStarted event by the holder
        /// </summary>
        /// <returns>the event</returns>
        public UnityEvent GetOnObjectiveStartedEvent()
        {
            return onStarted;
        }

        /// <summary>
        /// the text describing what need to be done to complete the objective
        /// </summary>
        /// <returns>the text</returns>
        public ObservableObject<string> GetObjectiveText() { return objectiveText; }

        public List<ObjectiveObject> GetObjectiveObjects() { return objectiveObjects; }
    }

}
