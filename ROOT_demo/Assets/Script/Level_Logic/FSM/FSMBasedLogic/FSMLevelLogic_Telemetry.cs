using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using com.ootii.Messages;
using ROOT.SetupAsset;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
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

        protected override void AddtionalMajorUpkeep()
        {
            if (CheckTelemetryStageInit())
            {
                TelemetryInit();
            }

            if (RoundLibDriver.IsBossRound && (bossType == BossStageType.Telemetry) && Animating)
            {
                LevelAsset.GameBoard.BoardGirdDriver.UpdateInfoZone(LevelAsset); //RISK 这里先放在这
            }
            else
            {
                WorldExecutor.CleanDestoryer(LevelAsset);
                //RISK 为了和商店同步，这里就先这样，但是可以检测只有购买后那一次才查一次。
                //总之稳了后，这个不能这么每帧调用。
                LevelAsset.occupiedHeatSink = LevelAsset.GameBoard.BoardGirdDriver.CheckHeatSink(RoundLibDriver.CurrentStage.Value);
                LevelAsset.GameBoard.BoardGirdDriver.UpdateInfoZone(LevelAsset); //RISK 这里先放在这
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
            base.AddtionalRecatIO();
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

        private void TelemetryInit()
        {
            var bossRoundData = LevelAsset.ActionAsset.BossSetup;
            var totalSprayCount = bossRoundData.BossLength * SprayCountPerAnimateInterval;
            //这个数据还得传过去。
            var targetInfoCount = Mathf.RoundToInt(bossRoundData.InfoCount * bossRoundData.InfoTargetRatio);

            SprayCountArray = Utils.SpreadOutLayingWRandomization(totalSprayCount, bossRoundData.InfoCount, bossRoundData.InfoVariantRatio);

            LevelAsset.DestroyerEnabled = true;
            WorldCycler.TelemetryStage = true;
            
            var signalInfo = new BoardSignalUpdatedInfo {SignalData = new BoardSignalUpdatedData()
            {
                InfoTarget = targetInfoCount,
                IsTelemetryStage=WorldCycler.TelemetryStage,//
            },};
            MessageDispatcher.SendMessage(signalInfo);
        }

        private void TelemetryPauseAction()
        {
            WorldExecutor.UpdateBoardData_Stepped(ref LevelAsset);
        }

        protected override void AdditionalInitLevel()
        {
            var message = new CurrencyUpdatedInfo()
            {
                CurrencyVal = Mathf.RoundToInt(LevelAsset.GameCurrencyMgr.Currency),
                IncomesVal = 0,
            };
            MessageDispatcher.SendMessage(message);
            
            if (LevelAsset.ActionAsset.RoundLib.Count > 0)
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
            LevelAsset.SkillMgr = FindObjectOfType<SkillMgr>();
            LevelAsset.CineCam = FindObjectOfType<CinemachineFreeLook>();
        }

        protected override void UpdateGameOverStatus()
        {
            //这个函数就很接近裁判要做的事儿了。
            if (!LevelAsset.ActionAsset.HasEnded(LevelAsset.StepCount)) return;
            PendingCleanUp = true;
            WorldCycler.Reset();
            //_actionDriver = null;
            LevelMasterManager.Instance.LevelFinished(LevelAsset);
        }

        protected override void BoardUpdatedHandler(IMessage rMessage)
        {
            base.BoardUpdatedHandler(rMessage);
            if (RoundLibDriver.CurrentRoundGist.HasValue)
            {
                WorldExecutor.UpdateRoundData_Instantly_Telemetry(ref LevelAsset);
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
        
        protected override void ModifiyRootFSMTransitions(ref HashSet<RootFSMTransition> RootFSMTransitions)
        {
            base.ModifiyRootFSMTransitions(ref RootFSMTransitions);
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