using UnityEngine;

namespace JJFramework.Runtime.Util
{
    [DisallowMultipleComponent]
    public class WaitForAnimation : CustomYieldInstruction
    {
        private readonly Animation __animation;
        private readonly string __animationName;

        public WaitForAnimation(Animation animation, string animationName)
        {
            __animation = animation;
            __animationName = animationName;
            __animation.PlayQueued(animationName);
        }

        public override bool keepWaiting => __animation.IsPlaying(this.__animationName);
    }
}
