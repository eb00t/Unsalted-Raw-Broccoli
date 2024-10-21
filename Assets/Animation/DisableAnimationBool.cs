using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableAnimationBool : StateMachineBehaviour
{
    public string TargetBool;
    public enum WhenToDisable
    {
        OnStateEnter,
        OnStateUpdate,
        OnStateExit,
    }

    public WhenToDisable whenToDisable;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (whenToDisable == WhenToDisable.OnStateEnter)
        {
            animator.SetBool(TargetBool, false);
        } 
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (whenToDisable == WhenToDisable.OnStateUpdate)
        {
            animator.SetBool(TargetBool, false);
        }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (whenToDisable == WhenToDisable.OnStateExit)
        {
            animator.SetBool(TargetBool, false);
        }
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
