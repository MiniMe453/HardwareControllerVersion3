using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Threading.Tasks;

using AlekGames.Placer.Shared;
using AlekGames.Placer.Profiles;

namespace AlekGames.Placer.Systems.Main
{
    public class stepGenerator : MonoBehaviour, ISpawner
    {
        public stepSettings stepSettingsSO;
        [Min(1), Tooltip("sometimes the system doesnt manage to spawn even the min count. in that case you can try increasing placePoints in step settings")]
        public Vector2Int minMaxCount = new Vector2Int(3, 7);

        [Tooltip("times to repete spawner")]
        public int iterations = 1;

        [Range(0,100), Tooltip("spawn chance of hole step")]
        public float spawnChance = 100;

        [Tooltip("range of placement. some settings in stepSettingsSO may affect this")]
        public Vector2 randPlacementRange = new Vector2(10, 1);

        public Vector2 minMaxStepSpawnDistance = new Vector2(1, 3);
        public bool maxStepSpawnDistanceToAll = false;

        [Tooltip("height offset of spawn positions(if not on suface)/detectors(if on surface using rays)")]
        public float heightOffset = 3;

        public bool midIterAwait = true;

        public bool midSpawnAwait = true;

        

        public async Task spawn()
        {
            await Spawn();
        }

        [ContextMenu("spawn")]
        public async Task<GameObject[]> Spawn()
        {

            if(stepSettingsSO == null)
            {
                Debug.LogError("null profile is step generator");
                return null;
            }

            List<GameObject> spawnerSpawned = new List<GameObject>();

            for (int iter = 0; iter < iterations; iter++)
            {
                if (!Chance.giveChance(spawnChance)) continue;

                int count = Random.Range(minMaxCount.x, minMaxCount.y + 1);

                //Debug.Log(transform.name + " " + count);
                List<GameObject> iSpawned = new List<GameObject>();

                if (midIterAwait) await Task.Delay(SInfo.awaitT);


                for (int i = 0; i < count; i++)
                {
                    Vector3[] previousPos = iSpawned.Select(t => t.transform.position).ToArray();

                    if(midSpawnAwait) await Task.Delay(SInfo.awaitT);

                    //gather points
                    pointInfo[] points = gatherStepPoints(previousPos);
                    //Debug.Log(transform.name + " 1: " + points.Length);

                    if (points.Length == 0)
                    {
                        continue;
                    }

                    pointInfo[] acceptedPoints = filterPoints(points, previousPos);


                    //Debug.Log(transform.name + " 2: " + acceptedPoints.Length);

                    if (acceptedPoints.Length == 0)
                        continue;


                    pointInfo picked = acceptedPoints.OrderBy(t => Random.Range(0, 100f)).OrderBy(t => t.weight).ToArray()[0];

                    GameObject s = spawnObj(picked.pos, Vector3.Lerp(Vector3.up, picked.normal, stepSettingsSO.normalAllighn), iter, i);

#if UNITY_EDITOR

                    UnityEditor.EditorUtility.SetDirty(s);
#endif
                    iSpawned.Add(s);
                    //Debug.Log(transform.name + " spawn");
                }

                spawnerSpawned.AddRange(iSpawned);


            }

            return spawnerSpawned.ToArray();
        }


        private pointInfo[] gatherStepPoints(Vector3[] previous)
        {
            List<pointInfo> points = new List<pointInfo>();


            Vector3[] pos = createPos(previous);

            if (stepSettingsSO.onSurface)
            {
                Vector3[] icoDir = null;
                if(stepSettingsSO.placeRayMethod == stepSettings.placeRayM.icoRay) icoDir = IcoSphere.getVertDir(stepSettingsSO.icoSubdivisons);

                foreach (Vector3 p in pos)
                {

                    if (stepSettingsSO.placeRayMethod == stepSettings.placeRayM.icoRay)
                    {
                        foreach (Vector3 dir in icoDir)
                        {

                            pointInfo? point = rayPointCreate(p, dir);

                            if (point != null)
                                points.Add(point.Value);

                        }

                    }
                    else //if(stepSettingsSO.placeRayMethod == stepSettings.placeRayM.rayDown)
                    {
                        pointInfo? point = rayPointCreate(p, Vector3.down);

                        if (point != null)
                            points.Add(point.Value);

                        break;
                    }
                }
            }
            else points.AddRange(pos.Select(t => new pointInfo(t, Vector3.up, getNeibours(t))));


            return points.ToArray();
        }

        private Vector3[] createPos(Vector3[] previous)
        {
            List<Vector3> picked = new List<Vector3>();

            if (previous.Length == 0)
            {
                for (int p = 0; p < stepSettingsSO.firstPlacePoints; p++)
                {
                    Vector2 randDir = Random.insideUnitCircle.normalized;
                    Vector3 posDir = new Vector3(
                        randDir.x * Random.Range(0, randPlacementRange.x),
                        Random.Range(0, randPlacementRange.y) + heightOffset,
                        randDir.y * Random.Range(0, randPlacementRange.x)
                        );

                    Vector3 pos = posDir + transform.position;

                    if (stepSettingsSO.centerSideMove != 0)
                    {
                        Vector3 csDir = stepSettingsSO.centerSideMove > 0 ?
                            (new Vector3(randDir.x, 0, randDir.y) * randPlacementRange.x + Vector3.up * pos.y) - posDir :
                            new Vector3(transform.position.x, pos.y, transform.position.z) - pos;


                        pos += csDir * Mathf.Abs(stepSettingsSO.centerSideMove); // csDir has proper dir, no need to inverse it
                    }


                    picked.Add(pos);
                }
            }
            else
            {

                /*
                if(maxStepSpawnDistanceToAll)
                {
                    spawnCenter = previous[0];

                    for (int i = 1; i < previous.Length; i++)
                    {
                        spawnCenter += previous[i];
                        spawnCenter /= 2;
                    }

                    spawnCenter.y = transform.position.y + heightOffset;

                    //spp.Add(spawnCenter);
                }
                */


                for (int p = 0; p < stepSettingsSO.closeByPlacePoints; p++)
                {
                    Vector3 pos;
                    Vector2 randDir = Random.insideUnitCircle.normalized; //get rand dir (pos on border)


                    Vector3 posDir = new Vector3(
                            randDir.x * Random.Range(minMaxStepSpawnDistance.x, minMaxStepSpawnDistance.y),
                            Random.Range(-randPlacementRange.y, randPlacementRange.y) + heightOffset,
                            randDir.y * Random.Range(minMaxStepSpawnDistance.x, minMaxStepSpawnDistance.y)
                            );

                    pos = posDir + previous[Random.Range(0, previous.Length)];

                    // distances will be chacked later, as pos still can change due to rays

                    picked.Add(pos);
                }
           
            }

            return picked.ToArray();
        }

        private pointInfo? rayPointCreate(Vector3 pos, Vector3 dir)
        {

            if (Physics.Raycast(pos, dir, out RaycastHit info, stepSettingsSO.rayLengh, stepSettingsSO.groundLayer, QueryTriggerInteraction.Ignore)) 
            {
                Vector3 point = info.point;
                if (!Physics.Raycast(pos, dir, Vector3.Distance(pos, point), stepSettingsSO.avoidedLayer, QueryTriggerInteraction.Collide))
                {
                    pointInfo pi = new pointInfo(point, info.normal, getNeibours(point));

                    return pi;
                }
             
            }


            return null;
        }

        private Vector3[] getNeibours(Vector3 pos)
        {

            if (stepSettingsSO.freeTightPlacePreference != 0)
            {
                Collider[] col = Physics.OverlapSphere(pos, stepSettingsSO.colFindRange, stepSettingsSO.objectsLayer, QueryTriggerInteraction.Ignore);

                return col.Select(t => t.transform.position).ToArray();
            }
            

            return new Vector3[0];
        }

        private pointInfo[] filterPoints(pointInfo[] points, Vector3[] spawnedPos)
        { 
            List<pointInfo> acceptedPoints = new List<pointInfo>();

            float sqrMinDis = minMaxStepSpawnDistance.x * minMaxStepSpawnDistance.x;
            float sqrMaxDis = minMaxStepSpawnDistance.y * minMaxStepSpawnDistance.y;


            for (int i = 0; i < points.Length; i++)
            {
                pointInfo p = points[i];

                //initial check
                bool minCorrect = true;
                bool maxCorrect = spawnedPos.Length > 0 ? maxStepSpawnDistanceToAll : true;

                Vector3 pos = p.pos;

                foreach (Vector3 sp in spawnedPos)
                {
                    float sqrDistance = (pos - sp).sqrMagnitude;

                    if (!maxStepSpawnDistanceToAll)
                    {
                        if (!maxCorrect)
                        {
                            if (sqrDistance <= sqrMaxDis)
                            {
                                //Debug.Log("far correct");
                                maxCorrect = true;
                            }
                        }
                    }
                    else if (sqrDistance > sqrMaxDis)
                    {
                        //Debug.Log("far incorrect");
                        maxCorrect = false;
                        break;
                    }


                    if (sqrDistance < sqrMinDis)
                    {
                        //Debug.Log("close incorrect " + (int)distance);
                        minCorrect = false;
                        break;
                    }
                }

                if (!minCorrect || !maxCorrect)
                {
                    //Debug.Log("nope"); 
                    continue;
                }

                float pointWeight = 0;

                if (stepSettingsSO.freeTightPlacePreference != 0)
                {
                    if (p.neighbours.Length < stepSettingsSO.minMaxNearObjects.x || p.neighbours.Length > stepSettingsSO.minMaxNearObjects.y) continue;


                    float tightness = 0;

                    foreach (Vector3 np in p.neighbours)
                    {
                        tightness += Vector3.Distance(np, p.pos) - stepSettingsSO.colFindRange;
                    }

                    tightness /= stepSettingsSO.colFindRange;

                    tightness *= stepSettingsSO.freeTightPlacePreference;


                    pointWeight += tightness * stepSettingsSO.placePreferenceWeight +
                        Random.Range(-stepSettingsSO.placeWeightRandAddon, stepSettingsSO.placeWeightRandAddon);
                }


                if (stepSettingsSO.onSurface)
                {
                    float angle = Vector3.Angle(p.normal, Vector3.up);

                    float normalAccurace = 0;

                    float angleDif = Mathf.Abs(stepSettingsSO.desiredNormalAngle - angle);

                    if (angleDif <= stepSettingsSO.normalIncorrectcionAcceptance)
                    {
                        normalAccurace += 1 - angleDif / stepSettingsSO.normalIncorrectcionAcceptance;
                    }
                    else continue;

                    pointWeight += normalAccurace * stepSettingsSO.normalAccuracyWeight +
                        Random.Range(-stepSettingsSO.normalWeightRandAddon, stepSettingsSO.normalWeightRandAddon);
                }

                p.weight = pointWeight;
                acceptedPoints.Add(p);
            }

            return acceptedPoints.ToArray();
        }

        private GameObject spawnObj(Vector3 pos, Vector3 normal, int iteration, int spawnedIndex)
        {


            GameObject toSpawn = stepSettingsSO.possibleSpawns[Random.Range(0, stepSettingsSO.possibleSpawns.Length - 1)];

            GameObject spawned = Instantiate(toSpawn, 
                pos, 
                Quaternion.identity,
                transform);

            spawned.transform.up = Vector3.Lerp(Vector3.up, normal, stepSettingsSO.normalAllighn);

            spawned.transform.rotation *= Quaternion.Euler(
                    Random.Range(-stepSettingsSO.maxRandRotAdd.x, stepSettingsSO.maxRandRotAdd.x),
                    Random.Range(-stepSettingsSO.maxRandRotAdd.y, stepSettingsSO.maxRandRotAdd.y),
                    Random.Range(-stepSettingsSO.maxRandRotAdd.z, stepSettingsSO.maxRandRotAdd.z)
                    );

            float scaleXZ = Random.Range(stepSettingsSO.minMaxXZToYScale.x, stepSettingsSO.minMaxXZToYScale.y);
            float scaleY = !stepSettingsSO.sameScaleOnAllAxis ? Random.Range(stepSettingsSO.minMaxXZToYScale.z, stepSettingsSO.minMaxXZToYScale.w) : scaleXZ;
            spawned.transform.localScale = new Vector3(scaleXZ, scaleY, scaleXZ);

            spawned.transform.position += spawned.transform.right * stepSettingsSO.localPlacementOffset.x + 
                spawned.transform.up * stepSettingsSO.localPlacementOffset.y + 
                spawned.transform.forward * stepSettingsSO.localPlacementOffset.z;

            //spawned.transform.name = toSpawn.name + "( i:" + iteration + " s:" + spawnedIndex + ")";

            return spawned;
        }

        private struct pointInfo
        {
            public Vector3 pos;
            public Vector3 normal;
            public Vector3[] neighbours;
            public float weight;

            public pointInfo(Vector3 pos, Vector3 normal, Vector3[] neighbours)
            {
                this.pos = pos;
                this.normal = normal;
                this.neighbours = neighbours;
                weight = 0;
            }
        }


#if UNITY_EDITOR

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Vector3 center = transform.position + Vector3.up * heightOffset;
            /*
            Gizmos.DrawWireSphere(center, randPlacementRange.x);
            Vector3 down = center + Vector3.down * randPlacementRange.y;
            Vector3 up = center + Vector3.up * randPlacementRange.y;
            Gizmos.DrawLine(down, up);
            Gizmos.DrawSphere(down, 0.2f);
            Gizmos.DrawSphere(up, 0.2f);
            */


            for (int h = -1; h <= 2; h += 2)
            {
                for (int i = 0; i < 24; i++)
                {
                    Vector3 pos = (Quaternion.AngleAxis(15 * i, Vector3.up) * transform.forward) * randPlacementRange.x + center;

                    Vector3 height = Vector3.up * h * randPlacementRange.y;

                    pos += height;

                    Gizmos.DrawLine(center + height, pos);

                    if(h == 1) Gizmos.DrawLine(pos, pos - height * 2);

                    Gizmos.DrawSphere(pos, 0.2f);
                }
            }
        }

#endif
    }
}
