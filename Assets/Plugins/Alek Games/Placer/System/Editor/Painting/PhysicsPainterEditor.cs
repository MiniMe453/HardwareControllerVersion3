using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;
using AlekGames.Placer.Systems.Main;
using AlekGames.Placer.Shared;

namespace AlekGames.Editor
{

    [CustomEditor(typeof(PhysicsPainter))]
    public class PhysicsPainterEditor : UnityEditor.Editor
    {
        [MenuItem("Tools/Alek Games/Placer/Add Physics Painter")]
        public static void addPainter()
        {
            GameObject pt = new GameObject("Physics Painter Tool");
            pt.AddComponent<PhysicsPainter>();

            EditorUtility.SetDirty(pt);
            Undo.RegisterCreatedObjectUndo(pt, "added physics painter tool");
        }


        public override void OnInspectorGUI()
        {

            PhysicsPainter p = (PhysicsPainter)target;

            Color old = GUI.color;
            if (p.enabledPaint) GUI.color = Color.red;

            if (GUILayout.Button("open palette selector"))
                prefabPalleteSelectorEditorWindow.showWindow(p);

            DrawDefaultInspector();

            GUILayout.Space(20);

            EditorGUI.BeginChangeCheck();


            if (p.enabledPaint)
            {
                if (GUILayout.Button("--- Disable ---")) p.Disable();
            }
            else if (GUILayout.Button("--- Enable ---")) p.Enable();

            GUILayout.Space(20);


            if (GUILayout.Button("Revert (delete children)")) p.revert();

            if (EditorGUI.EndChangeCheck()) EditorUtility.SetDirty(p);

            GUI.color = old;
        }


        private void OnSceneGUI()
        {
            PhysicsPainter p = (PhysicsPainter)target;

            if(p.enabledPaint)
            {
                Color c = Color.cyan;
                c.a = 0.5f;
                Handles.color = c;

                Handles.DrawSolidDisc(p.paintPos, Vector3.up, p.brushSize);
            }
        }
    }
}
