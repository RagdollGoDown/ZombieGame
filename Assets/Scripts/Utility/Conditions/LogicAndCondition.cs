using System.Collections.Generic;

namespace Utility.Conditions
{
    public class LogicAndCondition : MultipleConditions
    {
        private List<Condition> conditions;

        public LogicAndCondition()
        {
            conditions = new();
        }

        public override void RemoveCondition(Condition condition)
        {
            conditions.Remove(condition);
        }

        public override void AddCondition(Condition condition)
        {
            conditions.Add(condition);
        }

        /// <summary>
        /// If one of the conditions is false then this is false
        /// otherwise it is true
        /// </summary>
        /// <returns>true or false</returns>
        public override bool IsFullfilled()
        {
            for (int i = 0; i < conditions.Count; i++)
            {
                if (!conditions[i].IsFullfilled()) return false;
            }

            return true;
        }
    }
}