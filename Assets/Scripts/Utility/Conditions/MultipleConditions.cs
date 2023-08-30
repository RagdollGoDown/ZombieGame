using System.Collections.Generic;

namespace Utility.Conditions
{
    public abstract class MultipleConditions : Condition
    {
        /// <summary>
        /// Adds a condition to this one
        /// </summary>
        /// <param name="condition">the added condition</param>
        public abstract void AddCondition(Condition condition);

        /// <summary>
        /// Removes a condition from this one if it has it
        /// </summary>
        /// <param name="condition">the condition removed</param>
        public abstract void RemoveCondition(Condition condition);
    }
}