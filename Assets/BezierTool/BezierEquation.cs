using System;
using UnityEngine;

namespace BezierTool {
    public class BezierEquation {
        public static Vector3 CubeBezierPoint(Vector3[] ctrlPoints, float t) {
            float neg = 1 - t;
            float secPowNeg = neg * neg;
            float thirdPowNeg = secPowNeg * neg;
            float secPow = t * t;
            float thirdPow = secPow * t;
            Vector3 ret = ctrlPoints[0] * thirdPowNeg
                          + 3 * t * ctrlPoints[1] * secPowNeg
                          + 3 * secPow * ctrlPoints[2] * neg
                          + thirdPow * ctrlPoints[3];
            return ret;
        }

        public static Vector3[] Sampling(Vector3[] ctrlPoints, int count) {
            Vector3[] ret = new Vector3[count + 2];
            ret[0] = ctrlPoints[0];
            for (int i = 1; i <= count; i++) {
                ret[i] = CubeBezierPoint(ctrlPoints, (float) i / count);
            }

            ret[^1] = ctrlPoints[^1];
            return ret;
        }
    }
}