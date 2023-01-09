using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using AlekGames.Placer.Systems.Main;

namespace AlekGames.Editor
{
    [CustomEditor(typeof(stepManager))]
    public class StepManagerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            stepManager s = (stepManager)target;

            GUILayout.Space(20);

            if (GUILayout.Button("auto find steps from children")) s.steps = s.GetComponentsInChildren<stepGenerator>();

            GUILayout.Space(10);

            DrawDefaultInspector();

            GUILayout.Space(10);

            EditorGUI.BeginChangeCheck();

            if (GUILayout.Button("Spawn")) s.spawn();

            EditorGUILayout.HelpBox("revert works only when script data has been kept, so if you change scene/ reset unity, it will not work", MessageType.Warning);

            if (GUILayout.Button("Revert")) s.revert();

            if (EditorGUI.EndChangeCheck()) EditorUtility.SetDirty(s);
        }
    }
}
