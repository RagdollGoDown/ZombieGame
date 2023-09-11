namespace Utility.Observable
{
    public class ObservableFloat: ObservableObject < float >
    {
        public ObservableFloat(): base(0f) {}
        public ObservableFloat(float value): base(value) {}
    }
}