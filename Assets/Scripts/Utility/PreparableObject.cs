using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utility
{
    /// <summary>
    /// This is to be a replacement for awake as there are often conflicts of timing 
    /// with other awake calls. This gives alot more control over when the object is awoken.
    /// </summary>
    public interface IPreparable
    {
        public void Prepare();
    }
}