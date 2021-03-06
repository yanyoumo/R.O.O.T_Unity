using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using com.ootii.Messages;
using I2.Loc;
using ROOT.Common;
using ROOT.Message;
using ROOT.SetupAsset;
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
        
        protected override IEnumerable<int> GamePlayHintPagesByLevelType
        {
            get
            {
                var res = base.GamePlayHintPagesByLevelType.ToList();
                return res.Append(8);
            }
        }
        
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

        //private BossStageType bossType = BossStageType.Telemetry;

        private bool CheckTelemetryStageInit()
        {
            return RoundLibDriver.IsBossRound && (!WorldCycler.TelemetryStage);
        }

        private bool CheckTelemetryStage()
        {
            return RoundLibDriver.IsBossRound;
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
                        TargetActivatedTierSumA = normalRval,
                        TargetActivatedTierSumB = networkRval,
                    },
                };
                MessageDispatcher.SendMessage(signalInfo);

                if (LevelAsset.TimeLine.RequirementSatisfied && roundGist.Type == StageType.Require)
                {
                    LevelAsset.ReqOkCount++;
                }
            }
        }

        private void HandleHeatSink_Telemetry()
        {
            if (RoundLibDriver.IsRequireRound && IsForwardCycle)
            {
                LevelAsset.GameBoard.BoardGirdDriver.UpcountHeatSinkStep();
            }

            if (DestroyerRoundEnding && !WorldCycler.NeedAutoDriveStep.HasValue && !RoundLibDriver.IsLastNormalRound(LevelAsset.StepCount))
            {
                LevelAsset.GameBoard.BoardGirdDriver.DestoryHeatsinkOverlappedUnit();
            }

            if (CurrentRoundGist.SwitchHeatsink(TruncatedStep))
            {
                LevelAsset.GameBoard.BoardGirdDriver.UpdatePatternID();
            }
        }

        protected override void UpdateRoundData_Stepped()
        {
            HandleRoundStretch();
            HandleHeatSink_Telemetry();
            HandleShopDiscount();
            CheckSkillMgr();
            
            LevelAsset.DestroyerEnabled = WorldCycler.TelemetryStage;
            if ((LevelAsset.DestroyerEnabled && !RoundLibDriver.IsDestoryerRound) && !WorldCycler.TelemetryStage)
            {
                LevelAsset.WarningDestoryer.ForceReset();
            }
            if (RoundLibDriver.IsRequireRound || RoundLibDriver.IsShopRound)
            {
                UpdateSignalReq(CurrentRoundGist);
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

        private void UpdateInfoZone(List<Vector2Int> collectorZone) => LevelAsset.GameBoard.BoardGirdDriver.UpdateInfoData(EdgeStatus.InfoZone,collectorZone);
        private void UpdateSingleInfoZone(List<Vector2Int> collectorZone) => LevelAsset.GameBoard.BoardGirdDriver.UpdateInfoData(EdgeStatus.SingleInfoZone,collectorZone);

        protected override void AdditionalMajorUpkeep()
        {
            if (CheckTelemetryStageInit())
            {
                TelemetryInit();
            }

            if (!RoundLibDriver.IsBossRound || !Animating)
            {
                WorldExecutor.CleanDestoryer(LevelAsset);
                LevelAsset.GameBoard.BoardGirdDriver.UpkeepHeatSink(RoundLibDriver.CurrentStage.Value);
                LevelAsset.GameBoard.BoardGirdDriver.CheckOverlappedHeatSinkCount(out LevelAsset.occupiedHeatSinkCount);
                if (LevelAsset.SkillEnabled && HandlingSkill)
                {
                    LevelAsset.SkillMgr.UpKeepSkill(LevelAsset);
                }
            }

            UpdateInfoZone(LevelAsset.GameBoard.GetInfoCollectorZone());
            RoundLockTutorialVerHandler();
        }

        protected override void AdditionalMinorUpkeep()
        {
            if (CheckTelemetryStage())
            {
                TelemetryMinorUpdate();
            }
        }

        protected override void AdditionalReactIO()
        {
            AddtionalReactIO_Skill();
            AddtionalReactIO_Telemetry();
        }

        #region TelemetryRelated
        private void AddtionalReactIO_Telemetry()
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
            var signalInfo = new BoardSignalUpdatedInfo {SignalData = new BoardSignalUpdatedData()
            {
                IsTelemetryStage = WorldCycler.TelemetryStage,
                TelemetryPaused = WorldCycler.TelemetryPause
            },};
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

        private int TargetInfoCount => Mathf.RoundToInt(BossRoundData.InfoCount * BossRoundData.InfoTargetRatio / 100.0f);

        private void TelemetryInit()
        {
            SprayCountArray = Utils.SpreadOutLayingWRandomization(TotalSprayCount, BossRoundData.InfoCount, BossRoundData.InfoVariantRatio);

            LevelAsset.DestroyerEnabled = true;
            WorldCycler.TelemetryStage = true;

            var signalInfo = new BoardSignalUpdatedInfo
            {
                SignalData = new BoardSignalUpdatedData()
                {
                    InfoTarget = TargetInfoCount,
                    IsTelemetryStage = WorldCycler.TelemetryStage, //
                },
            };
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

        protected override void BoardUpdatedCallBack()
        {
            base.BoardUpdatedCallBack();
            if (RoundLibDriver.CurrentRoundGist.HasValue)
            {
                UpdateRoundData_Instantly_Telemetry();
            }
        }

        protected override void populateGameOverAsset(ref GameOverAsset _gameOverAsset)
        {
            base.populateGameOverAsset(ref _gameOverAsset);
            _gameOverAsset.ValueFloat = TotalSprayCount;//TODO 这个东西有问题。没有在哪记录
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
            if (rMessage.Type==WorldEvent.InGameOverlayToggleEvent)
            {
                displaySingleInfoZone = !displaySingleInfoZone;
            }
            var board = LevelAsset.GameBoard;
            var cursorPos = LevelAsset.Cursor.CurrentBoardPosition;
            board.BoardGirdDriver.ClearAllEdges(EdgeStatus.SingleInfoZone);
            if (displaySingleInfoZone && board.CheckBoardPosValidAndFilled(cursorPos))
            {
                var unit = board.FindUnitByPos(cursorPos);
                Debug.Assert(unit != null, nameof(unit) + " != null");
                if (unit.SignalCore.IsUnitActive && unit.UnitHardware == HardwareType.Field)
                {
                    UpdateSingleInfoZone(unit.SignalCore.SingleInfoCollectorZone);
                }
            }
        }
        
        protected override void Awake()
        {
            base.Awake();
            MessageDispatcher.AddListener(WorldEvent.InGameOverlayToggleEvent, InGameOverLayToggleHandler);
            MessageDispatcher.AddListener(WorldEvent.CursorMovedEvent, InGameOverLayToggleHandler);
            if (UseTutorialVer)
            {
                FeatureManager.RegistFSMFeature(FSMFeatures.Telemetry,new []{FSMFeatures.Shop,FSMFeatures.Currency,FSMFeatures.Round}, false);
            }
        }

        protected override void OnDestroy()
        {
            MessageDispatcher.RemoveListener(WorldEvent.CursorMovedEvent, InGameOverLayToggleHandler);
            MessageDispatcher.RemoveListener(WorldEvent.InGameOverlayToggleEvent, InGameOverLayToggleHandler);
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