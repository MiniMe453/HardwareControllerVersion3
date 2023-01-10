using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using AlekGames.Placer.Systems.Main;
using AlekGames.Placer.Shared;


namespace AlekGames.Editor
{

    [CustomEditor(typeof(textSpawner))]
    public class textSpawnerEditor : UnityEditor.Editor
    {

        [MenuItem("Tools/Alek Games/Placer/Add 3DText Spawner")]
        public static void add()
        {
            GameObject gs = new GameObject(" 3DText Spawner Tool");
            gs.AddComponent<textSpawner>();

            EditorUtility.SetDirty(gs);
            Undo.RegisterCreatedObjectUndo(gs, "added  3DText Spawner Tool");
        }



        public override void OnInspectorGUI()
        {
            textSpawner s = (textSpawner)target;

            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(s.font)));

            if(s.font == null)
            {
                EditorGUILayout.HelpBox("fill in font", MessageType.Error);
                return;
            }

            string t = s.text;
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(s.text)));
            serializedObject.ApplyModifiedProperties();
            if (t != s.text && s.autoUpdate)
            {
                s.revert();
                s.spawn();
                Repaint();
            }

            GUILayout.Space(10);

            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(s.autoUpdate)));

            GUILayout.Space(10);

            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(s.asPrefab)));

            GUILayout.Space(10);

            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(s.addSpaceColliders)));

            GUILayout.Space(10);

            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(s.betweenLettersSpace)));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(s.spaceLengh)));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(s.enterLengh)));

            GUILayout.Space(10);

            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(s.randPosAdd)));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(s.randRotAdd)));

            GUILayout.Space(10);

            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(s.minMaxScale)));

            GUILayout.Space(10);

            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(s.snapToGround)));
            if(s.snapToGround)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(s.normalAllighn)));
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(s.groundLayer)));
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(s.snapHeightOffset)));

                GUILayout.Space(10);

                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(s.snapRayDir)));


            }

            serializedObject.ApplyModifiedProperties();



            GUILayout.Space(10);


            EditorGUI.BeginChangeCheck();

            if (GUILayout.Button("Spawn")) s.spawn();

            GUILayout.Space(20);

            if (GUILayout.Button("Revert (delete children)")) s.revert();

            if (EditorGUI.EndChangeCheck()) EditorUtility.SetDirty(s);
        }


        private void OnSceneGUI()
        {
            textSpawner s = (textSpawner)target;

            if(s.snapToGround)
            {
                Vector3 before = s.snapRayDir;
                Vector3 now = Handles.PositionHandle(s.snapRayDir + s.transform.position, Quaternion.identity) - s.transform.position;

                if (now != before)
                {
                    Undo.RegisterCompleteObjectUndo(s, "text spawner snapDirChange");
                    s.snapRayDir = now;
                    EditorUtility.SetDirty(s);
                }
            }
        }
    }
}
