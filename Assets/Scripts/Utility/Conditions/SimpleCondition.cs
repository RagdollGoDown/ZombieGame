using Utility.Observable;

namespace Utility.Conditions
{
    public class SimpleCondition : Condition
    {
        private bool value;

        public SimpleCondition(ObservableBool observedBool)
        {
            observedBool.onValueChange += SetValue;
        }

        private void SetValue(bool value)
        {
            this.value = value;
        }

        public override bool IsFullfilled()
        {
            return value;
        }
    }
}