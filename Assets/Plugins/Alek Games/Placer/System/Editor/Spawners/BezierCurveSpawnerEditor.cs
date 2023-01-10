using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEditor;

using AlekGames.Placer.Systems.Main;
using AlekGames.Placer.Shared;


namespace AlekGames.Editor
{
    [CustomEditor(typeof(BezierCurveSpawner))]
    public class QuadraticSplineSpawnerEditor : UnityEditor.Editor
    {

        [MenuItem("Tools/Alek Games/Placer/Add Bezier Curve Spawner")]
        public static void add()
        {
            GameObject bc = new GameObject("Bezier Curve Spawner Tool");
            bc.AddComponent<BezierCurveSpawner>();

            EditorUtility.SetDirty(bc);
            Undo.RegisterCreatedObjectUndo(bc, "added Bezier Curve Spawner Tool");
        }


        private bool enabledPaint;
        private Vector3 paintPos;
        private int? insertIndex = null;
        private BezierCurveSpawner bcs;

        public override void OnInspectorGUI()
        {
            if (enabledPaint) GUI.color = Color.red;
            BezierCurveSpawner bcs = (BezierCurveSpawner)target;

            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(bcs.spawnType)));

            if (bcs.spawnType == BezierCurveSpawner.spawnT.Wall)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(bcs.wallPreset)));
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(bcs.allowOverPoint)));
            }
            else
            {
                if (GUILayout.Button("open palette selector"))
                    prefabPalleteSelectorEditorWindow.showWindow(bcs);
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(bcs.palette)));
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(bcs.specificIndex)));

            }

            GUILayout.Space(10);

            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(bcs.minSpawnDistance)));

            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(bcs.snapToGround)));

            GUILayout.Space(10);

            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(bcs.curveMode)));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(bcs.curveTangentMode)));
            if(bcs.curveTangentMode == BezierCurveSpawner.Tmode.halfManual)
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(bcs.otherTangentMode)));

            GUILayout.Space(10);

            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(bcs.stepMode)));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(bcs.curveSteps)));

            GUILayout.Space(20);

            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(bcs.curve)));

            serializedObject.ApplyModifiedProperties();

            GUILayout.Space(30);

            if(enabledPaint)
            {
                if (GUILayout.Button("stop painting points"))
                {
                    DisableP();
                }

                EditorGUILayout.HelpBox("by ctrl + click you insert a point between 2 points (look at handle (something like gizmos) preview)", MessageType.Info);
                EditorGUILayout.HelpBox("click to spawn points", MessageType.Info);
            }
            else if (GUILayout.Button("paint points"))
            {
                this.bcs = bcs;
                EnableP();
            }

            GUILayout.Space(10);

            if (bcs.curveTangentMode == BezierCurveSpawner.Tmode.manual || bcs.curveTangentMode == BezierCurveSpawner.Tmode.halfManual)
            {
                if (GUILayout.Button("auto Smooth"))
                {
                    bezierCurve.autoSmooth(bcs.curve);
                    GUI.changed = true;
                    EditorUtility.SetDirty(bcs);
                }
                GUILayout.Space(10);
            }




            if (GUILayout.Button("snap curve to ground"))
            {
                bcs.snapCurveToGround();
                GUI.changed = true;
            }

            GUILayout.Space(10);

            if (GUILayout.Button("spawn")) bcs.spawn();

            GUILayout.Space(20);

            if (GUILayout.Button("revert (deleate children)")) bcs.revert();
        }

        public void EnableP()
        {
            if (enabledPaint)
            {
                Debug.Log("already enabled");
                return;
            }

            if (!Application.isEditor)
            {
                Destroy(this);
                return;
            }

            Debug.Log("add paint feature to scene");

            enabledPaint = true;
            SceneView.beforeSceneGui += beforeScene;
        }
        public void DisableP()
        {
            if (!enabledPaint)
            {
                Debug.Log("already disabled");
                return;
            }

            if (!Application.isEditor)
            {
                Destroy(this);
                return;
            }

            Debug.Log("remove paint feature from scene");

            enabledPaint = false;
            SceneView.beforeSceneGui -= beforeScene;
        }



        private void beforeScene(SceneView scene)
        {
            if (!Selection.gameObjects.Contains(bcs.gameObject) || !Application.isEditor)
            {
                DisableP();
                return;
            }

            Event e = Event.current;

            if (e.isMouse)
            {
                Vector3 mousePos = e.mousePosition;
                float ppp = EditorGUIUtility.pixelsPerPoint;
                mousePos.y = scene.camera.pixelHeight - mousePos.y * ppp;
                mousePos.x *= ppp;

                if (Physics.Raycast(scene.camera.ScreenPointToRay(mousePos), out RaycastHit info))
                {
                    paintPos = info.point;
                    if (e.control)
                    {
                        List<bezierCurve.anchorSettings> a = bcs.curve.ToList();
                        bezierCurve.anchorSettings[] twoClosest = a.OrderBy(t => Vector3.Distance(t.anchor, paintPos)).Take(2).ToArray();

                        int index0 = a.IndexOf(twoClosest[0]);
                        int index1 = a.IndexOf(twoClosest[1]);
                        insertIndex = index0 > index1 ? index0 : index1;

                    }
                    else
                    {
                        if (insertIndex != null) GUI.changed = true;
                        insertIndex = null;
                    }


                    if (e.button == 0 && e.type == EventType.MouseDown)
                    {
                        e.Use();

                        Undo.RegisterCompleteObjectUndo(bcs, "added curve point with paint feature");

                        List<bezierCurve.anchorSettings> a = bcs.curve.ToList();
                        if(insertIndex != null)
                        {
                            a.Insert(
                                insertIndex.Value, //+ toInsertIndex == bcs.curve.Length -1? 0 : 1,
                                new bezierCurve.anchorSettings(paintPos)
                                );
                        }
                        else a.Add(new bezierCurve.anchorSettings(paintPos));
                        bcs.curve = a.ToArray();

                        EditorUtility.SetDirty(bcs);
                    }


                    SceneView.RepaintAll();
                }

            }
        }



        private void OnSceneGUI()
        {
            BezierCurveSpawner bcs = (BezierCurveSpawner)target;

            if(enabledPaint)
            {
                Color c = Color.green;
                c.a = 0.75f;
                Handles.color = c;
                Handles.DrawSolidDisc(paintPos, Vector3.up, 1);
            }

            if (bcs.curve.Length < 3) return;

            if (insertIndex != null && insertIndex.Value < bcs.curve.Length)
            {
                Handles.DrawLine(bcs.curve[insertIndex.Value].anchor, paintPos, 5);
                Handles.DrawLine(bcs.curve[insertIndex.Value - 1].anchor, paintPos, 5);
            }


            for (int i = 0; i < bcs.curve.Length; i++)
            {
                if(i < bcs.curve.Length - 1) // preview
                {
                    bezierCurve.anchorSettings a1 = bcs.curve[i];
                    bezierCurve.anchorSettings a2 = bcs.curve[i + 1];

                    switch(bcs.curveMode)
                    {
                        case bezierCurve.curveMode.cubic:
                            Handles.DrawBezier(a1.anchor, a2.anchor, a1.anchor + a1.outT, a2.anchor + a2.inT, Color.cyan, null, 4f);

                            break;
                        case bezierCurve.curveMode.quadratic:
                            Handles.color = Color.cyan;
                            int precision = (int)(Vector3.Distance(a1.anchor, a2.anchor) / 8) + 1;
                            Vector3 last = a1.anchor;
                            for (int bp = 0; bp < precision; bp++)
                            {
                                Vector3 next = bezierCurve.getOn2AnchorsPos(a1, a2, (float)(bp + 1) / (float)precision, bezierCurve.curveMode.quadratic);
                                Handles.DrawLine(last, next, 4f);
                                last = next;
                            }
                            break;
                        case bezierCurve.curveMode.linear:
                            Handles.color = Color.cyan;
                            Handles.DrawLine(a1.anchor, a2.anchor, 4f);
                            break;
                    }
                }


                Vector3 after = Handles.PositionHandle(bcs.curve[i].anchor, Quaternion.identity);

                if (bcs.curve[i].anchor != after)
                {
                    Undo.RegisterCompleteObjectUndo(bcs, "changed anchor points of bezier curve spawner");
                    bcs.curve[i].anchor = after;
                    EditorUtility.SetDirty(bcs);
                }


                bool halfMan = bcs.curveTangentMode == BezierCurveSpawner.Tmode.halfManual;

                if (halfMan || bcs.curveTangentMode == BezierCurveSpawner.Tmode.manual)
                {
                    if (i != 0)
                    {
                        Vector3 newInT = Handles.PositionHandle(bcs.curve[i].anchor + bcs.curve[i].inT, Quaternion.identity) - bcs.curve[i].anchor;

                        if (bcs.curve[i].inT != newInT)
                        {
                            Undo.RegisterCompleteObjectUndo(bcs, "changed anchor in tangent points of bezier curve spawner");

                            if (halfMan)
                            {
                                switch (bcs.otherTangentMode)
                                {
                                    case BezierCurveSpawner.halfManulalOtherTMode.none:
                                        bcs.curve[i].outT = -(newInT).normalized * bcs.curve[i].outT.magnitude;
                                        break;
                                    case BezierCurveSpawner.halfManulalOtherTMode.keepRatio:
                                        float ratio = (newInT.magnitude / bcs.curve[i].inT.magnitude);
                                        bcs.curve[i].outT = -(newInT).normalized * bcs.curve[i].outT.magnitude * ratio;

                                        break;
                                    case BezierCurveSpawner.halfManulalOtherTMode.Copy:

                                        bcs.curve[i].outT = -newInT;
                                        break;
                                }
                            }

                            bcs.curve[i].inT = newInT;

                            EditorUtility.SetDirty(bcs);
                        }
                    }

                    if (i != bcs.curve.Length - 1)
                    {
                        Vector3 newOutT = Handles.PositionHandle(bcs.curve[i].anchor + bcs.curve[i].outT, Quaternion.identity) - bcs.curve[i].anchor;

                        if (bcs.curve[i].outT != newOutT)
                        {

                            Undo.RegisterCompleteObjectUndo(bcs, "changed anchor out tangent points of bezier curve spawner");

                            if (halfMan)
                            {
                                switch(bcs.otherTangentMode)
                                {
                                    case BezierCurveSpawner.halfManulalOtherTMode.none:
                                        bcs.curve[i].inT = -(newOutT).normalized * bcs.curve[i].inT.magnitude;
                                        break;
                                    case BezierCurveSpawner.halfManulalOtherTMode.keepRatio:
                                        float ratio = (newOutT.magnitude / bcs.curve[i].outT.magnitude);
                                        bcs.curve[i].inT = -(newOutT).normalized * bcs.curve[i].inT.magnitude * ratio;

                                        break;
                                    case BezierCurveSpawner.halfManulalOtherTMode.Copy:

                                    bcs.curve[i].inT = -newOutT;
                                        break;
                                }
                            }

                            bcs.curve[i].outT = newOutT;

                            EditorUtility.SetDirty(bcs);
                        }
                    }
                }



                Handles.Label(bcs.curve[i].anchor + bcs.curve[i].inT, i + " IN");
                Handles.Label(bcs.curve[i].anchor + bcs.curve[i].outT, i + " OUT");
            }

            if (bcs.curveTangentMode == BezierCurveSpawner.Tmode.automatic)
            {
                bezierCurve.autoSmooth(bcs.curve);
            }
        }
    }
}
