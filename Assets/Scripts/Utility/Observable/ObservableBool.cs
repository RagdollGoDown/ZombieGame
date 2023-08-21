using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utility.Observable
{
    public class ObservableBool : ObservableObject<bool>
    {
        public ObservableBool(bool value) : base(value) {}
    }
}
    
