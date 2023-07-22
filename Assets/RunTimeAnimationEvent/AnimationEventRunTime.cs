using System;
using System.Collections.Generic;
using UnityEngine;

namespace RunTimeAnimationEvent
{
    public class AnimationEventRunTime : MonoBehaviour
    {
        [Serializable]
        public class AnimationStruct
        {
            public Action EventAction;
            public float eventTime;

            public string ActionName;

            public AnimationStruct(Action eventAction, float time)
            {
                EventAction = eventAction;
                eventTime = time;
                ActionName = eventAction.Method.Name;
            }

            public void InvokeAction()
            {
                EventAction?.Invoke();
            }
        }
        
        [SerializeField,HideInInspector]
        private List<AnimationStruct> animationStructs = new List<AnimationStruct>();

        public void AddEvent(RuntimeAnimation.AnimationData data)
        {
            animationStructs.Add(new AnimationStruct(data.Action, data.Time));

            data.Clip.AddEvent(new AnimationEvent
            {
                functionName = nameof(InvokeAction),
                floatParameter = data.Time,
                time = data.Time
            });
        }


        public void InvokeAction(float time)
        {
            foreach (var animationStruct in animationStructs)
            {
                if (!(Math.Abs(animationStruct.eventTime - time) < 0.01f)) continue;
                animationStruct.InvokeAction();
                break;
            }
        }
    }
}