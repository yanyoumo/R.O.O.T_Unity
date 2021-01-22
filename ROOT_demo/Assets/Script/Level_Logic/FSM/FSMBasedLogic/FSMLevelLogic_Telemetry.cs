using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ROOT
{
    using FSMActions = Dictionary<RootFSMStatus, Action>;
    using Trans = RootFSMTransition;
    using FSMTransitions = HashSet<RootFSMTransition>;
    using Status = RootFSMStatus;

    public class FSMLevelLogic_Telemetry : FSMLevelLogic //LEVEL-LOGIC/每一关都有一个这个类。
    {
        #region TelemetryStage

        private static float _TelemetryPauseCostTimer = 0.0f;
        private const float TelemetryPauseCostInterval = 1.0f;
        private const int TelemetryPricePerInterval = 1;

        private void TelemetryPauseRunStop()
        {
            Debug.Log("BossStagePauseRunStop");
            WorldCycler.TelemetryStagePause = false;
            StopTelemetryCost();
        }

        private IEnumerator TelemetryPauseCost()
        {
            yield return 0;
            while (true)
            {
                yield return 0;
                _TelemetryPauseCostTimer += Time.deltaTime;

                if (!(_TelemetryPauseCostTimer >= TelemetryPauseCostInterval)) continue;
                _TelemetryPauseCostTimer = 0.0f;
                LevelAsset.ReqOkCount -= TelemetryPricePerInterval;

                if (LevelAsset.ReqOkCount > 0) continue;
                TelemetryPauseRunStop();
                yield break;
            }
        }

        private Coroutine TelemetryPauseCostCo { set; get; }
        public void StartTelemetryCost()
        {
            TelemetryPauseCostCo=StartCoroutine(TelemetryPauseCost());
        }

        private void StopTelemetryCost()
        {
            StopCoroutine(TelemetryPauseCostCo);
            TelemetryPauseCostCo = null;
        }

        #endregion

        #region TelemetryTransit

        private bool CheckTelemetryStageInit()
        {
            return (Stage == StageType.Boss)&&(!WorldCycler.TelemetryStage);
        }

        private bool CheckTelemetryStage()
        {
            return (Stage == StageType.Boss);
        }

        private bool CheckTelemetryAndPaused()
        {
            return WorldCycler.TelemetryStage && WorldCycler.TelemetryStagePause;
        }

        private bool CheckTelemetryAndNotPaused()
        {
            return WorldCycler.TelemetryStage && !WorldCycler.TelemetryStagePause;
        }

        #endregion
        
        private void TelemetryMinorUpdate()
        {
            if (WorldCycler.TelemetryStagePause) return;
            _telemetryInfoSprayTimer += Time.deltaTime;
            if (_telemetryInfoSprayTimer >= _telemetryInfoSprayTimerInterval)
            {
                try
                {
                    LevelAsset.AirDrop.SprayInfo(SprayCountArray[SprayCounter]);
                }
                catch (IndexOutOfRangeException)
                {
                    LevelAsset.AirDrop.SprayInfo(3);
                }
                catch (NullReferenceException)
                {
                    return;
                }

                _telemetryInfoSprayTimerIntervalOffset = Random.Range(
                    -TelemetryInfoSprayTimerIntervalOffsetRange,
                    TelemetryInfoSprayTimerIntervalOffsetRange);
                _telemetryInfoSprayTimer = 0.0f;
                SprayCounter++;
            }
        }

        protected override void AddtionalMajorUpkeep()
        {
            if (CheckTelemetryStageInit())
            {
                TelemetryInit();
            }
            if (Stage == StageType.Boss && Animating)
            {
                LevelAsset.GameBoard.UpdateInfoZone(LevelAsset); //RISK 这里先放在这
            }
            else
            {
                WorldExecutor.CleanDestoryer(LevelAsset);
                //RISK 为了和商店同步，这里就先这样，但是可以检测只有购买后那一次才查一次。
                //总之稳了后，这个不能这么每帧调用。
                LevelAsset.occupiedHeatSink = LevelAsset.GameBoard.CheckHeatSink(Stage);
                LevelAsset.GameBoard.UpdateInfoZone(LevelAsset); //RISK 这里先放在这
                if (LevelAsset.SkillEnabled)
                {
                    LevelAsset.SkillMgr.UpKeepSkill(LevelAsset);
                }
            }
        }

        protected override void AddtionalMinorUpkeep()
        {
            if (CheckTelemetryStage())
            {
                TelemetryMinorUpdate();
            }
        }

        protected override void AddtionalRecatIO()
        {
            AddtionalRecatIO_Skill();
            AddtionalRecatIO_Telemetry();
        }

        private void AddtionalRecatIO_Skill()
        {
            if (LevelAsset.SkillEnabled)
            {
                LevelAsset.SkillMgr.TriggerSkill(LevelAsset, _ctrlPack);
            }
        }
        
        #region TelemetryRelated
        private void AddtionalRecatIO_Telemetry()
        {
            if (_ctrlPack.HasFlag(ControllingCommand.TelemetryResume) && CheckTelemetryAndPaused())
            {
                TelemetryPauseTriggered();
            }
        }

        private void TelemetryPauseTriggered()
        {
            if (LevelAsset.ReqOkCount <= 0) return;
            if (WorldCycler.TelemetryStagePause)
            {
                WorldCycler.TelemetryStagePause = false;
                StopTelemetryCost();
            }
            else
            {
                WorldCycler.TelemetryStagePause = true;
                StartTelemetryCost();
            }
            LevelAsset.SignalPanel.TelemetryPaused = WorldCycler.TelemetryStagePause;
        }
        private void DealTelemetryPauseBreaking()
        {
            if (CheckTelemetryAndNotPaused())
            {
                TelemetryPauseTriggered();
            }
        }
        
        //现在一共提供Info的计数是：Boss阶段*BossInfoSprayCount*SprayCountPerAnimateInterval;
        private const int SprayCountPerAnimateInterval = 4;
        private const float TelemetryInfoSprayTimerIntervalOffsetRange = 0.5f;

        private float _telemetryInfoSprayTimerIntervalBase => AnimationDuration / SprayCountPerAnimateInterval;
        private float _telemetryInfoSprayTimerInterval => _telemetryInfoSprayTimerIntervalBase + _telemetryInfoSprayTimerIntervalOffset; 

        private float _telemetryInfoSprayTimerIntervalOffset = 0.0f;
        private float _telemetryInfoSprayTimer = 0.0f;

        private int[] SprayCountArray;
        private int SprayCounter = 0;

        private void TelemetryInit()
        {
            var bossStageCount = LevelAsset.ActionAsset.BossStageCount;
            var totalSprayCount = bossStageCount * SprayCountPerAnimateInterval;
            //这个数据还得传过去。
            var targetInfoCount =
                Mathf.RoundToInt(LevelAsset.ActionAsset.InfoCount * LevelAsset.ActionAsset.InfoTargetRatio);
            LevelAsset.SignalPanel.SignalTarget = targetInfoCount;

            SprayCountArray = Utils.SpreadOutLayingWRandomization(totalSprayCount, LevelAsset.ActionAsset.InfoCount,
                LevelAsset.ActionAsset.InfoVariantRatio);

            LevelAsset.DestroyerEnabled = true;
            LevelAsset.SignalPanel.IsTelemetryStage = true;
            //FSM状态下，这个东西不用了。
            //ManualListenBossPauseKeyCoroutine = StartCoroutine(ManualPollingBossPauseKey());
            WorldCycler.TelemetryStage = true;
        }

        private void TelemetryPauseAction()
        {
            WorldExecutor.UpdateBoardData(ref LevelAsset);
        }

        protected override void AdditionalInitLevel()
        {
            if (LevelAsset.CostChart != null)
            {
                LevelAsset.CostChart.CurrencyVal = Mathf.RoundToInt(LevelAsset.GameStateMgr.GetCurrency());
            }
            if (LevelAsset.ActionAsset.RoundDatas.Length > 0)
            {
                //这个东西放在这里还是怎么着？就先这样吧。
                WorldCycler.InitCycler();
                if (LevelAsset.TimeLine != null)
                {
                    LevelAsset.TimeLine.InitWithAssets(LevelAsset);
                }
            }
        }

        #endregion
        
        protected override void AdditionalArtLevelReference(ref GameAssets LevelAsset)
        {
            LevelAsset.TimeLine = FindObjectOfType<TimeLine>();
            LevelAsset.CostLine = FindObjectOfType<CostLine>();
            LevelAsset.SkillMgr = FindObjectOfType<SkillMgr>();
            LevelAsset.CostChart = FindObjectOfType<CostChart>();
            LevelAsset.SignalPanel = FindObjectOfType<SignalPanel>();
            LevelAsset.CineCam = FindObjectOfType<CinemachineFreeLook>();
        }
        
        protected override FSMTransitions RootFSMTransitions
        {
            get
            {
                var transitions = new FSMTransitions
                {
                    new Trans(Status.PreInit, Status.MajorUpKeep, 1, CheckInited),
                    new Trans(Status.PreInit),
                    new Trans(Status.MajorUpKeep, Status.F_Cycle, 4, CheckTelemetryAndNotPaused),
                    new Trans(Status.MajorUpKeep, Status.R_Cycle, 3, CheckAutoR),
                    new Trans(Status.MajorUpKeep, Status.F_Cycle, 2, CheckAutoF),
                    new Trans(Status.MajorUpKeep, Status.R_IO, 1, CheckCtrlPackAny),
                    new Trans(Status.MajorUpKeep),
                    new Trans(Status.R_IO,Status.TelemetryPause,4,CheckTelemetryAndPaused),
                    new Trans(Status.R_IO, Status.Skill, 3, CheckIsSkill),
                    new Trans(Status.R_IO, Status.F_Cycle, 2, CheckFCycle),
                    new Trans(Status.R_IO, Status.Animate, 1, CheckStartAnimate, TriggerAnimation),
                    new Trans(Status.R_IO, Status.MajorUpKeep, 0, true),
                    new Trans(Status.TelemetryPause,Status.Career_Cycle),
                    new Trans(Status.F_Cycle, Status.Career_Cycle),
                    new Trans(Status.R_Cycle, Status.Career_Cycle),
                    new Trans(Status.Skill, Status.Career_Cycle),
                    new Trans(Status.Career_Cycle, Status.Animate, 1, CheckStartAnimate, TriggerAnimation),
                    new Trans(Status.Career_Cycle, Status.MinorUpKeep),
                    new Trans(Status.MinorUpKeep, Status.Animate, 1, true, CheckLoopAnimate),
                    new Trans(Status.MinorUpKeep, Status.CleanUp),
                    new Trans(Status.Animate, Status.MinorUpKeep),
                    new Trans(Status.CleanUp, Status.MajorUpKeep, 0, true),
                };
                return transitions;
            }
        }

        protected override Dictionary<BreakingCommand, Action> RootFSMBreakings
        {
            get
            {
                var breakings = new Dictionary<BreakingCommand, Action>
                {
                    {BreakingCommand.TelemetryPause, DealTelemetryPauseBreaking},
                };
                return breakings;
            }
        }
        
        protected override FSMActions fsmActions
        {
            get
            {
                //可能需要一个“整理节点（空节点）”这种概念的东西。
                var _fsmActions = new FSMActions
                {
                    {Status.PreInit, PreInit},
                    {Status.MajorUpKeep, MajorUpkeepAction},
                    {Status.MinorUpKeep, MinorUpKeepAction},
                    {Status.F_Cycle, ForwardCycle},
                    {Status.R_Cycle, ReverseCycle},
                    {Status.Career_Cycle, CareerCycle},
                    {Status.CleanUp, CleanUp},
                    {Status.TelemetryPause, TelemetryPauseAction},
                    {Status.Animate, AnimateAction},
                    {Status.R_IO, ReactIO},
                    {Status.Skill, SkillMajorUpkeep},
                };
                return _fsmActions;
            }
        }
    }
}