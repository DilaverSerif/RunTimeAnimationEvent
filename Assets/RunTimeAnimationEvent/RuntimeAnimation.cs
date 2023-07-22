using System;
using UnityEditor.Animations;
using UnityEngine;

namespace RunTimeAnimationEvent
{
    public enum ClipSearchType
    {
        ByStateName,
        ByClipName
    }

    public static class RuntimeAnimation
    {
        public struct AnimationData
        {
            public readonly AnimationClip Clip;
            public readonly Action Action;
            public readonly float Time;

            public AnimationData(AnimationClip clip, Action action, float time)
            {
                Clip = clip;
                Action = action;
                Time = time;
            }
        }

        private static void AddRunTimeAnimationEventComponent(this Animator animator, ref AnimationData animationData)
        {
            if (animator.gameObject.TryGetComponent(out AnimationEventRunTime runtimeAnimationEvent))
            {
                runtimeAnimationEvent.AddEvent(animationData);
            }
            else
            {
                runtimeAnimationEvent = animator.gameObject.AddComponent<AnimationEventRunTime>();
                runtimeAnimationEvent.AddEvent(animationData);
            }
        }

        public static void AddAnimationEvent(this Animator animator, string clipName, float time, Action action,
            ClipSearchType clipSearchType = ClipSearchType.ByClipName)
        {
            var clip = clipSearchType switch
            {
                ClipSearchType.ByClipName => GetClip(animator, clipName),
                ClipSearchType.ByStateName => GetClipByState(animator, clipName),

                _ => throw new ArgumentOutOfRangeException(nameof(clipSearchType), clipSearchType, null)
            };

            var animationData = new AnimationData(clip, action, time);

            AddRunTimeAnimationEventComponent(animator, ref animationData);
        }

        public static void AddAnimationEventNormalizedTime(this Animator animator, string clipName,
            float normalizedTime, Action action,
            ClipSearchType clipSearchType = ClipSearchType.ByClipName)
        {
            if (normalizedTime < 0 || normalizedTime > 1)
            {
                Debug.LogError("Normalized time must be between 0 and 1");
                return;
            }
            
            var clip = clipSearchType switch
            {
                ClipSearchType.ByClipName => GetClip(animator, clipName),
                ClipSearchType.ByStateName => GetClipByState(animator, clipName),

                _ => throw new ArgumentOutOfRangeException(nameof(clipSearchType), clipSearchType, null)
            };

            var time = normalizedTime * clip.length;

            var animationData = new AnimationData(clip, action, time);

            AddRunTimeAnimationEventComponent(animator, ref animationData);
        }


        public static AnimationClip GetClip(this Animator animator, string clipName)
        {
            var clips = animator.runtimeAnimatorController.animationClips;
            var clip = Array.Find(clips, c => c.name.Equals(clipName));
            if (clip != null) return clip;

            Debug.LogError("Clip not found");
            return null;
        }

        public static AnimationClip GetClipByState(this Animator animator, string clipName)
        {
            var runtimeController = animator.runtimeAnimatorController;

            if (runtimeController != null)
            {
                var animatorController = runtimeController as AnimatorController;
                if (animatorController == null) return null;

                foreach (var layer in animatorController.layers)
                {
                    foreach (var state in layer.stateMachine.states)
                    {
                        if (state.state.name != clipName) continue;

                        var clip = state.state.motion as AnimationClip;

                        if (clip == null) continue;

                        return clip;
                    }
                }
            }

            Debug.LogError("State not found: " + clipName);
            return null;
        }
    }
}