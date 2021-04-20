using System;
using System.Collections;
using ROOT.Common;
using ROOT.SetupAsset;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace ROOT
{
    //核心逻辑想了一下、估计意外地没办法优化了。现在Timeline的主体是要做开启和关闭的调整；
    //以及Timeline的重置、关闭Token、重置Token、调整Token等等管理性Feature。
    public class TimeLine : MonoBehaviour
    {
        public TimeLineGoalMarker GoalMarker;
        private GameAssets _currentGameAsset;

        public Transform DisabledCover;

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

        private float AnimationLerper
        {
            get
            {
                var res = animationTimer / FSMLevelLogic.AnimationDuration;
                res = Mathf.Clamp01(res);
                return Utils.EaseInOutCubic(res);
            }
        }

        private TimeLineStatus _currentStatus;
        public TimeLineStatus CurrentStatus
        {
            get => _currentStatus;
            set
            {
                _currentStatus = value;
                UpdateTimeLineByStatus();
            }
        }
        private void UpdateTimeLineByStatus()
        {
            switch (_currentStatus)
            {
                case TimeLineStatus.Normal:
                    DisabledCover.gameObject.SetActive(false);
                    UpdateMarkerToHideToken(false);
                    break;
                case TimeLineStatus.NoToken:
                    DisabledCover.gameObject.SetActive(false);
                    UpdateMarkerToHideToken(true);
                    break;
                case TimeLineStatus.Disabled:
                    DisabledCover.gameObject.SetActive(true);
                    UpdateMarkerToHideToken(false);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void UpdateMarkerToHideToken(bool Hide)
        {
            timeLineMarkers.ForEach(t => t.SetHide = Hide);
        }
        
        private TimeLineMarker[] timeLineMarkers => TimeLineMarkerRoot.GetComponentsInChildren<TimeLineMarker>();

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
        
        //private RoundLib _roundLib;
        private bool HasHeatsinkSwitch = false;

        private void CheckToken(TimeLineMarker marker, int j, int markerID)
        {
            var roundGist = new RoundGist();
            if (_currentGameAsset.ActionAsset.HasEnded(markerID))
            {
                roundGist.Type = StageType.Ending;
            }
            else
            {
                roundGist =_currentGameAsset.ActionAsset.GetCurrentRoundGist(markerID);
                var truncatedCount=_currentGameAsset.ActionAsset.GetTruncatedStep(markerID);
                HasHeatsinkSwitch = roundGist.SwitchHeatsink(truncatedCount);
            }

            var token = Instantiate(TimeLineTokenTemplate, marker.transform);
            var tokenS = token.GetComponent<TimeLineTokenQuad>();
            marker.Token = tokenS;
            tokenS.owner = this;
            tokenS.InitQuadShape(UnitLength, SubDivision, roundGist, HasHeatsinkSwitch);
            tokenS.MarkerID = markerID;
            if (_currentStatus == TimeLineStatus.NoToken)
            {
                //新建的Token要在这儿设置。
                tokenS.SetHideToken = true;
            }
        }

        private void CreateMarker(int placeID, int markerID)
        {
            var marker = Instantiate(TimeLineMarkerTemplate, TimeLineMarkerRoot);
            var unitLocalX = (UnitLength / SubDivision) * (placeID);
            marker.transform.localPosition = TimeLineMarkerZeroing.localPosition + new Vector3(unitLocalX, 0, 0);
            var markerS = marker.GetComponent<TimeLineMarker>();
            CheckToken(markerS, placeID, markerID);
            markerS.UseMajorMark = (markerID % SubDivision == 0);
        }

        void UpdateTimeLine()
        {
            #region NOTE
            //想和时序无关的话，就只能做这种全clear类似的逻辑。
            //想做帧间更新的逻辑就只能做之前那种时间耦合很强的。
            foreach (Transform child in TimeLineMarkerRoot.transform)
            {
                Destroy(child.gameObject);
            }

            #endregion

            for (var i = -(int) HeadingCount; i < TailingCount; i++)
            {
                var placeID = i;
                var markerID = StepCount + placeID;
                if (markerID >= 0)
                {
                    CreateMarker(placeID, markerID);
                }
            }
        }

        IEnumerator Animate(bool Forward)
        {
            AnimationTimerOrigin = Time.time;
            while (AnimationLerper < 1.0f)
            {
                yield return 0;
                TimeLineMarkerRoot.transform.localPosition = (Forward ? -1 : 1) * new Vector3(UnitLength / SubDivision, 0, 0) * AnimationLerper;
            }

            TimeLineMarkerRoot.transform.localPosition = Vector3.zero;
            UpdateTimeLine();
        }

        private int counter = 0;
        
        public void Step()
        {
            StartCoroutine(Animate(true));
        }

        public void Reverse()
        {
            StartCoroutine(Animate(false));
        }

        private uint HeadingCount = 2;
        private uint TailingCount = 9;

        public void InitWithAssets(GameAssets levelAsset)
        {
            _currentGameAsset = levelAsset;
            Debug.Assert(_currentGameAsset.StepCount == 0);
            //_roundLib = levelAsset.ActionAsset.RoundLibVal;
            UpdateTimeLine();
        }

        void Awake()
        {
            RequirementSatisfied = false;
        }
    }
}