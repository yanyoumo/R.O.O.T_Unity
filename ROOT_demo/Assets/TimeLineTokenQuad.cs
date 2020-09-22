using System;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using TMPro;
using UnityEngine;

namespace ROOT
{
    public class TimeLineTokenQuad : MonoBehaviour
    {
        [ReadOnly]
        public TimeLine owner;
        [ReadOnly]
        public RoundGist RoundGist;
        [ReadOnly]
        public int MarkerID;
        
        public GameObject QuadTemplate;

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

        public void SetValMarker(int val, TimeLineTokenType type)
        {
            if (type == TimeLineTokenType.RequireNormal)
            {
                valMarkerNormal.enabled = true;
                valMarkerNormal.text = val.ToString();
            }
            else if (type == TimeLineTokenType.RequireNetwork)
            {
                valMarkerNetwork.enabled = true;
                valMarkerNetwork.text = val.ToString();
            }
        }

        private void InitSubtoken(int i,int max,float unitLength,int subDivision,int ColorID)
        {
            var go = Instantiate(QuadTemplate, QuadTransform);
            transform.localPosition = Vector3.zero;
            transform.localScale = Vector3.one;
            var tokenHeightUnit = baseTokenHeight / max;
            var tokenHeight = tokenHeightUnit * (max - i);
            go.GetComponent<MeshRenderer>().material.color = QuadColors[ColorID];
            go.transform.localPosition = new Vector3((unitLength / subDivision) * 0.5f, yOffset * i, tokenHeight * 0.5f);
            go.transform.localScale = new Vector3(unitLength / subDivision, tokenHeight, 1.0f);
        }

        public void InitQuadShape(float unitLength, int subDivision, RoundGist gist)
        {
            RoundGist = gist;
            //TODO 没处理Ending的事情。

            switch (gist.Type)
            {
                case StageType.Shop:
                    InitSubtoken(0, 1, unitLength, subDivision, 5);
                    break;
                case StageType.Require:
                    InitSubtoken(0, 2, unitLength, subDivision, 0);
                    InitSubtoken(1, 2, unitLength, subDivision, 1);
                    break;
                case StageType.Destoryer:
                    InitSubtoken(0, 1, unitLength, subDivision, 2);
                    break;
                case StageType.Ending:
                    InitSubtoken(0, 1, unitLength, subDivision, 3);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void Awake()
        {
            QuadColors = new[]
            {
                ColorUtilityWrapper.ParseHtmlStringNotNull(ColorName.ROOT_TIMELINE_GENERAL),
                ColorUtilityWrapper.ParseHtmlStringNotNull(ColorName.ROOT_TIMELINE_NETWORK),
                ColorUtilityWrapper.ParseHtmlStringNotNull(ColorName.ROOT_TIMELINE_DISASTER),
                ColorUtilityWrapper.ParseHtmlStringNotNull(ColorName.ROOT_TIMELINE_ENDING),
                ColorUtilityWrapper.ParseHtmlStringNotNull("#FF8800"),
                ColorUtilityWrapper.ParseHtmlStringNotNull("#0E195E"),
            };
        }

        private void SetVal()
        {
            if (RoundGist.Type == StageType.Require|| RoundGist.Type == StageType.Shop)
            {
                if (MarkerID == owner.StepCount)
                {
                    SetValMarker(RoundGist.Val0, TimeLineTokenType.RequireNormal);
                    SetValMarker(RoundGist.Val1, TimeLineTokenType.RequireNetwork);
                }
            }
        }

        private void SetSingleToken(TimeLineToken _token)
        {
            if (_token.type == TimeLineTokenType.RequireNormal || _token.type == TimeLineTokenType.RequireNetwork)
            {
                if (MarkerID == _token.Range.x)
                {
                    if (_token.Range.x >= owner.StepCount)
                    {
                        SetValMarker(_token.RequireAmount, _token.type);
                    }
                }
                else if (MarkerID <= _token.Range.y)
                {
                    if (MarkerID == owner.StepCount)
                    {
                        SetValMarker(_token.RequireAmount, _token.type);
                    }
                }
                else if (_token.Range.y == -1)
                {
                    if (MarkerID == owner.StepCount)
                    {
                        SetValMarker(_token.RequireAmount, _token.type);
                    }
                }
            }
        }

        public void Update()
        {
            DisableValMarker();
            SetVal();
        }
    }
}