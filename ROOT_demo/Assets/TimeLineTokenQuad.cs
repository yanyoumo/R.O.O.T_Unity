using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace ROOT
{
    public class TimeLineTokenQuad : MonoBehaviour
    {
        public TimeLineToken token;

        public int markerID;

        /*public int rangMin;
        public int rangMax;
        public int RequiredVal;*/
        private readonly float baseTokenHeight = 0.38f;
        public Transform QuadTransform;
        public TextMeshPro valMarkerNormal;
        public TextMeshPro valMarkerNetwork;
        float yOffset = 0.01f;

        public Color QuadColor
        {
            set => GetComponentInChildren<MeshRenderer>().material.color = value;
        }

        Color[] QuadColors =
        {
            Color.green, Color.blue, Color.red, Color.black,
        };

        public void DisableValMarker()
        {
            valMarkerNormal.enabled = false;
            valMarkerNetwork.enabled = false;
        }

        public void SetValMarker(int val)
        {
            if (token.type == TimeLineTokenType.RequireNormal)
            {
                valMarkerNormal.enabled = true;
                valMarkerNormal.text = val.ToString();
            }
            else
            {
                valMarkerNetwork.enabled = true;
                valMarkerNetwork.text = val.ToString();
            }
        }

        public void SetQuadShape(float UnitLength, int SubDivision,TimeLineTokenType type, int val, int max, int j)
        {
            float TokenHeightUnit = baseTokenHeight / max;
            float TokenHeight = TokenHeightUnit * (max - val);
            GetComponent<TimeLineTokenQuad>().QuadColor = QuadColors[(int)type];
            transform.localPosition = new Vector3(0.0f, yOffset * val, 0.0f);
            transform.localScale = Vector3.one;
            QuadTransform.localScale = new Vector3(UnitLength / SubDivision, 1.0f, TokenHeight);
            DisableValMarker();
        }

        public void SetEndingQuadShape(float UnitLength, int SubDivision, int j)
        {
            float TokenHeight = baseTokenHeight;
            GetComponent<TimeLineTokenQuad>().QuadColor = Color.black;
            transform.localPosition = new Vector3(0.0f, yOffset * 3, 0.0f);
            transform.localScale = Vector3.one;
            QuadTransform.localScale = new Vector3(UnitLength / SubDivision, 1.0f, TokenHeight);
            DisableValMarker();
        }

        public void Update()
        {
            DisableValMarker();
            if (token.type == TimeLineTokenType.RequireNormal || token.type == TimeLineTokenType.RequireNetwork)
            {
                if (markerID == token.Range.x)
                {
                    if (token.Range.x >= TimeLine.StepCount)
                    {
                        SetValMarker(token.RequireAmount);
                    }
                }
                else if (markerID <= token.Range.y)
                {
                    if (markerID == TimeLine.StepCount)
                    {
                        SetValMarker(token.RequireAmount);
                    }
                }
                else if (token.Range.y == -1)
                {
                    if (markerID == TimeLine.StepCount)
                    {
                        SetValMarker(token.RequireAmount);
                    }
                }
            }
        }
    }
}