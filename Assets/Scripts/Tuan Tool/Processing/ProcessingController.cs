using DG.Tweening.Core.Easing;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TuanTool.Processing
{
    public class ProcessingController : MonoBehaviour
    {
        #region Singleton
        private static ProcessingController ins;
        public static ProcessingController Ins
        {
            get
            {
                if (ins == null)
                {
                    var a = FindObjectOfType<ProcessingController>() ?? new GameObject("Processing Controller").AddComponent<ProcessingController>();
                    a?.Awake();
                }
                return ins;
            }
            set => ins = value;
        }

        #endregion
        [SerializeField] List<ProcessStack> processStacks = new List<ProcessStack>();

        public void AddProcess(ProcessStack processStack)
        {
            if (!ProcessStacks.Contains(processStack))
            {
                ProcessStacks.Add(processStack);
            }
        }

        public void RemoveProcess(ProcessStack processStack)
        {
            if (ProcessStacks.Contains(processStack))
            {
                ProcessStacks.Remove(processStack);
            }
        }


        public ProcessStack CreatProcess(string name)
        {
            var newProcess = new ProcessStack(name);
            AddProcess(newProcess);
            return newProcess;
        }

        public List<ProcessStack> ProcessStacks { get => processStacks; private set => processStacks = value; }

        void Awake()
        {
            #region Singleton
            if (ins == null)
                ins = this;
            else
            {
                if (ins != this)
                    Destroy(gameObject);
                return;
            }
            #endregion
            DontDestroyOnLoad(gameObject);


        }

        void Update()
        {
            foreach (var item in ProcessStacks.ToArray())
            {
                if (item.State != ProcessStack.EState.Processing)
                {
                    RemoveProcess(item);
                }
            }
        }


        [System.Serializable]
        public class ProcessStack
        {

            [SerializeField] private string name;

            public ProcessStack(string name)
            {
                this.name = name;
            }

            public enum EState { none, Processing, Completed, Error }
            private System.Exception exception;
            [SerializeField] private EState state = EState.Processing;

            public EState State { get => state; private set => state = value; }
            public Exception Exception { get => exception; private set => exception = value; }
            public string Name { get => name; }

            public void SetCompletedState()
            {
                State = EState.Completed;
            }

            public void SetErrorState(System.Exception Exception)
            {
                State = EState.Error;
                this.Exception = Exception;
            }
        }
    }
}