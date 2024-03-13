using System.Collections;
using System.Threading;
using UnityEngine;

namespace Utility.Tweening
{
    public abstract class TweenComponentAsync<T> : TweenComponent<T> where T : Component
    {
        [SerializeField] private int _stepInMilliseconds = 50;
        protected int stepMilliseconds
        {
            get
            {
                return _stepInMilliseconds;
            }
        }

        private int _durationInMilliseconds = -1;
        protected int durationInMilliseconds
        {
            get
            {
                if (_durationInMilliseconds == -1)
                {
                    _durationInMilliseconds = (int)(Duration * 1000);
                }

                return _durationInMilliseconds;
            }
        }
    }
}