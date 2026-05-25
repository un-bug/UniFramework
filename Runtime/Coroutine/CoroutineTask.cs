using System.Collections;
using UnityEngine;

namespace UniFramework
{
    public class CoroutineTask
    {
        public string Name { get; }
        public IEnumerator Routine { get; }
        public Coroutine Coroutine { get; internal set; }
        public float StartTime { get; }
        public bool IsRunning { get; internal set; }
        public float ElapsedTime => Time.unscaledTime - StartTime;
        public CoroutineTask(string name, IEnumerator routine)
        {
            Name = string.IsNullOrWhiteSpace(name) ? "CoroutineTask" : name;
            Routine = routine;
            StartTime = Time.unscaledTime;
            IsRunning = true;
        }
        internal CoroutineModule Manager { get; set; }
        public void Stop()
        {
            if (Manager)
            {
                Manager.StopTask(this);
            }
            else
            {
                Debug.LogWarning($"CoroutineTask '{Name}' has no manager assigned. Cannot stop the task.");
            }
        }
    }
}