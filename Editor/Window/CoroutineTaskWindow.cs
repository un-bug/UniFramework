using UnityEditor;
using UnityEngine;

namespace UniFramework.Editor
{
    public class CoroutineTaskWindow : EditorWindow
    {
        private Vector2 m_ScrollPos;

        [MenuItem("UniFramework/Coroutine Task Window")]
        private static void Open()
        {
            var window = GetWindow<CoroutineTaskWindow>("Coroutine Tasks");
            window.minSize = new Vector2(650f, 450f);
        }

        private void OnEnable()
        {
            EditorApplication.update += RepaintWindow;
        }

        private void OnDisable()
        {
            EditorApplication.update -= RepaintWindow;
        }

        private void RepaintWindow()
        {
            if (Application.isPlaying)
            {
                Repaint();
            }
        }

        private void OnGUI()
        {
            if (Application.isPlaying == false)
            {
                EditorGUILayout.HelpBox("Enter Play Mode to view coroutine tasks.", MessageType.Info);
                return;
            }

            CoroutineModule manager = FindObjectOfType<CoroutineModule>();

            if (manager == null)
            {
                EditorGUILayout.HelpBox("CoroutineManager not found in scene.", MessageType.Warning);
                return;
            }

            DrawToolbar(manager);
            DrawTaskList(manager);
        }

        private void DrawToolbar(CoroutineModule manager)
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            GUILayout.Label($"Tasks: {manager.Tasks.Count}", GUILayout.Width(100));

            if (GUILayout.Button("Cleanup", EditorStyles.toolbarButton, GUILayout.Width(80)))
            {
                manager.CleanupFinishedTasks();
            }

            if (GUILayout.Button("Stop All", EditorStyles.toolbarButton, GUILayout.Width(80)))
            {
                manager.StopAllTasks();
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawTaskList(CoroutineModule manager)
        {
            EditorGUILayout.Space(4);

            DrawHeader();

            m_ScrollPos = EditorGUILayout.BeginScrollView(m_ScrollPos);

            for (int i = 0; i < manager.Tasks.Count; i++)
            {
                CoroutineTask task = manager.Tasks[i];

                if (task == null)
                {
                    continue;
                }

                DrawTaskItem(manager, task);
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawHeader()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

            GUILayout.Label("Name", EditorStyles.boldLabel, GUILayout.ExpandWidth(true));
            GUILayout.Label("State", EditorStyles.boldLabel, GUILayout.Width(80));
            GUILayout.Label("Elapsed", EditorStyles.boldLabel, GUILayout.Width(80));
            GUILayout.Label("Action", EditorStyles.boldLabel, GUILayout.Width(80));

            EditorGUILayout.EndHorizontal();
        }

        private void DrawTaskItem(CoroutineModule manager, CoroutineTask task)
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

            GUILayout.Label(task.Name, GUILayout.ExpandWidth(true));

            string state = task.IsRunning ? "Running" : "Finished";
            GUILayout.Label(state, GUILayout.Width(80));

            GUILayout.Label($"{task.ElapsedTime:F2}s", GUILayout.Width(80));

            GUI.enabled = task.IsRunning;

            if (GUILayout.Button("Stop", GUILayout.Width(80)))
            {
                manager.StopTask(task);
            }

            GUI.enabled = true;

            EditorGUILayout.EndHorizontal();
        }
    }
}