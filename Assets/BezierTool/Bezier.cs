using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

namespace BezierTool {
    public class Bezier : MonoBehaviour {
        enum Algorithm {
            Equation,
            Recursion
        }


        public static void DrawPointOnLine(Vector3 startPoint, Vector3 current, Color color) {
            Vector3 vec = current - startPoint;
            var prep = Vector3.Normalize(Vector3.Cross(vec, Vector3.forward));
            Gizmos.color = color;
            Gizmos.DrawLine(prep * 0.01f + current, prep * -0.01f + current);
        }

        [SerializeField] private Algorithm algorithm;
        [SerializeField] private List<GameObject> ctrlPointObj;
        [SerializeField] private int sampleCount;
        [SerializeField] private List<Color> levelColor;
        [SerializeField] private Color resultColor;

        [SerializeField] private int lineLevel = -1;
        [SerializeField] private Sprite ctrlHandle;

        private List<Vector3> retList;

        void DrawStartEnd(ref Vector3[] ctrlPoint) {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(ctrlPoint[0], ctrlPoint[1]);
            if (ctrlPoint.Length > 2) {
                Gizmos.DrawLine(ctrlPoint[^1], ctrlPoint[^2]);
            }
        }

        bool CheckValidArgs() {
            return ctrlPointObj is {Count: >= 2};
        }

        void GameObject2Point(out Vector3[] ctrlPoint) {
            ctrlPoint = new Vector3[ctrlPointObj.Count];
            for (int i = 0; i < ctrlPointObj.Count; i++) {
                ctrlPoint[i] = ctrlPointObj[i].transform.position;
            }
        }


        float NormalizeSamplingPoint(int index) {
            return (float) index / sampleCount;
        }

        Vector3[] DrawBezier([ReadOnly] ref Vector3[] ctrlPoint, float lerp) {
            var nextLevel = new Vector3 [ctrlPoint.Length - 1];


            for (int i = 1; i < ctrlPoint.Length; i++) {
                int last = i - 1;
                nextLevel[last] = Vector3.Lerp(ctrlPoint[last], ctrlPoint[i], lerp);

                if (ctrlPointObj.Count - ctrlPoint.Length == lineLevel || lineLevel == -1) {
                    DrawPointOnLine(ctrlPoint[last], nextLevel[last], levelColor[nextLevel.Length]);
                    if (last >= 1) {
                        Gizmos.color = levelColor[ctrlPointObj.Count - ctrlPoint.Length];
                        Gizmos.DrawLine(nextLevel[last], nextLevel[last - 1]);
                    }
                }
            }


            if (nextLevel.Length == 1) {
                retList.Add(nextLevel[0]);
                return nextLevel;
            }

            return DrawBezier(ref nextLevel, lerp);
        }


        void DrawBezierRecursion(Vector3[] ctrlPoint) {
            retList.Clear();
            retList.Add(ctrlPoint[0]);
            for (int i = 0; i < sampleCount; i++) {
                var lerp = NormalizeSamplingPoint(i);
                DrawBezier(ref ctrlPoint, lerp);
            }

            retList.Add(ctrlPoint[^1]);
            Gizmos.color = resultColor;
            for (int i = 1; i < retList.Count; i++) {
                Gizmos.DrawLine(retList[i - 1], retList[i]);
            }
        }

        void DrawBezierEquation(Vector3[] ctrlPoint) {
            Gizmos.color = resultColor;
            var ret = BezierEquation.Sampling(ctrlPoint, sampleCount);
            for (int i = 1; i < ret.Length; i++) {
                Gizmos.DrawLine(ret[i - 1], ret[i]);
            }
        }


        private void OnValidate() {
            foreach (Transform child in transform) {
                child.localScale = 0.1f * Vector3.one;
                var render = child.AddComponent<SpriteRenderer>();
                if (render != null) render.sprite = ctrlHandle;
            }
        }

        private void OnDrawGizmos() {
            if (!CheckValidArgs()) {
                return;
            }

            GameObject2Point(out var ctrlPoint);
            DrawStartEnd(ref ctrlPoint);

            switch (algorithm) {
                case Algorithm.Recursion:
                    DrawBezierRecursion(ctrlPoint);
                    break;
                case Algorithm.Equation:
                    DrawBezierEquation(ctrlPoint);
                    break;
            }

            //  Gizmos.DrawLine();
        }
    }
}