using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using com.ootii.Messages;
using ROOT.SetupAsset;
using ROOT.Signal;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ROOT
{
    public class FSMLevelLogic_Career : FSMLevelLogic_Barebone
    {
        
        public override int LEVEL_ART_SCENE_ID => StaticName.SCENE_ID_ADDITIONAL_VISUAL_CAREER;

        protected RoundLibDriver RoundLibDriver;

        public override bool IsTutorial => false;
        public override bool CouldHandleSkill => true;
        public override bool CouldHandleBoss => false;
        public override BossStageType HandleBossType => throw new ArgumentException("could not handle Boss");

        protected override bool IsForwardCycle => AutoForward || movedTile;
        private bool AutoForward => (AutoDrive.HasValue && AutoDrive.Value);
        private bool IsReverseCycle => (AutoDrive.HasValue && !AutoDrive.Value);
        protected bool IsSkillAllowed => !RoundLibDriver.IsShopRound;
        protected bool BoardCouldIOCurrency => (RoundLibDriver.IsRequireRound || RoundLibDriver.IsDestoryerRound);
        
        private bool CheckIsSkill() => LevelAsset.SkillMgr != null && LevelAsset.SkillMgr.CurrentSkillType.HasValue &&
                                       LevelAsset.SkillMgr.CurrentSkillType.Value == SkillType.Swap;

        private bool CheckAutoF() => AutoDrive.HasValue && AutoDrive.Value;
        private bool CheckAutoR() => IsReverseCycle;

        protected virtual void UpdateLevelAsset()
        {
            var lastStage = RoundLibDriver.PreviousRoundGist?.Type ?? StageType.Shop;
            var lastDestoryBool = lastStage == StageType.Destoryer;

            if (RoundLibDriver.IsRequireRound && IsForwardCycle)
            {
                LevelAsset.GameBoard.BoardGirdDriver.UpdatePatternDiminishing();
            }

            if ((lastDestoryBool && !RoundLibDriver.IsDestoryerRound) && !WorldCycler.NeedAutoDriveStep.HasValue)
            {
                LevelAsset.GameBoard.BoardGirdDriver.DestoryHeatsinkOverlappedUnit();
            }
        }

        protected override void AdditionalArtLevelReference(ref GameAssets LevelAsset)
        {
            LevelAsset.TimeLine = FindObjectOfType<TimeLine>();
            LevelAsset.SkillMgr = FindObjectOfType<SkillMgr>();
            LevelAsset.CineCam = FindObjectOfType<CinemachineFreeLook>();
        }
        
        protected override void AdditionalMajorUpkeep()
        {
            base.AdditionalMajorUpkeep();
            LevelAsset.GameBoard.BoardGirdDriver.UpkeepHeatSink(RoundLibDriver.CurrentStage.Value);
            LevelAsset.GameBoard.BoardGirdDriver.CheckOverlappedHeatSinkCount(out LevelAsset.occupiedHeatSinkCount);
            if (LevelAsset.SkillEnabled)
            {
                LevelAsset.SkillMgr.UpKeepSkill(LevelAsset);
            }
        }
        
        protected override void AdditionalInitLevel()
        {
            base.AdditionalInitLevel();
            WorldExecutor.InitDestoryer(ref LevelAsset);
            LevelAsset.DestroyerEnabled = false;
            WorldExecutor.InitShop(ref LevelAsset);
            WorldExecutor.StartShop(ref LevelAsset);
            
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

        protected virtual void UpdateRoundData_Stepped()
        {
            var roundGist = RoundLibDriver.CurrentRoundGist.Value;
            var tCount = LevelAsset.ActionAsset.GetTruncatedStep(LevelAsset.StepCount);
            if (roundGist.SwitchHeatsink(tCount))
            {
                LevelAsset.GameBoard.BoardGirdDriver.UpdatePatternID();
            }

            UpdateLevelAsset();

            var discount = 0;

            if (!LevelAsset.Shop.ShopOpening && RoundLibDriver.IsShopRound)
            {
                discount = LevelAsset.SkillMgr.CheckDiscount();
            }

            LevelAsset.Shop.OpenShop(RoundLibDriver.IsShopRound, discount);
            LevelAsset.SkillMgr.SkillEnabled = LevelAsset.SkillEnabled = IsSkillAllowed;
        }
        
        private void AddtionalRecatIO_Skill()
        {
            if (LevelAsset.SkillEnabled)
            {
                if (LevelAsset.SkillMgr != null)
                {
                    LevelAsset.SkillMgr.TriggerSkill(LevelAsset, _ctrlPack);
                }
            }
        }

        protected override void AddtionalRecatIO() => AddtionalRecatIO_Skill();

        private StageType? lastStageType = null;

        private void CareerCycle()
        {
            if (LevelAsset.DestroyerEnabled)
            {
                WorldExecutor.UpdateDestoryer(LevelAsset);
                if (LevelAsset.WarningDestoryer != null)
                {
                    LevelAsset.WarningDestoryer.Step(out var outCore);
                    LevelAsset.DestoryedCoreType = outCore;
                }
            }

            if (RoundLibDriver.CurrentRoundGist.HasValue)
            {
                UpdateRoundData_Stepped();
                if (lastStageType == null || lastStageType.Value != RoundLibDriver.CurrentRoundGist.Value.Type)
                {
                    //RISK 这个变成每个时刻都改了、想着加一个Guard
                    MessageDispatcher.SendMessage(WorldEvent.BoardUpdatedEvent); //为了令使和Round相关的数据强制更新。
                    var timingEvent = new TimingEventInfo
                    {
                        Type = WorldEvent.InGameStageChangedEvent,
                        CurrentStageType = RoundLibDriver.CurrentRoundGist.Value.Type,
                    };
                    var timingEvent2 = new TimingEventInfo
                    {
                        Type = WorldEvent.CurrencyIOStatusChangedEvent,
                        BoardCouldIOCurrencyData = BoardCouldIOCurrency,
                        UnitCouldGenerateIncomeData = RoundLibDriver.IsRequireRound,
                    };
                    MessageDispatcher.SendMessage(timingEvent);
                    MessageDispatcher.SendMessage(timingEvent2);
                    lastStageType = RoundLibDriver.CurrentRoundGist.Value.Type;
                }
            }
        }

        protected override bool CheckGameOver
        {
            get
            {
                var res=LevelAsset.ActionAsset.HasEnded(LevelAsset.StepCount);
                if (res) WorldCycler.Reset();
                return res;
            }
        }

        protected virtual int GetInCome() => Mathf.RoundToInt((TypeASignalScore + TypeBSignalScore) * LevelAsset.CurrencyRebate);

        private int Cost=>LevelAsset.GameBoard.BoardGirdDriver.HeatSinkCost;
        
        protected override void UpdateBoardData_Instantly()
        {
            base.UpdateBoardData_Instantly();
            //在这里更新DeltaCurrency并没有错、严格来说是更新DeltaCurrency的Cache；
            //这个DeltaCurrency只有在Stepped的时刻才会计算到Currency里面。
            LevelAsset.DeltaCurrency = BoardCouldIOCurrency ? (RoundLibDriver.IsRequireRound ? GetInCome() : 0)- Cost : 0;
            SendCurrencyMessage();
        }
        
        private void ReverseCycle()
        {
            WorldCycler.StepDown();
            LevelAsset.TimeLine.Reverse();
        }

        private void InitCareer()
        {
            CareerCycle();
            _mainFSM.currentStatus = RootFSMStatus.MajorUpKeep;
            _mainFSM.waitForNextFrame = false;
        }
        
        protected override void ModifyFSMActions(ref Dictionary<RootFSMStatus, Action> actions)
        {
            base.ModifyFSMActions(ref actions);
            actions.Add(RootFSMStatus.R_Cycle, ReverseCycle);
            actions.Add(RootFSMStatus.Career_Cycle, CareerCycle);
            actions.Add(RootFSMStatus.Skill, SkillMajorUpkeep);
        }

        public override IEnumerator UpdateArtLevelReference(AsyncOperation baseVisualScene,AsyncOperation addtionalVisualScene)
        {
            while (!baseVisualScene.isDone||!addtionalVisualScene.isDone)
            {
                yield return 0;
            }
            LevelAsset.Shop = FindObjectOfType<ShopBase>();
            AdditionalArtLevelReference(ref LevelAsset);
            SendHintData(HintEventType.ShowTutorialTextFrame, false);
            PopulateArtLevelReference();
        }
        
        protected override void ModifiyRootFSMTransitions(ref HashSet<RootFSMTransition> RootFSMTransitions)
        {
            base.ModifiyRootFSMTransitions(ref RootFSMTransitions);
            RootFSMTransitions.Remove(new RootFSMTransition(RootFSMStatus.F_Cycle, RootFSMStatus.Animate, 1, CheckStartAnimate, TriggerAnimation));
            RootFSMTransitions.Remove(new RootFSMTransition(RootFSMStatus.F_Cycle, RootFSMStatus.MinorUpKeep));

            #region ADD Consequence

            RootFSMTransitions.Remove(new RootFSMTransition(RootFSMStatus.PreInit, RootFSMStatus.MajorUpKeep, 1, CheckInited));
            RootFSMTransitions.Add(new RootFSMTransition(RootFSMStatus.PreInit, RootFSMStatus.MajorUpKeep, 1, CheckInited, InitCareer));

            #endregion

            RootFSMTransitions.Add(new RootFSMTransition(RootFSMStatus.Skill, RootFSMStatus.Career_Cycle));
            RootFSMTransitions.Add(new RootFSMTransition(RootFSMStatus.F_Cycle, RootFSMStatus.Career_Cycle));
            RootFSMTransitions.Add(new RootFSMTransition(RootFSMStatus.R_IO, RootFSMStatus.Skill, 3, CheckIsSkill));
            RootFSMTransitions.Add(new RootFSMTransition(RootFSMStatus.Career_Cycle, RootFSMStatus.Animate, 1, CheckStartAnimate, TriggerAnimation));
            RootFSMTransitions.Add(new RootFSMTransition(RootFSMStatus.Career_Cycle, RootFSMStatus.MinorUpKeep));
            RootFSMTransitions.Add(new RootFSMTransition(RootFSMStatus.MajorUpKeep, RootFSMStatus.R_Cycle, 3, CheckAutoR));
            RootFSMTransitions.Add(new RootFSMTransition(RootFSMStatus.MajorUpKeep, RootFSMStatus.F_Cycle, 2, CheckAutoF));
            RootFSMTransitions.Add(new RootFSMTransition(RootFSMStatus.R_Cycle, RootFSMStatus.Career_Cycle));
        }

        protected override void Awake()
        {
            base.Awake();
            RoundLibDriver = new RoundLibDriver {owner = this};
        }
    }
}