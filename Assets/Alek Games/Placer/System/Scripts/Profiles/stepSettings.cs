using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AlekGames.Placer.Profiles
{
    [CreateAssetMenu(menuName = "Alek Games/Profiles/Step Settings", fileName = "new StepSettings")]
    public class stepSettings : ScriptableObject
    {
        public enum placeRayM { icoRay, rayDown }


        public GameObject[] possibleSpawns;

        [Tooltip(" min max (random value between) scale. xy in minmax on xz axes, while zw in minmax scale on y axis")]
        public Vector4 minMaxXZToYScale = new Vector4(0.9f, 1.1f, 0.9f, 1.1f);
        [Tooltip("if all axis should have sacled applied same, from xy of minMaxXZToYScale")]
        public bool sameScaleOnAllAxis = false;
        public Vector3 maxRandRotAdd = new Vector3(0, 180, 0);
        [Tooltip("this is applied after rotating object")]
        public Vector3 localPlacementOffset = new Vector3(0, 0.01f, 0);

        [Tooltip("ammmout of trail and error attempts when doing first spawn of iteration")]
        public int firstPlacePoints = 150;
        [Tooltip("ammmout of placement tryies in not first iteration of step")]
        public int closeByPlacePoints = 30;

        [Tooltip("if shoud try to find suface to spawn on")]
        public bool onSurface = true;

        [Tooltip("this counts for each place point")]
        public placeRayM placeRayMethod;
        [Min(1), Tooltip("subdivisons of ico sphere. keep in mind that this value greatly affects performace. i advise to keep it between 1 - 3 (i use 2)")]
        public int icoSubdivisons = 2;
        [Tooltip("raycast leangh to find ground")]
        public float rayLengh = 8;

        [Tooltip("layer of stuff to place the object on")]
        public LayerMask groundLayer;
        [Tooltip("layer of stuff, that prevents object from being placed")]
        public LayerMask avoidedLayer;

        [Range(0, 180), Tooltip("system will place the object where there is the closest angle to angle between surface normal and v3.up. if the same angle will pick randomly")]
        public float desiredNormalAngle = 0;
        [Range(0, 1)]
        public float normalAllighn = 0.2f;
        [Range(0, 180), Tooltip("max normal angle diffrience of surface to desiredNormalAngle")]
        public float normalIncorrectcionAcceptance = 30;

        [Range(-1, 1), Tooltip("will apply to first point")]
        public float centerSideMove = 0;

        [Range(-1, 1)]
        public float freeTightPlacePreference;
        public Vector2 minMaxNearObjects = new Vector2(0, 15);
        public float colFindRange = 2;
        [Tooltip("layer of objects to find when trying to get tight/free space")]
        public LayerMask objectsLayer;





        [Header("Weights")]
        [Range(0, 1)]
        public float placePreferenceWeight = 1;
        [Range(0, 1), Tooltip("a value between -this to this is going to be added to calculated weight of placement")]
        public float placeWeightRandAddon = 0.075f;
        [Range(0, 1)]
        public float normalAccuracyWeight = 1;
        [Range(0, 1), Tooltip("a value between -this to this is going to be added to calculated weight of placement")]
        public float normalWeightRandAddon = 0.075f;


    }
}
