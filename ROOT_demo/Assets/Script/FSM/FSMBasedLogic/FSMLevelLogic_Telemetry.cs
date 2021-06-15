using System;
using System.Collections;
using System.Collections.Generic;
using com.ootii.Messages;
using I2.Loc;
using ROOT.Common;
using ROOT.SetupAsset;
using ROOT.UI;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ROOT
{
    using FSMActions = Dictionary<RootFSMStatus, Action>;
    using FSMTransitions = HashSet<RootFSMTransition>;
    using Trans = RootFSMTransition;
    using Status = RootFSMStatus;

    public class FSMLevelLogic_Telemetry : FSMLevelLogic_Career //LEVEL-LOGIC/每一关都有一个这个类。
    {
        protected override string SucceedEndingTerm => ScriptTerms.EndingMessageTelemetry_Successed;
        protected override string FailedEndingTerm => ScriptTerms.EndingMessageTelemetry_Failed;
        public override bool CouldHandleBoss => true;
        public override BossStageType HandleBossType => BossStageType.Telemetry;
        
        #region TelemetryStage

        private static float _TelemetryPauseCostTimer = 0.0f;
        private const float TelemetryPauseCostInterval = 1.0f;
        private const int TelemetryPricePerInterval = 1;

        private void TelemetryPauseRunStop()
        {
            Debug.Log("BossStagePauseRunStop");
            WorldCycler.TelemetryPause = false;
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

        //TODO
        private BossStageType bossType = BossStageType.Telemetry;

        private bool CheckTelemetryStageInit()
        {
            return RoundLibDriver.IsBossRound && (bossType == BossStageType.Telemetry) && (!WorldCycler.TelemetryStage);
        }

        private bool CheckTelemetryStage()
        {
            return RoundLibDriver.IsBossRound&&(bossType == BossStageType.Telemetry);
        }

        private bool CheckTelemetryAndPaused()
        {
            return WorldCycler.TelemetryStage && WorldCycler.TelemetryPause;
        }

        private bool CheckTelemetryAndNotPaused()
        {
            return WorldCycler.TelemetryStage && !WorldCycler.TelemetryPause;
        }

        #endregion
        
        private void UpdateSignalReq(RoundGist roundGist)
        {
            var normalRval = roundGist.normalReq;
            var networkRval = roundGist.networkReq;
            var noRequirement = (normalRval == 0 && networkRval == 0);
            if (noRequirement)
            {
                LevelAsset.TimeLine.RequirementSatisfied = true;
            }
            else
            {
                var signalInfo = new BoardSignalUpdatedInfo
                {
                    SignalData = new BoardSignalUpdatedData()
                    {
                        TgtTypeASignal = normalRval,
                        TgtTypeBSignal = networkRval,
                    },
                };
                MessageDispatcher.SendMessage(signalInfo);

                if (LevelAsset.TimeLine.RequirementSatisfied && roundGist.Type == StageType.Require)
                {
                    LevelAsset.ReqOkCount++;
                }
            }
        }

        
        protected override void UpdateLevelAsset()
        {
            base.UpdateLevelAsset();
            if ((LevelAsset.DestroyerEnabled && !RoundLibDriver.IsDestoryerRound) && !WorldCycler.TelemetryStage)
            {
                LevelAsset.WarningDestoryer.ForceReset();
            }
        }

        protected override void UpdateRoundData_Stepped()
        {
            base.UpdateRoundData_Stepped();
            LevelAsset.DestroyerEnabled = WorldCycler.TelemetryStage;
            var roundGist = RoundLibDriver.CurrentRoundGist.Value;
            if (RoundLibDriver.IsRequireRound || RoundLibDriver.IsShopRound)
            {
                UpdateSignalReq(roundGist);
            }
        }

        private void TelemetryMinorUpdate()
        {
            if (WorldCycler.TelemetryPause) return;
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

        protected override void AdditionalMajorUpkeep()
        {
            base.AdditionalMajorUpkeep();
            if (CheckTelemetryStageInit())
            {
                TelemetryInit();
            }

            if (RoundLibDriver.IsBossRound && (bossType == BossStageType.Telemetry) && Animating)
            {
                LevelAsset.GameBoard.BoardGirdDriver.UpdateInfoZone(LevelAsset.GameBoard.GetInfoCollectorZone()); //RISK 这里先放在这
            }
            else
            {
                WorldExecutor.CleanDestoryer(LevelAsset);
                //RISK 为了和商店同步，这里就先这样，但是可以检测只有购买后那一次才查一次。
                //总之稳了后，这个不能这么每帧调用。
                LevelAsset.GameBoard.BoardGirdDriver.UpkeepHeatSink(RoundLibDriver.CurrentStage.Value);
                LevelAsset.GameBoard.BoardGirdDriver.CheckOverlappedHeatSinkCount(out LevelAsset.occupiedHeatSinkCount);
                LevelAsset.GameBoard.BoardGirdDriver.UpdateInfoZone(LevelAsset.GameBoard.GetInfoCollectorZone()); //RISK 这里先放在这
                if (LevelAsset.SkillEnabled)
                {
                    LevelAsset.SkillMgr.UpKeepSkill(LevelAsset);
                }
            }
        }

        protected override void AdditionalMinorUpkeep()
        {
            base.AdditionalMinorUpkeep();
            if (CheckTelemetryStage())
            {
                TelemetryMinorUpdate();
            }
        }

        protected override void AdditionalReactIO()
        {
            base.AdditionalReactIO();
            AddtionalRecatIO_Telemetry();
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
            if (WorldCycler.TelemetryPause)
            {
                WorldCycler.TelemetryPause = false;
                StopTelemetryCost();
            }
            else
            {
                WorldCycler.TelemetryPause = true;
                StartTelemetryCost();
            }
            var signalInfo = new BoardSignalUpdatedInfo {SignalData = new BoardSignalUpdatedData() {TelemetryPaused = WorldCycler.TelemetryPause},};
            MessageDispatcher.SendMessage(signalInfo);
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

        private BossAdditionalSetupAsset BossRoundData => LevelAsset.ActionAsset.BossSetup;
        private int TotalSprayCount => BossRoundData.BossLength * SprayCountPerAnimateInterval;
        private int TargetInfoCount => Mathf.RoundToInt(BossRoundData.InfoCount * BossRoundData.InfoTargetRatio);
        
        private void TelemetryInit()
        {
            SprayCountArray = Utils.SpreadOutLayingWRandomization(TotalSprayCount, BossRoundData.InfoCount, BossRoundData.InfoVariantRatio);

            LevelAsset.DestroyerEnabled = true;
            WorldCycler.TelemetryStage = true;
            
            var signalInfo = new BoardSignalUpdatedInfo {SignalData = new BoardSignalUpdatedData()
            {
                InfoTarget = TargetInfoCount,
                IsTelemetryStage=WorldCycler.TelemetryStage,//
            },};
            MessageDispatcher.SendMessage(signalInfo);
        }

        protected override void UpdateBoardData_Stepped(ref GameAssets currentLevelAsset)
        {
            base.UpdateBoardData_Stepped(ref currentLevelAsset);
            if (currentLevelAsset.TimeLine != null)
            {
                currentLevelAsset.TimeLine.SetCurrentCount = currentLevelAsset.ReqOkCount;
            }

            var signalInfo = new BoardSignalUpdatedInfo
                {SignalData = new BoardSignalUpdatedData {CrtMission = currentLevelAsset.ReqOkCount},};
            MessageDispatcher.SendMessage(signalInfo);
        }

        private void TelemetryPauseAction()
        {
            UpdateBoardData_Stepped(ref LevelAsset);
        }

        #endregion
        
        private void UpdateRoundData_Instantly_Telemetry()
        {
            var levelAsset = LevelAsset;
            var lvlLogic = this;
            var roundGist = lvlLogic.RoundLibDriver.CurrentRoundGist.Value;

            if (lvlLogic.RoundLibDriver.IsRequireRound || lvlLogic.RoundLibDriver.IsShopRound)
            {
                levelAsset.TimeLine.RequirementSatisfied = (TypeASignalCount >= roundGist.normalReq) &&
                                                           (TypeBSignalCount >= roundGist.networkReq);
            }
        }

        protected override void BoardUpdatedHandler(IMessage rMessage)
        {
            base.BoardUpdatedHandler(rMessage);
            if (RoundLibDriver.CurrentRoundGist.HasValue)
            {
                UpdateRoundData_Instantly_Telemetry();
            }
        }

        protected override void populateGameOverAsset(ref GameOverAsset _gameOverAsset)
        {
            base.populateGameOverAsset(ref _gameOverAsset);
            _gameOverAsset.ValueInt = TotalSprayCount;//TODO 这个东西有问题。没有在哪记录
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

        private bool displaySingleInfoZone = false;
        
        private void InGameOverLayToggleHandler(IMessage rMessage)
        {
            displaySingleInfoZone = !displaySingleInfoZone;
            var board = LevelAsset.GameBoard;
            var cursorPos = LevelAsset.Cursor.CurrentBoardPosition;
            board.BoardGirdDriver.ClearAllEdges(EdgeStatus.SingleInfoZone);
            if (displaySingleInfoZone)
            {
                if (board.CheckBoardPosValidAndFilled(cursorPos))
                {
                    var unit = board.FindUnitByPos(cursorPos);
                    Debug.Assert(unit != null, nameof(unit) + " != null");
                    board.BoardGirdDriver.UpdateSingleInfoZone(unit.SignalCore.SingleInfoCollectorZone);
                }
            }
        }
        
        protected override void Awake()
        {
            base.Awake();
            MessageDispatcher.AddListener(WorldEvent.InGameOverlayToggleEvent, InGameOverLayToggleHandler);
        }

        protected override void OnDestroy()
        {
            MessageDispatcher.AddListener(WorldEvent.InGameOverlayToggleEvent, InGameOverLayToggleHandler);
            base.OnDestroy();
        }

        protected override void ModifyRootFSMTransitions(ref RootFSMTranstionLib RootFSMTransitions)
        {
            base.ModifyRootFSMTransitions(ref RootFSMTransitions);
            RootFSMTransitions.Add(new Trans(Status.MajorUpKeep, Status.F_Cycle, 4, CheckTelemetryAndNotPaused));
            RootFSMTransitions.Add(new Trans(Status.R_IO, Status.TelemetryPause, 4, CheckTelemetryAndPaused));
            RootFSMTransitions.Add(new Trans(Status.TelemetryPause, Status.Career_Cycle));
        }
        
        protected override void ModifyFSMActions(ref Dictionary<RootFSMStatus, Action> actions)
        {
            base.ModifyFSMActions(ref actions);
            actions.Add(Status.TelemetryPause, TelemetryPauseAction);
        }
    }
}