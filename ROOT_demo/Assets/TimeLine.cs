using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace ROOT
{
    public enum TimeLineTokenType
    {
        RequireNormal=0,//Green
        RequireNetwork = 1,//Blue
        DestoryerIncome = 2,//Red
        Ending = 3,//Black
    }

    [Serializable]
    public struct TimeLineToken
    {
        public int TokenID;
        public TimeLineTokenType type;
        [ShowIf("@this.type==TimeLineTokenType.RequireNormal||this.type==TimeLineTokenType.RequireNetwork")]
        public int RequireAmount;
        public Vector2Int Range;//[Starting,Ending),Ending==-1 means Always

        public bool InRange(int count)
        {
            if (Range.y >= 0)
            {
                return count >= Range.x && count < Range.y;
            }
            else
            {
                return count >= Range.x;
            }
        }
    }

    //[ExecuteInEditMode]
    public class TimeLine : MonoBehaviour
    {
        public MeshRenderer ArrowRenderer;
        private bool _requirementSatisfied;
        public bool RequirementSatisfied
        {
            set
            {
                ArrowRenderer.material.color = value ? Color.green : Color.red;
                _requirementSatisfied = value;
            }
            get => _requirementSatisfied;
        }

        public GameObject TimeLineMarkerTemplate;
        public GameObject TimeLineTokenTemplate;
        public Transform TimeLineMarkerZeroing;
        public Transform TimeLineMarkerEntering;
        public Transform TimeLineMarkerExiting;
        public Transform TimeLineMarkerRoot;
        public readonly int TotalUnitLength = 5;
        private readonly float _unitLength = 1.0f;
        public float UnitLength => _unitLength * UnitLengthScaling;
        private float UnitLengthScaling = 3.0f;
        public readonly int SubDivision = 5;
        public int MarkerCount;
        public int TotalCount;
        public static int StepCount;

        protected float AnimationTimerOrigin = 0.0f;//都是秒
        private float animationTimer => Time.time - AnimationTimerOrigin;
        private float AnimationLerper
        {
            get
            {
                float res = animationTimer / DefaultLevelLogic.AnimationDuration;
                return Mathf.Min(res, 1.0f);
            }
        }

        public TimeLineToken[] TimeLineTokens;

        bool HasEnding(List<TimeLineToken> timeLineTokens)
        {
            foreach (var timeLineToken in timeLineTokens)
            {
                if (timeLineToken.type == TimeLineTokenType.Ending)
                {
                    return true;
                }
            }

            return false;
        }

        void CheckToken(Transform MarkRoot, int j, int markerCount)
        {
            int i = 0;
            float baseTokenHeight = 0.38f;
            float TokenHeight = baseTokenHeight;

            List<TimeLineToken> timeLineTokens=new List<TimeLineToken>();
            foreach (var timeLineToken in TimeLineTokens)
            {
                if (timeLineToken.InRange(markerCount))
                {
                    timeLineTokens.Add(timeLineToken);
                }
            }

            if (timeLineTokens.Count > 0)
            {
                if (HasEnding(timeLineTokens))
                {
                    var token = Instantiate(TimeLineTokenTemplate, MarkRoot);
                    token.GetComponent<TimeLineTokenQuad>().SetEndingQuadShape(UnitLength, SubDivision, j);
                }
                else
                {
                    foreach (var timeLineToken in timeLineTokens)
                    {
                        //TODO 最后一个Token会支楞出去，但是先不用管
                        var token = Instantiate(TimeLineTokenTemplate, MarkRoot);
                        token.GetComponent<TimeLineTokenQuad>().SetQuadShape(UnitLength, SubDivision,timeLineToken.type, i, timeLineTokens.Count, j);
                        token.GetComponent<TimeLineTokenQuad>().markerID = markerCount;
                        token.GetComponent<TimeLineTokenQuad>().token = timeLineToken;
                        i++;
                    }
                }
            }
        }

        void CreateMarker(int i, int j, int markerCount)
        {
            var marker = Instantiate(TimeLineMarkerTemplate, TimeLineMarkerRoot);
            float x5 = UnitLength * i + (UnitLength / SubDivision) * (j);
            marker.transform.localPosition = TimeLineMarkerZeroing.localPosition + new Vector3(x5, 0, 0);
            CheckToken(marker.transform, j, markerCount);
            if (j != 0)
            {
                marker.GetComponent<TimeLineMarker>().TimeLineMarkerRoot.localScale = Vector3.one * 0.75f;
            }
        }

        void InitTimeLine()
        {
            TimeLineMarkerRoot.localPosition=Vector3.zero;
            TotalCount = 0;
            float x5=0;
            while (x5 <= TimeLineMarkerEntering.localPosition.x || TotalCount == 0)
            {
                int i = TotalCount / SubDivision;
                int j = TotalCount % SubDivision;
                x5 = UnitLength * i + (UnitLength / SubDivision) * (j);
                CreateMarker(i, j, TotalCount);
                TotalCount++;
            }
        }

        IEnumerator StepAnmation(Vector3 orgPos)
        {
            AnimationTimerOrigin = Time.time;
            while (AnimationLerper < 1.0f)
            {
                yield return 0;
                TimeLineMarkerRoot.transform.localPosition = orgPos - new Vector3(UnitLength / SubDivision, 0, 0)*AnimationLerper;
            }
            TimeLineMarkerRoot.transform.localPosition = orgPos - new Vector3(UnitLength / SubDivision, 0, 0);
        }

        //[Button("Step")]
        public void Step()
        {
            StepCount++;
            Vector3 orgPos = TimeLineMarkerRoot.transform.localPosition;
            StartCoroutine(StepAnmation(orgPos));
        }

        void Update()
        {
            var markers = TimeLineMarkerRoot.GetComponentsInChildren<TimeLineMarker>();
            float x2 = TimeLineMarkerRoot.transform.localPosition.x;
            MarkerCount = 0;
            float xMin = float.MaxValue;
            foreach (var timeLineMarker in markers)
            {
                float x1 = timeLineMarker.transform.localPosition.x;
                xMin = Math.Min(xMin, x1+x2);
                if (x1 + x2 <= TimeLineMarkerExiting.localPosition.x)
                {
                    timeLineMarker.PendingKill = true;
                }
                else
                {
                    MarkerCount++;
                }
            }
            MarkerCount ++;
            //TODO 这样整出来的有一个Wrap的Offset。看看咋回事儿。
            int i = MarkerCount / SubDivision;
            int j = MarkerCount % SubDivision;
            float x3 = UnitLength * i + (UnitLength / SubDivision) * j;
            //float x5 = TimeLineMarkerExiting.transform.localPosition.x;
            float x4 = x3 + xMin;
            if (x4 <= TimeLineMarkerEntering.localPosition.x)
            {
                int i1 = TotalCount / SubDivision;
                int j1 = TotalCount % SubDivision;
                CreateMarker(i1, j1, TotalCount);
                TotalCount++;
            }
        }

        public void InitWithTokens(TimeLineToken[] _TimeLineTokens)
        {
            TimeLineTokens = _TimeLineTokens;
            InitTimeLine();
        }

        void Awake()
        {
            RequirementSatisfied = false;
        }

        void OnEnable()
        {
            //InitTimeLine();
        }
    }
}