using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using UnityEngine;

namespace ROOT
{
    public class TimeLineTokenQuad : MonoBehaviour
    {
        public TimeLine owner;
        [HideInInspector]
        public TimeLineToken Token;
        //[Readonlyin]
        public int MarkerID;

        private readonly float baseTokenHeight = 0.38f;
        public Transform QuadTransform;
        public TextMeshPro valMarkerNormal;
        public TextMeshPro valMarkerNetwork;
        float yOffset = 0.01f;

        public Color QuadColor
        {
            set => GetComponentInChildren<MeshRenderer>().material.color = value;
        }

        private Color[] QuadColors;

        public void DisableValMarker()
        {
            valMarkerNormal.enabled = false;
            valMarkerNetwork.enabled = false;
        }

        public void SetValMarker(int val)
        {
            if (Token.type == TimeLineTokenType.RequireNormal)
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

        public void Awake()
        {
            QuadColors = new[]
            {
                ColorUtilityWrapper.ParseHtmlStringNotNull(ColorName.ROOT_TIMELINE_GENERAL),
                ColorUtilityWrapper.ParseHtmlStringNotNull(ColorName.ROOT_TIMELINE_NETWORK),
                ColorUtilityWrapper.ParseHtmlStringNotNull(ColorName.ROOT_TIMELINE_DISASTER),
                ColorUtilityWrapper.ParseHtmlStringNotNull(ColorName.ROOT_TIMELINE_ENDING),
            };
        }

        public void Update()
        {
            DisableValMarker();
            if (Token.type == TimeLineTokenType.RequireNormal || Token.type == TimeLineTokenType.RequireNetwork)
            {
                if (MarkerID == Token.Range.x)
                {
                    if (Token.Range.x >= owner.StepCount)
                    {
                        SetValMarker(Token.RequireAmount);
                    }
                }
                else if (MarkerID <= Token.Range.y)
                {
                    if (MarkerID == owner.StepCount)
                    {
                        SetValMarker(Token.RequireAmount);
                    }
                }
                else if (Token.Range.y == -1)
                {
                    if (MarkerID == owner.StepCount)
                    {
                        SetValMarker(Token.RequireAmount);
                    }
                }
            }
        }
    }
}