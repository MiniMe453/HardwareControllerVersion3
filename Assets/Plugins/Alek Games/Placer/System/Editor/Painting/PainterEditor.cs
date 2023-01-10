using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using AlekGames.Placer.Systems.Main;
using AlekGames.Placer.Shared;

namespace AlekGames.Editor
{
      [CustomEditor(typeof(Painter))]
    public class PainterEditor : UnityEditor.Editor
    {
        [MenuItem("Tools/Alek Games/Placer/Add Painter")]
        public static void addPainter()
        {          
            GameObject pt = new GameObject("Painter Tool");
            pt.AddComponent<Painter>();

            EditorUtility.SetDirty(pt);
            Undo.RegisterCreatedObjectUndo(pt, "added painter tool");
        }


        public override void OnInspectorGUI()
        {
            Painter p = (Painter)target;

            Color old = GUI.color;
            if (p.enabledPaint) GUI.color = Color.red;

            

            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(p.paintMode)));
            EditorGUILayout.HelpBox("you can switch between paint modes with 1,2,3,4 keys on your keybord when painting. \n when you want to switch make sure GUI is updating (holding left click will be enaugh)", MessageType.Info);

            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(p.recordUndo)));

            GUILayout.Space(10);

            if (p.isPlacingMode())
            {
                drawPalette(p);

                GUILayout.Space(10);

                drawGridSettings(p);

                GUILayout.Space(10);

                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(p.YRotationOffset)));
                EditorGUILayout.HelpBox("left click when painting to change by 90 degrees", MessageType.Info);

                GUILayout.Space(10);

                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(p.holdActivation)));

                if (p.holdActivation)
                {
                    EditorGUILayout.HelpBox("when using holdActivation make sure to not leave the scene window while holding mouse 0 button, and then realising. system doesnt register relises when you are not in scene window", MessageType.Warning);

                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(p.holdActivationDistance)));
                }


                if (p.paintMode == Painter.paintM.scatter)
                {
                    GUILayout.Space(10);

                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(p.brushSize)));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(p.scatterCount)));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(p.scatterAvoidenceDistance)));
                }

                GUILayout.Space(20);

                if(!p.enabledPaint) EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(p.spawnPreview)));
            }
            else
            {
                EditorGUILayout.HelpBox("in this mode, make sure to not leave the scene window while holding mouse 0 button, and then realising. system doesnt register relises when you are not in scene window", MessageType.Warning);

                GUILayout.Space(10);

                if(p.paintMode == Painter.paintM.replace)
                {
                    drawPalette(p);

                    GUILayout.Space(10);

                    drawGridSettings(p);

                    GUILayout.Space(10);
                }


                GUILayout.Space(10);

                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(p.massReplaceDestroy)));

                if (p.massReplaceDestroy)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(p.brushSize)));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(p.findCount)));
                }

            }

            serializedObject.ApplyModifiedProperties();

            GUILayout.Space(20);

            EditorGUI.BeginChangeCheck();

            if (p.enabledPaint)
            {
                if (GUILayout.Button("--- Disable ---")) p.Disable();
            }
            else if (GUILayout.Button("--- Enable ---")) p.Enable();

            GUILayout.Space(10);

            GUILayout.Space(20);


            if (GUILayout.Button("Revert (delete children)")) p.revert();

            if (EditorGUI.EndChangeCheck()) EditorUtility.SetDirty(p);

            GUI.color = old;
        }

        private void drawPalette(Painter p)
        {
            if (GUILayout.Button("open palette selector")) 
                prefabPalleteSelectorEditorWindow.showWindow(p);
            
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(p.palette)));

            if(p.palette == null)
            {
                EditorGUILayout.HelpBox("pallete not found", MessageType.Error);
                return;
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(p.specificIndex)));
            if (p.specificIndex >= p.palette.objects.Length) p.specificIndex = p.palette.objects.Length - 1;        
        }

        private void drawGridSettings(Painter p)
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(p.placeGridSize)));
            if (p.placeGridSize == 0) GUILayout.Label("disabled");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(p.heightSnap)));
            if (!p.usingHeightSnap()) GUILayout.Label("disabled");
            GUILayout.EndHorizontal();
            if(p.usingHeightSnap())
            {
                GUILayout.Label("type 'Inf' or 'Infinity' in this field to disable");
                EditorGUILayout.HelpBox("to change height snap while placing, just use scroll wheel", MessageType.Info);
                EditorGUILayout.HelpBox("with height snapping, normals will not matter anymore and objects will be placed as if normal would be v3.up", MessageType.Info);
            }
        }

        private void OnSceneGUI()
        {
            Painter p = (Painter)target;


            if(p.enabledPaint)
            {

                Color c;
                if (p.paintReturned) c = Color.red;
                else
                {
                    c = p.toBigNormal ?
                    Color.red 
                    :
                    Color.green;

                    c.a = 0.5f;
                }

                Handles.color = c;


                float range = 1;

                switch(p.paintMode)
                {
                    case Painter.paintM.scatter:
                        range = p.brushSize;
                        break;
                    case Painter.paintM.exact:
                        range = 1;
                        break;
                    case Painter.paintM.remove:
                        range = p.massReplaceDestroy ? p.brushSize : 1;
                        break;
                    case Painter.paintM.replace:
                        range = p.massReplaceDestroy ? p.brushSize : 1;
                        break;
                }

                Vector3 normal = Vector3.Lerp(Vector3.up, p.paintNormal, p.palette.normalAllighn);
                Handles.DrawSolidDisc(p.paintPos, normal, range);

                float randY = p.palette.randRotAdd.y;

                if(p.isPlacingMode() && p.usingHeightSnap())
                {
                    if(Physics.Raycast(p.paintPos, Vector3.down, out RaycastHit info, float.PositiveInfinity, p.palette.groundLayer))
                    {
                        c = Color.red;
                        //c.a = 0.5f;
                        Handles.color = c;
                        Handles.DrawLine(p.paintPos, info.point);
                        Handles.DrawWireDisc(info.point, Vector3.up, 0.5f);

                        Vector3 textMove = Vector3.ClampMagnitude((info.point - p.paintPos) / 2, 2);

                        Handles.Label(p.paintPos + textMove, new GUIContent(Vector3.Distance(p.paintPos, info.point).ToString("0.##"), "distance over ground"), GUI.skin.textArea);
                    }
                }

                if (randY < 180)
                {
                    c = Color.blue;
                    c.a = 0.5f;
                    Handles.color = c;
                    Quaternion rotOf = Quaternion.AngleAxis(p.YRotationOffset, Vector3.up);
                    Handles.DrawSolidArc(p.paintPos, normal, rotOf * Quaternion.AngleAxis(randY, Vector3.up) * Vector3.forward, Mathf.Clamp(randY * 2, 5, 360), 2);

                    if (randY <= 45)
                    {
                        c = Color.red;
                        c.a = 0.5f;
                        Handles.color = c;

                        Handles.DrawSolidArc(p.paintPos, normal, rotOf * Quaternion.AngleAxis(randY + 90, Vector3.up) * Vector3.forward, Mathf.Clamp(randY * 2, 5, 360), 2);
                    }
                }
            }
        }
    }
}
