using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using AlekGames.Placer.Systems.Main;
using AlekGames.Placer.Shared;

namespace AlekGames.Editor
{
    [CustomEditor(typeof(GridSpawner))]
    public class GridSpawnerEditor : UnityEditor.Editor
    {

        [MenuItem("Tools/Alek Games/Placer/Add Grid Spawner")]
        public static void add()
        {
            GameObject gs = new GameObject("Grid Spawner Tool");
            gs.AddComponent<GridSpawner>();

            EditorUtility.SetDirty(gs);
            Undo.RegisterCreatedObjectUndo(gs, "added Grid Spawner Tool");
        }


        public override void OnInspectorGUI()
        {
            GridSpawner g = (GridSpawner)target;

            if (GUILayout.Button("open palette selector"))
                prefabPalleteSelectorEditorWindow.showWindow(g);

            DrawDefaultInspector();

            GUILayout.Space(20);

            EditorGUI.BeginChangeCheck();

            g.drawGizmos = EditorGUILayout.Toggle("draw gizmos", g.drawGizmos);
            g.gizmoDistance = EditorGUILayout.FloatField("renderer distance", g.gizmoDistance);


            if (EditorGUI.EndChangeCheck()) EditorUtility.SetDirty(g);

            GUILayout.Space(10);

            if (GUILayout.Button("Spawn")) g.spawn();

            GUILayout.Space(20);

            if (GUILayout.Button("Revert (delete children)")) g.revert();
        }
    }
}
