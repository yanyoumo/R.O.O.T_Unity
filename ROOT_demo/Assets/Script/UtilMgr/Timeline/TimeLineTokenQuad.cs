using System;
using System.Collections.Generic;
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

        public void InitQuadShape(float unitLength, int subDivision, RoundGist gist,bool HeatsinkSwitch)
        {
            RoundGist = gist;
            var gistList = new List<int>();

            switch (gist.Type)
            {
                case StageType.Shop:
                    gistList.Add((int)TimeLineTokenType.ShopOpened);
                    break;
                case StageType.Require:
                    gistList.Add((int)TimeLineTokenType.RequireNormal);
                    gistList.Add((int)TimeLineTokenType.RequireNetwork);
                    break;
                case StageType.Destoryer:
                    gistList.Add((int)TimeLineTokenType.DestoryerIncome);
                    break;
                case StageType.Ending:
                    gistList.Add((int)TimeLineTokenType.Ending);
                    break;
                case StageType.Boss:
                    gistList.Add((int) TimeLineTokenType.BossStage);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            if (HeatsinkSwitch)
            {
                gistList.Add((int)TimeLineTokenType.HeatSinkSwitch);
            }
            for (var i = 0; i < gistList.Count; i++)
            {
                InitSubtoken(i, gistList.Count, unitLength, subDivision, gistList[i]);
            }
        }

        public void Awake()
        {
            DisableValMarker();
            QuadColors = new[]
            {
                ColorUtilityWrapper.ParseHtmlStringNotNull(ColorName.ROOT_TIMELINE_GENERAL),
                ColorUtilityWrapper.ParseHtmlStringNotNull(ColorName.ROOT_TIMELINE_NETWORK),
                ColorUtilityWrapper.ParseHtmlStringNotNull(ColorName.ROOT_TIMELINE_DISASTER),
                ColorUtilityWrapper.ParseHtmlStringNotNull(ColorName.ROOT_TIMELINE_ENDING),
                ColorUtilityWrapper.ParseHtmlStringNotNull("#FF8800"),
                ColorUtilityWrapper.ParseHtmlStringNotNull("#0E195E"),
                ColorUtilityWrapper.ParseHtmlStringNotNull("#AB00C2"),
            };
        }

        private void SetVal()
        {
            if (RoundGist.Type == StageType.Require|| RoundGist.Type == StageType.Shop)
            {
                if (MarkerID == owner.StepCount)
                {
                    SetValMarker(RoundGist.normalReq, TimeLineTokenType.RequireNormal);
                    SetValMarker(RoundGist.networkReq, TimeLineTokenType.RequireNetwork);
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