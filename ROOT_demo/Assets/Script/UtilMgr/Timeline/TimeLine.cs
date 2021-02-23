using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ROOT.SetupAsset;
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
        HeatSinkSwitch = 4,//ICON
        ShopOpened = 5,//
        BossStage = 6,//Purple
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

        private float AnimationLerper
        {
            get
            {
                float res = animationTimer / FSMLevelLogic.AnimationDuration;
                res = Mathf.Clamp01(res);
                return Utils.EaseInOutCubic(res);
            }
        }

        private RoundLib _roundLib;
        private bool HasHeatsinkSwitch = false;

        void CheckToken(Transform MarkRoot, int j, int markerID)
        {
            RoundGist roundGist = new RoundGist();
            //BUG 这里会出一个Exception，但是似乎不影响运行。
            if (_currentGameAsset.ActionAsset.HasEnded(markerID))
            {
                roundGist.Type = StageType.Ending;
            }
            else
            {
                /*var truncatedCount = _currentGameAsset.ActionAsset.GetTruncatedCount(markerID, out var RoundCount);

                if (RoundCount >= RoundDatas.Length || RoundCount == -1)
                {
                    return;
                }

                RoundData round = RoundDatas[RoundCount];
                //这里的逻辑还是不太行。
                var stage = round.CheckStage(truncatedCount, RoundCount == RoundDatas.Length - 1);
                if (!stage.HasValue) return;*/

                roundGist =_currentGameAsset.ActionAsset.RoundLibVal.GetCurrentRoundGist(markerID);
                var truncatedCount=_currentGameAsset.ActionAsset.RoundLibVal.GetTruncatedStep(markerID);
                HasHeatsinkSwitch = roundGist.SwitchHeatsink(truncatedCount);
            }

            var token = Instantiate(TimeLineTokenTemplate, MarkRoot);
            token.GetComponent<TimeLineTokenQuad>().owner = this;
            token.GetComponent<TimeLineTokenQuad>().InitQuadShape(UnitLength, SubDivision, roundGist, HasHeatsinkSwitch);
            token.GetComponent<TimeLineTokenQuad>().MarkerID = markerID;
        }

        void CreateMarker(int placeID, int markerID)
        {
            var marker = Instantiate(TimeLineMarkerTemplate, TimeLineMarkerRoot);
            var unitLocalX = (UnitLength / SubDivision) * (placeID);
            marker.transform.localPosition = TimeLineMarkerZeroing.localPosition + new Vector3(unitLocalX, 0, 0);
            CheckToken(marker.transform, placeID, markerID);
            marker.GetComponent<TimeLineMarker>().UseMajorMark = (markerID % SubDivision == 0);
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
            _roundLib = levelAsset.ActionAsset.RoundLibVal;
            UpdateTimeLine();
        }

        void Awake()
        {
            RequirementSatisfied = false;
        }
    }
}