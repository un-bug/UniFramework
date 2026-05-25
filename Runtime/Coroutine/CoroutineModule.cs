using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UniFramework
{
    [DisallowMultipleComponent]
    public class CoroutineModule : UniFrameworkModule
    {
        private float m_CleanupTimer = 10f;
        private readonly List<CoroutineTask> m_Tasks = new List<CoroutineTask>();
        public IReadOnlyList<CoroutineTask> Tasks => m_Tasks;

        private void Awake()
        {
        }

        private void OnDestroy()
        {
            StopAllTasks();
        }

        private void Update()
        {
            m_CleanupTimer -= Time.unscaledDeltaTime;
            if (m_CleanupTimer > 0f)
            {
                return;
            }

            m_CleanupTimer = 1f;
            CleanupFinishedTasks();
        }

        public CoroutineTask StartTask(IEnumerator routine)
        {
            string name = routine != null ? routine.ToString() : "CoroutineTask";
            return StartTask(name, routine);
        }

        public CoroutineTask StartTask(string name, IEnumerator routine)
        {
            if (routine == null)
            {
                Debug.LogError("StartTask failed. Routine is null.");
                return null;
            }

            CoroutineTask task = new CoroutineTask(name, routine);
            task.Manager = this;
            task.Coroutine = StartCoroutine(Wrap(task));
            m_Tasks.Add(task);
            return task;
        }

        public void StopTask(CoroutineTask task)
        {
            if (task == null)
            {
                return;
            }

            if (task.IsRunning && task.Coroutine != null)
            {
                StopCoroutine(task.Coroutine);
            }

            task.IsRunning = false;
            task.Coroutine = null;
        }

        public void StopAllTasks()
        {
            for (int i = 0; i < m_Tasks.Count; i++)
            {
                StopTask(m_Tasks[i]);
            }

            m_Tasks.Clear();
        }

        public void CleanupFinishedTasks()
        {
            m_Tasks.RemoveAll(task => task == null || task.IsRunning == false);
        }

        private IEnumerator Wrap(CoroutineTask task)
        {
            yield return task.Routine;
            task.IsRunning = false;
            task.Coroutine = null;
        }
    }
}