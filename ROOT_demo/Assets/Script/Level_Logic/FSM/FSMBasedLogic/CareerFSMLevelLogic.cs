using System;
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

    public class CareerFSMLevelLogic : LevelLogic //LEVEL-LOGIC/每一关都有一个这个类。
    {
        private void BossMinorUpdate()
        {
            if (WorldCycler.BossStagePause) return;
            _bossInfoSprayTimer += Time.deltaTime;
            if (_bossInfoSprayTimer >= _bossInfoSprayTimerInterval)
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

                _bossInfoSprayTimerIntervalOffset = Random.Range(
                    -BossInfoSprayTimerIntervalOffsetRange,
                    BossInfoSprayTimerIntervalOffsetRange);
                _bossInfoSprayTimer = 0.0f;
                SprayCounter++;
            }
        }

        protected override void AddtionalMajorUpkeep()
        {
            if (CheckBossStageInit())
            {
                BossInit();
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
            if (CheckBossStage())
            {
                BossMinorUpdate();
            }
        }

        protected override void AddtionalRecatIO()
        {
            AddtionalRecatIO_Skill();
            AddtionalRecatIO_Boss();
        }

        private void AddtionalRecatIO_Skill()
        {
            if (LevelAsset.SkillEnabled)
            {
                LevelAsset.SkillMgr.TriggerSkill(LevelAsset, _ctrlPack);
            }
        }
        
        #region BossRelated
        private void AddtionalRecatIO_Boss()
        {
            if (_ctrlPack.HasFlag(ControllingCommand.BossResume) && CheckBossAndPaused())
            {
                BossStagePauseTriggered();
            }
        }

        private void BossStagePauseTriggered()
        {
            if (LevelAsset.ReqOkCount <= 0) return;
            if (WorldCycler.BossStagePause)
            {
                WorldCycler.BossStagePause = false;
                LevelAsset.Owner.StopBossStageCost();
            }
            else
            {
                WorldCycler.BossStagePause = true;
                LevelAsset.Owner.StartBossStageCost();
            }
            LevelAsset.SignalPanel.BossStagePaused = WorldCycler.BossStagePause;
        }
        private void DealBossPauseBreaking()
        {
            if (CheckBossAndNotPaused())
            {
                BossStagePauseTriggered();
            }
        }
        
        //现在一共提供Info的计数是：Boss阶段*BossInfoSprayCount*SprayCountPerAnimateInterval;
        private const int SprayCountPerAnimateInterval = 4;
        private const float BossInfoSprayTimerIntervalOffsetRange = 0.5f;

        private float _bossInfoSprayTimerIntervalBase => AnimationDuration / SprayCountPerAnimateInterval;
        private float _bossInfoSprayTimerInterval => _bossInfoSprayTimerIntervalBase + _bossInfoSprayTimerIntervalOffset; 

        private float _bossInfoSprayTimerIntervalOffset = 0.0f;
        private float _bossInfoSprayTimer = 0.0f;
        //private Coroutine ManualListenBossPauseKeyCoroutine;

        private int[] SprayCountArray;
        private int SprayCounter = 0;

        private void BossInit()
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
            LevelAsset.SignalPanel.IsBossStage = true;
            //FSM状态下，这个东西不用了。
            //ManualListenBossPauseKeyCoroutine = StartCoroutine(ManualPollingBossPauseKey());
            WorldCycler.BossStage = true;
        }

        private void BossPauseAction()
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
                    new Trans(Status.PreInit, Status.F_Cycle, 1, CheckInited),
                    new Trans(Status.PreInit),
                    new Trans(Status.MajorUpKeep, Status.F_Cycle, 4, CheckBossAndNotPaused),
                    new Trans(Status.MajorUpKeep, Status.R_Cycle, 3, CheckAutoR),
                    new Trans(Status.MajorUpKeep, Status.F_Cycle, 2, CheckAutoF),
                    new Trans(Status.MajorUpKeep, Status.R_IO, 1, CheckCtrlPackAny),
                    new Trans(Status.MajorUpKeep),
                    new Trans(Status.R_IO,Status.BossPause,4,CheckBossAndPaused),
                    new Trans(Status.R_IO, Status.Skill, 3, CheckIsSkill),
                    new Trans(Status.R_IO, Status.F_Cycle, 2, CheckFCycle),
                    new Trans(Status.R_IO, Status.Animate, 1, CheckStartAnimate, TriggerAnimation),
                    new Trans(Status.R_IO, Status.MajorUpKeep, 0, true),
                    new Trans(Status.BossPause,Status.Career_Cycle),
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
                    {BreakingCommand.BossPause, DealBossPauseBreaking},
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
                    {Status.BossPause, BossPauseAction},
                    {Status.Animate, AnimateAction},
                    {Status.R_IO, ReactIO},
                    {Status.Skill, SkillMajorUpkeep},
                };
                return _fsmActions;
            }
        }
    }
}