using System;
using System.Collections;
using com.ootii.Messages;
using DG.Tweening;
using ROOT.Common;
using ROOT.SetupAsset;
using Sirenix.Utilities;
using UnityEngine;

namespace ROOT
{
    //煊煊的需求派生出如下几个实际feature：
    //1、MasterClock.Instance里面的Step数据可以重置、视在Step通过offset重置。
    //2、Timeline是否Disable
    //3、Timeline是否现实Token
    //4、Career及以上FSM是否处理round的事件。
    
    //前三个化为TutorialAction暴露给外面。
    //对于第四个需求、默认看是用一个和tutorialVer的Bool。但是、现在突然想到，有没有可能给FSM不同的层级加个以Tag分类的开关系统。
    //通过这些Tag挂在一个bool来整建制开关FSM的转移；并且FSM转移要通过Tag系统标记。

    //核心逻辑想了一下、估计意外地没办法优化了。现在Timeline的主体是要做开启和关闭的调整；
    //以及Timeline的重置、关闭Token、重置Token、调整Token等等管理性Feature。
    
    //下一个要搞的Featrue是吧Time的表现层和数据层分开、虽然数据层只有一个Step、但是尽量也和marker和token部分分开。
    //还有就是Token部分的更新。
    public class TimeLine : MonoBehaviour
    {
        public TimeLineGoalMarker GoalMarker;
        //private GameAssets _currentGameAsset;
        private RoundLibDriver _currentRoundLibDriver;

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

        public int StepCount => _currentRoundLibDriver.StepCount;
        
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
                    UpdateMarkerToHideToken(true);
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

        private void CreateToken(TimeLineMarker marker, int markerID)
        {
            var roundGist = new RoundGist();
            if (_currentRoundLibDriver.HasEnded(markerID))
            {
                roundGist.Type = StageType.Ending;
            }
            else
            {
                roundGist = _currentRoundLibDriver.GetCurrentRoundGist(markerID);
                var truncatedCount = _currentRoundLibDriver.GetTruncatedStep(markerID);
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
            CreateToken(markerS, markerID);
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

        private void Animate(bool Forward)
        {
            TimeLineMarkerRoot.transform
                .DOLocalMoveX((Forward ? -1 : 1) * UnitLength / SubDivision, FSMLevelLogic.AnimationDuration)
                .OnComplete(AnimationComplete);
        }
        
        private void AnimationComplete()
        {
            TimeLineMarkerRoot.transform.localPosition = Vector3.zero;
            UpdateTimeLine();
        }

        private int counter = 0;
        
        public void Step()
        {
            Animate(true);
        }

        public void Reverse()
        {
            Animate(false);
        }

        private uint HeadingCount = 2;
        private uint TailingCount = 9;

        public void InitWithAssets(RoundLibDriver currentRoundLibDriver)
        {
            _currentRoundLibDriver = currentRoundLibDriver;
            UpdateTimeLine();
        }

        private void ApparentStepResetedHandler(IMessage rMessage)
        {
            //目前是瞬间变过去、就先这样吧。
            UpdateTimeLine();
        }
        
        private void Awake()
        {
            RequirementSatisfied = false;
            MessageDispatcher.AddListener(WorldEvent.ApparentStepResetedEvent,ApparentStepResetedHandler);
        }

        private void OnDestroy()
        {
            MessageDispatcher.RemoveListener(WorldEvent.ApparentStepResetedEvent,ApparentStepResetedHandler);
        }
    }
}