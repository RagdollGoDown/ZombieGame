using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Observable;

namespace Utility.Observable
{
    public class ReadOnlyObservableFloat : ReadOnlyObservableObject<float>
    {
        public ReadOnlyObservableFloat(ObservableFloat observable) 
            : base(observable){ }
    }

}
