using System;
using System.Linq;
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
                runtimeAnimationEvent.AddEvent(ref animationData);
            }
            else
            {
                runtimeAnimationEvent = animator.gameObject.AddComponent<AnimationEventRunTime>();
                runtimeAnimationEvent.AddEvent(ref animationData);
            }
        }

        public static void RemoveAnimationEvent(this Animator animator, Action action, float time)
        {
            var animationData = new AnimationData(null, action, time);
            if (animator.gameObject.TryGetComponent(out AnimationEventRunTime runtimeAnimationEvent))
            {
                runtimeAnimationEvent.RemoveEvent(animationData);
                return;
            }

            Debug.LogWarning("This event does not exist");
        }

        public static void AddAnimationEventByStateName(this Animator animator, string stateName, float time, Action action,
                                                        string clipName = null,int animationLayerIndex = 0)
        {
            var clip = GetClipByState(animator, stateName, animationLayerIndex,clipName);
            var animationData = new AnimationData(clip, action, time);

            AddRunTimeAnimationEventComponent(animator, ref animationData);
        }
        
        public static void AddAnimationEvent(this Animator animator, string clipName, float time, Action action)
        {
            var clip = GetClip(animator, clipName);
            var animationData = new AnimationData(clip, action, time);

            AddRunTimeAnimationEventComponent(animator, ref animationData);
        }

        public static void AddAnimationEventNormalizedTime(this Animator animator, string clipName,
                                                           float normalizedTime, Action action)
        {
            if (normalizedTime < 0 || normalizedTime > 1)
            {
                Debug.LogError("Normalized time must be between 0 and 1");
                return;
            }

            var clip = GetClip(animator, clipName);
            var time = normalizedTime * clip.length;

            var animationData = new AnimationData(clip, action, time);

            AddRunTimeAnimationEventComponent(animator, ref animationData);
        }
        
        
        public static void AddAnimationEventNormalizedTimeByStateName(this Animator animator, string stateName,
                                                                      float normalizedTime, Action action,
                                                                      string clipName = null,int animationLayerIndex = 0)
        {
            if (normalizedTime < 0 || normalizedTime > 1)
            {
                Debug.LogError("Normalized time must be between 0 and 1");
                return;
            }

            var clip = GetClipByState(animator, stateName, animationLayerIndex,clipName);
            var time = normalizedTime * clip.length;

            var animationData = new AnimationData(clip, action, time);

            AddRunTimeAnimationEventComponent(animator, ref animationData);
        }



        private static AnimationClip GetClip(this Animator animator, string clipName)
        {
            var clips = animator.runtimeAnimatorController.animationClips;
            var clip = Array.Find(clips, c => c.name.Equals(clipName));
            if (clip != null) return clip;

            Debug.LogError("Clip not found");
            return null;
        }

        private static AnimationClip GetClipByState(this Animator animator, string stateName, int animationLayerIndex = 0, string targetClipName = null)
        {
            var stateInfo = animator.GetCurrentAnimatorStateInfo(animationLayerIndex);

            if (targetClipName == null && stateInfo.IsName(stateName))
            {
                var clips = animator.runtimeAnimatorController.animationClips;
                int count = clips.ToList().FindAll(c => c.name == stateName).Count;
                
                if (count > 1)
                {
                    Debug.LogWarning($"Found {count} clips with name {stateName}");
                }
            }
            
            if (stateInfo.IsName(stateName))
            {
                var clips = animator.runtimeAnimatorController.animationClips;

                foreach (var clip in clips)
                {
                    if (targetClipName == null)
                        return clip;
                    
                    if (clip.name == targetClipName)
                        return clip;
                }
            }
            
            Debug.LogError($"Not fount state with name {stateName}");
            return null;
        }
    }
}
