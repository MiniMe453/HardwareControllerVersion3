using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace AlekGames.Placer.Shared
{
    public static class bezierCurve
    {
        /// <summary>
        /// returns a point on a bezier curve of 'progerss' t based on a list of anchors.
        /// t has to be between 0-1
        /// </summary>
        /// <param name="anchors"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static Vector3 getOnBezierCurvePos(anchorSettings[] anchors, float t, curveMode mode = curveMode.cubic)
        {
            if (t <= 0) return anchors[0].anchor;
            if (t >= 1) return anchors.Last().anchor;

            float fullValue = t * (anchors.Length - 1);

            int beforeIndex = (int)fullValue;

            anchorSettings a1 = anchors[beforeIndex];
            anchorSettings a2 = anchors[beforeIndex + 1];

            float newt = fullValue - beforeIndex;
            return getOn2AnchorsPos(a1, a2, newt, mode);
        }

        /// <summary>
        /// returns a point based on these 2 anchors using a cubic curve
        /// t has to be between 0-1
        /// </summary>
        /// <param name="anchor1"></param>
        /// <param name="anchor2"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static Vector3 getOn2AnchorsPos(anchorSettings anchor1, anchorSettings anchor2, float t, curveMode mode = curveMode.cubic)
        {
            Vector3 outT = anchor1.outT + anchor1.anchor;
            Vector3 inT = anchor2.inT + anchor2.anchor;
            switch (mode)
            {
                case curveMode.cubic:
                    return CubicCurve(anchor1.anchor, outT, inT, anchor2.anchor, t);           
                case curveMode.quadratic:
                    return QuadraticCurve(anchor1.anchor, (outT + inT) / 2, anchor2.anchor, t);
                case curveMode.linear:
                    return Vector3.Lerp(anchor1.anchor, anchor2.anchor, t);                   
                default:
                    return Vector3.zero;
            }
        }

        public static Vector3 CubicCurve(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t)
        {
            return Vector3.Lerp(
                QuadraticCurve(a, b, c, t),
                QuadraticCurve(b, c, d, t),
                t
                );
        }

        public static Vector3 QuadraticCurve(Vector3 a, Vector3 b, Vector3 c, float t)
        {
            return Vector3.Lerp(
                Vector3.Lerp(a, b, t),
                Vector3.Lerp(b, c, t),
                t
                );
        }

        public static anchorSettings[] autoSmooth(anchorSettings[] spline)
        {
            for (int i = 1; i < spline.Length; i++)
            {
                if (i != 0 && i < spline.Length - 1)
                {
                    Vector3 minusA = spline[i - 1].anchor;
                    Vector3 plusA = spline[i + 1].anchor;
                    Vector3 thisA = spline[i].anchor;

                    Vector3 dir0 = (minusA - thisA);
                    Vector3 dir1 = (plusA - thisA);
                    Vector3 dir = (dir0 - dir1).normalized;
                    spline[i].inT = dir * Vector3.Distance(minusA, thisA) / 2;
                    spline[i].outT = -dir * Vector3.Distance(plusA, thisA) / 2;
                }
                else if (i != 0) //last
                {
                    Vector3 thisA = spline[i].anchor;
                    Vector3 minusA = spline[i - 1].anchor;
                    Vector3 minusT = minusA + spline[i - 1].outT;

                    spline[i].inT = (minusT - thisA).normalized * Vector3.Distance(minusA, minusT) + (minusT - minusA) / 3;
                }
            }

            //firt part here couse it uses later tangents, calculated in the loop
            Vector3 thisA0 = spline[0].anchor;
            Vector3 plusA0 = spline[1].anchor;
            Vector3 plusT0 = plusA0 + spline[1].inT;

            spline[0].outT = (plusT0 - thisA0).normalized * Vector3.Distance(plusA0, plusT0) + (plusT0 - plusA0) / 3;
            spline[0].inT = -spline[0].outT;

            return spline;
        }

        [System.Serializable]
        public struct anchorSettings
        {
            public Vector3 anchor;
            public Vector3 inT;
            public Vector3 outT;

            public anchorSettings(Vector3 anchor, Vector3 inT, Vector3 outT)
            {
                this.anchor = anchor;
                this.inT = inT;
                this.outT = outT;
            }

            public anchorSettings(Vector3 anchor)
            {
                this.anchor = anchor;
                outT = inT = Vector3.zero;
            }
        }

        public enum curveMode { cubic, quadratic, linear };

    }
}
