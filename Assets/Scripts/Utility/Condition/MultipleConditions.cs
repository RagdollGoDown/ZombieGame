using System.Collections.Generic;

namespace Utility.Condition
{
    public class MultipleConditions : Condition
    {
        private List<Condition> conditions;

        public MultipleConditions(List<Condition> conditions)
        {
            this.conditions = conditions;
        }

        public override bool IsFullfilled()
        {
            foreach(Condition c in conditions)
            {
                if (!c.IsFullfilled()) return false;
            }

            return true;
        }
    }
}