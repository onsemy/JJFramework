using UnityEngine;

namespace JJFramework.Runtime.Util
{
    // ref: http://tsubakit1.hateblo.jp/entry/2016/02/11/021743

    [DisallowMultipleComponent]
    public class WaitForAnimator : CustomYieldInstruction
    {
        private readonly Animator __animator;
        private readonly int __hash = 0;
        private readonly int __layerIndex = 0;

        public WaitForAnimator(Animator animator, int layerIndex)
        {
            __layerIndex = layerIndex;
            __animator = animator;
            __hash = animator.GetCurrentAnimatorStateInfo(layerIndex).fullPathHash;
        }

        public override bool keepWaiting
        {
            get
            {
                var currentAnimatorState = this.__animator.GetCurrentAnimatorStateInfo(this.__layerIndex);
                return ((currentAnimatorState.fullPathHash == this.__hash) && (currentAnimatorState.normalizedTime < 1));
            }
        }
    }
}
