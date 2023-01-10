using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

using UnityEngine;

using AlekGames.Placer.Shared;
using AlekGames.Placer.Profiles;


namespace AlekGames.Placer.Systems.Main
{
    public class BezierCurveSpawner : MonoBehaviour, ISpawner, IPrefabPalleteUser
    {
        public enum stepM { oneShoot, segmental };
        public enum spawnT { Wall, PrefabPalette};

        public spawnT spawnType;

        public wallSettings wallPreset;

        [Tooltip(" if should go further than picked position, to ensuer that the wall has indeed went threw it, and spawned when it was picked. this may resul in a lot of mess and inaccuracy")]
        public bool allowOverPoint;

        public prefabPalette palette;

        public int specificIndex = -1;

        [Range(0,30), Tooltip("min distanc between spawns (or their attempts)")]
        public float minSpawnDistance = 1;

        [Tooltip("if should snap spawned objets to ground")]
        public bool snapToGround = true;


        [Tooltip("curvature calculation used by the curve. the curve preview does not match the curve then, only the spawn points previews do")]
        public bezierCurve.curveMode curveMode;


        public stepM stepMode;

        [Range(0.001f, 0.1f)]
        public float curveSteps = 0.03f;



        public bezierCurve.anchorSettings[] curve = new bezierCurve.anchorSettings[0];




        public async Task spawn()
        {
            Vector3 lastSpawn = new Vector3(float.PositiveInfinity, 0, 0);

            if(spawnType == spawnT.Wall)
            {
                Vector3 pos0 = curve[0].anchor;
                wallPreset.spawn(wallSettings.toSpawnType.post, pos0, Quaternion.LookRotation(curve[0].outT - pos0, Vector3.up), transform);
                lastSpawn = pos0;
            }

            Vector3 checkUpOffset = Vector3.up * 15;
            float sqrMinSpawnDist = minSpawnDistance * minSpawnDistance;

            await Task.Delay(1);

            switch (stepMode)
            {
                case stepM.segmental:
                    for (int i = 0; i < curve.Length - 1; i++)
                    {
                        bezierCurve.anchorSettings a1 = curve[i];
                        bezierCurve.anchorSettings a2 = curve[i + 1];
                        for (float t = curveSteps; t < 1; t += curveSteps)
                        {
                            Vector3 point = bezierCurve.getOn2AnchorsPos(a1, a2, t, curveMode);

                            if ((lastSpawn - point).sqrMagnitude > sqrMinSpawnDist)
                            {
                                if (snapToGround)
                                {
                                    if (Physics.Raycast(point + checkUpOffset, Vector3.down, out RaycastHit info))
                                    {
                                        lastSpawn = onPosSpawn(info.point, spawnType == spawnT.Wall ? lastSpawn : info.normal);
                                    }
                                }
                                else lastSpawn = onPosSpawn(point, spawnType == spawnT.Wall ? lastSpawn : Vector3.up);
                            }
                        }
                    }
                    break;
                case stepM.oneShoot:
                    for (float t = 0; t < 1; t += curveSteps)
                    {
                        Vector3 point = bezierCurve.getOnBezierCurvePos(curve, t, curveMode);

                        if ((lastSpawn - point).sqrMagnitude > sqrMinSpawnDist)
                        {
                            if (snapToGround)
                            {
                                if (Physics.Raycast(point + checkUpOffset, Vector3.down, out RaycastHit info))
                                {
                                    lastSpawn = onPosSpawn(info.point, spawnType == spawnT.Wall ? lastSpawn : info.normal);
                                }
                            }
                            else lastSpawn = onPosSpawn(point, spawnType == spawnT.Wall ? lastSpawn : Vector3.up);
                        }
                    }
                    break;

            }
        }

        /// <summary>
        /// otherVect is last spawn pos if spawning wall or surface normal if spawning from palette
        /// </summary>
        /// <param name="to"></param>
        /// <param name="otherVec"></param>
        /// <returns></returns>
        private Vector3 onPosSpawn(Vector3 pos, Vector3 otherVec)
        {
            if(spawnType == spawnT.Wall)
            {
                Vector3? nextWallPos = wallPreset.spawnWall(otherVec, pos, allowOverPoint, transform);

                if (nextWallPos != null) return nextWallPos.Value;         
                else return otherVec;             
            }
            else
            {
                palette.spawn(pos, otherVec, transform, 0, specificIndex);
                return pos;
            }
        }

        public void revert()
        {
            while (transform.childCount > 0)
            {
                DestroyImmediate(transform.GetChild(0).gameObject);
            }
        }

        public void snapCurveToGround()
        {
            Vector3 upAddon = Vector3.up * 0.25f;
            for (int i = 0; i < curve.Length; i++)
            {
                if (Physics.Raycast(curve[i].anchor + Vector3.up * 10, Vector3.down, out RaycastHit info))
                    curve[i].anchor = info.point + upAddon;
                if (Physics.Raycast(curve[i].inT + Vector3.up * 10, Vector3.down, out info))
                    curve[i].inT = info.point + upAddon;
                if (Physics.Raycast(curve[i].outT + Vector3.up * 10, Vector3.down, out info))
                    curve[i].outT = info.point + upAddon;
            }
        }



        public prefabPalette getPalette()
        {
            return palette;
        }

        public int getSpecificIndex()
        {
            return specificIndex;
        }

        public void setPalette(prefabPalette p) => palette = p;

        public void setSpecificIndex(int index) => specificIndex = index;


#if UNITY_EDITOR
        public enum Tmode { automatic, halfManual, manual };
        public enum halfManulalOtherTMode { none, keepRatio, Copy};
        [Tooltip("edit mode of tangents. automatic will place tangents for you, half manual, will let you mov them, while keeping  straight line with the other one, and manula doesnt give a f, and lets you do anything")]
        public Tmode curveTangentMode;
        public halfManulalOtherTMode otherTangentMode = halfManulalOtherTMode.keepRatio;

        [Range(0.1f, 1f)]
        public float previewSpawnBallsSize = 0.4f;

        private void OnDrawGizmosSelected()
        {
            if (curve.Length < 2) return;

            Gizmos.color = Color.cyan;
            float sqrMinSpawnDist = minSpawnDistance * minSpawnDistance;
            Vector3 lastChecked = new Vector3(float.PositiveInfinity, 0, 0);
            Vector3 checkUpOffset = Vector3.up * 15;

            switch (stepMode)
            {
                case stepM.segmental:
                    for (int i = 0; i < curve.Length - 1; i++)
                    {
                        bezierCurve.anchorSettings a1 = curve[i];
                        bezierCurve.anchorSettings a2 = curve[i + 1];
                        for (float t = curveSteps; t < 1; t += curveSteps)
                        {
                            Vector3 point = bezierCurve.getOn2AnchorsPos(a1, a2, t, curveMode);

                            if ((lastChecked - point).sqrMagnitude > sqrMinSpawnDist)
                            {
                                if (snapToGround && Physics.Raycast(point + checkUpOffset, Vector3.down, out RaycastHit info))
                                {
                                    Gizmos.DrawSphere(info.point, previewSpawnBallsSize);
                                }
                                else
                                    Gizmos.DrawSphere(point, previewSpawnBallsSize);

                                lastChecked = point;
                            }


                        }
                    }
                    break;
                case stepM.oneShoot:
                    for (float t = 0; t < 1; t += curveSteps)
                    {
                        Vector3 point = bezierCurve.getOnBezierCurvePos(curve, t, curveMode);

                        if ((lastChecked - point).sqrMagnitude > sqrMinSpawnDist)
                        {
                            if (snapToGround && Physics.Raycast(point + checkUpOffset, Vector3.down, out RaycastHit info))
                            {
                                Gizmos.DrawSphere(info.point, previewSpawnBallsSize);
                            }
                            else
                                Gizmos.DrawSphere(point, previewSpawnBallsSize);

                            lastChecked = point;
                        }
                    }
                    break;
            
        }


            for (int i = 0; i < curve.Length; i++)
            {
                Gizmos.color = Color.red;
                Vector3 anchor = curve[i].anchor;
                Gizmos.DrawSphere(curve[i].anchor, 0.75f);
                Gizmos.color = Color.yellow;
                if(i != 0) Gizmos.DrawSphere(anchor + curve[i].inT, 0.6f);
                if (i != curve.Length - 1) Gizmos.DrawSphere(anchor + curve[i].outT, 0.6f);

            }
        }

        private void Reset()
        {
            curve = new bezierCurve.anchorSettings[4] {
                new bezierCurve.anchorSettings(transform.position + transform.forward * 1.5f),
                new bezierCurve.anchorSettings(transform.position + transform.forward * 0.5f),
                new bezierCurve.anchorSettings(transform.position - transform.forward * 0.5f),
                new bezierCurve.anchorSettings(transform.position - transform.forward * 1.5f)
            };

            bezierCurve.autoSmooth(curve);
        }

#endif
    }
}
