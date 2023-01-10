using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using AlekGames.Placer.Profiles;

namespace AlekGames.Editor
{
    [CustomEditor(typeof(prefabPalette))]
    public class prefabPaletteEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            prefabPalette pp = (prefabPalette)target;

            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(pp.objects)));

            GUILayout.Space(10);

            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(pp.spawnAsPrefab)));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(pp.snapChildren)));
            if(pp.snapChildren) EditorGUILayout.HelpBox("when snappng children make sure to seperate them into child objects of prefab", MessageType.Info);

            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(pp.minMaxScale)));
            if (pp.minMaxScale == Vector2.zero) GUILayout.Label("disabled");
            EditorGUILayout.EndHorizontal();
            if (pp.minMaxScale != Vector2.zero) EditorGUILayout.HelpBox("to disable overiding transform scale, set minMaxScale to v2.zero (0,0)", MessageType.Info);
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(pp.randRotAdd)));

            GUILayout.Space(10);

            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(pp.maxNormal)));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(pp.normalAllighn)));

            GUILayout.Space(10);

            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(pp.groundLayer)));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(pp.avoidedLayer)));

            serializedObject.ApplyModifiedProperties();
        }
    }
}
