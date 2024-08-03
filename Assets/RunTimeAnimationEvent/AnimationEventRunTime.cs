using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace RunTimeAnimationEvent
{
    public class AnimationEventRunTime : MonoBehaviour
    {
        [Serializable]
        public class RunTimeEventData
        {
            public Action EventAction;
            public float eventTime;
            public string actionName;

            public RunTimeEventData(Action eventAction, float time)
            {
                EventAction = eventAction;
                eventTime = time;
                actionName = eventAction.Method.Name;
            }

            public void InvokeAction()
            {
                EventAction?.Invoke();
            }
        }
        
        [SerializeField,HideInInspector]
        private List<RunTimeEventData> animationStructs = new List<RunTimeEventData>();

        public void AddEvent(ref RuntimeAnimation.AnimationData data)
        {
            if (animationStructs.Contains(new RunTimeEventData(data.Action, data.Time)))
            {
                Debug.LogWarning("This event already exists");
                return;
            }
            
            animationStructs.Add(new RunTimeEventData(data.Action, data.Time));

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
                if (animationStruct.actionName.Equals(data.Action.Method.Name) &&
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
            }
            
            //Debug.LogWarning("This event does not exist or has already removed");
        }
    }
}