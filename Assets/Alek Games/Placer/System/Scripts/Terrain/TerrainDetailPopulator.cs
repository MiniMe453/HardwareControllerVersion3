
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using AlekGames.Placer.Shared;

namespace AlekGames.Placer.Systems.Main
{
    public class TerrainDetailPopulator : MonoBehaviour
    {
        [Tooltip("index of a detail on details of terrain")]
        public int detailILayer = 0;

        [Space]

        [Range(0, 10), Tooltip("density of detail on a checked spot if decided to place detail there")]
        public int density = 2;
        [Range(0, 100), Tooltip("chance for a bold spot")]
        public float zeroChance = 10;
        [Range(0, 90), Tooltip("max normal angle of ground for detai to be placed")]
        public float maxNormal = 30;

        [Space]

        public Vector2 minmaxHeight = new Vector2(0, 100);
        [Tooltip("obstacle layer. grass will not be spawned on spot covered by obstacles")]
        public LayerMask obstacles;

        public avoidedTerrainLayer[] avoidedTerrainLayers;

        [Space]

        [Tooltip("for performace, await per outher loop repetes")]
        public int perYwait = 100;



        [ContextMenu("populate")]
        public async void populate()
        {

            Terrain terrain = GetComponent<Terrain>();

            if (terrain == null)
            {
                Debug.LogError("no terrain found");
                return;
            }

            TerrainData terrainData = terrain.terrainData;

            int width = terrainData.detailWidth;
            int height = terrainData.detailHeight;
            int[,] details = new int[width, height];

            int yrep = 0;

            Vector2 size = new Vector2(terrainData.size.x, terrainData.size.z);

            float widthToPos = size.x / width;
            float heightToPos = size.y / height;

            Vector3 terrainPos = terrain.transform.position;

            for (int y = 0; y < height; y++)
            {
                if (yrep >= perYwait)
                {
                    await Task.Delay(1);
                    yrep = 0;
                }

                for (int x = 0; x < width; x++)
                {
                    int chosen = 0;

                    if (!Chance.giveChance(zeroChance))
                    {
                        Vector3 pos = transform.position + new Vector3((y * widthToPos), minmaxHeight.y, (x * heightToPos));

                        bool alfaMapCorrect = true;

                        if (avoidedTerrainLayers.Length > 0)
                        {
                            Vector2Int alphaPos = ConvertPositionToAlphamap(terrainData, terrainPos, pos);

                            foreach (avoidedTerrainLayer atl in avoidedTerrainLayers)
                            {
                                float[,,] aMap = terrainData.GetAlphamaps(alphaPos.x, alphaPos.y, 1, 1);
                                if (aMap[0, 0, atl.layerIndex] > atl.avoidedWeightOver) 
                                {
                                    alfaMapCorrect = false;
                                }
                            }
                        }

                        if (alfaMapCorrect)
                        {

                            Physics.Raycast(pos, Vector3.down, out RaycastHit info, Mathf.Abs(minmaxHeight.y - minmaxHeight.x), 1 << gameObject.layer, QueryTriggerInteraction.Ignore);

                            if (info.point.y > minmaxHeight.x && info.point.y < minmaxHeight.y)
                            {
                                if (!Physics.Raycast(pos, Vector3.down, Vector3.Distance(pos, info.point) - 0.001f, obstacles))
                                {
                                    if (Vector3.Angle(info.normal, Vector3.up) <= maxNormal) chosen = density;
                                }
                            }
                        }

                    }

                    details[x, y] = chosen;
                }
            }

            terrainData.SetDetailLayer(0, 0, detailILayer, details);
        }


        private Vector2Int ConvertPositionToAlphamap(TerrainData data, Vector3 terrainPos, Vector3 checkedPos)
        {
            Vector3 terrainPosition = checkedPos - terrainPos; ;
            Vector3 mapPosition = new Vector3
            (terrainPosition.x / data.size.x, 0,
            terrainPosition.z / data.size.z);
            float xCoord = mapPosition.x * data.alphamapWidth;
            float zCoord = mapPosition.z * data.alphamapHeight;
            return new Vector2Int( (int)xCoord, (int)zCoord);
        }


        [ContextMenu("clear")]
        public void clearTerrainLayerDetail()
        {
            Terrain terrain = GetComponent<Terrain>();

            if (terrain == null)
            {
                Debug.LogError("no terrain found");
                return;
            }

            TerrainData terrainData = terrain.terrainData;

            int width = terrainData.detailWidth;
            int height = terrainData.detailHeight;
            int[,] details = new int[width, height];
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    details[x, y] = 0;

            terrainData.SetDetailLayer(0, 0, detailILayer, details);
        }

        [System.Serializable]
        public struct avoidedTerrainLayer
        {
            public int layerIndex;

            public float avoidedWeightOver;
        }


#if UNITY_EDITOR

        [SerializeField]
        private bool drawGizmos = true;

        private void OnDrawGizmosSelected()
        {
            if (!drawGizmos) return;

            Gizmos.color = Color.red;

            Vector3 one = transform.position + Vector3.up * minmaxHeight.x;
            Vector3 two = transform.position + Vector3.up * minmaxHeight.y;
            Gizmos.DrawLine(one, two);
            Gizmos.DrawSphere(one, 0.6f);
            Gizmos.DrawSphere(two, 0.6f);
            Color c = Color.red;

            Terrain terrain = GetComponent<Terrain>();

            if (terrain == null)
            {
                Debug.LogError("no terrain found");
                return;
            }

            TerrainData terrainData = terrain.terrainData;
            c.a = 0.5f;
            Gizmos.color = c;
            Vector3 terSize = new Vector3(terrainData.size.x, 0, terrainData.size.z);
            Gizmos.DrawCube(one + terSize / 2, terSize);
            Gizmos.DrawCube(two + terSize / 2, terSize);
        }

#endif
    }
}
