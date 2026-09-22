using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace TuanTool
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Animator))]
    public class AnimationEvent : MonoBehaviour
    {

        public List<StackEvent> Events = new List<StackEvent>();
        Dictionary<string, StackEvent> dictEvents = new Dictionary<string, StackEvent>();

        bool isInit = false;
        private void Awake()
        {
            if (isInit)
                return;

            isInit = true;
            foreach (var item in Events)
            {
                if (dictEvents.ContainsKey(item.EventName))
                    Debug.LogError($"Contain 2 AnimationEvent with name \"{item.EventName}\" in \"{gameObject.name}\"!!!");
                else
                    dictEvents.Add(item.EventName, item);
            }
        }



        public void CallEvent(string EventName)
        {
            if (!isInit)
                Awake();
            if (dictEvents.ContainsKey(EventName))
                dictEvents[EventName].OnCall?.Invoke();
            else
                Debug.LogError($"Don't have AnimationEvent with name <color=green>{EventName}</color> in <color=green>{gameObject.name}</color> !");
        }

        public UnityEvent GetEvent(string EventName)
        {
            if (!isInit)
                Awake();
            if (dictEvents.ContainsKey(EventName))
                return dictEvents[EventName].OnCall;
            else
            {
                Debug.LogError($"Don't have AnimationEvent with name <color=green>{EventName}</color> in <color=green>{gameObject.name}</color> !");
                return null;
            }
        }

        [Serializable]
        public class StackEvent
        {
            public string EventName;
            public UnityEvent OnCall = new UnityEvent();
        }
    }
}