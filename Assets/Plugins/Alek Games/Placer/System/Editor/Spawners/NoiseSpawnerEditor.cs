using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using AlekGames.Placer.Systems.Main;
using AlekGames.Placer.Shared;

namespace AlekGames.Editor
{
    [CustomEditor(typeof(NoiseSpawner))]
    public class NoiseSpawnerEditor : UnityEditor.Editor
    {

        [MenuItem("Tools/Alek Games/Placer/Add Noise Spawner")]
        public static void add()
        {
            GameObject gs = new GameObject("Noise Spawner Tool");
            gs.AddComponent<NoiseSpawner>();

            EditorUtility.SetDirty(gs);
            Undo.RegisterCreatedObjectUndo(gs, "added Noise Spawner Tool");
        }

        public override void OnInspectorGUI()
        {
            NoiseSpawner n = (NoiseSpawner)target;

            if (GUILayout.Button("open palette selector"))
                prefabPalleteSelectorEditorWindow.showWindow(n);

            DrawDefaultInspector();

            GUILayout.Space(20);

            EditorGUI.BeginChangeCheck();

            n.drawGizmos = EditorGUILayout.Toggle("draw gizmos", n.drawGizmos);
            n.gizmoDistance = EditorGUILayout.FloatField("renderer distance", n.gizmoDistance);

            GUILayout.Space(30);

            if (GUILayout.Button("randomize Seed"))
            {
                n.setSeed(Random.Range(int.MinValue, int.MaxValue));
            }

            if(EditorGUI.EndChangeCheck()) EditorUtility.SetDirty(n);

            GUILayout.Space(10);


            if (GUILayout.Button("Spawn")) n.spawn();

            GUILayout.Space(20);

            if (GUILayout.Button("Revert (delete children)")) n.revert();

            
        }
    }
}