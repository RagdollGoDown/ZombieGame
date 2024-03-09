using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Utility.Events
{
    public class InvokeAnimationStateEvents : StateMachineBehaviour
    {

        public UnityEvent<AnimationStateData> onEnter;
        public UnityEvent<AnimationStateData> onUpdate;
        public UnityEvent<AnimationStateData> onExit;

        // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
           onEnter?.Invoke(new AnimationStateData { animator = animator, stateInfo = stateInfo, layerIndex = layerIndex });
        }

        // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
        override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            onUpdate?.Invoke(new AnimationStateData { animator = animator, stateInfo = stateInfo, layerIndex = layerIndex });
        }

        // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            onExit?.Invoke(new AnimationStateData { animator = animator, stateInfo = stateInfo, layerIndex = layerIndex });
        }

        // OnStateMove is called right after Animator.OnAnimatorMove()
        //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        //{
        //    // Implement code that processes and affects root motion
        //}

        // OnStateIK is called right after Animator.OnAnimatorIK()
        //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        //{
        //    // Implement code that sets up animation IK (inverse kinematics)
        //}
    }

    public class AnimationStateData
    {
        public Animator animator;
        public AnimatorStateInfo stateInfo;
        public int layerIndex;
    }   
}
