using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using AlekGames.Placer.Profiles;
using AlekGames.Placer.Shared;

#if UNITY_EDITOR
using UnityEditor;

namespace AlekGames.Placer.Systems.Main
{
    public class wallPainter : MonoBehaviour
    {
        public wallSettings wallPreset;

        public bool AllowOverPaint = false;

        public bool recordUndo = true;


        public Vector3 paintPos { get; private set; }
        public Vector3 paintNormal { get; private set; }

        public bool enabledPaint { get; private set; }

        private Vector3? lastPos;

        private List<Vector3> undoPosStack = new List<Vector3>();


        [ContextMenu("enable")]
        public void Enable()
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

        [ContextMenu("disable")]
        public void Disable()
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

            lastPos = null;
            undoPosStack.Clear();

            enabledPaint = false;
            SceneView.beforeSceneGui -= beforeScene;
        }

        public void revert()
        {
            while (transform.childCount > 0)
            {
                DestroyImmediate(transform.GetChild(0).gameObject);
            }

            lastPos = null;
            undoPosStack.Clear();
        }

        public void undo()
        {
            Undo.PerformUndo();
            lastPos = undoPosStack[undoPosStack.Count - 1];
            undoPosStack.RemoveAt(undoPosStack.Count - 1);
        }

        private void beforeScene(SceneView scene)
        {
            if(!Selection.gameObjects.Contains(gameObject) || !Application.isEditor)
            {
                Disable();
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
                    paintNormal = info.normal;

                    if (e.button == 0)
                    {
                        if (e.type == EventType.MouseDown)
                        {
                            e.Use();

                            if (lastPos == null)
                            {
                                Vector3 forward = scene.camera.transform.right;
                                forward.y = 0;
                                wallPreset.spawn(wallSettings.toSpawnType.post, paintPos, Quaternion.LookRotation(forward, Vector3.up), transform);
                                lastPos = paintPos;
                            }
                            else
                            {

                                Vector3? pos = wallPreset.spawnWall(lastPos.Value, paintPos, AllowOverPaint, transform);

                                if (pos != null)
                                {
                                    undoPosStack.Add(lastPos.Value);
                                    lastPos = pos;
                                }
                            }

                            
                        }
                        else if (e.type == EventType.MouseUp)
                        {
                            e.Use();
                        }

                    }


                    SceneView.RepaintAll();
                }

            }
        }
    }
}
#endif
