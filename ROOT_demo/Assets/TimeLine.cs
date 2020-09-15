using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
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
    public class TimeLineToken: IComparable
    {
        public int TokenID;
        public TimeLineTokenType type;
        [ShowIf("@this.type==TimeLineTokenType.RequireNormal||this.type==TimeLineTokenType.RequireNetwork")]
        public int RequireAmount;
        public Vector2Int Range;//[Starting,Ending),Ending==-1 means Always

        public int CompareTo(object obj)
        {
            switch (obj)
            {
                case null:
                    return 1;
                case TimeLineToken other:
                    return (int) type - (int) other.type;
                default:
                    throw new ArgumentException("Object is not a TimeLineToken");
            }
        }

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
        public TimeLineGoalMarker GoalMarker;
        private GameAssets _currentGameAsset;

        public void SetNoCount()
        {
            SetGoalCount = 0;
            SetCurrentCount = 0;
        }

        public int SetGoalCount
        {
            set => GoalMarker.TargetCount = value;
        }

        public int SetCurrentCount
        {
            set => GoalMarker.CurrentCount = value;
        }

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
        private int TotalCount;

        public int StepCount => _currentGameAsset.StepCount;

        protected float AnimationTimerOrigin = 0.0f; //都是秒
        private float animationTimer => Time.time - AnimationTimerOrigin;

        float easeInOutCubic(float x)
        {
            return x < 0.5f ? 4.0f * x * x * x : 1.0f - Mathf.Pow(-2.0f * x + 2.0f, 3.0f) / 2.0f;
        }

        private float AnimationLerper
        {
            get
            {
                float res = animationTimer / LevelLogic.AnimationDuration;
                res = Mathf.Clamp01(res);
                return easeInOutCubic(res);
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
            var i = 0;
            var timeLineTokens = TimeLineTokens.Where(timeLineToken => timeLineToken.InRange(markerCount)).ToList();
            if (timeLineTokens.Count <= 0) return;

            var token = Instantiate(TimeLineTokenTemplate, MarkRoot);
            token.GetComponent<TimeLineTokenQuad>().owner = this;
            token.GetComponent<TimeLineTokenQuad>().InitQuadShape(UnitLength, SubDivision, timeLineTokens.ToArray());
            token.GetComponent<TimeLineTokenQuad>().MarkerID = markerCount;
        }

        void CreateMarker(int i, int j, int markerCount)
        {
            var marker = Instantiate(TimeLineMarkerTemplate, TimeLineMarkerRoot);
            var unitLocalX = UnitLength * i + (UnitLength / SubDivision) * (j);
            marker.transform.localPosition = TimeLineMarkerZeroing.localPosition + new Vector3(unitLocalX, 0, 0);
            CheckToken(marker.transform, j, markerCount);
            marker.GetComponent<TimeLineMarker>().UseMajorMark = (j == 0);
        }

        void InitTimeLine()
        {
            TimeLineMarkerRoot.localPosition = Vector3.zero;
            TotalCount = 0;
            float x5 = 0;
            while (x5 <= TimeLineMarkerEntering.localPosition.x || TotalCount == 0)
            {
                int i = TotalCount / SubDivision;
                int j = TotalCount % SubDivision;
                x5 = UnitLength * i + (UnitLength / SubDivision) * (j);
                CreateMarker(i, j, TotalCount);
                TotalCount++;
            }
        }

        IEnumerator StepAnimation(Vector3 orgPos)
        {
            AnimationTimerOrigin = Time.time;
            while (AnimationLerper < 1.0f)
            {
                yield return 0;
                TimeLineMarkerRoot.transform.localPosition =
                    orgPos - new Vector3(UnitLength / SubDivision, 0, 0) * AnimationLerper;
            }

            TimeLineMarkerRoot.transform.localPosition = orgPos - new Vector3(UnitLength / SubDivision, 0, 0);
        }

        public void Step()
        {
            Vector3 orgPos = TimeLineMarkerRoot.transform.localPosition;
            StartCoroutine(StepAnimation(orgPos));
        }

        private void UpdateMarkerExistence(TimeLineMarker tm, ref int markerCount, in float markerRootX)
        {
            if (tm.transform.localPosition.x + markerRootX <= TimeLineMarkerExiting.localPosition.x)
                tm.PendingKill = true;
            else
                MarkerCount++;
        }

        private Tuple<int, int> UnrollMarker(int markerCount)
        {
            var i = markerCount / SubDivision;
            var j = markerCount % SubDivision;
            return new Tuple<int, int>(i, j);
        }

        void Update()
        {
            MarkerCount = 0;
            var markers = TimeLineMarkerRoot.GetComponentsInChildren<TimeLineMarker>();
            var markerRootX = TimeLineMarkerRoot.transform.localPosition.x;
            var minMarkerX = markers.Length != 0
                ? markers.Select(marker => marker.transform.localPosition.x).Min() + markerRootX
                : 0;

            markers.ForEach(marker => UpdateMarkerExistence(marker, ref MarkerCount, in markerRootX));
            MarkerCount++;

            var (i, j) = UnrollMarker(MarkerCount);
            var unitLocalPosX = UnitLength * i + (UnitLength / SubDivision) * j;
            var unitPosX = unitLocalPosX + minMarkerX;
            if (unitPosX <= TimeLineMarkerEntering.localPosition.x)
            {
                var (i1, j1) = UnrollMarker(TotalCount);
                CreateMarker(i1, j1, TotalCount);
                TotalCount++;
            }
        }

        public void InitWithAssets(GameAssets levelAsset)
        {
            _currentGameAsset = levelAsset;
            Debug.Assert(_currentGameAsset.StepCount == 0);
            TimeLineTokens = levelAsset.ActionAsset.TimeLineTokens;
            InitTimeLine();
        }

        void Awake()
        {
            RequirementSatisfied = false;
        }
    }
}