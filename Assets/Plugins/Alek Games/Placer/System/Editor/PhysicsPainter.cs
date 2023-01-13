using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

using AlekGames.Placer.Profiles;
using AlekGames.Placer.Shared;

#if UNITY_EDITOR

using UnityEditor;

namespace AlekGames.Placer.Systems.Main
{
    public class PhysicsPainter : MonoBehaviour, IPrefabPalleteUser
    {

        public bool recordUndo = true;

        [Space]

        public prefabPalette palette;

        public int specificIndex = -1;

        [Space]

        public bool holdActivation = true;

        public float holdActivationDistance = 2;

        [Space]

        [Min(0), Tooltip("up offset of spawn to the paint position")]
        public float upOffset = 5;

        [Range(0, 150)]
        public float brushSize = 5;

        [Range(0, 200)]
        public int scatterCount = 8;

        [Range(0, 100)]
        public float scatterAvoidenceDistance = 1;

        [Space]

        [Tooltip("if should keep simulated objects in scene, even if the rigidbody is sleeping (not moveing). if false, if object is not moveing, the system will remove it from scene")]
        public bool keepInScene = false;

        [Tooltip("how ofte should simulation update realtime. this value is in miliseconds"), Min(1)]
        public int simulationDelta = 2;

        [Tooltip("amount of iterations per physics refresh"), Min(1)]
        public int iterations = 8;

        [Tooltip("step for Physics.Simulate per iteration."), Min(0.001f)]
        public float iterationStep = 0.02f;

        public CollisionDetectionMode simulationCollisionsDetection = CollisionDetectionMode.Discrete;

        public bool enabledPaint { get; private set; }
        public Vector3 paintPos { get; private set; }

        private bool isPlacing;

        private Vector3? lastSpawn = null;
        private List<objInfo> simulatedRb = new List<objInfo>();

        private Scene simulationScene;


        [ContextMenu("enable")]
        public void Enable()
        {
            if (enabledPaint)
            {
                Debug.LogError("already enabled");
                return;
            }

            if (!Application.isEditor)
            {
                Destroy(this);
                return;
            }

            Debug.Log("added simulation scene");

            isPlacing = false;
            enabledPaint = true;
            SceneView.beforeSceneGui += beforeScene;

            Debug.Log("added paint feature to scene");

            AddSimulationScene();

            simulationUpdater();

            Debug.Log("initiated simulation");
        }

        [ContextMenu("add simulation Scene")]
        private void AddSimulationScene()
        {
            if(simulationScene != null && simulationScene.IsValid())
            {
                Debug.Log("simulation scene is already valid");
                return;
            }

            simulationScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            simulationScene.name = "[don't touch] Placer Physics Simulation Scene";
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

            enabledPaint = false;
            SceneView.beforeSceneGui -= beforeScene;

            for (int i = 0; i < simulatedRb.Count; i++) 
                addToMainScene(simulatedRb[i]);
            

            simulatedRb.Clear();
            EditorSceneManager.UnloadSceneAsync(simulationScene);
        }

        public void revert()
        {
            while (transform.childCount > 0)
            {
                DestroyImmediate(transform.GetChild(0).gameObject);
            }
        }

        void beforeScene(SceneView scene)
        {
            if (!Selection.gameObjects.Contains(gameObject) || !Application.isEditor)
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

                Ray ray = scene.camera.ScreenPointToRay(mousePos);
                bool rayHit = Physics.Raycast(ray, out RaycastHit info);

                if(rayHit) paintPos = info.point;


                if (e.button == 0)
                {

                    if (e.type == EventType.MouseUp)
                    {
                        lastSpawn = null;
                        isPlacing = false;
                        e.Use();
                    }
                    else if ((e.type == EventType.MouseDown) || (isPlacing && holdActivation))
                    {
                        isPlacing = true;
                        e.Use();

                        if (lastSpawn != null && Vector3.Distance(lastSpawn.Value, paintPos) < holdActivationDistance) return;

                        scatter();

                        if (holdActivation)
                            lastSpawn = paintPos;

                    }
                }



                GUI.changed = true;
            }


        }


        private void scatter()
        {
            Vector3 rayCenter = paintPos + Vector3.up * 3;

            List<Vector3> spawnedPos = new List<Vector3>();

            float sqrAvoidiance = scatterAvoidenceDistance * scatterAvoidenceDistance;

            for (int i = 0; i < scatterCount; i++)
            {
                Vector2 move = Random.insideUnitCircle * brushSize;

                Vector3 offset = new Vector3(move.x, 0, move.y);

                Vector3 nCenter = rayCenter + offset;

                Ray ray = new Ray(rayCenter + offset, Vector3.down);

                if (Physics.Raycast(ray, out RaycastHit info))
                {
                    bool tooCloase = false;

                    foreach (Vector3 p in spawnedPos)
                    {
                        if ((p - nCenter).sqrMagnitude < sqrAvoidiance)
                        {
                            tooCloase = true;
                            break;
                        }
                    }

                    if (tooCloase) continue;

                    spawnObj(info.point + Vector3.up * upOffset, info.normal);
                    spawnedPos.Add(nCenter);

                }
            }
        }


        private void spawnObj(Vector3 pos, Vector3 surfaceNormal)
        {
            GameObject spawned = palette.spawn(pos, surfaceNormal, transform, 0, specificIndex);

            bool hadRb = false;

            Rigidbody rb = spawned.GetComponent<Rigidbody>();
            if (rb == null) rb = spawned.AddComponent<Rigidbody>();
            else hadRb = true;

            simulatedRb.Add(new objInfo(rb, hadRb, rb.collisionDetectionMode));
            rb.collisionDetectionMode = simulationCollisionsDetection;
            addToPhysicsScene(spawned);

            if (recordUndo) Undo.RegisterCreatedObjectUndo(spawned, "painted object");
            EditorUtility.SetDirty(spawned);
        }


        private void addToPhysicsScene(GameObject obj)
        {
            obj.transform.parent = null;
            EditorSceneManager.MoveGameObjectToScene(obj, simulationScene);
        }

            private void addToMainScene(objInfo info)
        {
            GameObject obj = info.rb.gameObject;
            EditorSceneManager.MoveGameObjectToScene(obj, EditorSceneManager.GetSceneAt(0));
            obj.transform.parent = transform;

            if (!info.maintairRb) DestroyImmediate(info.rb);
            else info.rb.collisionDetectionMode = info.collisionMode;


            EditorUtility.SetDirty(obj);
        }

        private async void simulationUpdater()
        {
            await Task.Delay(1);

            while (true)
            {
                if (!enabledPaint) break;

                if (simulatedRb.Count > 0)
                {
                    Physics.autoSimulation = false;
                    for(int i = 0; i < iterations; i++)
                        simulationScene.GetPhysicsScene().Simulate(iterationStep);
                    Physics.autoSimulation = true;


                    for (int i = simulatedRb.Count - 1; i >= 0; i--)
                    {
                        if (simulatedRb[i].rb == null)
                        {
                            simulatedRb.RemoveAt(i);
                            continue;
                        }

                        if (!keepInScene)
                        {
                            if (simulatedRb[i].rb.IsSleeping())
                            {
                                addToMainScene(simulatedRb[i]);
                                simulatedRb.RemoveAt(i);
                            }
                        }
                    }
                }

                await Task.Delay(simulationDelta);
            }
        }

        #region palette user

        public void setPalette(prefabPalette palette)
        {
            this.palette = palette;
            specificIndex = Mathf.Clamp(specificIndex, -1, palette.objects.Length - 1);
        }
        public prefabPalette getPalette()
        {
            return palette;
        }

        public void setSpecificIndex(int index)
        {
            specificIndex = index;
        }
        public int getSpecificIndex()
        {
            return specificIndex;
        }

        #endregion

        public struct objInfo
        {
            public Rigidbody rb;
            public bool maintairRb;
            public CollisionDetectionMode collisionMode;

            public objInfo(Rigidbody rb, bool maintairRb, CollisionDetectionMode collisionMode)
            {
                this.rb = rb;
                this.maintairRb = maintairRb;
                this.collisionMode = collisionMode;
            }
        }
    }
}

#endif
