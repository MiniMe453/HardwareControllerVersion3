using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

using AlekGames.Placer.Shared;

namespace AlekGames.Placer.Systems.Main
{
    public class stepManager : MonoBehaviour, ISpawner
    {
        [SerializeField]
        public stepGenerator[] steps;

        private List<GameObject> spawned = new List<GameObject>();

        public float maxTimeMilPerStep = 10000;



        [ContextMenu("spawn")]
        public async Task spawn()
        {

            for (int i = 0; i < steps.Length; i++)
            {
                Debug.Log("next step");

                Task<GameObject[]> spawner = steps[i].Spawn();

                int mil = 0;
                while (!spawner.IsCompleted)
                {
                    await Task.Delay(1);
                    mil++;
                    if (mil > maxTimeMilPerStep)
                    {
                        spawner.Dispose();
                        Debug.LogWarning("step: " + i + " failed. disposing");
                        break;
                    }
                }

                await Task.Delay(SInfo.awaitT);

                spawned.AddRange(spawner.Result);
            }

            Debug.Log("endSpawn");
        }

        [ContextMenu("revert")]
        public void revert() 
        {
            for (int i = 0; i < spawned.Count; i++) DestroyImmediate(spawned[i]);
            spawned = new List<GameObject>();
        }




    }
}
