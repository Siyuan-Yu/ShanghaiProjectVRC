using NukoTween;
using Sirenix.OdinInspector;
using UdonSharp;
using UnityEngine;
using VRRefAssist;

namespace Utilities
{
    [Singleton]
    public class TweenManager: UdonSharpBehaviour
    {
        [Required] public NukoTweenEngine tween;

        public int MoveTo(GameObject target, Vector3 to, float duration, float delay, int easeId, bool relative)
        {
            return tween.MoveTo(target, to, duration, delay, easeId, relative);
        }


        public int LocalMoveTo(GameObject target, Vector3 to, float duration, float delay, int easeId, bool relative)
        {
            return tween.LocalMoveTo(target, to, duration, delay, easeId, relative);
        }

        public int LocalScaleTo(GameObject target, Vector3 to, float duration, float delay, int easeId, bool relative)
        {
            return tween.LocalScaleTo(target, to, duration, delay, easeId, relative);
        }
        public int RotateTo(GameObject target, Vector3 to, float duration, float delay, int easeId, bool relative)
        {
            return tween.RotateTo(target, to, duration, delay, easeId, relative);
        }

        public int LocalRotateTo(GameObject target, Vector3 to, float duration, float delay, int easeId, bool relative)
        {
            return tween.LocalRotateTo(target, to, duration, delay, easeId, relative);
        }

        public int LocalRotateQuaternionTo(GameObject target, Quaternion to, float duration, float delay, int easeId,
            bool relative)
        {
            return tween.LocalRotateQuaternionTo(target, to, duration, delay, easeId, relative);
        }

        public void DelayedCall(UdonSharpBehaviour target, string customEventName, float delay)
        {
            tween.DelayedCall(target, customEventName, delay);
        }

        public void Complete(int tweenId)
        {
            tween.Complete(tweenId);
        }

        public void CompleteAll()
        {
            tween.CompleteAll();
        }

        public void Kill(int tweenId)
        {
            tween.Kill(tweenId);
        }

        public void KillAll()
        {
            tween.KillAll();
        }

        public void LoopRestart(int tweenId, int loops)
        {
            tween.LoopRestart(tweenId, loops);
        }

        public void LoopIncremental(int tweenId, int loops)
        {
            tween.LoopIncremental(tweenId, loops);
        }


        #region enum from NukoTween

        #region enum Ease

        public readonly int EaseLinear = 0;
        public readonly int EaseInSine = 10;
        public readonly int EaseOutSine = 11;
        public readonly int EaseInOutSine = 12;
        public readonly int EaseInQuad = 20;
        public readonly int EaseOutQuad = 21;
        public readonly int EaseInOutQuad = 22;
        public readonly int EaseInCubic = 30;
        public readonly int EaseOutCubic = 31;
        public readonly int EaseInOutCubic = 32;
        public readonly int EaseInQuart = 40;
        public readonly int EaseOutQuart = 41;
        public readonly int EaseInOutQuart = 42;
        public readonly int EaseInQuint = 50;
        public readonly int EaseOutQuint = 51;
        public readonly int EaseInOutQuint = 52;
        public readonly int EaseInExpo = 60;
        public readonly int EaseOutExpo = 61;
        public readonly int EaseInOutExpo = 62;
        public readonly int EaseInCirc = 70;
        public readonly int EaseOutCirc = 71;
        public readonly int EaseInOutCirc = 72;
        public readonly int EaseInBack = 80;
        public readonly int EaseOutBack = 81;
        public readonly int EaseInOutBack = 82;
        public readonly int EaseInElastic = 90;
        public readonly int EaseOutElastic = 91;
        public readonly int EaseInOutElastic = 92;
        public readonly int EaseInBounce = 100;
        public readonly int EaseOutBounce = 101;
        public readonly int EaseInOutBounce = 102;

        #endregion

        #endregion
    }
}