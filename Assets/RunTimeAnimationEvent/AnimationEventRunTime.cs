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

        public void AddEvent(ref RuntimeAnimation.AnimationData data)
        {
            if (animationStructs.Contains(new AnimationStruct(data.Action, data.Time)))
            {
                Debug.LogWarning("This event already exists");
                return;
            }
            
            animationStructs.Add(new AnimationStruct(data.Action, data.Time));

            data.Clip.AddEvent(new AnimationEvent
            {
                functionName = nameof(InvokeAction),
                floatParameter = data.Time,
                time = data.Time
            });
        }
        
        public void RemoveEvent(RuntimeAnimation.AnimationData data)
        {
            foreach (var animationStruct in animationStructs)
            {
                if (animationStruct.ActionName.Equals(data.Action.Method.Name) &&
                    Math.Abs(animationStruct.eventTime - data.Time) < 0.01f)
                {
                    animationStructs.Remove(animationStruct);
                    return;
                }
            }
            
            Debug.LogWarning("This event does not exist or has already removed");
        }


        public void InvokeAction(float time)
        {
            foreach (var animationStruct in animationStructs)
            {
                if (!(Math.Abs(animationStruct.eventTime - time) < 0.01f)) 
                    continue;
                
                animationStruct.InvokeAction();
                return;
            }
            
            Debug.LogWarning("This event does not exist or has already removed");
        }
    }
}