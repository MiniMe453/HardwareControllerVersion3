using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;

using AlekGames.Placer.Systems.Main;
using AlekGames.Placer.Shared;

namespace AlekGames.Editor
{
    [CustomEditor(typeof(wallPainter))]
    public class wallPainterEditor : UnityEditor.Editor
    {
        [MenuItem("Tools/Alek Games/Placer/Add Wall Painter")]
        public static void addPainter()
        {
            GameObject pt = new GameObject("Wall Paint Tool");
            pt.AddComponent<wallPainter>();

            EditorUtility.SetDirty(pt);
            Undo.RegisterCreatedObjectUndo(pt, "added painter tool");
        }

        

        public override void OnInspectorGUI()
        {
            wallPainter p = (wallPainter)target;

            Color old = GUI.color;
            if (p.enabledPaint) GUI.color = Color.red;


            DrawDefaultInspector();

            if(p.wallPreset == null)
            {
                EditorGUILayout.HelpBox("fill in wall preset", MessageType.Error);
                return;
            }

            GUILayout.Space(20);

            EditorGUI.BeginChangeCheck();

            if (p.enabledPaint)
            {
                if (GUILayout.Button("Undo")) p.undo();

                GUILayout.Space(20);

                if (GUILayout.Button("--- Disable ---")) p.Disable();
            }
            else if (GUILayout.Button("--- Enable ---")) p.Enable();

            GUILayout.Space(20);


            if (GUILayout.Button("Revert (delete children)")) p.revert();



            GUI.color = old;
        }

        private void OnSceneGUI()
        {
            wallPainter p = (wallPainter)target;


            if (p.enabledPaint)
            {
                Handles.color = Color.green;
                Handles.DrawSolidDisc(p.paintPos, p.paintNormal, 1);
            }
            
        }
    }
}
