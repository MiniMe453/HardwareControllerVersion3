using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

using AlekGames.Placer.Shared;
using AlekGames.Placer.Profiles;


namespace AlekGames.Placer.Systems.Main
{
    public class NoiseSpawner : MonoBehaviour, ISpawner, IPrefabPalleteUser
    {

        [SerializeField] 
        private bool spawnOnAwakeWithRandSeed = true;
        [Header("Spawn")]
        [SerializeField]
        prefabPalette palette;

        [Header("spawn rules")]
        [SerializeField, Min(0), Tooltip("min noise value to attempt spawning an object")] 
        private float noiseSpawnThreshold = 0.7f;
        [SerializeField, Range(0,100), Tooltip("chance of spawning an object in a point")] 
        private float spawnChance = 100;
        [SerializeField, Min(0), Tooltip("min distance diffrienc between 2 spawned objects. 0 to disable. i advise to just use placePosScale, if in playmode, couse it will be faster")] 
        private float avoidienceDistance = 5;
        [SerializeField, Tooltip("if should add the picked po to avoided pos list even if chance failed")]
        private bool addPosIfChanceFail = true;

        [Header("placing")]
        [SerializeField, Tooltip("position scale (makes stuff spawn further)")] 
        private float placePosScale = 1;
        [SerializeField] 
        private float randPosOffset = 3;

        [SerializeField] 
        private bool snapToGround;

        [SerializeField] 
        private string callOnSpawned;
        [SerializeField] 
        private bool autoCallSpawners = true;
        [SerializeField, Min(1)] 
        private int atOnceSpawners = 5;

        [SerializeField, Min(1)] private int perLoopWait = 3;

        [Space]

        [SerializeField]
        private int specificIndex = -1;

        [Space]

        [SerializeField]
        private PerlinNoise.noiseSettings noiseSettings;


        // Start is called before the first frame update
        async void Start()
        {
            if (!spawnOnAwakeWithRandSeed) return;
            noiseSettings.seed = Random.Range(int.MinValue, int.MaxValue);
            await spawn();
            Debug.Log("spawned");
        }

        public async Task spawn()
        {

            List<Vector3> posCount = new List<Vector3>();

            float[,] noise = PerlinNoise.get2DNoise(noiseSettings);

            int curSpawners = 0;
            int curLoop = 0;

            float sqrAvoidiance = avoidienceDistance * avoidienceDistance;

            for (int x = 0; x < noise.GetLength(0); x++)
            {
                for (int y = 0; y < noise.GetLength(1); y++)
                {

                    if (noise[x, y] < noiseSpawnThreshold) continue;

                    Vector3 pos = transform.position + new Vector3(x * placePosScale, 0, y * placePosScale);

                    if (!Chance.giveChance(spawnChance))
                    {
                        if (addPosIfChanceFail) posCount.Add(pos);

                        continue;
                    }


                    if (avoidienceDistance > 0)
                    {
                        bool tooClose = false;
                        foreach (Vector3 p in posCount)
                        {
                            if ((p - pos).sqrMagnitude <= sqrAvoidiance)
                            {
                                tooClose = true;
                                break;
                            }
                        }

                        if (tooClose) continue;
                    }

                    Vector3 up = Vector3.up;

                    if (snapToGround)
                    {
                        Ray r = new Ray(pos + new Vector3(Random.Range(-randPosOffset, randPosOffset), 0, Random.Range(-randPosOffset, randPosOffset)), Vector3.down);
                        if (Physics.Raycast(r, out RaycastHit info, 300, palette.groundLayer, QueryTriggerInteraction.Ignore))
                        {
                            if (Physics.Raycast(r, Vector3.Distance(r.origin, info.point), palette.avoidedLayer)) continue;

                            bool angleCorrect = false;
                            if (palette.maxNormal != 180)
                            {
                                if (Vector3.Angle(Vector3.up, info.normal) <= palette.maxNormal) angleCorrect = true;
                            }
                            else angleCorrect = true;

                            if (!angleCorrect) continue;

                            pos = info.point;
                            up = info.normal;
                        }
                        else
                        {
                            continue;
                        }
                    }

                    GameObject spawned = palette.spawn(pos, up, transform, 0, specificIndex);


                    if (callOnSpawned != string.Empty) spawned.SendMessage(callOnSpawned, SendMessageOptions.DontRequireReceiver);

                    if (autoCallSpawners)
                    {
                        Task t = spawned.GetComponent<ISpawner>().spawn();
                        curSpawners++;

                        if (curSpawners >= atOnceSpawners)
                        {
                            await t;
                            curSpawners = 0;
                        }
                    }

                    if (avoidienceDistance > 0) posCount.Add(new Vector3(pos.x, transform.position.y, pos.z)); // pos changed

#if UNITY_EDITOR
                    UnityEditor.EditorUtility.SetDirty(spawned);
#endif

                }

                curLoop++;
                if (curLoop >= perLoopWait)
                {
                    await Task.Yield();
                    curLoop = 0;
                }
            }

        }

        public void setSeed(int seed) => noiseSettings.seed = seed;

        public void revert()
        {
            while(transform.childCount > 0)
            { 
                DestroyImmediate(transform.GetChild(0).gameObject);
            }
        }

        public void setPalette(prefabPalette palette)
        {
            this.palette = palette;
        }
        public prefabPalette getPalette()
        {
            return palette;
        }

        public void setSpecificIndex(int index) => specificIndex = index;
        public int getSpecificIndex()
        {
            return specificIndex;
        }

#if UNITY_EDITOR
        [HideInInspector]
        public bool drawGizmos;

        [HideInInspector]
        public float gizmoDistance = 60;

        private void OnDrawGizmosSelected()
        {
            if (!drawGizmos) return;

            float[,] noise = PerlinNoise.get2DNoise(noiseSettings);

            float sqrRenderDistance = gizmoDistance * gizmoDistance;
            Vector3 curCamPos = UnityEditor.SceneView.lastActiveSceneView.camera.transform.position;

            for (int x = 0; x < noise.GetLength(0); x++)
                for (int y = 0; y < noise.GetLength(1); y++)
                {
                    float value = noise[x, y];


                    Vector3 pos = transform.position + new Vector3( x * placePosScale, 0, y * placePosScale);


                    if ((pos - curCamPos).sqrMagnitude <= sqrRenderDistance)
                    {

                        Color c = value >= noiseSpawnThreshold ? Color.red : Color.cyan;
                        c.a = value;
                        Gizmos.color = c;
                        Gizmos.DrawCube(pos, new Vector3(placePosScale, 0.1f, placePosScale));
                    }
                }

        }

#endif
    }
}
