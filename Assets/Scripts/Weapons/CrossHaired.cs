using Utility.Observable;
using UnityEngine.Events;
using UnityEngine;

public interface CrossHaired
{
    void SetSpreadOrigin(Transform origin);

    ReadOnlyObservableFloat GetSpread();
}
